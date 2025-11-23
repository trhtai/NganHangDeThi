using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using System.IO;
using System.Windows;

namespace NganHangDeThi.Services;

public static class ExportDapAnService
{
    public static void Export(DeThi deThi, string filePath, string imageBasePath = "")
    {
        // ... (Giữ nguyên phần Load Template) ...
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
            if (body == null) return;

            var placeholderMappings = new Dictionary<string, string>
            {
                { "<<tieu_de>>", deThi.TieuDe.ToUpper() },
                { "<<ky_thi>>", deThi.KyThi.ToUpper() },
                { "<<lop_hoc>>", deThi.LopHoc!.MaLop },
                { "<<mon_hoc>>", deThi.MonHoc!.TenMon },
                { "<<ma_de>>", deThi.MaDe.ToString() },
            };
            DocxHelper.ReplacePlaceholders(body, placeholderMappings);

            // --- LOGIC XUẤT ĐÁP ÁN ---
            // Gom nhóm câu hỏi theo thứ tự trong đề thi
            // Lưu ý: DsChiTietDeThi đang lưu danh sách câu hỏi con đã được chọn
            var chiTiets = deThi.DsChiTietDeThi.OrderBy(ct => ct.Id).ToList(); // Giả sử Id tăng dần theo thứ tự add

            int stt = 1;
            foreach (var chiTiet in chiTiets)
            {
                var cauHoi = chiTiet.CauHoi;

                var pCauHoi = new Paragraph();
                pCauHoi.Append(new Run(
                    new RunProperties(new Bold(), new FontSize { Val = "24" }),
                    new Text($"Câu {stt++}: ")
                ));

                // In nội dung câu hỏi (ngắn gọn)
                pCauHoi.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, cauHoi.NoiDung, imageBasePath));
                body.Append(pCauHoi);

                // List đáp án
                var dsDapAn = chiTiet.DsDapAnTrongDe.OrderBy(d => d.ViTri).ToList();
                char ma = 'A';
                foreach (var dapAn in dsDapAn)
                {
                    var pAns = new Paragraph();
                    string prefix = $"{ma++}. ";

                    pAns.Append(new Run(new Text(prefix)));
                    pAns.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, dapAn.NoiDung, imageBasePath));

                    if (dapAn.LaDapAnDung)
                    {
                        pAns.Append(new Run(new Text("  ✅") { Space = SpaceProcessingModeValues.Preserve }));
                    }
                    body.Append(pAns);
                }
                body.Append(new Paragraph(new Run(new Break())));
            }

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }
}