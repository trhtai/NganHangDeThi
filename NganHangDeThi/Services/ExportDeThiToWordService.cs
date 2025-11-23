using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using NganHangDeThi.Models;
using System.IO;

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

        // QUAN TRỌNG: Gom nhóm theo ParentId
        // Nếu ParentId null thì gom theo chính Id của nó (để nó đứng 1 mình)
        var groups = dataList.GroupBy(x => x.CauHoi.ParentId ?? x.CauHoi.Id).ToList();

        foreach (var group in groups)
        {
            // Kiểm tra xem nhóm này có phải là Câu chùm/Điền khuyết không?
            // Lấy đại diện 1 item để check Parent
            var firstItem = group.First().CauHoi;

            // Nếu có Parent -> In nội dung Parent (Giả thuyết chung/Đoạn văn) trước
            if (firstItem.Parent != null)
            {
                var pParent = new Paragraph();
                // Có thể thêm chữ "Đọc đoạn văn sau..." nếu muốn
                var contentParent = HtmlToWordHelper.ConvertHtmlToElements(mainPart, firstItem.Parent.NoiDung, imageBasePath);

                // Xử lý ảnh của câu Parent (nếu có)
                if (!string.IsNullOrWhiteSpace(firstItem.Parent.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, firstItem.Parent.HinhAnh));
                    if (drawing != null) contentParent.Add(new Run(drawing));
                }

                pParent.Append(contentParent);
                insertAfter = body.InsertAfter(pParent, insertAfter);
            }

            // In danh sách các câu hỏi con (hoặc câu đơn)
            foreach (var item in group)
            {
                var cauHoi = item.CauHoi;
                var dapAns = item.DapAn;

                // 1. In nội dung câu hỏi
                var pCauHoi = new Paragraph();
                pCauHoi.Append(new Run(
                    new RunProperties(new Bold(), new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" }),
                    new Text($"Câu {stt++}: ") { Space = SpaceProcessingModeValues.Preserve }
                ));

                // Nội dung câu hỏi (từ HTML)
                pCauHoi.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, cauHoi.NoiDung, imageBasePath));

                // Ảnh câu hỏi (nếu có)
                if (!string.IsNullOrWhiteSpace(cauHoi.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, cauHoi.HinhAnh));
                    if (drawing != null) pCauHoi.Append(new Run(drawing));
                }

                insertAfter = body.InsertAfter(pCauHoi, insertAfter);

                // 2. In các đáp án
                char ma = 'A';
                foreach (var d in dapAns.OrderBy(x => x.ViTriGoc)) // Sắp xếp theo vị trí đã trộn
                {
                    var pDapAn = new Paragraph(new ParagraphProperties(new Indentation { Left = "720" })); // Thụt lề

                    pDapAn.Append(new Run(
                        new RunProperties(new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" }, new Bold()),
                        new Text($"{ma++}. ")
                    ));

                    // Nội dung đáp án (từ HTML)
                    pDapAn.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, d.NoiDung, imageBasePath, true));

                    // Ảnh đáp án (nếu có)
                    if (!string.IsNullOrWhiteSpace(d.HinhAnh))
                    {
                        var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, d.HinhAnh));
                        if (drawing != null) pDapAn.Append(new Run(drawing));
                    }

                    insertAfter = body.InsertAfter(pDapAn, insertAfter);
                }

                // Dòng trống ngăn cách
                insertAfter = body.InsertAfter(new Paragraph(new Run(new Text(""))), insertAfter);
            }
        }

        placeholderPara.Remove();
    }
}