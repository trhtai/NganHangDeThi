using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Common.Enum;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace NganHangDeThi.Helpers
{
    public static class DocxHelper
    {
        /// <summary>
        /// Tạo một ô table với các tuỳ chọn.
        /// </summary>
        /// <param name="text">Nội dung của ô.</param>
        /// <param name="isBold">Có in đậm hay không (mặc định false).</param>
        /// <param name="justification">
        /// Căn lề của nội dung. Nếu null thì không thêm ParagraphProperties.
        /// Ví dụ: JustificationValues.Left, JustificationValues.Center, JustificationValues.Right.
        /// </param>
        /// <param name="marginSide">
        /// Tuỳ chọn margin. Nếu là Left hoặc Right thì sẽ áp dụng margin tương ứng (mặc định None).
        /// </param>
        /// <returns>TableCell được tạo ra với các định dạng đã chọn.</returns>
        public static TableCell CreateCell(
            string text,
            bool isBold = false,
            JustificationValues? justification = null,
            MarginSide marginSide = MarginSide.None,
            string? fontSize = "24")
        {
            // Thiết lập RunProperties: cỡ chữ 9pt (18 half-points) và in đậm nếu cần.
            var runProperties = new RunProperties(new FontSize { Val = fontSize });
            if (isBold)
            {
                runProperties.Append(new Bold());
            }
            var run = new Run(runProperties, new Text(text));

            // Tạo Paragraph, thêm ParagraphProperties nếu có chỉ định căn lề.
            Paragraph paragraph;
            if (justification.HasValue)
            {
                paragraph = new Paragraph(
                    new ParagraphProperties(new Justification { Val = justification.Value }),
                run);
            }
            else
            {
                paragraph = new Paragraph(run);
            }

            // Thiết lập TableCellProperties với VerticalAlignment mặc định.
            var cellPropsElements = new List<OpenXmlElement>
            {
                new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center }
            };

            // Áp dụng margin theo marginSide nếu có.
            if (marginSide == MarginSide.Left)
            {
                cellPropsElements.Add(new TableCellMargin(new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa }));
            }
            else if (marginSide == MarginSide.Right)
            {
                cellPropsElements.Add(new TableCellMargin(new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa }));
            }

            var cellProps = new TableCellProperties(cellPropsElements.ToArray());

            // Tạo TableCell kết hợp cell properties và paragraph.
            return new TableCell(cellProps, paragraph);
        }

        public static void ReplacePlaceholders(Body body, Dictionary<string, string> placeholderMappings)
        {
            // Xây dựng biểu thức chính quy chỉ khớp các placeholder đã được định nghĩa trong từ điển
            string pattern = string.Join("|", placeholderMappings.Keys.Select(Regex.Escape));
            var regex = new Regex(pattern);

            // Duyệt qua tất cả các phần tử Text trong tài liệu
            foreach (var text in body!.Descendants<Text>())
            {
                if (text.Text.Contains("<<"))
                {
                    text.Text = regex.Replace(text.Text, match =>
                        placeholderMappings.TryGetValue(match.Value, out var replacement)
                            ? replacement
                            : match.Value);
                }
            }
        }
    }
}
