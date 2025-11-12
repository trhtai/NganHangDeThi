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

        // Tạo tag mở đầu câu hỏi
        string prefix = cauHoi.Loai == LoaiCauHoi.TuLuan ? "<T" : "<";
        string tag = cauHoi.MucDo switch
        {
            MucDoCauHoi.NhanBiet => $"{prefix}NB>",
            MucDoCauHoi.ThongHieu => $"{prefix}TH>",
            MucDoCauHoi.VanDung => $"{prefix}VD>",
            MucDoCauHoi.VanDungCao => $"{prefix}VDC>",
            _ => "<NB>"
        };

        // Câu hỏi (nội dung + ảnh chèn đúng vị trí)
        string rawNoiDung = $"{tag} {cauHoi.NoiDung}";
        string? cauHoiImgPath = !string.IsNullOrWhiteSpace(cauHoi.HinhAnh)
            ? Path.Combine(imageBasePath, cauHoi.HinhAnh)
            : null;
        AppendTextWithImages(mainPart, body, rawNoiDung, cauHoiImgPath);

        // Các đáp án
        foreach (var d in cauHoi.DsCauTraLoi.OrderBy(x => x.ViTriGoc))
        {
            string prefixAns = d.LaDapAnDung ? "<$*>" : "<$>";
            if (!d.DaoViTri) prefixAns += "<@>";
            string rawDapAn = $"{prefixAns} {d.NoiDung}";

            string? dapAnImgPath = !string.IsNullOrWhiteSpace(d.HinhAnh)
                ? Path.Combine(imageBasePath, d.HinhAnh)
                : null;
            AppendTextWithImages(mainPart, body, rawDapAn, dapAnImgPath);
        }
    }

    private void AppendTextWithImages(MainDocumentPart mainPart, Body body, string input, string? imagePath)
    {
        var parts = input.Split("<img>", StringSplitOptions.None);

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
