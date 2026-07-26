using System.Globalization;
using System.Text;

namespace NganHangDeThi.Helpers;

public static class StringHelper
{
    public static string ToUnSign(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // 1. Xử lý riêng chữ Đ/đ của tiếng Việt
        string str = input.Replace("Đ", "D").Replace("đ", "d");

        // 2. Tách các dấu thanh ra khỏi ký tự gốc (ví dụ: "á" -> "a" + "´")
        str = str.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in str)
        {
            // Bỏ qua các ký tự là dấu (NonSpacingMark)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // 3. Ghép lại và chuyển về chữ thường để tối ưu khi Search (toan hoc)
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }
}
