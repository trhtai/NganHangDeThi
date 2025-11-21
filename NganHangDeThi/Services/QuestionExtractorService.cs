using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;
using NganHangDeThi.Common.Configs;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Models;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Path = System.IO.Path;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

namespace NganHangDeThi.Services;

public class QuestionExtractorService
{
    private readonly AppDbContext _dbContext;
    private readonly string _baseImageFolder;
    private readonly string _sessionImageFolder;

    private readonly Regex _tagRegex = new(@"^<(NB|TH|VD|VDC|G)>", RegexOptions.IgnoreCase);

    public QuestionExtractorService(AppDbContext dbContext, IOptions<ImageStorageOptions> options)
    {
        _dbContext = dbContext;
        _baseImageFolder = options.Value.FolderPath;
        _sessionImageFolder = Path.Combine(_baseImageFolder, $"temp-{Guid.NewGuid()}");
        Directory.CreateDirectory(_sessionImageFolder);
    }

    public List<CauHoiRaw> ExtractQuestionsFromDocx(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        var paragraphs = doc.MainDocumentPart!.Document.Body!.Elements<Paragraph>().ToList();

        var result = new List<CauHoiRaw>();

        CauHoiRaw? currentGroup = null;
        CauHoiRaw? currentQuestion = null;

        for (int i = 0; i < paragraphs.Count; i++)
        {
            var para = paragraphs[i];
            string rawText = para.InnerText.Trim();

            bool shouldCloseGroup = rawText.Contains("</G>", StringComparison.OrdinalIgnoreCase);
            if (shouldCloseGroup)
            {
                rawText = Regex.Replace(rawText, "</G>", "", RegexOptions.IgnoreCase).Trim();
            }

            string htmlContent = ConvertToHtml(para, doc);
            if (shouldCloseGroup)
            {
                htmlContent = Regex.Replace(htmlContent, "&lt;/G&gt;", "", RegexOptions.IgnoreCase);
                htmlContent = Regex.Replace(htmlContent, "</G>", "", RegexOptions.IgnoreCase);
            }

            if (string.IsNullOrWhiteSpace(rawText) && ExtractImage(para, doc) == null)
            {
                if (shouldCloseGroup) goto CloseGroupStep;
                continue;
            }

            // 1. Bắt đầu nhóm <G>
            if (rawText.StartsWith("<G>", StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrentQuestion(result, currentGroup, currentQuestion);
                currentQuestion = null;

                // Check ngay dòng đầu tiên
                bool isClozeTest = rawText.Contains("___") || htmlContent.Contains("___");
                LoaiCauHoi groupType = isClozeTest ? LoaiCauHoi.DienKhuyet : LoaiCauHoi.TracNghiemMotDapAn;

                currentGroup = new CauHoiRaw
                {
                    Loai = groupType,
                    NoiDung = CleanTags(htmlContent, "<G>"),
                    MucDo = MucDoCauHoi.ThongHieu
                };
            }
            // 2. Bắt đầu câu hỏi <NB>, <TH>...
            else if (_tagRegex.IsMatch(rawText))
            {
                SaveCurrentQuestion(result, currentGroup, currentQuestion);

                currentQuestion = new CauHoiRaw();
                string tag = _tagRegex.Match(rawText).Value;

                currentQuestion.MucDo = ParseMucDo(tag);
                currentQuestion.Loai = LoaiCauHoi.TracNghiemMotDapAn;
                currentQuestion.NoiDung = CleanTags(htmlContent, tag);
            }
            // 3. Đáp án <$>, <$*>
            else if (rawText.StartsWith("<$"))
            {
                if (currentQuestion != null)
                {
                    bool hasRedColor = htmlContent.Contains("color:red") || htmlContent.Contains("color: red") || htmlContent.Contains("color:#FF0000");
                    bool isFixed = rawText.Contains("<@>");

                    string cleanAns = CleanTags(htmlContent, "<$>");
                    cleanAns = cleanAns.Replace("<@>", "").Replace("<$*>", "");

                    currentQuestion.DapAn.Add(new CauTraLoiRaw(
                        cleanAns, hasRedColor, (byte)(currentQuestion.DapAn.Count + 1), !isFixed, ExtractImage(para, doc)
                    ));
                }
            }
            // 4. Nội dung tiếp diễn (xuống dòng)
            else
            {
                if (currentQuestion != null)
                {
                    if (currentQuestion.DapAn.Count > 0)
                    {
                        var lastAns = currentQuestion.DapAn.Last();
                        lastAns.NoiDung += "<br/>" + htmlContent;
                    }
                    else
                    {
                        currentQuestion.NoiDung += "<br/>" + htmlContent;
                    }
                }
                else if (currentGroup != null)
                {
                    currentGroup.NoiDung += "<br/>" + htmlContent;

                    // --- FIX QUAN TRỌNG: Check tiếp diễn cho Điền Khuyết ---
                    // Nếu dòng nối tiếp này có chứa "___" -> Cập nhật nhóm thành Điền Khuyết ngay lập tức
                    if (currentGroup.Loai != LoaiCauHoi.DienKhuyet && (rawText.Contains("___") || htmlContent.Contains("___")))
                    {
                        currentGroup.Loai = LoaiCauHoi.DienKhuyet;
                    }
                    // -------------------------------------------------------
                }
            }

        // Xử lý đóng nhóm
        CloseGroupStep:
            if (shouldCloseGroup)
            {
                SaveCurrentQuestion(result, currentGroup, currentQuestion);
                currentQuestion = null;

                if (currentGroup != null)
                {
                    // Logic cập nhật ngược (chỉ chạy nếu KHÔNG PHẢI là Điền khuyết)
                    // Vì nếu đã là Điền khuyết (do có ___) thì ưu tiên giữ nguyên là Điền khuyết
                    if (currentGroup.Loai != LoaiCauHoi.DienKhuyet && currentGroup.CauHoiCon.Any())
                    {
                        if (currentGroup.CauHoiCon.Any(c => c.Loai == LoaiCauHoi.TuLuan))
                            currentGroup.Loai = LoaiCauHoi.TuLuan;
                        else if (currentGroup.CauHoiCon.Any(c => c.Loai == LoaiCauHoi.TracNghiemNhieuDapAn))
                            currentGroup.Loai = LoaiCauHoi.TracNghiemNhieuDapAn;
                    }

                    result.Add(currentGroup);
                    currentGroup = null;
                }
            }
        }

        SaveCurrentQuestion(result, currentGroup, currentQuestion);
        if (currentGroup != null)
        {
            if (currentGroup.Loai != LoaiCauHoi.DienKhuyet && currentGroup.CauHoiCon.Any())
            {
                if (currentGroup.CauHoiCon.Any(c => c.Loai == LoaiCauHoi.TuLuan))
                    currentGroup.Loai = LoaiCauHoi.TuLuan;
                else if (currentGroup.CauHoiCon.Any(c => c.Loai == LoaiCauHoi.TracNghiemNhieuDapAn))
                    currentGroup.Loai = LoaiCauHoi.TracNghiemNhieuDapAn;
            }
            result.Add(currentGroup);
        }

        return result;
    }

    private void SaveCurrentQuestion(List<CauHoiRaw> result, CauHoiRaw? group, CauHoiRaw? question)
    {
        if (question == null) return;

        int totalDapAn = question.DapAn.Count;
        int correctCount = question.DapAn.Count(x => x.LaDapAnDung);

        // Phân loại câu hỏi con
        if (totalDapAn == 1) question.Loai = LoaiCauHoi.TuLuan;
        else if (correctCount > 1) question.Loai = LoaiCauHoi.TracNghiemNhieuDapAn;
        else question.Loai = LoaiCauHoi.TracNghiemMotDapAn;

        if (group != null)
            group.CauHoiCon.Add(question);
        else
            result.Add(question);
    }

    // --- Helper functions (ParseMucDo, CleanTags, ConvertToHtml, ExtractImage...) giữ nguyên ---
    private MucDoCauHoi ParseMucDo(string tag)
    {
        tag = tag.ToUpper();
        if (tag.Contains("VDC")) return MucDoCauHoi.VanDungCao;
        if (tag.Contains("VD")) return MucDoCauHoi.VanDung;
        if (tag.Contains("TH")) return MucDoCauHoi.ThongHieu;
        return MucDoCauHoi.NhanBiet;
    }

    private string CleanTags(string html, string tagToRemove)
    {
        string pattern = Regex.Escape(tagToRemove).Replace("<", "&lt;").Replace(">", "&gt;");
        var res = Regex.Replace(html, tagToRemove, "", RegexOptions.IgnoreCase);
        res = Regex.Replace(res, pattern, "", RegexOptions.IgnoreCase);
        res = res.Replace("&lt;@&gt;", "").Replace("<@>", "");
        return res.Trim();
    }

    private string ConvertToHtml(Paragraph p, WordprocessingDocument doc, bool checkColorOnly = false)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var run in p.Descendants<Run>())
        {
            string text = run.InnerText;
            if (string.IsNullOrEmpty(text))
            {
                var imgPath = ExtractImageFromRun(run, doc);
                if (imgPath != null) sb.Append($"{{{{IMG:{imgPath}}}}}");
                continue;
            }

            text = System.Net.WebUtility.HtmlEncode(text);
            var props = run.RunProperties;
            if (props != null)
            {
                if (checkColorOnly)
                {
                    if (props.Color != null && (props.Color.Val == "FF0000" || props.Color.Val == "red")) return "color: red";
                    continue;
                }
                if (props.Bold != null) text = $"<b>{text}</b>";
                if (props.Italic != null) text = $"<i>{text}</i>";
                if (props.Underline != null) text = $"<u>{text}</u>";
                if (props.VerticalTextAlignment != null)
                {
                    if (props.VerticalTextAlignment.Val == VerticalPositionValues.Superscript) text = $"<sup>{text}</sup>";
                    else if (props.VerticalTextAlignment.Val == VerticalPositionValues.Subscript) text = $"<sub>{text}</sub>";
                }
                if (props.Color != null && (props.Color.Val == "FF0000" || props.Color.Val == "red"))
                {
                    text = $"<span style='color:red'>{text}</span>";
                }
            }
            sb.Append(text);
        }
        return sb.ToString();
    }

    private string? ExtractImage(Paragraph para, WordprocessingDocument doc)
    {
        foreach (var run in para.Descendants<Run>())
        {
            var img = ExtractImageFromRun(run, doc);
            if (img != null) return img;
        }
        return null;
    }

    private string? ExtractImageFromRun(Run run, WordprocessingDocument doc)
    {
        var blip = run.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
        if (blip == null || string.IsNullOrEmpty(blip.Embed)) return null;

        var imagePart = (ImagePart)doc.MainDocumentPart!.GetPartById(blip.Embed);
        var fileName = $"{Guid.NewGuid()}.png";
        var savePath = Path.Combine(_sessionImageFolder, fileName);

        using (var stream = imagePart.GetStream())
        using (var fileStream = File.Create(savePath))
        {
            stream.CopyTo(fileStream);
        }
        return fileName;
    }

    public void CleanupTemporaryImages()
    {
        if (Directory.Exists(_sessionImageFolder)) Directory.Delete(_sessionImageFolder, true);
    }

    public void CommitImages()
    {
        if (!Directory.Exists(_sessionImageFolder)) return;
        foreach (var file in Directory.GetFiles(_sessionImageFolder))
        {
            var dest = Path.Combine(_baseImageFolder, Path.GetFileName(file));
            if (!File.Exists(dest)) File.Move(file, dest);
        }
        Directory.Delete(_sessionImageFolder, true);
    }

    public int SaveToDatabase(List<CauHoiRaw> questions, int chuongId)
    {
        using var transaction = _dbContext.Database.BeginTransaction();
        try
        {
            int count = 0;
            foreach (var q in questions)
            {
                count += SaveQuestionRecursive(q, chuongId, null);
            }
            _dbContext.SaveChanges();
            transaction.Commit();
            return count;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private int SaveQuestionRecursive(CauHoiRaw qRaw, int chuongId, int? parentId)
    {
        var entity = new CauHoi
        {
            NoiDung = qRaw.NoiDung,
            MucDo = qRaw.MucDo,
            Loai = qRaw.Loai,
            ChuongId = chuongId,
            HinhAnh = qRaw.HinhAnh,
            ParentId = parentId,
            DaRaDe = false
        };

        foreach (var ans in qRaw.DapAn)
        {
            entity.DsCauTraLoi.Add(new CauTraLoi
            {
                NoiDung = ans.NoiDung,
                LaDapAnDung = ans.LaDapAnDung,
                ViTriGoc = ans.ViTriGoc,
                DaoViTri = ans.DaoViTri,
                HinhAnh = ans.HinhAnh
            });
        }

        _dbContext.CauHoi.Add(entity);
        _dbContext.SaveChanges();

        int savedCount = 1;
        if (qRaw.CauHoiCon.Any())
        {
            foreach (var child in qRaw.CauHoiCon)
            {
                savedCount += SaveQuestionRecursive(child, chuongId, entity.Id);
            }
        }
        return savedCount;
    }
}