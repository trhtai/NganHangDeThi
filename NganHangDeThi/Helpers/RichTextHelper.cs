using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NganHangDeThi.Helpers;

public static class RichTextHelper
{
    // Chuyển đổi nội dung từ RichTextBox sang chuỗi HTML đơn giản
    public static string GetHtmlFromRichTextBox(RichTextBox rtb)
    {
        TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        if (string.IsNullOrWhiteSpace(textRange.Text)) return string.Empty;

        StringBuilder sb = new StringBuilder();

        foreach (var block in rtb.Document.Blocks)
        {
            if (block is Paragraph para)
            {
                foreach (var inline in para.Inlines)
                {
                    if (inline is Run run)
                    {
                        string text = System.Net.WebUtility.HtmlEncode(run.Text);

                        // Xử lý các định dạng
                        if (run.FontWeight > FontWeights.Normal) text = $"<b>{text}</b>";
                        if (run.FontStyle == FontStyles.Italic) text = $"<i>{text}</i>";
                        if (run.TextDecorations.Contains(TextDecorations.Underline[0])) text = $"<u>{text}</u>";

                        if (run.Typography.Variants == FontVariants.Superscript) text = $"<sup>{text}</sup>";
                        if (run.Typography.Variants == FontVariants.Subscript) text = $"<sub>{text}</sub>";

                        // Xử lý màu đỏ (cho đáp án đúng)
                        if (run.Foreground is SolidColorBrush brush && brush.Color == Colors.Red)
                        {
                            text = $"<span style='color:red'>{text}</span>";
                        }

                        sb.Append(text);
                    }
                    else if (inline is LineBreak)
                    {
                        sb.Append("<br/>");
                    }
                }
                // Xuống dòng giữa các đoạn văn
                if (block != rtb.Document.Blocks.LastBlock)
                {
                    sb.Append("<br/>");
                }
            }
        }

        return sb.ToString();
    }

    // (Tùy chọn) Hàm hỗ trợ clear định dạng hoặc setup ban đầu nếu cần
    public static void ClearRichTextBox(RichTextBox rtb)
    {
        rtb.Document.Blocks.Clear();
    }
}