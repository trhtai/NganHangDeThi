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

            // --- 1. XỬ LÝ TIỀN TỐ "CÂU" / "QUESTION" ---
            string tenMon = data.DeThi.MonHoc?.TenMon ?? "";

            // Danh sách từ khóa nhận diện môn Tiếng Anh
            string[] tuKhoaTiengAnh = { "tiếng anh", "anh văn", "english", "ngoại ngữ" };

            bool laMonTiengAnh = tuKhoaTiengAnh.Any(k => tenMon.Contains(k, StringComparison.OrdinalIgnoreCase));
            string prefixCauHoi = laMonTiengAnh ? "Question" : "Câu";
            // ---------------------------------------------

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

            // Truyền thêm biến prefixCauHoi vào hàm Replace
            ReplaceDanhSachCauHoi(mainPart, body, data.CauHoiVaDapAn, imageBasePath, prefixCauHoi);

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }

    // Thêm tham số 'prefixLabel' (Câu/Question)
    private static void ReplaceDanhSachCauHoi(MainDocumentPart mainPart, Body body,
        List<(CauHoi CauHoi, List<CauTraLoi> DapAn)> dataList, string imageBasePath, string prefixLabel)
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

            // 1. In nội dung Parent (Đoạn văn/Giả thuyết) - Nếu có
            if (firstItem.Parent != null)
            {
                var pParent = new Paragraph();
                string parentContent = firstItem.Parent.NoiDung;

                // Logic tự động đánh số vào chỗ trống ___ -> (1) _______
                if (parentContent.Contains("___"))
                {
                    var regexBlank = new Regex(@"_{3,}");
                    int tempStt = stt;
                    parentContent = regexBlank.Replace(parentContent, match =>
                    {
                        string replacement = $" ({tempStt}) _______ ";
                        tempStt++;
                        return replacement;
                    });
                }

                var contentParent = HtmlToWordHelper.ConvertHtmlToElements(mainPart, parentContent, imageBasePath);
                if (!string.IsNullOrWhiteSpace(firstItem.Parent.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, firstItem.Parent.HinhAnh));
                    if (drawing != null) contentParent.Add(new Run(drawing));
                }

                pParent.Append(contentParent);
                insertAfter = body.InsertAfter(pParent, insertAfter);
            }

            // 2. In danh sách các câu hỏi con
            foreach (var item in group)
            {
                var cauHoi = item.CauHoi;
                var dapAns = item.DapAn;

                // In Câu hỏi (Sử dụng prefixLabel động)
                var pCauHoi = new Paragraph();
                pCauHoi.ParagraphProperties = new ParagraphProperties(new SpacingBetweenLines { Before = "120" }); // 6pt

                pCauHoi.Append(new Run(
                    new RunProperties(new Bold { Val = true }, new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" }),
                    new Text($"{prefixLabel} {stt++}: ") { Space = SpaceProcessingModeValues.Preserve } // <-- ĐỔI Ở ĐÂY
                ));
                pCauHoi.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, cauHoi.NoiDung, imageBasePath));

                if (!string.IsNullOrWhiteSpace(cauHoi.HinhAnh))
                {
                    var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, Path.Combine(imageBasePath, cauHoi.HinhAnh));
                    if (drawing != null) pCauHoi.Append(new Run(drawing));
                }
                insertAfter = body.InsertAfter(pCauHoi, insertAfter);

                // In Đáp án
                char ma = 'A';
                foreach (var d in dapAns.OrderBy(x => x.ViTriGoc))
                {
                    var pDapAn = new Paragraph();

                    pDapAn.Append(new Run(
                        new RunProperties(new RunFonts { Ascii = "Times New Roman" }, new FontSize { Val = "24" },
                        new Bold { Val = false }, new BoldComplexScript { Val = false }),
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
            }

            // 3. SAU KHI HẾT NHÓM -> Thêm dòng trống
            insertAfter = body.InsertAfter(new Paragraph(new Run(new Text(""))), insertAfter);
        }

        placeholderPara.Remove();
    }
}