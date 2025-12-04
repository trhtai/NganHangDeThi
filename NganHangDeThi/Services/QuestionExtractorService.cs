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

                // Check ngay dòng đầu tiên xem có phải điền khuyết không
                bool isClozeTest = rawText.Contains("___") || htmlContent.Contains("___");
                // Lưu ý: Nếu chưa xác định được loại chùm cụ thể, ta tạm để TracNghiemMotDapAn (sẽ được Validate lại sau)
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
                    if (currentGroup.Loai != LoaiCauHoi.DienKhuyet && (rawText.Contains("___") || htmlContent.Contains("___")))
                    {
                        currentGroup.Loai = LoaiCauHoi.DienKhuyet;
                    }
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
                    // --- VALIDATE VÀ GÁN LOẠI CHÙM ---
                    ValidateAndSetClusterType(currentGroup);
                    // ---------------------------------

                    if (currentGroup.CauHoiCon.Any())
                    {
                        var maxLevel = currentGroup.CauHoiCon.Max(c => (int)c.MucDo);
                        currentGroup.MucDo = (MucDoCauHoi)maxLevel;
                    }

                    result.Add(currentGroup);
                    currentGroup = null;
                }
            }
        }

        SaveCurrentQuestion(result, currentGroup, currentQuestion);
        if (currentGroup != null)
        {
            ValidateAndSetClusterType(currentGroup);
            if (currentGroup.CauHoiCon.Any())
            {
                var maxLevel = currentGroup.CauHoiCon.Max(c => (int)c.MucDo);
                currentGroup.MucDo = (MucDoCauHoi)maxLevel;
            }
            result.Add(currentGroup);
        }

        return result;
    }

    // --- HÀM QUAN TRỌNG: Kiểm tra câu con và gán loại câu cha ---
    private void ValidateAndSetClusterType(CauHoiRaw group)
    {
        if (!group.CauHoiCon.Any()) return;

        // 1. Nếu là Điền Khuyết -> Giữ nguyên
        if (group.Loai == LoaiCauHoi.DienKhuyet) return;

        // 2. Kiểm tra tính đồng nhất
        var firstType = group.CauHoiCon.First().Loai;
        bool isMixed = group.CauHoiCon.Any(c => c.Loai != firstType);

        if (isMixed)
        {
            string preview = group.NoiDung.Length > 50 ? group.NoiDung.Substring(0, 47) + "..." : group.NoiDung;
            throw new InvalidDataException(
                $"Lỗi định dạng tại câu chùm: \"{preview}\"\n" +
                "Nguyên nhân: Câu chùm chứa các câu hỏi con KHÔNG CÙNG LOẠI.\n" +
                "Vui lòng tách riêng chúng ra các nhóm <G> khác nhau.");
        }

        // 3. Gán loại cho câu Cha (Mapping 1-1 với câu con)
        switch (firstType)
        {
            case LoaiCauHoi.TracNghiemMotDapAn:
                group.Loai = LoaiCauHoi.ChumTracNghiemMotDapAn;
                break;
            case LoaiCauHoi.TracNghiemNhieuDapAn:
                group.Loai = LoaiCauHoi.ChumTracNghiemNhieuDapAn;
                break;
            case LoaiCauHoi.TuLuan:
                group.Loai = LoaiCauHoi.ChumTuLuan;
                break;
            default:
                // Trường hợp fallback (ít xảy ra)
                group.Loai = LoaiCauHoi.ChumTracNghiemMotDapAn;
                break;
        }
    }

    private void SaveCurrentQuestion(List<CauHoiRaw> result, CauHoiRaw? group, CauHoiRaw? question)
    {
        if (question == null) return;

        int totalDapAn = question.DapAn.Count;
        int correctCount = question.DapAn.Count(x => x.LaDapAnDung);

        // Phân loại câu hỏi con
        if (totalDapAn == 1) question.Loai = LoaiCauHoi.TuLuan; // Tự luận đơn
        else if (correctCount > 1) question.Loai = LoaiCauHoi.TracNghiemNhieuDapAn;
        else question.Loai = LoaiCauHoi.TracNghiemMotDapAn;

        if (group != null)
            group.CauHoiCon.Add(question);
        else
            result.Add(question);
    }

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
                // (Phần xử lý Bold/Italic cũ giữ nguyên, vì HtmlToWordHelper mới là nơi quan trọng hơn)
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
            var dbQuestionsContent = _dbContext.CauHoi
                .Where(x => x.ChuongId == chuongId && x.ParentId == null)
                .Select(x => x.NoiDung)
                .ToList();

            var existingSignatures = new HashSet<string>(
                dbQuestionsContent.Select(c => GetComparisonKey(c))
            );

            int count = 0;
            foreach (var q in questions)
            {
                string currentSignature = GetComparisonKey(q.NoiDung);
                if (existingSignatures.Contains(currentSignature)) continue;

                existingSignatures.Add(currentSignature);
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

        // Nếu là câu chùm (có con) thì không tính câu cha (savedCount = 0), ngược lại tính là 1
        int savedCount = qRaw.CauHoiCon.Any() ? 0 : 1;

        if (qRaw.CauHoiCon.Any())
        {
            foreach (var child in qRaw.CauHoiCon)
            {
                savedCount += SaveQuestionRecursive(child, chuongId, entity.Id);
            }
        }

        return savedCount;
    }

    private string GetComparisonKey(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        string normalized = Regex.Replace(content, @"\{\{IMG:[^}]+\}\}", "{{IMG}}");
        return normalized.Trim().ToLowerInvariant();
    }
}