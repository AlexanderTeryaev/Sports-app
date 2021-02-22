using System;
using System.Globalization;
using iTextSharp.text.pdf;
using System.Text;
using DataService.DTO;
using iTextSharp.text;
using Resources;
using Font = iTextSharp.text.Font;
using Rectangle = iTextSharp.text.Rectangle;
using System.Linq;
using CmsApp.Models;
using System.Web.Hosting;
using System.Web;

namespace CmsApp.Helpers
{
    public class ExportReportToPdfHelper
    {
        private const string regex_match_arabic_hebrew = @"[\u0600-\u06FF,\u0590-\u05FF]+";
        private static BaseFont bfArialUniCode;
        private string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIALUNI.TTF");

        public ExportReportToPdfHelper()
        {
            bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        }
        private static bool IsHebrew(string value)
        {
            string hebrew = @"אבגדהוזחטיכלמנסעפצקרשתץףןם";
            var returnValue = false;
            foreach (var symbol in hebrew)
            {
                if (value.Contains(symbol)) returnValue = true;
            }
            return returnValue;
        }

        private static void AddCell(PdfPTable tableLayout, string cellText, bool isCentered = false)
        {
            Font font = new Font(bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            var cell = new PdfPCell(new Phrase(cellText, font));
            if (IsHebrew(cellText))
                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            if (isCentered)
                cell.HorizontalAlignment = 1;
            tableLayout.AddCell(cell);
        }

        public void AddHeaders(Document document, string workerFullName, int? workerId, string workerIdentNum, string seasonName, string workerType,
            string startDate, string endDate, string city, string address, bool isHebrew)
        {
            StringBuilder stringBuilder = new StringBuilder();

            #region Fonts and settings

            Font format1 = new Font(bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            Font format2 = new Font(bfArialUniCode, 18, Font.BOLD, BaseColor.BLACK);

            #endregion

            #region Phrases

            Phrase firstPhrase = new Phrase { new Chunk($" {Messages.SalaryReport} {workerFullName}", format1) };
            Phrase secondPhrase = new Phrase
            {
                new Chunk($"{Messages.ReportNameOfOfficial} {workerFullName}",format2),
                new Chunk($"     {Messages.ReportIdOfOfficial} {workerIdentNum}", format2)
            };
            Phrase thirdPhrase = new Phrase
            {
                new Chunk($"{Messages.Season} {seasonName}", format1),
                new Chunk($"     {Messages.Period} {startDate} - {endDate}", format1)
            };
            Phrase fourthPhrase = new Phrase
            {
                new Chunk($"{Messages.Address}: {address}", format1),
                new Chunk($"     {Messages.City}: {city}", format1)
            };

            #endregion

            PdfPTable table = new PdfPTable(1)
            {
                RunDirection = isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
            };

            table.AddCell(new PdfPCell(firstPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(secondPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(thirdPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(fourthPhrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase("")) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase("")) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            document.Add(table);
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
        }



        public void AddMainTable(Document pdfDocument, WorkerReportDTO vmWorkerReportInfo, string startDate, string endDate,
            bool isHebrew, string sectionAlias, string reportType)
        {
            if (vmWorkerReportInfo?.GamesAssigned == null || vmWorkerReportInfo.GamesCount == 0)
            {
                PdfPTable table = new PdfPTable(1)
                {
                    RunDirection = isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR
                };

                var phrase = new Phrase($"{Messages.NoGamesAssigned} {vmWorkerReportInfo.WorkerFullName} " +
                    $"{Messages.At.ToLowerInvariant()} {Messages.Period.ToLowerInvariant()} {startDate ?? ""} - {endDate ?? ""}",
                    new Font(bfArialUniCode, 16, Font.ITALIC, BaseColor.BLACK));
                table.AddCell(new PdfPCell(phrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
                pdfDocument.Add(table);
            }
            else
            {
                PdfPTable table = new PdfPTable(sectionAlias == GamesAlias.Athletics? 10:11)
                {
                    HorizontalAlignment = 0,
                    TotalWidth = 800f,
                    LockedWidth = true
                };

                if (isHebrew)
                {
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    float[] widths = { 90f, 60f, 60f, 100f, 60f, 50f, 100f, 75f, 80f, 75f, 30f };
                    if (sectionAlias == GamesAlias.Athletics)
                    {
                        widths = new float[] { 90f, 60f, 60f, 50f, 50f, 100f, 80f, 65f, 75f, 30f };
                    }
                    table.SetWidths(widths);
                }
                else
                {
                    float[] widths = { 30f, 75f, 80f, 75f, 100f, 100f,60f, 60f, 60f, 60f, 100f };
                    if (sectionAlias == GamesAlias.Athletics)
                    {
                        widths = new float[] { 30f, 75f, 80f, 65f, 100f, 50f, 50f, 60f, 60f, 90f };
                    }
                    table.SetWidths(widths);
                }


                #region Table header
                AddCell(table, "##", true);
                AddCell(table, sectionAlias != GamesAlias.Athletics ? Messages.ReportTable_DateOfGame : Messages.DateOfCompetition);
                AddCell(table, sectionAlias.Equals(SectionAliases.Waterpolo, StringComparison.OrdinalIgnoreCase)
                    ? Messages.Pools
                    : sectionAlias != GamesAlias.Athletics ? Messages.ReportTable_Auditorium : Messages.Stadiums);
                AddCell(table, Messages.Role);
                if(sectionAlias != GamesAlias.Athletics)
                {
                    AddCell(table, Messages.HomeTeam);
                    AddCell(table, Messages.GuestTeam);
                }
                AddCell(table, sectionAlias != GamesAlias.Athletics ? Messages.League : Messages.Competition);
                AddCell(table, Messages.Day);
                if (sectionAlias == GamesAlias.Athletics)
                {
                    AddCell(table, Messages.Hours);
                }
                AddCell(table, $"{Messages.ReportTable_Fee} ({Messages.Nis})");
                AddCell(table, $"{Messages.ReportTable_TravelDistance}({Messages.Km})");
                AddCell(table, Messages.Comment);

                #endregion

                #region Table body

                foreach (var game in vmWorkerReportInfo.GamesAssigned.OrderBy(c => c.StartDate).Select((value, index) => new { Value = value, Index = index + 1 }))
                {
                    AddCell(table, game.Index.ToString(), true);
                    AddCell(table, game.Value?.StartDate?.ToString("dd/MM/yyyy HH:mm") ?? "");
                    AddCell(table, game.Value?.AuditoriumName ?? "");
                    AddCell(table, LangHelper.GetRoleName(game.Value?.Role ?? string.Empty));
                    if (sectionAlias != GamesAlias.Athletics)
                    {
                        AddCell(table, game.Value?.HomeTeamName ?? "");
                        AddCell(table, game.Value?.GuestTeamName ?? "");
                    }
                    AddCell(table, game.Value?.League?.Name ?? "");
                    AddCell(table, LangHelper.GetDayOfWeek(game.Value?.StartDate));
                    if (sectionAlias == GamesAlias.Athletics)
                    {
                        AddCell(table, game.Value?.WorkedHours.ToString());
                    }
                    AddCell(table, game.Value?.LeagueFee?.ToString() ?? "");
                    AddCell(table, game.Value?.TravelDistance?.ToString() ?? "", true);
                    AddCell(table, game.Value?.Comment ?? string.Empty, true);
                }

                if (reportType == "reportWithoutSum")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(" ")){ FixedHeight = 30f };
                            table.AddCell(cell);
                        }
                    }
                }

                #endregion

                #region Table footer 
                Font font = new Font(bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
                if (reportType == "reportWithoutSum")
                    table.AddCell(new PdfPCell(new Phrase($"{Messages.Total}", font)) { Colspan = 2, Border = Rectangle.NO_BORDER });
                else
                    table.AddCell(new PdfPCell(new Phrase($"{Messages.Total} {vmWorkerReportInfo.GamesCount}", font)) { Colspan = 2, Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, Colspan = sectionAlias == GamesAlias.Athletics ? 5 : 6 });
                table.AddCell(new PdfPCell(new Phrase(vmWorkerReportInfo.TotalFeeCount.ToString(CultureInfo.InvariantCulture), font)) {  Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(vmWorkerReportInfo.GamesAssigned.Sum(c => c.TravelDistance ?? 0).ToString(CultureInfo.InvariantCulture), font)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 1 });
                if (sectionAlias == GamesAlias.Athletics)
                {
                    table.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, Colspan = 1 });
                }
                else
                {
                    table.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, Colspan = 2 });
                }
                
                #endregion


                pdfDocument.Add(table);
            }
        }

        public void AddSecondTable(Document document, WorkerReportDTO workerReportInfo, bool isHebrew, bool isReferee, string reportType, string sectionAlias)
        {
            if (!(workerReportInfo?.GamesAssigned == null || workerReportInfo.GamesCount == 0))
            {
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                if (workerReportInfo?.GamesAssigned == null)
                    return;

                PdfPTable table = new PdfPTable(2);
                document.Add(table);
                table.HorizontalAlignment = 0;
                table.TotalWidth = 300f;
                table.LockedWidth = true;

                if (isHebrew)
                {
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    float[] widths = { 80f, 220f };
                    table.SetWidths(widths);
                }
                else
                {
                    float[] widths = { 220f, 80f };
                    table.SetWidths(widths);
                }


                table.DefaultCell.Border = Rectangle.NO_BORDER;

                Font fontNotBold = new Font(bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
                Font fontBold = new Font(bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);

                #region Values
                var hasMoreThanOneLigue = workerReportInfo?.LeaguesGrouped?.Count() > 1 && sectionAlias != GamesAlias.Athletics;
                var travelDistance = workerReportInfo.GamesAssigned?.Sum(c => c.TravelDistance ?? 0) ?? 0D;
                decimal? leaguePaymentPerKm;
                if (!hasMoreThanOneLigue)
                {
                    var item = workerReportInfo.GamesAssigned?.FirstOrDefault();
                    if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateA")
                    {
                        leaguePaymentPerKm = item?.IsUnionReport == true
                    ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateAForTravel
                    : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateAForTravel;
                    }
                    else if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateB")
                    {
                        leaguePaymentPerKm = item?.IsUnionReport == true
                    ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateBForTravel
                    : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateBForTravel;
                    }
                    else if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateC")
                    {
                        leaguePaymentPerKm = item?.IsUnionReport == true
                    ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateCForTravel
                    : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateCForTravel;
                    }
                    else
                    {
                        leaguePaymentPerKm = item?.IsUnionReport == true
                    ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.PaymentTravel
                    : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.PaymentTravel;
                    }


                }
                else
                {
                    leaguePaymentPerKm = 0;
                }

                var totalPayment = !hasMoreThanOneLigue ? Convert.ToDecimal(travelDistance) * leaguePaymentPerKm : 0;

                double totalHours = workerReportInfo.GamesCount * 2;
                if(sectionAlias == GamesAlias.Athletics)
                {
                    totalHours = workerReportInfo.GamesAssigned.Sum(g => g.WorkedHours);
                }


                #endregion

                #region Table body

                //Total summary
                table.AddCell(new PdfPCell(new Phrase(Messages.ReportTable_TotalSummary, fontBold)) { Border = Rectangle.BOTTOM_BORDER });
                table.AddCell(new PdfPCell(new Phrase(" ")) { Border = Rectangle.BOTTOM_BORDER });

                if (isReferee)
                {
                    //Total days
                    table.AddCell(new PdfPCell(new Phrase(Messages.TotalDays, fontBold)) { Border = reportType != "reportWithoutSum" ? Rectangle.NO_BORDER : Rectangle.BOTTOM_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{workerReportInfo?.DaysCount.ToString()}")) { Border = reportType != "reportWithoutSum" ?  Rectangle.NO_BORDER : Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });

                    //Total hours
                    table.AddCell(new PdfPCell(new Phrase(Messages.Total_Hours, fontBold)) { Border = reportType != "reportWithoutSum" ? Rectangle.NO_BORDER : Rectangle.BOTTOM_BORDER });
                    if (reportType != "reportWithoutSum")
                    {
                        table.AddCell(new PdfPCell(new Phrase($"{totalHours.ToString()}")) { Border =  Rectangle.NO_BORDER, HorizontalAlignment = 2 });
                    }
                    else
                    {
                        table.AddCell(new PdfPCell(new Phrase("   ")) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });
                    }
                    if (reportType == "reportWithoutSum")
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(Messages.Comment,fontBold)) { Border = Rectangle.BOTTOM_BORDER, FixedHeight = 30f };
                        table.AddCell(cell);
                        table.AddCell(new PdfPCell(new Phrase("   ")) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });
                    }
                }
                if (!isReferee && reportType == "reportWithoutSum")
                {
                    table.AddCell(new PdfPCell(new Phrase(Messages.TotalDays, fontBold)) { Border = Rectangle.BOTTOM_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{workerReportInfo?.DaysCount.ToString()}")) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });
                    table.AddCell(new PdfPCell(new Phrase(Messages.TotalHours, fontBold)) { Border = Rectangle.BOTTOM_BORDER });
                    table.AddCell(new PdfPCell(new Phrase("   ")) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });
                    PdfPCell cell = new PdfPCell(new Phrase(Messages.Comment, fontBold)) { Border = Rectangle.BOTTOM_BORDER, FixedHeight = 30f };
                    table.AddCell(cell);
                    table.AddCell(new PdfPCell(new Phrase("   ")) { Border = Rectangle.BOTTOM_BORDER, HorizontalAlignment = 2 });
                }
                if (reportType != "reportWithoutSum")
                {
                    //Total fees
                    table.AddCell(new PdfPCell(new Phrase(Messages.ReportTable_TotalFees, fontBold)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{workerReportInfo?.TotalFeeCount.ToString()} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2 });


                    if (hasMoreThanOneLigue)
                    {
                        foreach (var group in workerReportInfo?.LeaguesGrouped)
                        {
                            bool isUnionReport = group?.FirstOrDefault()?.IsUnionReport == true;
                            var leagueName = group.Key?.Name;
                            decimal? paymentPerKm = 0M;
                            if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateA")
                            {
                                paymentPerKm = group?.FirstOrDefault()?.IsUnionReport == true
                                    ? group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateAForTravel
                                    : group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateAForTravel;
                            }
                            else if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateB")
                            {
                                paymentPerKm = group?.FirstOrDefault()?.IsUnionReport == true
                                    ? group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateBForTravel
                                    : group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateBForTravel;
                            }
                            else if (workerReportInfo.WorkerRate != null && workerReportInfo.WorkerRate == "RateC")
                            {
                                paymentPerKm = group?.FirstOrDefault()?.IsUnionReport == true
                                    ? group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateCForTravel
                                    : group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateCForTravel;
                            }
                            else
                            {
                                paymentPerKm = group?.FirstOrDefault()?.IsUnionReport == true
                                    ? group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.PaymentTravel
                                    : group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.PaymentTravel;
                            }
                            var travelLeagueDistance = isUnionReport ? workerReportInfo.GamesAssigned.Sum(c => c.TravelDistance ?? 0) : group?.Sum(c => c.TravelDistance ?? 0) ?? 0;
                            var paymentByDistanceOfLeague = Convert.ToDecimal(travelLeagueDistance) * paymentPerKm;
                            totalPayment += paymentByDistanceOfLeague;

                            //Total travel distance
                            table.AddCell(new PdfPCell(new Phrase(isUnionReport ? $"{Messages.ReportTable_TotalTravelDistance}"
                                : $"{Messages.ReportTable_TotalTravelDistance} - {leagueName}", fontBold))
                            { Border = Rectangle.NO_BORDER });
                            table.AddCell(new PdfPCell(new Phrase($"{travelLeagueDistance} ({Messages.Km})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2 });

                            //Payment per km
                            table.AddCell(new PdfPCell(new Phrase(isUnionReport ? $"{Messages.ReportTable_PaymentPerKm}"
                                : $"{Messages.ReportTable_PaymentPerKm} - {leagueName}", fontBold))
                            { Border = Rectangle.NO_BORDER });
                            table.AddCell(new PdfPCell(new Phrase($"{paymentPerKm} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                            //Total payment for travel
                            table.AddCell(new PdfPCell(new Phrase(isUnionReport ? $"{Messages.ReportTable_TotalPayment}"
                                : $"{Messages.ReportTable_TotalPayment} - {leagueName}", fontBold))
                            { Border = Rectangle.NO_BORDER });
                            table.AddCell(new PdfPCell(new Phrase(String.Format("{0:0.00} ({1})", paymentByDistanceOfLeague, Messages.Nis), fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                            if (isUnionReport) break;
                        }
                    }
                    else
                    {
                        //Total travel distance
                        table.AddCell(new PdfPCell(new Phrase($"{Messages.ReportTable_TotalTravelDistance}", fontBold)) { Border = Rectangle.NO_BORDER });
                        table.AddCell(new PdfPCell(new Phrase($"{travelDistance} ({Messages.Km})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                        //Payment per km
                        table.AddCell(new PdfPCell(new Phrase($"{Messages.ReportTable_PaymentPerKm}", fontBold)) { Border = Rectangle.NO_BORDER });
                        table.AddCell(new PdfPCell(new Phrase($"{leaguePaymentPerKm} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                        //Total payment for travel
                        table.AddCell(new PdfPCell(new Phrase($"{Messages.ReportTable_TotalPayment}", fontBold)) { Border = Rectangle.NO_BORDER });
                        table.AddCell(new PdfPCell(new Phrase($"{totalPayment} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });
                    }

                    //Total intermediate
                    var totalIntermediate = Convert.ToDecimal(totalPayment) + workerReportInfo.TotalFeeCount;
                    table.AddCell(new PdfPCell(new Phrase(Messages.ReportTable_TotalIntermediate, fontBold)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{String.Format("{0:0.00}", totalIntermediate)} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                    //% tax
                    table.AddCell(new PdfPCell(new Phrase(Messages.ReportTable_Tax, fontBold)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{workerReportInfo?.WithholdingTax} %", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                    //Withholding tax
                    var withholdingTax = workerReportInfo.WithholdingTax.HasValue ? (totalIntermediate * workerReportInfo.WithholdingTax) / 100 : 0;
                    table.AddCell(new PdfPCell(new Phrase(Messages.WithholdingTax, fontBold)) { Border = Rectangle.NO_BORDER });
                    if (isHebrew)
                        table.AddCell(new PdfPCell(new Phrase($"{String.Format("-{0:0.00} ({1})", withholdingTax, Messages.Nis)}", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });
                    else
                        table.AddCell(new PdfPCell(new Phrase($"{String.Format("{0}{1:0.00} ({2})", "-", withholdingTax, Messages.Nis)}", fontNotBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });

                    //Withholding tax
                    var forPayment = totalIntermediate - withholdingTax;
                    table.AddCell(new PdfPCell(new Phrase(Messages.ReportTable_ForPayment, fontBold)) { Border = Rectangle.TOP_BORDER });
                    table.AddCell(new PdfPCell(new Phrase($"{String.Format("{0:0.00}", forPayment)} ({Messages.Nis})", fontNotBold)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = 2, RunDirection = isHebrew ? 3 : 2 });
                }
                #endregion
                document.Add(table);
            }


        }

        public void AddNotifications(Document document, bool isHebrew)
        {
            Font font = new Font(bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            document.Add(new Paragraph(" "));

            Phrase phrase = new Phrase
            {
                new Chunk($"* {Messages.TotalHoursInfo}", font)
            };

            PdfPTable notifications = new PdfPTable(1);
            notifications.DefaultCell.BorderWidth = 0;
            if (isHebrew)
            {
                notifications.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }
            notifications.AddCell(new PdfPCell(phrase) { Border = Rectangle.NO_BORDER });
            document.Add(notifications);
        }

        public void AddCopyrights(Document document, bool isHebrew)
        {
            Font fontBold = new Font(bfArialUniCode, 12, Font.BOLD, BaseColor.BLACK);
            Font font = new Font(bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            document.Add(new Paragraph(" "));
            Phrase phrase = new Phrase
            {
                new Chunk($"© {Messages.Report_Copyright} ", font),
                new Chunk($"{Messages.Report_LogLigLtd}", fontBold)
            };
            PdfPTable copyright = new PdfPTable(1);
            copyright.DefaultCell.BorderWidth = 0;
            if (isHebrew)
            {
                copyright.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }
            copyright.AddCell(new PdfPCell(phrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            document.Add(copyright);
        }

        public void CreateTable(Document document, SalaryReportForm vm, bool isHebrew, bool isReferee,
            string sectionAlias, string reportType)
        {
            AddHeaders(document, vm.WorkerReportInfo?.WorkerFullName, vm.WorkerReportInfo?.WorkerId, vm.WorkerReportInfo?.WorkerIdentNum,
                vm.WorkerReportInfo?.SeasonName, vm.OfficialType, 
                vm.ReportStartDate.ToShortDateString(),
                vm.ReportEndDate.ToShortDateString(),
                vm.WorkerReportInfo?.City,
                vm.WorkerReportInfo?.Address,
                isHebrew);
            AddMainTable(document, vm.WorkerReportInfo, 
                vm.ReportStartDate.ToShortDateString(),
                vm.ReportEndDate.ToShortDateString(),
                isHebrew, sectionAlias, reportType);
            AddSecondTable(document, vm.WorkerReportInfo, isHebrew, isReferee, reportType, sectionAlias);
            if (isReferee && reportType != "reportWithoutSum" && sectionAlias != GamesAlias.Athletics)
            {
                AddNotifications(document, isHebrew);
            }
            AddCopyrights(document, isHebrew);
        }
    }
}