using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace NganHangDeThi.Services;

public static class ExportDapAnService
{
    public static void Export(DeThi deThi, string filePath, string imageBasePath = "")
    {
        string mauTNPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Templates", "maudapan.docx");
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
                MessageBox.Show("File đáp án mẫu có vấn đề!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var placeholderMappings = new Dictionary<string, string>
            {
                { "<<tieu_de>>", deThi.TieuDe.ToUpper() },
                { "<<ky_thi>>", deThi.KyThi.ToUpper() },
                { "<<lop_hoc>>", deThi.LopHoc!.MaLop },
                { "<<mon_hoc>>", deThi.MonHoc!.TenMon },
                { "<<ma_de>>", deThi.MaDe.ToString() },
            };
            DocxHelper.ReplacePlaceholders(body, placeholderMappings);

            int stt = 1;
            foreach (var chiTiet in deThi.DsChiTietDeThi.OrderBy(ct => ct.Id))
            {
                var cauHoi = chiTiet.CauHoi;
                string rawNoiDung = $"Câu {stt++}: {SanitizeText(cauHoi.NoiDung)}";
                string? cauHoiImgPath = !string.IsNullOrWhiteSpace(cauHoi.HinhAnh)
                    ? Path.Combine(imageBasePath, cauHoi.HinhAnh)
                    : null;

                var cauHoiParas = CreateParagraphsWithImage(mainPart, rawNoiDung, cauHoiImgPath, boldPrefix: true);
                foreach (var para in cauHoiParas)
                    body.Append(para);

                var dsDapAn = chiTiet.DsDapAnTrongDe.OrderBy(d => d.ViTri).ToList();
                char ma = 'A';

                foreach (var dapAn in dsDapAn)
                {
                    string prefix = $"{ma++}. ";
                    string rawDapAn = $"{prefix}{SanitizeText(dapAn.NoiDung)}";

                    if (dapAn.LaDapAnDung)
                        rawDapAn += "  ✅";

                    string? dapAnImgPath = !string.IsNullOrWhiteSpace(dapAn.HinhAnh)
                        ? Path.Combine(imageBasePath, dapAn.HinhAnh)
                        : null;

                    var dapAnParas = CreateParagraphsWithImage(mainPart, rawDapAn, dapAnImgPath);
                    foreach (var para in dapAnParas)
                        body.Append(para);
                }

                body.Append(new Paragraph(new Run(new Break()))); // Dòng trống sau mỗi câu
            }

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }

    private static List<Paragraph> CreateParagraphsWithImage(MainDocumentPart mainPart, string input, string? imagePath, bool boldPrefix = false)
    {
        var paragraphs = new List<Paragraph>();

        if (!input.Contains("<img>", StringComparison.OrdinalIgnoreCase))
        {
            paragraphs.Add(CreateStyledTextParagraph(input, boldPrefix));
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                var imgPara = CreateImageParagraph(mainPart, imagePath);
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
                paragraphs.Add(CreateStyledTextParagraph(part, boldPrefix && i == 0));

            if (i < parts.Length - 1 && !string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                var imgPara = CreateImageParagraph(mainPart, imagePath);
                if (imgPara != null)
                    paragraphs.Add(imgPara);
            }
        }

        return paragraphs;
    }

    private static Paragraph CreateStyledTextParagraph(string text, bool bold = false, string fontSize = "24", JustificationValues alignment = JustificationValues.Left)
    {
        var runProps = new RunProperties(
            new RunFonts { Ascii = "Times New Roman" },
            new FontSize { Val = fontSize });

        if (bold)
            runProps.Append(new Bold());

        var run = new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        var paraProps = new ParagraphProperties(new Justification { Val = alignment });

        return new Paragraph(paraProps, run);
    }

    private static Paragraph? CreateImageParagraph(MainDocumentPart mainPart, string imagePath)
    {
        var drawing = CreateImageDrawing(mainPart, imagePath);
        if (drawing == null) return null;

        var para = new Paragraph(new Run(drawing));
        return para;
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

    private static string SanitizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return Regex.Replace(input, "<[^>]+>", "").Trim(); // loại bỏ <NB>, <TH>, <$...>
    }
}


