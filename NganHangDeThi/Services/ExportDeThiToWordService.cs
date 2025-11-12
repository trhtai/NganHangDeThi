using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

public static class ExportDeThiToWordService
{
    public static void Export(DeThiExportData data, string filePath, string imageBasePath = "")
    {
        string mauTNPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Templates", "maudethi.docx");
        byte[] docxBytes = File.ReadAllBytes(mauTNPath);
        using var inputStream = new MemoryStream(docxBytes);
        using var docxStream = new MemoryStream();
        inputStream.CopyTo(docxStream);
        docxStream.Position = 0;

        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxStream, true))
        {
            var mainPart = wordDoc.MainDocumentPart!;
            var body = mainPart.Document.Body;
            if (body == null)
            {
                MessageBox.Show("File đề thi mẫu có vấn đề!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var placeholderMappings = new Dictionary<string, string>
            {
                { "<<tieu_de>>", data.DeThi.TieuDe.ToUpper() },
                { "<<ky_thi>>", data.DeThi.KyThi.ToUpper() },
                { "<<lop_hoc>>", data.DeThi.LopHoc!.MaLop },
                { "<<mon_hoc>>", data.DeThi.MonHoc!.TenMon },
                { "<<thoi_gian_thi>>", data.DeThi.ThoiGianLamBai.ToString() },
                { "<<ma_de>>", data.DeThi.MaDe.ToString() },
                { "<<ghi_chu>>", data.DeThi.GhiChu },
            };
            DocxHelper.ReplacePlaceholders(body, placeholderMappings);

            ReplaceDanhSachCauHoiWithStyledContent(mainPart, body, data.CauHoiVaDapAn, imageBasePath);

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }

    private static void ReplaceDanhSachCauHoiWithStyledContent(MainDocumentPart mainPart, Body body,
        List<(CauHoi, List<CauTraLoi>)> cauHoiVaDapAn, string imageBasePath)
    {
        var paragraphs = body.Elements<Paragraph>().ToList();
        var placeholderPara = paragraphs.FirstOrDefault(p => p.InnerText.Contains("<<danh_sach_cau_hoi>>"));
        if (placeholderPara == null) return;

        var paragraphProps = placeholderPara.GetFirstChild<ParagraphProperties>()?.CloneNode(true);
        var runProps = placeholderPara.Descendants<RunProperties>().FirstOrDefault()?.CloneNode(true);
        OpenXmlElement insertAfter = placeholderPara;

        int stt = 1;
        foreach (var (cauHoi, dapAnList) in cauHoiVaDapAn)
        {
            string rawNoiDung = $"Câu {stt++}: {SanitizeText(cauHoi.NoiDung)}";
            string? cauHoiImgPath = !string.IsNullOrWhiteSpace(cauHoi.HinhAnh)
                ? Path.Combine(imageBasePath, cauHoi.HinhAnh)
                : null;

            var cauHoiParas = CreateStyledParagraphWithImages(mainPart, rawNoiDung, cauHoiImgPath, paragraphProps, runProps);
            foreach (var para in cauHoiParas)
                insertAfter = body.InsertAfter(para, insertAfter);

            char ma = 'A';
            foreach (var d in dapAnList.OrderBy(d => d.ViTriGoc))
            {
                string rawDapAn = $"{ma++}. {SanitizeText(d.NoiDung)}";
                string? dapAnImgPath = !string.IsNullOrWhiteSpace(d.HinhAnh)
                    ? Path.Combine(imageBasePath, d.HinhAnh)
                    : null;

                var dapAnParas = CreateStyledParagraphWithImages(mainPart, rawDapAn, dapAnImgPath, paragraphProps, runProps);
                foreach (var para in dapAnParas)
                    insertAfter = body.InsertAfter(para, insertAfter);
            }

            insertAfter = body.InsertAfter(new Paragraph(new Run(new Text(""))), insertAfter);
        }

        placeholderPara.Remove();
    }

    private static List<Paragraph> CreateStyledParagraphWithImages(MainDocumentPart mainPart, string input,
    string? imagePath, OpenXmlElement? paraProps, OpenXmlElement? runProps)
    {
        var paragraphs = new List<Paragraph>();

        // Nếu không có <img> thì xử lý đơn giản
        if (!input.Contains("<img>", StringComparison.OrdinalIgnoreCase))
        {
            paragraphs.Add(CreateStyledTextParagraph(input, paraProps, runProps));
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                var imgPara = CreateImageParagraph(mainPart, imagePath, paraProps);
                if (imgPara != null)
                    paragraphs.Add(imgPara);
            }
            return paragraphs;
        }

        var parts = Regex.Split(input, "(?i)<img>", RegexOptions.IgnoreCase);
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim();

            if (!string.IsNullOrEmpty(part))
                paragraphs.Add(CreateStyledTextParagraph(part, paraProps, runProps));

            // Nếu có hình sau đoạn này
            if (i < parts.Length - 1 && !string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                var imgPara = CreateImageParagraph(mainPart, imagePath, paraProps);
                if (imgPara != null)
                    paragraphs.Add(imgPara);
            }
        }

        return paragraphs;
    }

    private static Paragraph CreateStyledTextParagraph(string input, OpenXmlElement? paraProps, OpenXmlElement? runProps)
    {
        var paragraph = new Paragraph();
        if (paraProps != null)
            paragraph.Append((OpenXmlElement)paraProps.CloneNode(true));

        string boldPrefix = "", remainingText = input;
        var match = Regex.Match(input, @"^(Câu\s*\d+:)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            boldPrefix = match.Groups[1].Value;
            remainingText = input.Substring(boldPrefix.Length).Trim();
        }

        if (!string.IsNullOrEmpty(boldPrefix))
        {
            var boldRun = new Run();
            var rp = runProps != null ? (RunProperties)runProps.CloneNode(true) : new RunProperties();
            rp.Bold = new Bold();
            boldRun.Append(rp);
            boldRun.Append(new Text(boldPrefix + " "));
            paragraph.Append(boldRun);
        }

        if (!string.IsNullOrEmpty(remainingText))
        {
            var normalRun = new Run();
            if (runProps != null)
                normalRun.Append((OpenXmlElement)runProps.CloneNode(true));
            normalRun.Append(new Text(remainingText));
            paragraph.Append(normalRun);
        }

        return paragraph;
    }

    private static Paragraph? CreateImageParagraph(MainDocumentPart mainPart, string imagePath, OpenXmlElement? paraProps)
    {
        var drawing = CreateImageDrawing(mainPart, imagePath);
        if (drawing == null) return null;

        var para = new Paragraph();
        if (paraProps != null)
            para.Append((OpenXmlElement)paraProps.CloneNode(true));

        para.Append(new Run(drawing));
        return para;
    }

    private static string SanitizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input, "<[^>]+>", "").Trim(); // Loại bỏ các tag <NB>, <TH>, <$...>
    }

    private static Drawing? CreateImageDrawing(MainDocumentPart mainPart, string imagePath)
    {
        if (!File.Exists(imagePath)) return null;

        var imageType = GetImagePartType(imagePath);
        var imagePart = mainPart.AddImagePart(imageType);

        using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        imagePart.FeedData(stream);
        var imagePartId = mainPart.GetIdOfPart(imagePart);

        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = 990000L, Cy = 792000L },
                new DW.EffectExtent(),
                new DW.DocProperties { Id = (UInt32Value)1U, Name = Path.GetFileName(imagePath) },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = Path.GetFileName(imagePath) },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = imagePartId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset(),
                                    new A.Extents { Cx = 990000L, Cy = 792000L }),
                                new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle })
                        ))
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
        {
            DistanceFromTop = 0U,
            DistanceFromBottom = 0U,
            DistanceFromLeft = 0U,
            DistanceFromRight = 0U
        });
    }

    private static ImagePartType GetImagePartType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" or ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            _ => ImagePartType.Jpeg
        };
    }
}
