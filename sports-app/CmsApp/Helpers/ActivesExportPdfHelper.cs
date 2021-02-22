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
    public class ActivesExportPdfHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        private PdfPTable athleticsTable;
        private PdfPTable clubsTable;
        private PdfPTable summaryTable;
        Font unionWordFont;
        Font clubWordFont;
        Font teamWordFont;
        Font tableWordFont;
        private string _contentPath;
        private string _playersName;
        private bool _isIndividual;
        private bool _isAthleticsNew = false;
        bool isAthletics = false;

        public ActivesExportPdfHelper(IEnumerable<PlayerStatusViewModel> players, Union union, Club club, bool isHebrew, string contentPath, string playersName, bool isIndividual, bool isAthleticsNew = false)
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isIndividual = isIndividual;
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            _playersName = playersName;
            _isAthleticsNew = isAthleticsNew;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            unionWordFont = new Font(_bfArialUniCode, 34, Font.BOLD, new BaseColor(0, 0, 128));
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            if (union != null)
            {
                isAthletics = union.Section.Alias == SectionAliases.Athletics;
                AddDateOfCreation(club, isAthletics);
                AddIconAndUnionName(union.Name, union.Logo);
            }
            else
            {
                AddDateOfCreation(club, false);
            }
            if (club != null)
            {
                AddIconAndClubName(club.Name, club.Logo);
            }
            if (!isAthletics)
            {
                AddNewRow();
                AddStats(players.Where(p => p.IsApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && p.IsNotApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && !p.IsNotApproveChecked).Count());
            }
            AddNewRow();

            int tableCells = _isIndividual ? 19 : 17;
            if (isAthletics)
            {
                tableCells = _isIndividual ? 22 : 20;
            }
            if (_isAthleticsNew)
            {
                //tableCells -= 2;
            }
            athleticsTable = new PdfPTable(tableCells)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };

            IEnumerable<PlayerStatusViewModel> sortedPlayers;
            if (_isAthleticsNew)
            {
                sortedPlayers = players.OrderByDescending(p => p.CompetitionCount ?? int.MinValue).ThenBy(p => p.LastName).ThenBy(p => p.FirstName);
            }
            else
                sortedPlayers = players.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

            AddPlayersTable(sortedPlayers);
            _document.Add(athleticsTable);
            AddNewRow();
            if (isAthletics && !_isAthleticsNew)
            {
                AddNewRow();
                AddStats(players.Where(p => p.IsApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && p.IsNotApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && !p.IsNotApproveChecked).Count());
                AddNewRow();
            }
        }

        public ActivesExportPdfHelper(IEnumerable<TeamPlayerItem> players, Union union, Club club, Team team, bool isHebrew, string contentPath, string playersName, bool isIndividual, bool isAthleticsNew = false)
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isIndividual = isIndividual;
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            _playersName = playersName;
            _isAthleticsNew = isAthleticsNew;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            unionWordFont = new Font(_bfArialUniCode, 34, Font.BOLD, new BaseColor(0, 0, 128));
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, new BaseColor(0, 0, 128));
            teamWordFont = new Font(_bfArialUniCode, 20, Font.BOLD, new BaseColor(0, 0, 128));
            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            if (union != null)
            {
                isAthletics = union.Section.Alias == SectionAliases.Athletics;
                AddDateOfCreation(club, isAthletics);
                AddIconAndUnionName(union.Name, union.Logo);
            }
            else
            {
                AddDateOfCreation(club, false);
            }
            if (club != null)
            {
                AddIconAndClubName(club.Name, club.Logo);
            }
            if (team != null)
            {
                AddTeamName(team.Title);
            }
            if (!isAthletics)
            {
                AddNewRow();
                AddStats(players.Where(p => p.IsActive && (p.IsPlayerRegistrationApproved || p.IsApprovedByManager == true)).Count(), players.Where(p => !p.IsApproveChecked && p.IsNotApproveChecked).Count(), players.Where(p => !p.IsPlayerRegistrationApproved && p.IsApprovedByManager == false).Count());
            }
            AddNewRow();

            int tableCells = _isIndividual ? 19 : 17;
            if (isAthletics)
            {
                tableCells = _isIndividual ? 22 : 20;
            }
            if (_isAthleticsNew)
            {
                //tableCells -= 2;
            }
            athleticsTable = new PdfPTable(tableCells)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };
            IEnumerable<TeamPlayerItem> sortedPlayers;
            if (_isAthleticsNew)
            {
                sortedPlayers = players.OrderByDescending(p => p.CompetitionCount ?? int.MinValue).ThenBy(p => p.LastName).ThenBy(p => p.FirstName);
            }
            else
                sortedPlayers = players.OrderBy(p => p.LastName).ThenBy(p => p.FirstName);

            AddPlayersTable(sortedPlayers);
            _document.Add(athleticsTable);
            AddNewRow();
            if (isAthletics && !_isAthleticsNew)
            {
                AddNewRow();
                AddStats(players.Where(p => p.IsApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && p.IsNotApproveChecked).Count(), players.Where(p => !p.IsApproveChecked && !p.IsNotApproveChecked).Count());
                AddNewRow();
            }
        }

        /// <summary>
        /// Export all Union Clubs Debts
        /// </summary>
        /// <param name="union"></param>
        /// <param name="clubs"></param>
        /// <param name="isHebrew"></param>
        /// <param name="contentPath"></param>
        public ActivesExportPdfHelper(Union union, List<Club> clubs, List<ClubBalanceDto> clubsBalance, bool isHebrew, string contentPath)
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            unionWordFont = new Font(_bfArialUniCode, 34, Font.BOLD, new BaseColor(0, 0, 128));
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();

            if (union != null)
            {
                AddDateOfCreation(union);
                AddIconAndUnionName(union.Name, union.Logo);
            }
            AddNewRow();

            clubsTable = new PdfPTable(8)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };

            IEnumerable<Club> sortedClubs = clubs.OrderBy(p => p.Name).ThenBy(p => p.ClubDisplayName);

            decimal? sum = AddClubsTable(sortedClubs, clubsBalance);
            AddNewRow();
            _document.Add(clubsTable);
            AddNewRow();
            AddClubsSammaryTable(sum);
            AddNewRow();
        }

        private void AddDateOfCreation(Club club, bool isAthletics)
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = Messages.ActivesClubReportCreationDate;
            if (club == null)
            {
                dateString = Messages.ActivesUnionReportCreationDate;
            }
            if (isAthletics)
            {
                dateString = dateString.Replace(Messages.ExportActivesRaw, Messages.FourCompetitionsReport).Replace(Messages.Players, UIHelpers.GetPlayerCaption(GamesAlias.Athletics, true));
            }
            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk($"{dateString}: {DateTime.Now.ToString("dd/MM/yyyy")}", tableWordFont) })
                                    {
                                        Border = Rectangle.NO_BORDER,
                                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                                        Colspan = 1
                                    });
            _document.Add(datePhraseTable);
        }

        private void AddDateOfCreation(Union union)
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = Messages.DebtReport;
            if (union == null)
            {
                dateString = Messages.ActivesUnionReportCreationDate;
            }
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

        public void AddIconAndUnionName(string unionName, string url)
        {
            iTextSharp.text.Image logo = null;

            var iconeUnionPhraseTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            try
            {
                var savePath = _contentPath + "/union/" + url;
                logo = iTextSharp.text.Image.GetInstance(savePath);
                float ratio = logo.Width / logo.Height;
                logo.ScaleAbsoluteHeight(60);
                logo.ScaleAbsoluteWidth(60 * ratio);

                iconeUnionPhraseTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER
                });
            }
            catch (System.Net.WebException exception)
            {
                if (logo == null)
                {
                    iconeUnionPhraseTable.AddCell(new PdfPCell()
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER
                    });
                }
                //iconeUnionPhraseTable.AddCell(
                //new PdfPCell(new Phrase { new Chunk("", clubWordFont) })
                //{
                //    Border = Rectangle.NO_BORDER,
                //    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                //    Colspan = 1
                //});
            }
            catch (Exception ex)
            {
                iconeUnionPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk("", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 1
                });
            }

            iconeUnionPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{unionName}", unionWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 4
            });
            _document.Add(iconeUnionPhraseTable);
        }

        public void AddIconAndClubName(string clubName, string url)
        {
            var iconeUnionPhraseTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            /*
            try
            {
                var savePath = _contentPath + "/Clubs/" + url;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(savePath);
                logo.ScaleAbsoluteHeight(60);
                logo.ScaleAbsoluteWidth(60);

                iconeUnionPhraseTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER
                });
            }
            catch (System.Net.WebException exception)
            {
                iconeUnionPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk("", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 1
                });
            }
            catch (Exception ex)
            {
                iconeUnionPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk("", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 1
                });
            }
            */
            iconeUnionPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{clubName}", clubWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 5
            });
            _document.Add(iconeUnionPhraseTable);
        }
        public void AddTeamName(string teamName)
        {
            var teamPhraseTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            teamPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{teamName}", teamWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 5
            });
            _document.Add(teamPhraseTable);
        }

        public void AddStats(int approved, int refused, int notApproved)
        {
            var athleticsPhraseTable = new PdfPTable(3)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            var para = new Paragraph(new Chunk($"{_playersName} {Messages.WhomApproved} : {approved.ToString()}", tableWordFont));
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


            var para3 = new Paragraph(new Chunk($"{Messages.NotApproved} : {refused.ToString()}", tableWordFont));
            para3.Alignment = Element.ALIGN_CENTER;
            athleticsPhraseTable.AddCell(
                new PdfPCell(para3)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                });
            var para2 = new Paragraph(new Chunk($"{Messages.WaitingForApproval} : {notApproved.ToString()}", tableWordFont));
            para2.Alignment = Element.ALIGN_CENTER;
            athleticsPhraseTable.AddCell(
                new PdfPCell(para2)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                });
            _document.Add(athleticsPhraseTable);
        }

        public void AddAthleticData(string order, string identNumber, string firstName, string lastName, string fullName, string birthDate, string gender, string isApproved, string competitionsCount, string athleteNumber = null, bool underline = false)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            athleticsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            athleticsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{identNumber}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (string.IsNullOrWhiteSpace(firstName))
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{fullName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 6,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            else
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{lastName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 3,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{firstName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 3,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            athleticsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{birthDate}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (isAthletics)
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{athleteNumber}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 3,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            athleticsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{gender}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 1,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (!_isAthleticsNew)
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{isApproved}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 2,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            else
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{isApproved}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 2,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
            if (_isIndividual)
            {
                athleticsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{competitionsCount}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 2,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }
        }


        public void AddPlayersTable(IEnumerable<PlayerStatusViewModel> players)
        {
            AddAthleticData(Messages.MeasureNo, $"{Messages.IdentNum}/{Messages.PassportNum}", Messages.FirstName, Messages.LastName, Messages.FullName, Messages.BirthDay, Messages.Gender, _isAthleticsNew ? Messages.Team : Messages.Approved, Messages.Competitions, isAthletics ? Messages.AthleteNumber : string.Empty, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                AddAthleticData(counter.ToString(), string.IsNullOrWhiteSpace(player.IdentNum) ? player.PassportNum : player.IdentNum, player.FirstName, player.LastName, player.FullName, player.Birthday.HasValue ? player.Birthday.Value.ToString("dd/MM/yyyy") : "", LangHelper.GetGenderCharById(player.GenderId), _isAthleticsNew ? player.TeamName : player.IsApproveChecked ? Messages.Yes : Messages.No, player.CompetitionCount.ToString(), player.AthletesNumbers.HasValue ? player.AthletesNumbers.Value.ToString() : string.Empty);
                counter += 1;
            }
        }

        public void AddPlayersTable(IEnumerable<TeamPlayerItem> players)
        {
            AddAthleticData(Messages.MeasureNo, $"{Messages.IdentNum}/{Messages.PassportNum}", Messages.FirstName, Messages.LastName, Messages.FullName, Messages.BirthDay, Messages.Gender, _isAthleticsNew ? Messages.Team : Messages.Approved, Messages.Competitions, isAthletics ? Messages.AthleteNumber : string.Empty, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                AddAthleticData(counter.ToString(), string.IsNullOrWhiteSpace(player.IdentNum) ? player.PassportNum : player.IdentNum, player.FirstName, player.LastName, player.FullName, player.Birthday.HasValue ? player.Birthday.Value.ToString("dd/MM/yyyy") : "", LangHelper.GetGenderCharById(player.GenderId.Value), _isAthleticsNew ? player.TeamName : player.IsApproveChecked ? Messages.Yes : Messages.No, player.CompetitionCount.ToString(), player.AthletesNumbers.HasValue ? player.AthletesNumbers.Value.ToString() : string.Empty);
                counter += 1;
            }
        }

        public void AddPlayersTable(IEnumerable<TeamsPlayer> players)
        {
            AddAthleticData(Messages.MeasureNo, $"{Messages.IdentNum}/{Messages.PassportNum}", Messages.FirstName, Messages.LastName, Messages.FullName, Messages.BirthDay, Messages.Gender, _isAthleticsNew ? Messages.Team : Messages.Approved, Messages.Competitions, isAthletics ? Messages.AthleteNumber : string.Empty, true);
            int counter = 1;
            for (int i = 0; i < players.Count(); i++)
            {
                var player = players.ElementAt(i);
                AddAthleticData(counter.ToString(), string.IsNullOrWhiteSpace(player.User.IdentNum) ? player.User.PassportNum : player.User.IdentNum, player.User.FirstName, player.User.LastName, player.User.FullName, player.User.BirthDay.HasValue ? player.User.BirthDay.Value.ToString("dd/MM/yyyy") : "", LangHelper.GetGenderCharById(player.User.GenderId.Value), _isAthleticsNew ? (player.Team.TeamsDetails.FirstOrDefault()?.TeamName ?? player.Team.Title) : !player.WithoutLeagueRegistration ? Messages.Yes : Messages.No, "", player.User.AthleteNumbers.Any() ? player.User.AthleteNumbers.OrderByDescending(x => x.SeasonId).FirstOrDefault().AthleteNumber1.Value.ToString() : string.Empty);
                counter += 1;
            }
        }

        public decimal? AddClubsTable(IEnumerable<Club> clubs, IEnumerable<ClubBalanceDto> clubsBalance)
        {
            decimal? sum = 0;
            AddClubData(Messages.MeasureNo, Messages.ClubName, Messages.AccountingKeyNumber, Messages.Balance, true);
            int counter = 1;
            for (int i = 0; i < clubs.Count(); i++)
            {
                var club = clubs.ElementAt(i);
                var balance = clubsBalance.ElementAt(i);
                string clubBalance = balance == null ? "אין פרטים" : balance.Balance.ToString();
                sum += balance == null ? 0 : balance.Balance;
                AddClubData(counter.ToString(), club.Name, club.AccountingKeyNumber.ToString(), clubBalance.ToString(), true);
                counter++;
            }
            return sum;
        }

        public void AddClubsSammaryTable(decimal? sum)
        {
            AddClubsSammaryData(Messages.TotalForBalance, sum, true);
        }

        public void AddClubData(string order, string clubName, string accountingKeyNumber, string balance, bool underline = false)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;

            clubsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{order}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            clubsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            clubsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{accountingKeyNumber}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            clubsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{balance}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 2,
                    PaddingBottom = padding,
                    Border = border
                }
            );
        }

        public void AddClubsSammaryData(string title, decimal? sum, bool underline = false)
        {
            var summaryTable = new PdfPTable(2)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            summaryTable.WidthPercentage = 100f;
            //set column widths
            int[] tablecellwidth = { 25, 250 };
            summaryTable.SetWidths(tablecellwidth);

            var para = new Paragraph(new Chunk($"{title}", tableWordFont));
            para.Alignment = Element.ALIGN_CENTER;
            summaryTable.AddCell(
                new PdfPCell(para)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true,
                });


            var para2 = new Paragraph(new Chunk($"{sum}", tableWordFont));
            para2.Alignment = Element.ALIGN_CENTER;
            summaryTable.AddCell(
                new PdfPCell(para2)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    NoWrap = true,
                    UseAscender = true
                });
            _document.Add(summaryTable);
        }

        private void AddNewRow()
        {
            _document.Add(new Paragraph(" "));
        }

    }
}