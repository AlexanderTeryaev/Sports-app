using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using AppModel;
using DataService;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Resources;

namespace CmsApp.Helpers
{
    public class TournamentRosterPdfExportHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private readonly Font _normalFont;
        private readonly Font _normalBigFont;
        private readonly Font _boldFont;

        public TournamentRosterPdfExportHelper(bool isHebrew)
        {
            var widthFormat = new Rectangle(PageSize.A4.Rotate());
            _document = new Document(widthFormat);

            _isHebrew = isHebrew;

            _stream = new MemoryStream();

            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIALUNI.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            _normalFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            _normalBigFont = new Font(_bfArialUniCode, 15, Font.NORMAL, BaseColor.BLACK);
            _boldFont = new Font(_bfArialUniCode, 16, Font.BOLD, BaseColor.BLACK);

            var writer = PdfWriter.GetInstance(_document, _stream);
            writer.CloseStream = false;

            _document.Open();
        }

        public byte[] GetDocumentContent()
        {
            _document.Close();

            return _stream.ToArray();
        }

        private bool IsHebrew(string value)
        {
            string hebrew = @"אבגדהוזחטיכלמנסעפצקרשתץףןם";
            var returnValue = false;
            foreach (var symbol in hebrew)
            {
                if (value?.Contains(symbol) == true) returnValue = true;
            }
            return returnValue;
        }

        private void AddCell(PdfPTable tableLayout, string cellText, bool isCentered = false)
        {
            var font = new Font(_bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            var cell = new PdfPCell(new Phrase(cellText, font));
            if (IsHebrew(cellText))
                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            if (isCentered)
                cell.HorizontalAlignment = 1;
            tableLayout.AddCell(cell);
        }

        public void AddHeader(string clubName, string leagueName, string teamName, List<UserJobDto> teamOfficials)
        {
            #region Phrases

            var clubLeagueName = new Phrase
            {
                new Chunk(
                    string.IsNullOrWhiteSpace(clubName)
                        ? $"{Messages.League}: "
                        : $"{Messages.Club}: ",
                    _normalBigFont),
                new Chunk(string.IsNullOrWhiteSpace(clubName)
                    ? leagueName
                    : clubName, _boldFont)
            };

            var nameOfTeam = new Phrase
            {
                new Chunk($"{Messages.Team}: ", _normalBigFont),
                new Chunk($"{teamName}", _boldFont)
            };
            var dateOfExport = new Phrase(new Chunk($"{Messages.ExportDate}: {DateTime.Now.ToShortDateString()}", _normalFont));
            //Phrase thirdPhrase = new Phrase
            //{
            //    new Chunk($"{Messages.Season} {seasonName}", normalFont),
            //    new Chunk($"     {Messages.Period} {startDate} - {endDate}", normalFont)
            //};
            //Phrase fourthPhrase = new Phrase
            //{
            //    new Chunk($"{Messages.Address}: {address}", normalFont),
            //    new Chunk($"     {Messages.City}: {city}", normalFont)
            //};

            #endregion

            var headerTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            headerTable.AddCell(new PdfPCell(clubLeagueName) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            headerTable.AddCell(new PdfPCell(nameOfTeam) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            headerTable.AddCell(new PdfPCell(dateOfExport) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });

            //table.AddCell(new PdfPCell(thirdPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            //table.AddCell(new PdfPCell(fourthPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            //table.AddCell(new PdfPCell(new Phrase("")) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            //table.AddCell(new PdfPCell(new Phrase("")) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            _document.Add(headerTable);

            _document.Add(new Paragraph($" "));
            _document.Add(new DottedLineSeparator());
            _document.Add(new Paragraph($" "));

            var officialsTitleTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            officialsTitleTable.AddCell(
                new PdfPCell(new Phrase {new Chunk($"{Messages.TeamOfficials}:", _normalFont)})
                {
                    Border = Rectangle.NO_BORDER
                });

            _document.Add(officialsTitleTable);

            _document.Add(new Paragraph($" "));

            if (teamOfficials.Any())
            {
                var teamOfficialsTable = new PdfPTable(3)
                {
                    HorizontalAlignment = 0,
                    //TotalWidth = 800f,
                    //LockedWidth = true
                };

                if (_isHebrew)
                {
                    teamOfficialsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    float[] widths = { 50, 50, 50 };
                    teamOfficialsTable.SetWidths(widths);
                }
                else
                {
                    float[] widths = { 50, 50, 50 };
                    teamOfficialsTable.SetWidths(widths);
                }

                #region Header

                AddCell(teamOfficialsTable, Messages.Name);
                AddCell(teamOfficialsTable, Messages.Phone);
                AddCell(teamOfficialsTable, Messages.Role);

                #endregion

                #region Body

                foreach (var official in teamOfficials)
                {
                    AddCell(teamOfficialsTable, official.FullName);
                    AddCell(teamOfficialsTable, official.Phone);
                    AddCell(teamOfficialsTable, official.JobName);
                }

                #endregion

                _document.Add(teamOfficialsTable);

            }
            else
            {
                PdfPTable table = new PdfPTable(1)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
                };

                var phrase = new Phrase($"{Messages.NoDataFound}", new Font(_bfArialUniCode, 12, Font.ITALIC, BaseColor.BLACK));
                table.AddCell(new PdfPCell(phrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });

                _document.Add(table);
            }
        }

        public void AddBody(List<TeamsPlayer> teamPlayers)
        {
            _document.Add(new Paragraph($" "));
            _document.Add(new DottedLineSeparator());
            _document.Add(new Paragraph($" "));

            var rosterTitleTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            rosterTitleTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.TeamPlayers_Rugby_NextTournamentRoster}:", _normalFont) })
                {
                    Border = Rectangle.NO_BORDER
                });

            _document.Add(rosterTitleTable);

            _document.Add(new Paragraph($" "));

            if (teamPlayers.Any())
            {
                var playerTable = new PdfPTable(8)
                {
                    HorizontalAlignment = 0,
                    WidthPercentage = 100,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                };

                #region Header

                AddCell(playerTable, Messages.IdentNum);
                AddCell(playerTable, Messages.FullName);
                AddCell(playerTable, Messages.Email);
                AddCell(playerTable, Messages.Phone);
                AddCell(playerTable, Messages.City);
                AddCell(playerTable, Messages.BirthDay);
                AddCell(playerTable, Messages.ShirtNumber);
                AddCell(playerTable, Messages.Position);

                #endregion

                #region Body

                foreach (var player in teamPlayers)
                {
                    AddCell(playerTable, player.User.IdentNum);
                    AddCell(playerTable, player.User.FullName);
                    AddCell(playerTable, player.User.Email);
                    AddCell(playerTable, player.User.Telephone);
                    AddCell(playerTable, player.User.City);
                    AddCell(playerTable, player.User.BirthDay?.ToString("d"));
                    AddCell(playerTable, player.ShirtNum.ToString());
                    AddCell(playerTable, player.Position?.Title);
                }

                #endregion

                _document.Add(playerTable);
            }
            else
            {
                PdfPTable table = new PdfPTable(1)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
                };

                var phrase = new Phrase($"{Messages.NoDataFound}", new Font(_bfArialUniCode, 12, Font.ITALIC, BaseColor.BLACK));
                table.AddCell(new PdfPCell(phrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });

                _document.Add(table);
            }

            _document.Add(new Paragraph($" "));
            _document.Add(new DottedLineSeparator());
        }
    }
}