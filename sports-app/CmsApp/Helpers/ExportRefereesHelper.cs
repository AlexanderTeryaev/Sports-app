using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using DataService.DTO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Resources;
using AppModel;

namespace CmsApp.Helpers
{
    public class ExportRefereesHelper
    {

        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        Font titleWordFont;
        Font footerWordFont;
        Font unionWordFont;
        Font clubWordFont;
        private string _contentPath;
        private float cellHeight = 25f;
        private int eachEmptyPage = 9;
        private int eachFirstPage = 8;

        public ExportRefereesHelper(Union union, DateTime? startReportDate, DateTime? endReportDate, int seasonId, bool isHebrew, string contentPath, List<User> users, List<WorkerReportDTO> workerReportInfos)
        {
            _document = new Document(PageSize.A4.Rotate(), 30, 30, 25, 25);
            _isHebrew = isHebrew;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            new Font(_bfArialUniCode, 22, Font.BOLD, new BaseColor(0, 0, 128));
            titleWordFont = new Font(_bfArialUniCode, 16, Font.NORMAL, BaseColor.BLACK);
            new Font(_bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            new Font(_bfArialUniCode, 5, Font.NORMAL, BaseColor.BLACK);
            unionWordFont = new Font(_bfArialUniCode, 34, Font.BOLD, new BaseColor(0, 0, 128));
            clubWordFont = new Font(_bfArialUniCode, 26, Font.BOLD, new BaseColor(0, 0, 128));

            footerWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            new Font(_bfArialUniCode, 4, Font.NORMAL, BaseColor.BLACK);
            new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            new Font(_bfArialUniCode, 16, Font.BOLD, BaseColor.BLACK);
            new Font(_bfArialUniCode, 32, Font.BOLD, new BaseColor(0, 0, 128));

            writer = PdfWriter.GetInstance(_document, _stream);
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            writer.CloseStream = false;
            _document.Open();
            AddTitle(union, startReportDate, endReportDate);
            AddTable(union, users, workerReportInfos);
            AddFooter();
            AddCopyrights();
        }

        private void AddTitle(Union union, DateTime? startReportDate, DateTime? endReportDate)
        {
            AddIconAndUnionName(union.Name, union.Logo);

            var titleTable = new PdfPTable(6)
            {
                WidthPercentage = 100
            };

            titleTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"     {Messages.RefereesSummaryReport} - {startReportDate.Value.ToShortDateString()} - {endReportDate.Value.ToShortDateString()}", titleWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 6
            });

            titleTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 6
                });

            _document.Add(titleTable);
        }

        public void AddIconAndUnionName(string unionName, string url)
        {
            var iconeUnionPhraseTable = new PdfPTable(5)
            {
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                WidthPercentage = 100
            };

            try
            {
                var savePath = _contentPath + "/union/" + url;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(savePath);
                float ratio = logo.Width / logo.Height;
                logo.ScaleAbsoluteHeight(55);
                logo.ScaleAbsoluteWidth(55 * ratio);

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

            iconeUnionPhraseTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"      {unionName}", unionWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 4
            });
            _document.Add(iconeUnionPhraseTable);
        }

        private void AddTable(Union union, List<User> users, List<WorkerReportDTO> workerReportInfos)
        {
            PdfPTable table = new PdfPTable(14)
            {
                HorizontalAlignment = 0,
                TotalWidth = 800f,
                LockedWidth = true
            };

            if (_isHebrew)
            {
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                float[] widths = { 60f, 60f, 45f, 55f, 55f, 70f, 60f, 60f, 50f, 50f, 50f, 65f, 90f, 30f };
                table.SetWidths(widths);
            }
            else
            {
                float[] widths = { 30f, 90f, 65f, 50f, 50f, 50f, 60f, 60f, 70f, 55f, 55f, 45f, 60f, 60f };
                table.SetWidths(widths);
            }

            AddTableHeader(table, union);
            AddTableBody(table, union, users, workerReportInfos);
            _document.Add(table);
        }

        private void AddTableHeader(PdfPTable table, Union union)
        {
            AddCell(table, "##", true);
            AddCell(table, Messages.FullName);
            AddCell(table, Messages.Id);
            AddCell(table, Messages.NumberOfDays);
            AddCell(table, Messages.NumberOfHours);
            if (union.Section.IsIndividual)
            {
                AddCell(table, Messages.Competitions);
            }
            else
            {
                AddCell(table, Messages.NumberOfGames);
            }

            AddCell(table, Messages.PaymentOfWeekdays);
            AddCell(table, Messages.PaymentOfWeekends);
            AddCell(table, Messages.TotalTravel);
            AddCell(table, Messages.TravelPayment);
            AddCell(table, Messages.TotalPayment);
            AddCell(table, Messages.ReportTable_Tax);
            AddCell(table, Messages.ReportTable_WithholdingTax);
            AddCell(table, Messages.ReportTable_ForPayment);
        }

        private void AddTableBody(PdfPTable table, Union union, List<User> users, List<WorkerReportDTO> workerReportInfos)
        {
            var daysSum = 0;
            double hoursSum = 0;
            var gamesSum = 0;
            decimal paymentOfWeekdaysSum = 0;
            decimal paymentOfWeekendsSum = 0;
            double travelDistanceSum = 0;
            decimal travelPaymentSum = 0;
            decimal totalPaymentSum = 0;
            decimal withholdingTaxSum = 0;
            decimal forPaymentSum = 0;
            int rowNumber = 1;
            var sortedUsers = users.OrderBy(p => p.FullName);
            foreach (User referee in sortedUsers)
            {
                WorkerReportDTO reportDto = workerReportInfos.FirstOrDefault(x => x.WorkerId == referee.UserId);
                if (reportDto != null)
                {
                    AddCell(table, rowNumber.ToString(), true);
                    rowNumber++;
                    AddCell(table, referee.FullName);
                    AddCell(table, referee.IdentNum);
                    daysSum += reportDto.DaysCount;
                    AddCell(table, reportDto.DaysCount.ToString());
                    double hours = 0;
                    if (union.Section.IsIndividual)
                    {
                        hours = reportDto.GamesAssigned.Sum(x => x.WorkedHours);
                    }
                    else
                    {
                        hours = reportDto.GamesCount * 2;
                    }
                    hoursSum += hours;
                    AddCell(table, hours.ToString());
                    gamesSum += reportDto.GamesCount;
                    AddCell(table, reportDto.GamesCount.ToString());
                    var paymentOfWeekdays = reportDto.GamesAssigned.Where(x => !IsWeekend(x.StartDate, union)).Sum(x => x.LeagueFee);
                    paymentOfWeekdaysSum += paymentOfWeekdays.HasValue ? paymentOfWeekdays.Value : 0;
                    AddCell(table, paymentOfWeekdays.HasValue ? $"{String.Format("{0:0.00}", paymentOfWeekdays.Value)}" : "0");
                    var paymentOfWeekends = reportDto.GamesAssigned.Where(x => IsWeekend(x.StartDate, union)).Sum(x => x.LeagueFee);
                    paymentOfWeekendsSum += paymentOfWeekends.HasValue ? paymentOfWeekends.Value : 0;
                    AddCell(table, paymentOfWeekends.HasValue ? $"{String.Format("{0:0.00}", paymentOfWeekends.Value)}" : "0");
                    var travelDistance = reportDto.GamesAssigned.Sum(c => c.TravelDistance ?? 0);
                    travelDistanceSum += travelDistance;
                    AddCell(table, $"{String.Format("{0:0.00}", travelDistance)}");
                    var travelPayment = GetTravelPayment(reportDto, union);
                    travelPaymentSum += travelPayment;
                    AddCell(table, $"{String.Format("{0:0.00}", travelPayment)}");
                    var totalPayment = paymentOfWeekdays + paymentOfWeekends + travelPayment;
                    totalPaymentSum += totalPayment.HasValue ? totalPayment.Value : 0;
                    AddCell(table, totalPayment.HasValue ? $"{String.Format("{0:0.00}", totalPayment.Value)}" : "0");
                    AddCell(table, reportDto.WithholdingTax.HasValue ? reportDto.WithholdingTax.Value + " %" : "0 %", false, true);
                    var withholdingTax = reportDto.WithholdingTax.HasValue ? (totalPayment * reportDto.WithholdingTax) / 100 : 0;
                    withholdingTaxSum += withholdingTax.HasValue ? withholdingTax.Value : 0;
                    AddCell(table, withholdingTax.HasValue ? $"{String.Format("{0:0.00}", withholdingTax.Value)}" : "0");
                    var forPayment = totalPayment - withholdingTax;
                    forPaymentSum += forPayment.HasValue ? forPayment.Value : 0;
                    AddCell(table, forPayment.HasValue ? $"{String.Format("{0:0.00}", forPayment.Value)}" : "0");
                }
            }

            AddTotalCell(table, Messages.Total);
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, $"{String.Format("{0:0.00}", daysSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", hoursSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", gamesSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", paymentOfWeekdaysSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", paymentOfWeekendsSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", travelDistanceSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", travelPaymentSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", totalPaymentSum)}");
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, $"{String.Format("{0:0.00}", withholdingTaxSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", forPaymentSum)}");
        }



        private bool IsWeekend(DateTime? startDateValue, Union union)
        {
            bool isWeekend = false;
            if (union.SaturdaysTariff.HasValue && union.SaturdaysTariff.Value && union.SaturdaysTariffFromTime.HasValue && union.SaturdaysTariffToTime.HasValue && startDateValue.HasValue)
            {
                DayOfWeek fromDay = union.SaturdaysTariffFromTime.Value.DayOfWeek;
                int fromHour = union.SaturdaysTariffFromTime.Value.Hour;
                DayOfWeek toDay = union.SaturdaysTariffToTime.Value.DayOfWeek;
                int toHour = union.SaturdaysTariffToTime.Value.Hour;

                if (toDay <= fromDay && (toDay != fromDay || toHour <= fromHour))
                {
                    union.SaturdaysTariffToTime.Value.AddDays(7);
                }

                DayOfWeek gameDay = startDateValue.Value.DayOfWeek;
                var gameHour = startDateValue.Value.Hour;
                DateTime testDate = new DateTime(1970, 2, (int)gameDay + 1);
                testDate = testDate.AddHours(gameHour);
                if (union.SaturdaysTariffFromTime < testDate && union.SaturdaysTariffToTime > testDate)
                {
                    isWeekend = true;
                }
            }

            return isWeekend;
        }

        private decimal GetTravelPayment(WorkerReportDTO reportDto, Union union)
        {
            var hasMoreThanOneLigue = reportDto?.LeaguesGrouped?.Count() > 1 && union.Section.Alias != GamesAlias.Athletics;
            decimal? travelPayment = 0;
            if (hasMoreThanOneLigue)
            {
                foreach (var group in reportDto.LeaguesGrouped)
                {
                    bool isUnionReport = @group?.FirstOrDefault()?.IsUnionReport == true;
                    var leagueName = @group.Key?.Name;
                    decimal? paymentPerKm = 0M;
                    if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateA")
                    {
                        paymentPerKm = @group?.FirstOrDefault()?.IsUnionReport == true
                            ? @group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateAForTravel
                            : @group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateAForTravel;
                    }
                    else if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateB")
                    {
                        paymentPerKm = @group?.FirstOrDefault()?.IsUnionReport == true
                            ? @group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateBForTravel
                            : @group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateBForTravel;
                    }
                    else if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateC")
                    {
                        paymentPerKm = @group?.FirstOrDefault()?.IsUnionReport == true
                            ? @group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateCForTravel
                            : @group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateCForTravel;
                    }
                    else
                    {
                        paymentPerKm = @group?.FirstOrDefault()?.IsUnionReport == true
                            ? @group?.FirstOrDefault()?.Union?.UnionOfficialSettings?.FirstOrDefault()?.PaymentTravel
                            : @group?.FirstOrDefault()?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.PaymentTravel;
                    }

                    var travelLeagueDistance = isUnionReport
                        ? reportDto.GamesAssigned.Sum(c => c.TravelDistance ?? 0)
                        : @group?.Sum(c => c.TravelDistance ?? 0) ?? 0;
                    var paymentByDistanceOfLeague = Convert.ToDecimal(travelLeagueDistance) * paymentPerKm;
                    travelPayment += paymentByDistanceOfLeague;
                }
            }
            else
            {
                decimal? leaguePaymentPerKm;
                var item = reportDto.GamesAssigned?.FirstOrDefault();
                if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateA")
                {
                    leaguePaymentPerKm = item?.IsUnionReport == true
                ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateAForTravel
                : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateAForTravel;
                }
                else if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateB")
                {
                    leaguePaymentPerKm = item?.IsUnionReport == true
                ? item?.Union?.UnionOfficialSettings?.FirstOrDefault()?.RateBForTravel
                : item?.League?.LeagueOfficialsSettings?.FirstOrDefault()?.RateBForTravel;
                }
                else if (reportDto.WorkerRate != null && reportDto.WorkerRate == "RateC")
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
                var travelDistance = reportDto.GamesAssigned.Sum(c => c.TravelDistance ?? 0);
                travelPayment = !hasMoreThanOneLigue ? Convert.ToDecimal(travelDistance) * leaguePaymentPerKm : 0;
            }


            return travelPayment.HasValue ? travelPayment.Value : 0;
        }

        private void AddFooter()
        {
            var footerTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            footerTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"{Messages.ReportDate} - {DateTime.Now.ToShortDateString()}", footerWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                Colspan = 1
            });

            _document.Add(footerTable);
        }

        public void AddCopyrights()
        {
            Font fontBold = new Font(_bfArialUniCode, 12, Font.BOLD, BaseColor.BLACK);
            Font font = new Font(_bfArialUniCode, 12, Font.NORMAL, BaseColor.BLACK);
            _document.Add(new Paragraph(" "));
            Phrase phrase = new Phrase
            {
                new Chunk($"© {Messages.Report_Copyright} ", font),
                new Chunk($"{Messages.Report_LogLigLtd}", fontBold)
            };
            PdfPTable copyright = new PdfPTable(1);
            copyright.DefaultCell.BorderWidth = 0;
            if (_isHebrew)
            {
                copyright.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }
            copyright.AddCell(new PdfPCell(phrase) { HorizontalAlignment = Element.ALIGN_CENTER, Border = Rectangle.NO_BORDER });
            _document.Add(copyright);
        }

        public MemoryStream GetDocumentStream()
        {
            _document.Close();
            writer.Close();
            return _stream;
        }

        private void AddCell(PdfPTable tableLayout, string cellText, bool isCentered = false, bool isPercentage = false)
        {
            if (cellText == null)
            {
                cellText = string.Empty;
            }
            Font font = new Font(_bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            var cell = new PdfPCell(new Phrase(cellText, font));
            if (IsHebrew(cellText))
            {
                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }

            if (isCentered)
            {
                cell.HorizontalAlignment = 1;
            }

            if (isPercentage)
            {
                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            }
            cell.FixedHeight = cellHeight;
            tableLayout.AddCell(cell);
        }

        private void AddTotalCell(PdfPTable tableLayout, string cellText, bool isCentered = false)
        {
            if (cellText == null)
            {
                cellText = string.Empty;
            }
            Font font = new Font(_bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            var cell = new PdfPCell(new Phrase(cellText, font));
            cell.BorderWidthTop = 2f;
            if (IsHebrew(cellText))
            {
                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
            }

            if (isCentered)
            {
                cell.HorizontalAlignment = 1;
            }
            cell.FixedHeight = cellHeight;
            tableLayout.AddCell(cell);
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
    }
}