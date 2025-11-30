using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using System.IO;
using System.Text.RegularExpressions;

namespace NganHangDeThi.Services;

public static class ExportDeThiToWordService
{
    public static void Export(DeThiExportData data, string filePath, string imageBasePath = "")
    {
        string mauTNPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Templates", "maudethi.docx");
        // ... (Giữ nguyên đoạn copy template) ...
        byte[] docxBytes = File.ReadAllBytes(mauTNPath);
        using var inputStream = new MemoryStream(docxBytes);
        using var docxStream = new MemoryStream();
        inputStream.CopyTo(docxStream);
        docxStream.Position = 0;

        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(docxStream, true))
        {
            var mainPart = wordDoc.MainDocumentPart!;
            var body = mainPart.Document.Body;
            if (body == null) return;

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

            // Gọi hàm replace mới
            ReplaceDanhSachCauHoi(mainPart, body, data.CauHoiVaDapAn, imageBasePath);

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }

    private static void ReplaceDanhSachCauHoi(MainDocumentPart mainPart, Body body,
        List<(CauHoi CauHoi, List<CauTraLoi> DapAn)> dataList, string imageBasePath)
    {
        var paragraphs = body.Elements<Paragraph>().ToList();
        var placeholderPara = paragraphs.FirstOrDefault(p => p.InnerText.Contains("<<danh_sach_cau_hoi>>"));
        if (placeholderPara == null) return;

        OpenXmlElement insertAfter = placeholderPara;
        int stt = 1;

        // Gom nhóm theo ParentId
        var groups = dataList.GroupBy(x => x.CauHoi.ParentId ?? x.CauHoi.Id).ToList();

        foreach (var group in groups)
        {
            var firstItem = group.First().CauHoi;

            // Nếu có Parent -> In nội dung Parent (Giả thuyết chung/Đoạn văn)
            if (firstItem.Parent != null)
            {
                var pParent = new Paragraph();
                string parentContent = firstItem.Parent.NoiDung;

                // --- LOGIC MỚI: Tự động đánh số vào chỗ trống ---
                // Kiểm tra nếu đoạn văn có chứa dấu gạch dưới (đại diện cho chỗ trống)
                if (parentContent.Contains("___"))
                {
                    // Regex tìm các chuỗi gạch dưới liên tiếp (ví dụ: ___, ____, _____)
                    var regexBlank = new Regex(@"_{3,}");

                    int tempStt = stt; // Biến tạm để đếm số thứ tự mà không ảnh hưởng biến chính

                    // Thay thế mỗi dấu gạch dưới tìm thấy bằng số thứ tự tương ứng
                    // Ví dụ: "____" -> " (1) ____ "
                    parentContent = regexBlank.Replace(parentContent, match =>
                    {
                        string replacement = $" ({tempStt}) _______ ";
                        tempStt++;
                        return replacement;
                    });
                }
                // ------------------------------------------------

                var contentParent = HtmlToWordHelper.ConvertHtmlToElements(mainPart, parentContent, imageBasePath);

                // Xử lý ảnh của câu Parent
                if (!string.IsNullOrWhiteSpace(firstItem.Parent.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, firstItem.Parent.HinhAnh));
                    if (drawing != null) contentParent.Add(new Run(drawing));
                }

                pParent.Append(contentParent);
                insertAfter = body.InsertAfter(pParent, insertAfter);
            }

            // In danh sách các câu hỏi con
            foreach (var item in group)
            {
                var cauHoi = item.CauHoi;
                var dapAns = item.DapAn;

                // 1. In nội dung câu hỏi
                var pCauHoi = new Paragraph();
                // Thêm khoảng cách nhỏ phía trên mỗi câu hỏi (để thoáng hơn chút, nhưng không quá xa)
                pCauHoi.Append(new Run(
                    new RunProperties(new Bold(), new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" }),
                    new Text($"Câu {stt++}: ") { Space = SpaceProcessingModeValues.Preserve }
                ));

                pCauHoi.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, cauHoi.NoiDung, imageBasePath));

                if (!string.IsNullOrWhiteSpace(cauHoi.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, cauHoi.HinhAnh));
                    if (drawing != null) pCauHoi.Append(new Run(drawing));
                }

                insertAfter = body.InsertAfter(pCauHoi, insertAfter);

                // 2. In các đáp án
                char ma = 'A';

                // Nếu là trắc nghiệm điền khuyết hoặc câu đơn bình thường -> In dọc hoặc ngang tùy ý
                // Ở đây giữ logic cũ: Mỗi đáp án 1 dòng (dạng liệt kê)
                // Bạn có thể cải tiến để in 4 đáp án trên 1 dòng nếu nội dung ngắn (dùng Table hoặc Tab)
                foreach (var d in dapAns.OrderBy(x => x.ViTriGoc))
                {
                    var pDapAn = new Paragraph();

                    pDapAn.Append(new Run(
                        new RunProperties(new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" }, new Bold()),
                        new Text($"{ma++}. ")
                    ));

                    pDapAn.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, d.NoiDung, imageBasePath, true));

                    if (!string.IsNullOrWhiteSpace(d.HinhAnh))
                    {
                        var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, d.HinhAnh));
                        if (drawing != null) pDapAn.Append(new Run(drawing));
                    }

                    insertAfter = body.InsertAfter(pDapAn, insertAfter);
                }

                //insertAfter = body.InsertAfter(new Paragraph(new Run(new Text(""))), insertAfter);
            }
            // 3. SAU KHI HẾT NHÓM (Hết bài đọc hoặc hết câu lẻ) -> MỚI THÊM DÒNG TRỐNG LỚN
            // Tạo khoảng cách rõ ràng giữa các nhóm câu hỏi
            insertAfter = body.InsertAfter(new Paragraph(new Run(new Text(""))), insertAfter);
        }

        placeholderPara.Remove();
    }
}