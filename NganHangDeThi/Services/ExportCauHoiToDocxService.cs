using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NganHangDeThi.Common.Enum;
using NganHangDeThi.Data.Entity;
using NganHangDeThi.Helpers;
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

        // Kiểm tra nếu là câu chùm
        if (cauHoi.DsCauHoiCon != null && cauHoi.DsCauHoiCon.Any())
        {
            // 1. Xuất nội dung cha <G>
            // Tách thẻ <G> ra để nó không bị coi là HTML cần render, giữ nguyên text để import lại sau này
            AppendQuestionPart(mainPart, body, "<G> ", cauHoi.NoiDung, cauHoi.HinhAnh, imageBasePath);

            // 2. Xuất các câu con
            foreach (var child in cauHoi.DsCauHoiCon.OrderBy(c => c.Id))
            {
                ExportSingleQuestion(mainPart, body, child, imageBasePath);
            }

            // 3. Đóng thẻ </G>
            body.Append(new Paragraph(new Run(new Text("</G>"))));
        }
        else
        {
            // Câu đơn bình thường
            ExportSingleQuestion(mainPart, body, cauHoi, imageBasePath);
        }

        mainPart.Document.Save();
    }

    private void ExportSingleQuestion(MainDocumentPart mainPart, Body body, CauHoi cauHoi, string imageBasePath)
    {
        // Tạo tag mở đầu câu hỏi (NB, TH, VD, VDC)
        string prefix = cauHoi.Loai == LoaiCauHoi.TuLuan ? "<T" : "<";
        string tag = cauHoi.MucDo switch
        {
            MucDoCauHoi.NhanBiet => $"{prefix}NB>",
            MucDoCauHoi.ThongHieu => $"{prefix}TH>",
            MucDoCauHoi.VanDung => $"{prefix}VD>",
            MucDoCauHoi.VanDungCao => $"{prefix}VDC>",
            _ => "<NB>"
        };

        // 1. Xuất câu hỏi (Tag + Nội dung HTML đã render)
        AppendQuestionPart(mainPart, body, tag + " ", cauHoi.NoiDung, cauHoi.HinhAnh, imageBasePath);

        // 2. Các đáp án
        foreach (var d in cauHoi.DsCauTraLoi.OrderBy(x => x.ViTriGoc))
        {
            string prefixAns = d.LaDapAnDung ? "<$*> " : "<$> ";
            if (!d.DaoViTri) prefixAns = prefixAns.Trim() + "<@> ";

            AppendQuestionPart(mainPart, body, prefixAns, d.NoiDung, d.HinhAnh, imageBasePath);
        }
    }

    // Hàm helper mới: Tách biệt Tag hệ thống và Nội dung HTML
    private void AppendQuestionPart(MainDocumentPart mainPart, Body body, string prefixTag, string htmlContent, string? entityImagePath, string imageBasePath)
    {
        var para = new Paragraph();

        // 1. Thêm Tag (Ví dụ: <NB>, <$>) dưới dạng Text thuần
        // Dùng SpaceProcessingModeValues.Preserve để giữ khoảng trắng sau tag
        var runTag = new Run(new Text(prefixTag) { Space = SpaceProcessingModeValues.Preserve });

        // Font cho Tag (Times New Roman 12pt)
        runTag.RunProperties = new RunProperties(
            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
            new FontSize { Val = "24" }
        );
        para.Append(runTag);

        // 2. Convert nội dung HTML sang Word Elements (Giữ in đậm, màu sắc...)
        // Bật enableBold = true để hiển thị đúng thẻ <b>
        var contentElements = HtmlToWordHelper.ConvertHtmlToElements(mainPart, htmlContent, imageBasePath, ignoreColor: false, enableBold: false);
        para.Append(contentElements);

        body.Append(para);

        // 3. Chèn ảnh (nếu có) vào đoạn văn riêng ngay bên dưới
        if (!string.IsNullOrWhiteSpace(entityImagePath))
        {
            string fullPath = Path.Combine(imageBasePath, entityImagePath);
            var drawing = HtmlToWordHelper.CreateImageDrawing(mainPart, fullPath);

            if (drawing != null)
            {
                var imgPara = new Paragraph(new Run(drawing));
                body.Append(imgPara);
            }
        }
    }
}