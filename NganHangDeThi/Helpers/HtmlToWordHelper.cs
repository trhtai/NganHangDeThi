using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace NganHangDeThi.Helpers;

public static class HtmlToWordHelper
{
    // Regex tách token: Thẻ HTML, Placeholder ảnh, hoặc Text thường
    private static readonly Regex _tokenRegex = new Regex(@"(<[^>]+>)|(\{\{IMG:[^}]+\}\})|([^<{}]+)", RegexOptions.Compiled);

    public static List<OpenXmlElement> ConvertHtmlToElements(MainDocumentPart mainPart, string html, string imageBasePath, bool ignoreColor = false)
    {
        var elements = new List<OpenXmlElement>();
        if (string.IsNullOrWhiteSpace(html)) return elements;

        string decodedHtml = WebUtility.HtmlDecode(html);
        var matches = _tokenRegex.Matches(decodedHtml);

        // Trạng thái định dạng
        bool isBold = false;
        bool isItalic = false;
        bool isUnderline = false;
        bool isSub = false;
        bool isSup = false;
        bool isRed = false;

        foreach (Match match in matches)
        {
            string token = match.Value;

            if (match.Groups[1].Success) // Là Tag HTML
            {
                string lowerTag = token.ToLowerInvariant();
                if (lowerTag.Contains("<b>") || lowerTag.Contains("<strong>")) isBold = true;
                else if (lowerTag.Contains("</b>") || lowerTag.Contains("</strong>")) isBold = false;
                else if (lowerTag.Contains("<i>") || lowerTag.Contains("<em>")) isItalic = true;
                else if (lowerTag.Contains("</i>") || lowerTag.Contains("</em>")) isItalic = false;
                else if (lowerTag.Contains("<u>")) isUnderline = true;
                else if (lowerTag.Contains("</u>")) isUnderline = false;
                else if (lowerTag.Contains("<sub>")) isSub = true;
                else if (lowerTag.Contains("</sub>")) isSub = false;
                else if (lowerTag.Contains("<sup>")) isSup = true;
                else if (lowerTag.Contains("</sup>")) isSup = false;

                else if (lowerTag.Contains("color:red") || lowerTag.Contains("color: red"))
                {
                    if (!ignoreColor) isRed = true;
                }
                else if (lowerTag.Contains("</span>")) isRed = false;

                else if (lowerTag.Contains("<br>") || lowerTag.Contains("<br/>"))
                {
                    elements.Add(new Run(new Break()));
                }
            }
            else if (match.Groups[2].Success) // Placeholder ảnh
            {
                string fileName = token.Replace("{{IMG:", "").Replace("}}", "");
                var drawing = CreateImageDrawing(mainPart, Path.Combine(imageBasePath, fileName));
                if (drawing != null) elements.Add(new Run(drawing));
            }
            else if (match.Groups[3].Success) // Text
            {
                var run = new Run(new Text(token) { Space = SpaceProcessingModeValues.Preserve });
                var props = new RunProperties();
                bool hasProps = false; // Biến này thực ra không cần thiết nữa vì ta luôn set font/size, nhưng giữ lại cho logic cũ

                // --- SỬA ĐỔI QUAN TRỌNG TẠI ĐÂY ---
                // Thay vì chỉ set khi True, ta set luôn giá trị Val = isBold.
                // Nếu isBold = false -> Nó sẽ sinh ra thẻ <w:b val="0"/> (Tắt in đậm).
                // Điều này giúp ngắt sự kế thừa in đậm từ "Câu 1:"
                props.Bold = new Bold { Val = isBold };

                if (isItalic) { props.Italic = new Italic(); }
                if (isUnderline) { props.Underline = new Underline { Val = UnderlineValues.Single }; }

                if (isRed) { props.Color = new Color { Val = "FF0000" }; }

                if (isSub) { props.VerticalTextAlignment = new VerticalTextAlignment { Val = VerticalPositionValues.Subscript }; }
                if (isSup) { props.VerticalTextAlignment = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }; }

                props.RunFonts = new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" };
                props.FontSize = new FontSize { Val = "24" };

                run.RunProperties = props;
                elements.Add(run);
            }
        }

        return elements;
    }

    // ... (Hàm CreateImageDrawing và GetImagePartType giữ nguyên) ...
    public static Drawing? CreateImageDrawing(MainDocumentPart mainPart, string imagePath)
    {
        if (!File.Exists(imagePath)) return null;

        var imageType = GetImagePartType(imagePath);
        var imagePart = mainPart.AddImagePart(imageType);

        using var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        imagePart.FeedData(stream);
        var imagePartId = mainPart.GetIdOfPart(imagePart);

        long cx = 990000L * 2;
        long cy = 792000L * 2;

        return new Drawing(
            new DW.Inline(
                new DW.Extent { Cx = cx, Cy = cy },
                new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
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
                                    new A.Offset { X = 0L, Y = 0L },
                                    new A.Extents { Cx = cx, Cy = cy }),
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

    private static ImagePartType GetImagePartType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => ImagePartType.Png,
        ".jpg" or ".jpeg" => ImagePartType.Jpeg,
        ".gif" => ImagePartType.Gif,
        ".bmp" => ImagePartType.Bmp,
        _ => ImagePartType.Jpeg
    };
}