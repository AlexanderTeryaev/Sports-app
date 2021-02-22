using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using AppModel;
using DataService;
using DataService.DTO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Resources;


namespace CmsApp.Helpers
{
    public class AthleteNumberProductionPdfExportHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        Font athleticsNumberfont;
        Font athleticsWordFont;
        Font yearWordFont;
        Font unionWordFont;
        Font playersWordFont;
        string iconPath = HostingEnvironment.MapPath("~/Content/images/logo-athletics.png");

        public AthleteNumberProductionPdfExportHelper(bool isHebrew)
        {
            _document = new Document(PageSize.A4, 5, 5, 5, 10);

            _isHebrew = isHebrew;

            _stream = new MemoryStream();

            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            athleticsWordFont = new Font(_bfArialUniCode, 50, Font.BOLD, BaseColor.BLACK);
            athleticsNumberfont = new Font(_bfArialUniCode, 230, Font.BOLD, BaseColor.BLACK);
            yearWordFont = new Font(_bfArialUniCode, 30, Font.BOLD, BaseColor.BLUE);
            unionWordFont = new Font(_bfArialUniCode, 42, Font.BOLD, BaseColor.BLACK);
            playersWordFont = new Font(_bfArialUniCode, 32, Font.BOLD, BaseColor.BLACK);

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;

            _document.Open();
        }

        public MemoryStream GetDocumentStream()
        {
            _document.Close();
            writer.Close();
            return _stream;
        }

        public void ProduceAthleteNumbersForPlayersList(List<PlayerStatusViewModel> players, string unionName, DateTime startDate, List<Club> clubs) {
            string startYearInString = startDate.Year.ToString();
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                Club pclub = clubs.Where(c => c.ClubId == player.ClubId).FirstOrDefault();
                AddAthletePublicationWithSimilarClub(player, startYearInString, pclub != null ? pclub.Name : "", unionName);
                if (i%2 == 1)
                {
                    //_document.NewPage();
                }
            }
        }

        public void AddAthleticsWord() {
            var athleticsPhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.AthleticsTitle}", athleticsWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER
                });

            _document.Add(athleticsPhraseTable);
        }

        public void AddSeasonDate(string year)
        {
            var yearPhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
                yearPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{year}", yearWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    PaddingBottom = 0
                });

            _document.Add(yearPhraseTable);
        }


        public void AddIconAndUnionName(string unionName)
        {
            var iconeUnionPhraseTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(iconPath);
            png.ScaleAbsoluteHeight(50);
            png.ScaleAbsoluteWidth(80);
            //png.Alignment = iTextSharp.text.Image.UNDERLYING;

            iconeUnionPhraseTable.AddCell(new PdfPCell(png){
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER
            });
            iconeUnionPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{unionName}", unionWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 4
            });
            _document.Add(iconeUnionPhraseTable);
        }



        public void AddAthleticNumber(int num)
        {
            var athleticsPhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            var para = new Paragraph( new Chunk($"{num.ToString()}", athleticsNumberfont) );
            para.Alignment = Element.ALIGN_CENTER;
            athleticsPhraseTable.AddCell(
                new PdfPCell(para)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                });

            _document.Add(athleticsPhraseTable);
        }

        public void AddAthleticData(string fullName, string gender, string clubName, string birthDate, string identity)
        {
            var athleticsPhraseTable = new PdfPTable(12)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 4
            };
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{fullName}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 4
                }
            );
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{gender}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 1
                }
            );

            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3
                }
            );
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{birthDate}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 2
                }
            );
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{identity}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 2
                }
            );
            _document.Add(athleticsPhraseTable);
        }

        public void AddAthleticData(string fullName, string gender, string clubName, string birthDate)
        {
            var fixedHeight = 70f;
            var athleticsPhraseTable = new PdfPTable(20)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{fullName}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 9,
                    FixedHeight = fixedHeight
                }
            );
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{gender}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    FixedHeight = fixedHeight
                }
            );

            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 6,
                    FixedHeight = fixedHeight
                }
            );
            athleticsPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{birthDate}", playersWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    FixedHeight = fixedHeight
                }
            );
            _document.Add(athleticsPhraseTable);
        }

        private void AddNewRow() {
            _document.Add(new Paragraph(" "));
        }

        public void AddAthletePublicationWithSimilarClub(PlayerStatusViewModel athlete, string startYearInString, string clubName, string unionName) {
            AddNewRow();          
            AddIconAndUnionName(unionName);
            AddNewRow();
            AddNewRow();
            AddAthleticNumber(athlete.AthletesNumbers.Value);
            AddSeasonDate(startYearInString);
            AddAthleticData(athlete.FullName, LangHelper.GetGenderCharById(athlete.GenderId), clubName, athlete.Birthday.Value.Year.ToString());
            AddNewRow();
            AddNewRow();
        }

    }
}