using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Net;
using AppModel;
using DataService.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Resources;
using Element = iTextSharp.text.Element;

namespace CmsApp.Helpers
{
    public class ExportRefereesCompetitionHelper
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

        public ExportRefereesCompetitionHelper(League league, bool isHebrew, string contentPath, List<User> users, List<UsersJob> usersJobs, List<UsersJob> usersJobsOnUnionLevel)
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
            AddTitle(league);
            AddTable(users, usersJobs, usersJobsOnUnionLevel);
            AddFooter();
            AddCopyrights();
        }

        private void AddTitle(League league)
        {
            AddIconAndUnionName(league.Union.Name, league.Union.Logo);

            var titleTable = new PdfPTable(6)
            {
                WidthPercentage = 100
            };

            titleTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($"     {Messages.RefereesSummaryReport} - {league.Name} - {league.LeagueStartDate.Value.ToShortDateString()}", titleWordFont) })
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

        private void AddTable(List<User> users, List<UsersJob> usersJobs, List<UsersJob> usersJobsOnUnionLevel)
        {
            PdfPTable table = new PdfPTable(11)
            {
                HorizontalAlignment = 0,
                TotalWidth = 800f,
                LockedWidth = true
            };

            if (_isHebrew)
            {
                table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                float[] widths = { 70f, 70f, 60f, 70f, 85f, 75f, 75f, 60f, 75f, 120f, 40f };
                table.SetWidths(widths);
            }
            else
            {
                float[] widths = { 40f, 120f, 75f, 60f, 75f, 75f, 85f, 70f, 60f, 70f, 70f };
                table.SetWidths(widths);
            }

            AddTableHeader(table);
            AddTableBody(table, users, usersJobs, usersJobsOnUnionLevel);
            _document.Add(table);
        }

        private void AddTableHeader(PdfPTable table)
        {
            AddCell(table, "##", true);
            AddCell(table, Messages.FullName);
            AddCell(table, Messages.Id);
            AddCell(table, Messages.PaymentRateType);
            AddCell(table, Messages.NumberOfHours);
            AddCell(table, Messages.TotalPayment);
            AddCell(table, Messages.TotalTravel);
            AddCell(table, Messages.TravelPayment);
            AddCell(table, Messages.ReportTable_Tax);
            AddCell(table, Messages.ReportTable_WithholdingTax);
            AddCell(table, Messages.ReportTable_ForPayment);
        }

        private void AddTableBody(PdfPTable table, List<User> users, List<UsersJob> usersJobs, List<UsersJob> usersJobsOnUnionLevel)
        {
            double numberOfHoursSum = 0;
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
                var userJob = usersJobs.FirstOrDefault(x => x.UserId == referee.UserId);
                var userJobOnUnionLevel = usersJobsOnUnionLevel.FirstOrDefault(x=>x.UserId == referee.UserId);
                if (userJob != null && userJobOnUnionLevel != null)
                {
                    AddCell(table, rowNumber.ToString(), true);
                    rowNumber++;
                    AddCell(table, referee.FullName);
                    AddCell(table, referee.IdentNum);
                    AddCell(table, GetRateTypeTranslated(userJobOnUnionLevel.RateType));
                    double numberOfHours = GetWorkedHours(userJob);
                    numberOfHoursSum += numberOfHours;
                    AddCell(table, $"{String.Format("{0:0.00}", numberOfHours)}");
                    var feePerHour = GetLeaguePayment(userJob.League.Union.UnionOfficialSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == userJob.Job.JobsRole.RoleName), userJobOnUnionLevel.RateType, "Game");
                    var totalPayment = Decimal.Round((decimal)numberOfHours * GetJobFee(feePerHour, userJob.League.LeagueStartDate, userJob.League.Union.SaturdaysTariff, userJob.League.Union.SaturdaysTariffFromTime, userJob.League.Union.SaturdaysTariffToTime), 2);
                    totalPaymentSum += totalPayment;
                    AddCell(table, $"{String.Format("{0:0.00}", totalPayment)}");
                    var travelDistance = GetTravelDistance(userJob);
                    travelDistanceSum += travelDistance;
                    AddCell(table, $"{String.Format("{0:0.00}", travelDistance)}");
                    var feePerKm = GetLeaguePayment(userJob.League.Union.UnionOfficialSettings?.FirstOrDefault(ls => ls.JobsRole?.RoleName == userJob.Job.JobsRole.RoleName), userJobOnUnionLevel.RateType, "Travel") ?? 0;
                    var travelPayment = Decimal.Round((decimal)travelDistance * feePerKm, 2);
                    travelPaymentSum += travelPayment;
                    AddCell(table, $"{String.Format("{0:0.00}", travelPayment)}");
                    AddCell(table, userJobOnUnionLevel.WithhodlingTax.HasValue ? userJobOnUnionLevel.WithhodlingTax.Value + " %" : "0 %", false, true);
                    var withholdingTax = userJobOnUnionLevel.WithhodlingTax.HasValue ? (totalPayment * userJobOnUnionLevel.WithhodlingTax.Value) / 100 : 0;
                    withholdingTaxSum += withholdingTax;
                    AddCell(table, $"{String.Format("{0:0.00}", withholdingTax)}");
                    var forPayment = travelPayment + totalPayment - withholdingTax;
                    forPaymentSum += forPayment;
                    AddCell(table, $"{String.Format("{0:0.00}", forPayment)}");
                }
            }

            AddTotalCell(table, Messages.Total);
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, $"{String.Format("{0:0.00}", numberOfHoursSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", totalPaymentSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", travelDistanceSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", travelPaymentSum)}");
            AddTotalCell(table, string.Empty);
            AddTotalCell(table, $"{String.Format("{0:0.00}", withholdingTaxSum)}");
            AddTotalCell(table, $"{String.Format("{0:0.00}", forPaymentSum)}");
        }

        private static double GetTravelDistance(UsersJob userJob)
        {
            double travelDistance = 0;
            foreach (var travelInformation in userJob.TravelInformations)
            {
                travelDistance += (travelInformation.NoTravel.HasValue && travelInformation.NoTravel.Value) ? 0 : userJob.OfficialGameReportDetails.Count() > 0 &&
                                                                                                                  userJob.OfficialGameReportDetails.FirstOrDefault().TravelDistance.HasValue
                    ? userJob.OfficialGameReportDetails.FirstOrDefault().TravelDistance.Value
                    : 2 * new GoogleMapsApiService().GetDistance(userJob.User.Address,
                          travelInformation.IsUnionTravel ? userJob.League.Union.Address : userJob.League.PlaceOfCompetition);
            }
            return travelDistance;
        }

        private decimal GetJobFee(decimal? hourFee, DateTime? LeagueStartDate, bool? isSaturdayTariff, DateTime? fromTime, DateTime? toTime)
        {
            if (!hourFee.HasValue)
            {
                return 0;
            }
            decimal multiplier = 1;
            if (isSaturdayTariff.HasValue && isSaturdayTariff.Value && fromTime.HasValue && toTime.HasValue && LeagueStartDate.HasValue)
            {
                DayOfWeek fromDay = fromTime.Value.DayOfWeek;
                int fromHour = fromTime.Value.Hour;
                DayOfWeek toDay = toTime.Value.DayOfWeek;
                int toHour = toTime.Value.Hour;

                if (toDay <= fromDay && (toDay != fromDay || toHour <= fromHour))
                {
                    toTime.Value.AddDays(7);
                }

                DayOfWeek gameDay = LeagueStartDate.Value.DayOfWeek;
                var gameHour = LeagueStartDate.Value.Hour;
                DateTime testDate = new DateTime(1970, 2, (int)gameDay + 1);
                testDate = testDate.AddHours(gameHour);
                if (fromTime < testDate && toTime > testDate)
                {
                    multiplier = 1.5M;
                }
            }
            return hourFee.Value * multiplier;
        }

        private void AddFooter()
        {
            var footerTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };
            bool hasStage = false;
            bool hasInstrumentWeight = false;
            bool hasFillDetails = false;

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

        public decimal? GetLeaguePayment(dynamic officialsSetting, string rateType, string type)
        {
            switch (rateType)
            {
                case "RateA":
                    return string.Equals(type, "Game") ? officialsSetting?.RateAPerGame : officialsSetting?.RateAForTravel;
                case "RateB":
                    return string.Equals(type, "Game") ? officialsSetting?.RateBPerGame : officialsSetting?.RateBForTravel;
                case "RateC":
                    return string.Equals(type, "Game") ? officialsSetting?.RateCPerGame : officialsSetting?.RateCForTravel;
                default:
                    return string.Equals(type, "Game") ? officialsSetting?.PaymentPerGame : officialsSetting?.PaymentTravel;
            }
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

        private string GetRateTypeTranslated(string rateType)
        {
            switch (rateType)
            {
                case "RateA":
                case "תעריף א'":
                case "Клас A":
                    return Messages.RateA;
                case "RateB":
                case "תעריף ב'":
                case "Клас B":
                    return Messages.RateB;
                case "RateC":
                case "תעריף ג":
                case "Клас C":
                    return Messages.RateC;
                case "Default rate":
                case "תעריף בסיס":
                case "Базова ставка":
                    return Messages.DefaultRate;
                case "Referee A":
                case "רפרי A":
                case "Суддя А":
                    return Messages.RefereeA;
                case "Profissional B":
                case "בכיר B":
                case "Професіонал B":
                    return Messages.ProfissionalB;
                case "Advanced C":
                case "מתקדם C":
                case "Розширений C":
                    return Messages.AdvancedC;
                case "Basic D":
                case "בסיסי D":
                case "Базовий D":
                    return Messages.BasicD;
                default:
                    return rateType;
            }
        }

        private static double GetWorkedHours(UsersJob userJob)
        {
            double workedHours = 0;
            foreach (var travelInformation in userJob.TravelInformations)
            {
                workedHours += Math.Round((travelInformation.ToHour - travelInformation.FromHour).Value.TotalMinutes / 60.0f, 2); ;
            }
            return workedHours;
        }
    }
}