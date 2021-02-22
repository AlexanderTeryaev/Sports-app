using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Net;
using AppModel;
using DataService;
using DataService.DTO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Resources;
using CmsApp.Models;

namespace CmsApp.Helpers
{
    public class PhotofinishErrorPdf
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        Font titleWordFont;
        Font reportBlackWordFont;
        Font reportRedWordFont;
        Font reportGreenWordFont;

        public PhotofinishErrorPdf(List<PhotoFinishReportItem> errors, bool isHebrew)
        {
            _document = new Document(PageSize.A4);
            _isHebrew = isHebrew;
            _stream = new MemoryStream();

            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            titleWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            reportBlackWordFont = new Font(_bfArialUniCode, 9, Font.NORMAL, BaseColor.BLACK);
            reportRedWordFont = new Font(_bfArialUniCode, 9, Font.NORMAL, BaseColor.RED);
            reportGreenWordFont = new Font(_bfArialUniCode, 9, Font.NORMAL, BaseColor.GREEN);

            writer = PdfWriter.GetInstance(_document, _stream);      
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            _document.Open();
            var errorsTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };
            errorsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"Photofinish Results", titleWordFont) })
                    {
                        Border = Rectangle.NO_BORDER,
                        //RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER
                    });
            for (int i = 0; i < errors.Count ; i++)
                {
                    var error = errors.ElementAt(i);
                    errorsTable.AddCell(
                            new PdfPCell(new Phrase { new Chunk($"{error.Message}", GetFontByColor(error.Color)) })
                            {
                                Border = Rectangle.NO_BORDER,
                                //RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                                HorizontalAlignment = PdfPCell.ALIGN_CENTER
                            });
            }
            _document.Add(errorsTable);
        }


        private Font GetFontByColor(string color)
        {
            switch (color)
            {
                case "black":
                    return reportBlackWordFont;
                case "red":
                    return reportRedWordFont;
                case "green":
                    return reportGreenWordFont;
                default:
                    return reportBlackWordFont;
            }
        }


        public MemoryStream GetDocumentStream()
        {
            _document.Close();
            writer.Close();
            return _stream;
        }


    }
}