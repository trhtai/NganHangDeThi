using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Options;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Models;
using System.IO;
using System.Text.RegularExpressions;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Path = System.IO.Path;

namespace NganHangDeThi.Services;

public class QuestionExtractorService
{
    private readonly AppDbContext _dbContext;
    private readonly string _baseImageFolder;
    private readonly string _sessionImageFolder;
    private readonly Guid _sessionId;

    public QuestionExtractorService(AppDbContext dbContext, IOptions<ImageStorageOptions> options)
    {
        _dbContext = dbContext;
        _baseImageFolder = options.Value.FolderPath;
        _sessionId = Guid.NewGuid();
        _sessionImageFolder = Path.Combine(_baseImageFolder, $"temp-{_sessionId}");
        Directory.CreateDirectory(_sessionImageFolder);
    }

    public List<CauHoiRaw> ExtractQuestionsFromDocx(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        var result = new List<CauHoiRaw>();
        List<CauTraLoiRaw> currentAnswers = new();
        string? currentContent = null;
        string? currentHinhAnh = null;
        MucDoCauHoi mucDo = default;
        LoaiCauHoi loai = default;
        byte viTri = 1;

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var paragraph = paragraphs[i];
            string text = paragraph.InnerText.Trim();

            if (string.IsNullOrWhiteSpace(text) && !ContainsImage(paragraph))
                continue;

            // Nếu là câu hỏi
            if (Regex.IsMatch(text, @"^<T?(NB|TH|VD|VDC)>", RegexOptions.IgnoreCase))
            {
                if (currentContent != null && currentAnswers.Any())
                {
                    result.Add(new CauHoiRaw(currentContent.Trim(), mucDo, loai, new(currentAnswers), currentHinhAnh));
                    currentAnswers.Clear();
                    currentHinhAnh = null;
                    viTri = 1;
                }

                loai = text.StartsWith("<T") ? LoaiCauHoi.TuLuan : LoaiCauHoi.TracNghiemMotDapAn;
                mucDo = text.Contains("NB") ? MucDoCauHoi.NhanBiet :
                        text.Contains("TH") ? MucDoCauHoi.ThongHieu :
                        text.Contains("VD") && !text.Contains("VDC") ? MucDoCauHoi.VanDung :
                        MucDoCauHoi.VanDungCao;

                currentContent = Regex.Replace(text, @"^<T?(NB|TH|VD|VDC)>", "").Trim();
                currentHinhAnh = null;

                while (i + 1 < paragraphs.Count)
                {
                    var next = paragraphs[i + 1];
                    string nextText = next.InnerText.Trim();

                    if (nextText.StartsWith("<$")) break;
                    i++;

                    // Chèn tag <img> nếu đoạn là ảnh
                    if (ContainsImage(next))
                    {
                        if (currentHinhAnh == null)
                            currentHinhAnh = SaveImageFromParagraph(next, doc);

                        currentContent += " <img>";
                    }

                    // Nối đoạn text nếu có
                    if (!string.IsNullOrWhiteSpace(nextText))
                    {
                        currentContent += " " + nextText;
                    }
                }
            }
            // Nếu là đáp án
            else if (text.StartsWith("<$"))
            {
                bool daoViTri = text.Contains("<@>");
                bool laDapAnDung = text.StartsWith("<$*>");

                string nd = text
                    .Replace("<$*>", "")
                    .Replace("<$>", "")
                    .Replace("<@>", "")
                    .Trim();

                var imagePath = SaveImageFromParagraph(paragraph, doc);
                if (imagePath != null)
                    nd += " <img>";

                currentAnswers.Add(new CauTraLoiRaw(nd, laDapAnDung, viTri++, !daoViTri, imagePath));
            }
        }

        if (currentContent != null && currentAnswers.Count != 0)
        {
            result.Add(new CauHoiRaw(currentContent.Trim(), mucDo, loai, currentAnswers, currentHinhAnh));
        }

        return result;
    }

    private bool ContainsImage(Paragraph para)
    {
        return para.Descendants<Blip>().Any();
    }

    private string? SaveImageFromParagraph(Paragraph para, WordprocessingDocument doc)
    {
        if (!Directory.Exists(_sessionImageFolder))
            Directory.CreateDirectory(_sessionImageFolder);

        var blip = para.Descendants<Blip>().FirstOrDefault();
        if (blip == null) return null;

        var embed = blip.Embed?.Value;
        if (string.IsNullOrEmpty(embed)) return null;

        var imagePart = (ImagePart)doc.MainDocumentPart!.GetPartById(embed);
        var imageExtension = GetImageExtension(imagePart.ContentType);
        var fileName = $"{Guid.NewGuid()}{imageExtension}";
        var imagePath = Path.Combine(_sessionImageFolder, fileName);

        using var stream = imagePart.GetStream();
        using var file = File.Create(imagePath);
        stream.CopyTo(file);

        return fileName;
    }

    private string GetImageExtension(string contentType)
    {
        return contentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/gif" => ".gif",
            _ => ".img"
        };
    }

    public void CleanupTemporaryImages()
    {
        if (Directory.Exists(_sessionImageFolder))
        {
            Directory.Delete(_sessionImageFolder, true);
        }
    }

    public void CommitImages()
    {
        if (!Directory.Exists(_sessionImageFolder)) return;

        foreach (var file in Directory.GetFiles(_sessionImageFolder))
        {
            var fileName = Path.GetFileName(file);
            var targetPath = Path.Combine(_baseImageFolder, fileName);

            if (!File.Exists(targetPath))
            {
                File.Move(file, targetPath);
            }
        }

        Directory.Delete(_sessionImageFolder, true);
    }

    public int SaveToDatabase(List<CauHoiRaw> questions, int chuongId)
    {
        using var transaction = _dbContext.Database.BeginTransaction();

        try
        {
            var noiDungCauHoiTrongDb = _dbContext.CauHoi
                .Where(c => c.ChuongId == chuongId)
                .Select(c => c.NoiDung.Trim().ToLower())
                .ToHashSet();

            int soCauHoiDaThem = 0;

            foreach (var q in questions)
            {
                var noiDungChuanHoa = q.NoiDung.Trim().ToLower();

                if (noiDungCauHoiTrongDb.Contains(noiDungChuanHoa))
                    continue;

                var cauHoi = new CauHoi
                {
                    NoiDung = q.NoiDung,
                    MucDo = q.MucDo,
                    Loai = q.Loai,
                    ChuongId = chuongId,
                    HinhAnh = q.HinhAnh,
                    DaRaDe = false,
                    DsCauTraLoi = []
                };

                foreach (var d in q.DapAn)
                {
                    cauHoi.DsCauTraLoi.Add(new CauTraLoi
                    {
                        NoiDung = d.NoiDung,
                        LaDapAnDung = d.LaDapAnDung,
                        ViTriGoc = d.ViTriGoc,
                        DaoViTri = d.DaoViTri,
                        HinhAnh = d.HinhAnh
                    });
                }

                _dbContext.CauHoi.Add(cauHoi);
                soCauHoiDaThem++;
            }

            _dbContext.SaveChanges();
            transaction.Commit();
            return soCauHoiDaThem;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
