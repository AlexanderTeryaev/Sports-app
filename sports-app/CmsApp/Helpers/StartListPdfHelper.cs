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
    public class StartListPdfHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        Font dateWordFont;
        Font clubWordFont;
        Font unionWordFont;
        Font tableWordFont;
        Font heatWordFont;
        private DisciplineRecord _record;
        private string _contentPath;
        private int seasonId;
        private string _sectionAlias;

        public StartListPdfHelper(List<IGrouping<string, CompetitionDisciplineRegistration>> registrationsByHeat, League league, string disciplineName, bool isHebrew, string contentPath,DateTime? startTime, DisciplineRecord record = null, string sectionAlias = "")
        {
            _document = new Document(PageSize.A4, 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _record = record;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            seasonId = league.SeasonId.Value;
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            _sectionAlias = sectionAlias;

            dateWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            clubWordFont = new Font(_bfArialUniCode, 22, Font.BOLD, BaseColor.BLACK);
            heatWordFont = new Font(_bfArialUniCode, 16, Font.BOLD, BaseColor.BLACK);
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
            AddTime(startTime.HasValue ? startTime.Value.ToShortTimeString() : "");
            AddNewRow();
            if ( record != null)
            {
                BuildRecordTable(record, league.Name);
            }

            for (int i = 0; i < registrationsByHeat.Count; i++)
            {
                bool isLast = false;
                var registrationByHeat = registrationsByHeat.ElementAt(i);
                if (i + 1 == registrationsByHeat.Count)
                {
                    isLast = true;
                }
                AddHeatRegistration(registrationByHeat, isLast, seasonId);
            }


            AddNewRow();
        }

        private void BuildRecordTable(DisciplineRecord record, string competitionName)
        {
            int tableCells = string.IsNullOrEmpty(record.CompetitionRecord) ? 3 : 4;
            var recordTable = new PdfPTable(tableCells)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0
            };
            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{Messages.IsraeliRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });
            if (!string.IsNullOrEmpty(record.CompetitionRecord))
            {
                recordTable.AddCell(
                            new PdfPCell(new Phrase { new Chunk($"{Messages.CompetitionRecord}", tableWordFont) })
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                                Colspan = 1
                            });
            }
            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{Messages.IntentionalIsraeliRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });
            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{Messages.SeasonRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });




            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{record.IsraeliRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });
            if (!string.IsNullOrEmpty(record.CompetitionRecord))
            {
                recordTable.AddCell(
                            new PdfPCell(new Phrase { new Chunk($"{record.CompetitionRecord}", tableWordFont) })
                            {
                                Border = Rectangle.NO_BORDER,
                                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                                Colspan = 1
                            });
            }
            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{record.IntentionalIsraeliRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });
            string seasonRecord = string.Empty;;
            if (record.SeasonRecords.FirstOrDefault(x => x.SeasonId == seasonId) != null)
            {
                seasonRecord = record.SeasonRecords.FirstOrDefault(x => x.SeasonId == seasonId).SeasonRecord1;
            }
            
            recordTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{seasonRecord}", tableWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            Colspan = 1
                        });

            _document.Add(recordTable);
            AddNewRow();
        }

        private void AddDateOfCreation()
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = $"{Messages.StartList}";

            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk($"{dateString}: {DateTime.Now.ToString("dd/MM/yyyy")}", tableWordFont) })
                                    {
                                        Border = Rectangle.NO_BORDER,
                                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                                        Colspan = 1
                                    });
            _document.Add(datePhraseTable);
        }

        private void AddTime(string time)
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };
            string dateString = $"{Messages.StartList}";

            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk($"{time}", heatWordFont) })
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
            new PdfPCell(new Phrase { new Chunk($"{competitionName} - {startDate}", clubWordFont) })
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
            new PdfPCell(new Phrase { new Chunk($"{disciplineName}", clubWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 5
            });
            _document.Add(iconeUnionPhraseTable);
            _document.Add(leaguePhraseAndDateTable);
            _document.Add(disciplinePhraseAndDateTable);
        }

        public void AddPlayerDataByCompetition(PdfPTable registrationsTable, string place, string athleteNumber, string lane, string lastName,string firstName, string clubName, string birthYear, string seasonalBest, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{place}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border,
                        FixedHeight = 30f
                    }
            );
            if (_sectionAlias != SectionAliases.Climbing)
            {
                registrationsTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{athleteNumber}", tableWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 1,
                            PaddingBottom = padding,
                            Border = border
                        }
                );

                registrationsTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{lane}", tableWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 1,
                            PaddingBottom = padding,
                            Border = border
                        }
                );
            }
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{lastName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border
                    }
            );
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{firstName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border
                    }
            );
            registrationsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 1,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            registrationsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{birthYear}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 1,
                    PaddingBottom = padding,
                    Border = border
                }
            );
            if (_record != null)
            {
                registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{seasonalBest}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 1,
                        PaddingBottom = padding,
                        Border = border
                    }
                );
            }

            
        }


        public string GetLastNameByFullName(string fullName)
        {
            var fullNameArray = fullName?.Split(' ')?.ToList();
            var resultString = string.Empty;
            if (fullNameArray?.Any() == true)
            {
                for (var i = 0; i < fullNameArray.Count; i++)
                {
                    if (fullNameArray.Count == i+1)
                        resultString += fullNameArray[i];
                }
            }
            return resultString;
        }

        public string GetFirstNameByFullName(string fullName)
        {
            var fullNameArray = fullName?.Split(' ')?.ToList();
            var resultString = string.Empty;
            if (fullNameArray?.Any() == true)
            {
                for (var i = 0; i < fullNameArray.Count; i++)
                {
                    if(fullNameArray.Count != i+1)
                    {
                        resultString += fullNameArray[i];
                        resultString += " ";
                    }
                }
                if (resultString.Length - 1 >= 0)
                    resultString = resultString.Remove(resultString.Length - 1);
            }
            return resultString;
        }

        private string getFirstName(User user)
        {
            if (string.IsNullOrEmpty(user.FirstName))
            {
                return GetFirstNameByFullName(user.FullName);
            }
            return user.FirstName;
        }

        private string getLastName(User user)
        {
            if (string.IsNullOrEmpty(user.LastName))
            {
                return GetLastNameByFullName(user.FullName);
            }
            return user.LastName;
        }

        public void AddRegistrationsByHeat(IGrouping<string, CompetitionDisciplineRegistration> registrationByHeat, PdfPTable registrationsTable, int? seasonId)
        {
            AddPlayerDataByCompetition(registrationsTable, "#", Messages.AthleteNumber, Messages.Lane, Messages.LastName, Messages.FirstName, Messages.ClubName, Messages.YearOfBirth, "SB", true);
            for (int i = 0; i < registrationByHeat.Count(); i++)
            {
                var registration = registrationByHeat.ElementAt(i);
                registration.User.LastName = getLastName(registration.User);
                registration.User.FirstName = getFirstName(registration.User);
            }
            var eachHeatData = registrationByHeat
                .OrderBy(r => r.CompetitionResult.FirstOrDefault()?.Lane ?? int.MaxValue)
                .ThenBy(r => r.User.LastName)
                .ToList();
            
            for (int i = 0; i < eachHeatData.Count(); i++)
            {
                var registration = eachHeatData.ElementAt(i);
                var result = registration.CompetitionResult.FirstOrDefault();

                var athleteNumber = registration.User
                                        .AthleteNumbers
                                        .FirstOrDefault(x => x.SeasonId == seasonId)
                                        ?.AthleteNumber1;

                AddPlayerDataByCompetition(registrationsTable,
                    (i + 1).ToString(),
                    athleteNumber?.ToString() ?? string.Empty,
                    result?.Lane?.ToString() ?? "",
                    registration.User.LastName,
                    registration.User.FirstName,
                    registration.Club.Name,
                    registration.User.BirthDay.HasValue ? registration.User.BirthDay.Value.Year.ToString() : string.Empty,
                    UIHelpers.GetCompetitionDisciplineResultString(registration.SeasonalBest, _record?.Format ?? 0),
                    registrationByHeat.Count() == i + 1 ? true : false);
            }
        }

        public void AddHeatRegistration(IGrouping<string, CompetitionDisciplineRegistration> registrationByHeat, bool isLast, int? seasonId)
        {
            int tableCells = _record == null ? 7 : 8;
            if (_sectionAlias == SectionAliases.Climbing) tableCells = tableCells - 2;
            var registrationsTable = new PdfPTable(tableCells)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100,
                PaddingTop = 0,
                HeaderRows = 1
            };
            var heatName = Messages.Without;
            if (!isLast || !string.IsNullOrWhiteSpace(registrationByHeat.Key))
            {
                heatName = registrationByHeat.Key;
            }
            AddHeatTitle(heatName);
            AddRegistrationsByHeat(registrationByHeat, registrationsTable, seasonId);
            _document.Add(registrationsTable);
            AddNewRow();
        }

        private void AddHeatTitle(string title)
        {
            var datePhraseTable = new PdfPTable(1)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            datePhraseTable.AddCell(
                                    new PdfPCell(new Phrase { new Chunk($"{Messages.Heat}: {title}", heatWordFont) })
                                    {
                                        Border = Rectangle.NO_BORDER,
                                        HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                                        Colspan = 1
                                    });
            _document.Add(datePhraseTable);
        }

        private void AddNewRow()
        {
            _document.Add(new Paragraph(" "));
        }

    }
}