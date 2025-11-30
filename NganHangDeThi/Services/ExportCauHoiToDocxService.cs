using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.Entity;
using System.IO;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace NganHangDeThi.Services;

public class ExportCauHoiToDocxService
{
    public void ExportToDocx(CauHoi cauHoi, string filePath, string imageBasePath)
    {
        using WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());

        var body = mainPart.Document.Body;

        // Kiểm tra nếu là câu chùm (có danh sách con)
        if (cauHoi.DsCauHoiCon != null && cauHoi.DsCauHoiCon.Any())
        {
            // 1. Mở thẻ <G> và nội dung cha
            string rawParent = $"<G> {cauHoi.NoiDung}";
            AppendTextWithImages(mainPart, body, rawParent, GetImagePath(cauHoi, imageBasePath));

            // 2. Xuất các câu con
            foreach (var child in cauHoi.DsCauHoiCon.OrderBy(c => c.Id))
            {
                ExportSingleQuestion(mainPart, body, child, imageBasePath);
            }

            // 3. Đóng thẻ </G>
            body.Append(new Paragraph(new Run(new Text("</G>"))));
        }
        else
        {
            // Câu đơn bình thường
            ExportSingleQuestion(mainPart, body, cauHoi, imageBasePath);
        }

        mainPart.Document.Save();
    }

    private void ExportSingleQuestion(MainDocumentPart mainPart, Body body, CauHoi cauHoi, string imageBasePath)
    {
        // Tạo tag mở đầu câu hỏi (NB, TH, VD, VDC)
        string prefix = cauHoi.Loai == LoaiCauHoi.TuLuan ? "<T" : "<";
        string tag = cauHoi.MucDo switch
        {
            MucDoCauHoi.NhanBiet => $"{prefix}NB>",
            MucDoCauHoi.ThongHieu => $"{prefix}TH>",
            MucDoCauHoi.VanDung => $"{prefix}VD>",
            MucDoCauHoi.VanDungCao => $"{prefix}VDC>",
            _ => "<NB>"
        };

        // Nếu muốn cố định câu hỏi con trong chùm, thêm <@>
        // (Ở đây tạm thời chưa xử lý logic <@> cho câu hỏi, chỉ xử lý cho đáp án)

        // Nội dung câu hỏi
        string rawNoiDung = $"{tag} {cauHoi.NoiDung}";
        AppendTextWithImages(mainPart, body, rawNoiDung, GetImagePath(cauHoi, imageBasePath));

        // Các đáp án
        foreach (var d in cauHoi.DsCauTraLoi.OrderBy(x => x.ViTriGoc))
        {
            string prefixAns = d.LaDapAnDung ? "<$*>" : "<$>";
            if (!d.DaoViTri) prefixAns += "<@>";
            string rawDapAn = $"{prefixAns} {d.NoiDung}";

            AppendTextWithImages(mainPart, body, rawDapAn, GetImagePath(d, imageBasePath));
        }
    }

    private void AppendTextWithImages(MainDocumentPart mainPart, Body body, string input, string? imagePath)
    {
        var parts = input.Split(new[] { "<img>" }, StringSplitOptions.None);

        for (int i = 0; i < parts.Length; i++)
        {
            string textPart = parts[i].Trim();

            if (!string.IsNullOrWhiteSpace(textPart))
            {
                var para = new Paragraph(new Run(new Text(textPart)));
                body.Append(para);
            }

            // Nếu còn <img> sau đoạn này thì chèn ảnh
            if (i < parts.Length - 1 && !string.IsNullOrWhiteSpace(imagePath))
            {
                // 1. Dòng trống trước ảnh
                body.Append(new Paragraph());

                // 2. Dòng ảnh
                var drawing = CreateImageDrawing(mainPart, imagePath);
                if (drawing != null)
                {
                    var imgPara = new Paragraph(new Run(drawing));
                    body.Append(imgPara);
                }

                // 3. Dòng trống sau ảnh
                body.Append(new Paragraph());
            }
        }
    }

    private string? GetImagePath(object entity, string basePath)
    {
        string? relativePath = null;
        if (entity is CauHoi ch) relativePath = ch.HinhAnh;
        else if (entity is CauTraLoi ctl) relativePath = ctl.HinhAnh;

        if (string.IsNullOrWhiteSpace(relativePath)) return null;
        return Path.Combine(basePath, relativePath);
    }

    private Drawing? CreateImageDrawing(MainDocumentPart mainPart, string imagePath)
    {
        if (!File.Exists(imagePath)) return null;

        ImagePartType imageType = GetImagePartType(imagePath);
        ImagePart imagePart = mainPart.AddImagePart(imageType);

        using FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        imagePart.FeedData(stream);

        string imagePartId = mainPart.GetIdOfPart(imagePart);

        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = 990000L, Cy = 792000L }, // kích thước ảnh
                new DW.EffectExtent
                {
                    LeftEdge = 0L,
                    TopEdge = 0L,
                    RightEdge = 0L,
                    BottomEdge = 0L
                },
                new DW.DocProperties
                {
                    Id = (UInt32Value)1U,
                    Name = Path.GetFileName(imagePath)
                },
                new DW.NonVisualGraphicFrameDrawingProperties(
                    new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties
                                {
                                    Id = 0U,
                                    Name = Path.GetFileName(imagePath)
                                },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip
                                {
                                    Embed = imagePartId,
                                    CompressionState = A.BlipCompressionValues.Print
                                },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = 990000L, Cy = 792000L }),
                                new A.PresetGeometry(new A.AdjustValueList())
                                { Preset = A.ShapeTypeValues.Rectangle })
                        ))
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
            )
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            });
    }

    private ImagePartType GetImagePartType(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return ImagePartType.Jpeg;

        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".png" => ImagePartType.Png,
            ".jpg" or ".jpeg" => ImagePartType.Jpeg,
            ".gif" => ImagePartType.Gif,
            ".bmp" => ImagePartType.Bmp,
            _ => ImagePartType.Jpeg
        };
    }
}
