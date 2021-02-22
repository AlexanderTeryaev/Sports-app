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
    public class PaymentReportPdfExportHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        private PdfPTable playersTable;
        private PdfPTable teamsTable;
        Font dateWordFont;
        Font clubWordFont;
        Font unionWordFont;
        Font teamWordFont;
        Font tableWordFont;
        private decimal costPerPlayer = 60;
        private decimal costTeamRegistrations = 0;
        private string _contentPath;
        private int seasonId;

        public PaymentReportPdfExportHelper(List<TeamPlayersPayment> players, List<TeamRegistrationPayment> teams, Club club, DateTime? issueTime, bool isHebrew, string contentPath)
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            seasonId = club.SeasonId.Value;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            dateWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, BaseColor.BLACK);
            unionWordFont = new Font(_bfArialUniCode, 32, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            AddDateOfCreation(club, issueTime);
            if (club != null)
            {
                AddIconAndClubName(club.Name, club.Union.Logo, club.Union.Name);
            }
            AddNewRow();
            if (players.Count() > 0)
            {
                int tableCells = 22;
                playersTable = new PdfPTable(tableCells)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    WidthPercentage = 100,
                    PaddingTop = 0
                };
                players.ForEach(p => p.TeamPlayers.ClubPaymentFee = p.Fee);
                var sortedPlayers = players.Select(p => p.TeamPlayers).ToList();

                AddPlayersTable(sortedPlayers);
                _document.Add(playersTable);
                AddNewRow();
                AddStats(players.Count(), costPerPlayer * players.Where(p => p.Fee > 0).Count(), false);
                AddNewRow();
            }
            AddNewRow();
            teamsTable = new PdfPTable(14)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };

            var properTeamRegisrations = teams.Select(p => p.TeamRegistrations).ToList();
            AddTeamsTable(properTeamRegisrations);
            _document.Add(teamsTable);
            AddNewRow();
            AddStats(properTeamRegisrations.Where(r => !r.IsDeleted).Count(), costTeamRegistrations, true);
            AddNewRow();
            AddNewRow();
            AddStats(null, costPerPlayer * players.Where(p => p.Fee > 0).Count() + costTeamRegistrations, true);
        }



        public PaymentReportPdfExportHelper(IEnumerable<TeamsPlayer> players, List<TeamRegistration> teams, Club club, bool isHebrew, string contentPath)
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            seasonId = club.SeasonId.Value;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            dateWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, BaseColor.BLACK);
            unionWordFont = new Font(_bfArialUniCode, 32, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            AddDateOfCreation(club, null);
            if (club != null)
            {
                AddIconAndClubName(club.Name, club.Union.Logo, club.Union.Name);
            }
            AddNewRow();
            var distinctPlayers = players.GroupBy(p => p.UserId).Select(g => g.First()).ToList();
            if (players.Count() > 0)
            {
                int tableCells = 22;
                playersTable = new PdfPTable(tableCells)
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    WidthPercentage = 100,
                    PaddingTop = 0
                };

                var duplications = players.Select(x => x).ToList();
                foreach (var teamPlayer in distinctPlayers)
                {
                    duplications.Remove(teamPlayer);
                }

                foreach (var teamPlayer in distinctPlayers)
                {
                    teamPlayer.ClubPaymentFee = costPerPlayer;
                }
                foreach (var teamPlayer in duplications)
                {
                    teamPlayer.ClubPaymentFee = 0.0M;
                }
                AddPlayersTable(players);
                _document.Add(playersTable);
                AddNewRow();
                AddStats(players.Count(), costPerPlayer * distinctPlayers.Count(), false);
                AddNewRow();
            }
            AddNewRow();
            teamsTable = new PdfPTable(14)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };

            AddTeamsTable(teams);
            _document.Add(teamsTable);
            AddNewRow();
            AddStats(teams.Where(r => !r.IsDeleted).Count(), costTeamRegistrations, true);
            AddNewRow();
            AddNewRow();
            AddStats(null, costPerPlayer * distinctPlayers.Count() + costTeamRegistrations, true);
        }
        

        private void AddDateOfCreation(Club club, DateTime? issueTime)
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = $"{Messages.ClubPaymentReport} {Messages.ToDate}" ;

            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk(issueTime.HasValue ? $"{dateString}: {issueTime.Value.ToString("dd/MM/yyyy")}" : $"{dateString}: {DateTime.Now.ToString("dd/MM/yyyy")}", tableWordFont) })
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

        public void AddIconAndClubName(string clubName, string url, string unionName)
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

            var clubPhraseAndDateTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            clubPhraseAndDateTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{clubName}", clubWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 5
            });


            _document.Add(iconeUnionPhraseTable);
            _document.Add(clubPhraseAndDateTable);

        }

        public void AddStats(int? playersNumber, decimal totalSum, bool secondTime)
        {
            var statPhraseTable = new PdfPTable(3)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            var para = new Paragraph( new Chunk($"", tableWordFont) );
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
            var title = secondTime ? Messages.NumberOfTeams : Messages.NumberOfPlayers;
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
            else {
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
            var para3 = new Paragraph(new Chunk($"{Messages.ReportTable_TotalSummary} : {totalSum} {Messages.Nis}", tableWordFont));
            para3.Alignment = Element.ALIGN_CENTER;
            statPhraseTable.AddCell(
                new PdfPCell(para3)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                });

            _document.Add(statPhraseTable);
        }

        public void AddPlayerData(string order, string firstName, string lastName, string fullName, string teamName, string leagueName, string tenicardValidity, string cost, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (string.IsNullOrWhiteSpace(firstName)){
                playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{fullName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 5,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            else {
                playersTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{firstName} {lastName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 5,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }

            //   player.User.TenicardValidity.Value.ToShortDateString(), costPerPlayer.ToString());

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{teamName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 4,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{leagueName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 4,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{tenicardValidity}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 4,
                    PaddingBottom = padding,
                    Border = border
                }
            );

            playersTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{cost} {Messages.Nis}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding,
                    Border = border
                }
            );
        }

        public void AddPlayersTable(IEnumerable<TeamsPlayer> players)
        {
            AddPlayerData("#", Messages.FullName, "", null, Messages.TeamName, Messages.LeagueName, Messages.TenicardValidity, Messages.Activity_Tenicard_Price, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                string leageName = "";
                List<string> leagueNames = new List<string>();
                foreach (var teamRegistration in player.Team.TeamRegistrations.Where(tr => tr.SeasonId == seasonId && !tr.IsDeleted && !tr.Team.IsArchive && !tr.League.IsArchive))
                {
                    leagueNames.Add(teamRegistration.League.Name);
                }
                leageName = String.Join(",", leagueNames);

                AddPlayerData(counter.ToString(), player.User.FirstName, player.User.LastName, player.User.FullName, player.Team.Title, leageName, player.User.TenicardValidity.HasValue ? player.User.TenicardValidity.Value.ToShortDateString() : "", player.ClubPaymentFee.ToString(), players.Count() == i+1 ? true : false);
                counter += 1;
            }
        }
        //name of team, team registration price (get from League/Info page- "team registration price".
        public void AddTeamData(string order, string teamName, string leagueName, string registrationCost, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            teamsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );

            teamsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{teamName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 4,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            teamsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{leagueName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 4,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (string.IsNullOrWhiteSpace(registrationCost))
            {
                teamsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 4,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            else
            {
                teamsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{registrationCost} {Messages.Nis}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 4,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
        }

        public void AddTeamsTable(IEnumerable<TeamRegistration> teamRegistrations)
        {
            AddTeamData("#", Messages.TeamName, Messages.League, Messages.LeagueDetail_TeamRegistrationPrice, true);
            int counter = 1;
            for (int i = 0; i < teamRegistrations.Count(); i++)
            {
                var teamRegistration = teamRegistrations.ElementAt(i);
                decimal? priceOfTeamRegistration = teamRegistration.League.LeaguesPrices.FirstOrDefault(p => p.PriceType == 1)?.Price;
                if (priceOfTeamRegistration.HasValue) {
                    costTeamRegistrations += priceOfTeamRegistration.Value;
                }
                AddTeamData(counter.ToString(), teamRegistration.Team.Title, teamRegistration.League.Name, priceOfTeamRegistration.HasValue? priceOfTeamRegistration?.ToString() : "", teamRegistrations.Count() == i + 1 ? true : false);
                counter += 1;
            }
        }
        


        private void AddNewRow() {
            _document.Add(new Paragraph(" "));
        }

    }
}