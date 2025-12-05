using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
using System.IO;

namespace NganHangDeThi.Services;

public static class ExportDapAnService
{
    // Hàm xuất 1 đề (Giữ lại để dùng cho nút "Xuất đề thi" lẻ ở DataGrid)
    public static void Export(DeThi deThi, string filePath, string imageBasePath = "")
    {
        ExportBatch(new List<DeThi> { deThi }, filePath, imageBasePath);
    }

    // Hàm xuất NHIỀU đề vào 1 file
    public static void ExportBatch(List<DeThi> listDeThi, string filePath, string imageBasePath = "")
    {
        // 1. Kiểm tra/Tạo file mẫu rỗng nếu cần (để tránh lỗi crash)
        string mauTNPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Templates", "maudapan.docx");
        if (!File.Exists(mauTNPath))
        {
            using var newDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            newDoc.AddMainDocumentPart().Document = new Document(new Body());
            newDoc.Save();
            return;
        }

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

            // Xóa hết placeholder cũ của template (nếu có) để tránh rác, vì ta sẽ tự append nội dung mới
            var textElements = body.Descendants<Text>().Where(t => t.Text.Contains("<<")).ToList();
            foreach (var t in textElements) t.Text = "";

            // Xóa toàn bộ nội dung body mẫu để ta tự build lại từ đầu cho sạch (hoặc giữ lại header chung nếu muốn)
            // Ở đây tôi chọn phương án: Giữ header template, append nội dung xuống dưới.

            // --- LOOP QUA TỪNG ĐỀ ---
            for (int i = 0; i < listDeThi.Count; i++)
            {
                var deThi = listDeThi[i];

                // 2. Tạo nội dung cho từng đề
                AppendDapAnChoMotDe(body, deThi, mainPart, imageBasePath);

                // 3. Ngắt trang (Page Break) nếu chưa phải đề cuối cùng
                if (i < listDeThi.Count - 1)
                {
                    body.Append(new Paragraph(new Run(new Break() { Type = BreakValues.Page })));
                }
            }

            wordDoc.MainDocumentPart!.Document.Save();
        }

        File.WriteAllBytes(filePath, docxStream.ToArray());
    }

    private static void AppendDapAnChoMotDe(Body body, DeThi deThi, MainDocumentPart mainPart, string imageBasePath)
    {
        // --- 1. Header Mã đề ---
        var pMaDe = new Paragraph(new Run(
            new RunProperties(new Bold(), new FontSize { Val = "32" }, new Color { Val = "000000" }), // Cỡ chữ 16
            new Text($"MÃ ĐỀ: {deThi.MaDe} - {deThi.TieuDe}")
        ));
        pMaDe.ParagraphProperties = new ParagraphProperties(new Justification { Val = JustificationValues.Center });
        body.Append(pMaDe);

        body.Append(new Paragraph()); // Dòng trống

        // --- 2. Phân loại câu hỏi ---
        var allQuestions = deThi.DsChiTietDeThi.OrderBy(ct => ct.Id).ToList();
        var trAcNghiem = allQuestions.Where(q => q.CauHoi.Loai != LoaiCauHoi.TuLuan && q.CauHoi.Loai != LoaiCauHoi.ChumTuLuan).ToList();
        var tuLuan = allQuestions.Where(q => q.CauHoi.Loai == LoaiCauHoi.TuLuan || q.CauHoi.Loai == LoaiCauHoi.ChumTuLuan).ToList();

        // --- 3. BẢNG TRẮC NGHIỆM (Matrix Style) ---
        if (trAcNghiem.Count > 0)
        {
            // Tạo bảng
            Table table = new Table();

            // Style bảng (Viền đơn, rộng 100%)
            TableProperties tblProp = new TableProperties(
                new TableBorders(
                    new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                    new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                ),
                new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                new TableJustification { Val = TableRowAlignmentValues.Center }
            );
            table.AppendChild(tblProp);

            // Cấu hình: 10 cột
            int columns = 10;
            int totalRows = (int)Math.Ceiling((double)trAcNghiem.Count / columns);

            for (int r = 0; r < totalRows; r++)
            {
                TableRow tr = new TableRow();
                tr.TableRowProperties = new TableRowProperties(new TableRowHeight { Val = 340, HeightType = HeightRuleValues.AtLeast });

                for (int c = 0; c < columns; c++)
                {
                    int index = r * columns + c;
                    if (index < trAcNghiem.Count)
                    {
                        var q = trAcNghiem[index];
                        int sttThucTe = allQuestions.IndexOf(q) + 1;

                        // Tìm đáp án đúng
                        var dapAnDung = q.DsDapAnTrongDe.FirstOrDefault(d => d.LaDapAnDung);
                        string letter = dapAnDung != null ? ((char)('A' + dapAnDung.ViTri)).ToString() : "?";

                        // Nội dung ô: "1. C"
                        tr.Append(CreateCell($"{sttThucTe}. {letter}", true));
                    }
                    else
                    {
                        tr.Append(CreateCell("", false)); // Ô trống
                    }
                }
                table.Append(tr);
            }
            body.Append(table);
            body.Append(new Paragraph()); // Xuống dòng
        }

        // --- 4. PHẦN TỰ LUẬN ---
        if (tuLuan.Count > 0)
        {
            var pTitleTL = new Paragraph(new Run(
                new RunProperties(new Bold(), new FontSize { Val = "28" }, new Underline { Val = UnderlineValues.Single }),
                new Text("HƯỚNG DẪN CHẤM TỰ LUẬN")
            ));
            body.Append(pTitleTL);

            foreach (var chiTiet in tuLuan)
            {
                int stt = allQuestions.IndexOf(chiTiet) + 1;
                var cauHoi = chiTiet.CauHoi;

                // Câu hỏi
                var pCauHoi = new Paragraph();
                pCauHoi.Append(new Run(new RunProperties(new Bold(), new FontSize { Val = "24" }), new Text($"Câu {stt}: ")));
                pCauHoi.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, cauHoi.NoiDung, imageBasePath));
                body.Append(pCauHoi);

                // Đáp án
                var dapAn = chiTiet.DsDapAnTrongDe.FirstOrDefault();
                if (dapAn != null)
                {
                    var pAns = new Paragraph();
                    pAns.Append(new Run(new Text("=> Đáp án: ") { Space = SpaceProcessingModeValues.Preserve }));
                    pAns.Append(HtmlToWordHelper.ConvertHtmlToElements(mainPart, dapAn.NoiDung, imageBasePath));
                    body.Append(pAns);
                }
                body.Append(new Paragraph(new Run(new Break())));
            }
        }
    }

    // Helper tạo ô bảng
    private static TableCell CreateCell(string text, bool bold)
    {
        var runProps = new RunProperties(new FontSize { Val = "24" });
        if (bold) runProps.Append(new Bold());

        var pProps = new ParagraphProperties();
        pProps.Justification = new Justification { Val = JustificationValues.Center };
        pProps.SpacingBetweenLines = new SpacingBetweenLines { After = "0", Before = "0" };

        var cellProps = new TableCellProperties(new TableCellVerticalAlignment { Val = TableVerticalAlignmentValues.Center });

        return new TableCell(cellProps, new Paragraph(pProps, new Run(runProps, new Text(text))));
    }
}