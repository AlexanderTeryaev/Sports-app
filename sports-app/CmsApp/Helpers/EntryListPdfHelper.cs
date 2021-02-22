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


namespace CmsApp.Helpers
{
    public class EntryListPdfHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        private PdfPTable playersTable;
        Font dateWordFont;
        Font clubWordFont;
        Font unionWordFont;
        Font tableWordFont;
        Font clubdateWordFont;

        private decimal costTeamRegistrations = 0;
        private string _contentPath;
        private int seasonId;
        private string _sectionAlias;

        public EntryListPdfHelper(List<AthleticRegDto> players, League league, string disciplineName, bool isHebrew, string contentPath, string sectionAlias = "")
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _sectionAlias = sectionAlias;
            _stream = new MemoryStream();
            seasonId = league.SeasonId.Value;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            dateWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, BaseColor.BLACK);
            clubdateWordFont = new Font(_bfArialUniCode, 22, Font.BOLD, BaseColor.BLACK);
            unionWordFont = new Font(_bfArialUniCode, 32, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            AddDateOfCreation();
            if (league != null)
            {
                AddIconAndClubName(league.Name, league.LeagueStartDate.HasValue ? league.LeagueStartDate.Value.ToShortDateString() : "", disciplineName, league.Union.Logo, league.Union.Name);
            }
            AddNewRow();
            if (players.Count() > 0)
            {
                int tableCells = _sectionAlias != SectionAliases.Athletics ? 8 : 10;
                playersTable = new PdfPTable(tableCells)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    WidthPercentage = 100,
                    PaddingTop = 0
                };

                AddPlayersTableByCompetition(players);
                _document.Add(playersTable);
                AddNewRow();
                AddStats(players.Count());
                AddNewRow();
            }
            AddNewRow();
        }

        public EntryListPdfHelper(List<AthleticRegDto> players, Club club, League league, string disciplineName, bool isHebrew, string contentPath, string sectionAlias = "")
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _sectionAlias = sectionAlias;
            _stream = new MemoryStream();
            seasonId = club.SeasonId.Value;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            dateWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, BaseColor.BLACK);
            clubdateWordFont = new Font(_bfArialUniCode, 22, Font.BOLD, BaseColor.BLACK);

            unionWordFont = new Font(_bfArialUniCode, 32, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            AddDateOfCreation();
            if (league != null)
            {
                AddIconAndClubName(league.Name, league.LeagueStartDate.HasValue ? league.LeagueStartDate.Value.ToShortDateString() : "", disciplineName, club.Union.Logo, club.Union.Name);
            }
            AddNewRow();
            if (players.Count() > 0)
            {
                int tableCells = _sectionAlias != SectionAliases.Athletics ? 10 : 14;
                playersTable = new PdfPTable(tableCells)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    WidthPercentage = 100,
                    PaddingTop = 0
                };

                AddPlayersTableByClub(players);
                _document.Add(playersTable);
                AddNewRow();
                AddStats(players.Count());
                AddNewRow();
            }
            AddNewRow();
        }

        private void AddDateOfCreation()
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = $"{Messages.EntryList}";

            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk($"{dateString}: {DateTime.Now.ToString("dd/MM/yyyy")}", tableWordFont) })
                                    {
                                        Border = Rectangle.NO_BORDER,
                                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                                        Colspan = 1
                                    });
            _document.Add(datePhraseTable);
        }


        public MemoryStream GetDocumentStream()
        {
            _document.Close();
            writer.Close();
            return _stream;
        }

        public void AddIconAndClubName(string competitionName, string startDate, string disciplineName, string url, string unionName)
        {
            var iconeUnionPhraseTable = new PdfPTable(10)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            var unionColSpan = 6;
            try
            {
                var savePath = _contentPath + "/union/" + url;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(savePath);
                float ratio = logo.Width / logo.Height;
                logo.ScaleAbsoluteHeight(60);
                logo.ScaleAbsoluteWidth(60 * ratio);

                iconeUnionPhraseTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 2
                });
            }
            catch (System.Net.WebException exception)
            {
                unionColSpan = 10;
            }
            catch (Exception ex)
            {
                iconeUnionPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk("", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 2
                });
            }
            iconeUnionPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{unionName}", unionWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = unionColSpan
            });
            if (unionColSpan == 6)
            {
                iconeUnionPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk("", unionWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    Colspan = 2
                });
            }

            var leaguePhraseAndDateTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            leaguePhraseAndDateTable.AddCell(
                new PdfPCell(new Phrase{
                    new Chunk($"{competitionName} - ", clubWordFont),
                    new Chunk($"{startDate}", clubdateWordFont)
                })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 5
            });

            var disciplinePhraseAndDateTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            disciplinePhraseAndDateTable.AddCell(
                new PdfPCell(new Phrase{
                    new Chunk($"{disciplineName}", clubWordFont)
                })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 5
                });

            _document.Add(iconeUnionPhraseTable);
            _document.Add(leaguePhraseAndDateTable);
            _document.Add(disciplinePhraseAndDateTable);
        }

        public void AddStats(int? playersNumber)
        {
            var statPhraseTable = new PdfPTable(3)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            var para = new Paragraph(new Chunk($"", tableWordFont));
            para.Alignment = Element.ALIGN_CENTER;
            statPhraseTable.AddCell(
                new PdfPCell(para)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                }
           );
            var title = Messages.NumberOfPlayers.Replace(Messages.Players,Messages.Athletes);
            if (playersNumber.HasValue)
            {
                var para2 = new Paragraph(new Chunk($"{title} : {playersNumber}", tableWordFont));
                para2.Alignment = Element.ALIGN_CENTER;
                statPhraseTable.AddCell(
                    new PdfPCell(para2)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        NoWrap = true,
                        UseAscender = true
                    });

            }
            else
            {
                var para2 = new Paragraph(new Chunk($"", tableWordFont));
                para2.Alignment = Element.ALIGN_CENTER;
                statPhraseTable.AddCell(
                    new PdfPCell(para2)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        NoWrap = true,
                        UseAscender = true
                    });

            }
            

            _document.Add(statPhraseTable);
        }

        public void AddPlayerDataByCompetition(string order, string athleteNumber, string lastName, string firstName, string clubName, string dateName, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 1,
                    PaddingBottom = padding,
                    Border = border,
                    FixedHeight = 30f
        }
            );
            if (_sectionAlias != SectionAliases.Bicycle && _sectionAlias != SectionAliases.Climbing)
            {
                playersTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{athleteNumber}", tableWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 2,
                            PaddingBottom = padding,
                            Border = border
                        }
                );
            }
            playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{lastName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border
                    }
            );

            playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{firstName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border
                    }
            );

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{dateName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
        }

        public void AddPlayerDataByClub(string order, string athleteNumber, string lastName, string firstName, string disciplineName, string categoryName, string dateName, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 1,
                    PaddingBottom = padding,
                    Border = border,
                    FixedHeight = 30f
                }
            );

            if (_sectionAlias != SectionAliases.Bicycle && _sectionAlias != SectionAliases.Climbing)
            {
                playersTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{athleteNumber}", tableWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 2,
                            PaddingBottom = padding,
                            Border = border
                        }
                );
            }
            playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{lastName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 3,
                        PaddingBottom = padding,
                        Border = border
                    }
            );
            playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{firstName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 3,
                        PaddingBottom = padding,
                        Border = border
                    }
            );
            if (_sectionAlias != SectionAliases.Bicycle && _sectionAlias != SectionAliases.Climbing)
            {
                playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{disciplineName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 2,
                        PaddingBottom = padding,
                        Border = border
                }
                );
            }
            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{categoryName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{dateName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
        }
        


        public void AddPlayersTableByCompetition(List<AthleticRegDto> players)
        {

            AddPlayerDataByCompetition("#", Messages.AthleteNumber, Messages.LastName, Messages.FirstName, Messages.ClubName, Messages.YearOfBirth, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                AddPlayerDataByCompetition(counter.ToString(), player.AthleteNumber.ToString(), player.LastName, player.FirstName, player.ClubName, player.BirthDay.HasValue ? player.BirthDay.Value.Year.ToString() : "", players.Count() == i + 1 ? true : false);
                counter += 1;
            }
        }

        public void AddPlayersTableByClub(List<AthleticRegDto> players)
        {
            AddPlayerDataByClub("#", Messages.AthleteNumber, Messages.LastName, Messages.FirstName, Messages.DisciplineName, Messages.Category, Messages.YearOfBirth, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                AddPlayerDataByClub(counter.ToString(), player.AthleteNumber.ToString(), player.LastName, player.FirstName, player.DisciplineName, player.CategoryName, player.BirthDay.HasValue ? player.BirthDay.Value.Year.ToString() : "", players.Count() == i + 1 ? true : false);
                counter += 1;
            }
        }
        
        
        private void AddNewRow()
        {
            _document.Add(new Paragraph(" "));
        }

    }
}