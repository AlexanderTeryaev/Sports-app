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
    public class RefereeFormPdfHelper
    {
        private readonly Document _document;
        private readonly bool _isHebrew;
        private readonly MemoryStream _stream;
        private readonly BaseFont _bfArialUniCode;
        private PdfWriter writer;
        Font clubWordFont;
        Font titleWordFont;
        Font tableWordFont;
        Font heatWordFont;
        Font tablePlaceWordFont;
        Font tablePlaceWordFont10;
        Font footerWordFont;
        private string _contentPath;
        private int _format;
        private string _gender;
        private string _ages;
        private float cellHeight = 38f;
        private float header = 90.0f;
        public RefereeFormPdfHelper(List<IGrouping<string, CompetitionDisciplineRegistration>> registrationsByHeat, League league, string disciplineName, int format, string gender, string ages, bool isHebrew, string contentPath)
        {
            _document = new Document(PageSize.A4.Rotate(), 30, 30, header+45, 25);
            _isHebrew = isHebrew;
            _format = format;
            _gender = gender;
            _ages = ages;
            _contentPath = contentPath;
            _stream = new MemoryStream();
            string fontPath = HostingEnvironment.MapPath("~/Content/fonts/ARIAL.TTF");
            _bfArialUniCode = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            clubWordFont = new Font(_bfArialUniCode, 22, Font.BOLD, new BaseColor(0, 0, 128));
            titleWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tableWordFont = new Font(_bfArialUniCode, 10, Font.NORMAL, BaseColor.BLACK);
            tablePlaceWordFont = new Font(_bfArialUniCode, 5, Font.NORMAL, BaseColor.BLACK);

            footerWordFont = new Font(_bfArialUniCode, 14, Font.NORMAL, BaseColor.BLACK);
            tablePlaceWordFont10 = new Font(_bfArialUniCode, 4, Font.NORMAL, BaseColor.BLACK);
            heatWordFont = new Font(_bfArialUniCode, 16, Font.BOLD, BaseColor.BLACK);

            writer = PdfWriter.GetInstance(_document, _stream);
            
            writer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

            if (_format > 7 && _format != 10 && _format != 11)
            {
                if (firstRun)
                {
                    writer.CloseStream = false;
                    _document.Open();
                    firstRun = false;

                }
                _document.Add(new Paragraph("No form for this discipline Format yet."));
            }
            else
            {
                for (int i = 0; i < registrationsByHeat.Count ; i++)
                {
                    bool isLast = false;
                    var registrationByHeat = registrationsByHeat.ElementAt(i);
                    if (i + 1 == registrationsByHeat.Count)
                    {
                        isLast = true;
                    }
                    AddHeatRegistration(registrationByHeat, league, disciplineName, isLast);
                }
                if (firstRun)
                {
                    writer.CloseStream = false;
                    _document.Open();
                    firstRun = false;
                    _document.Add(new Paragraph("No Athletes Registered to this Discipline."));
                }
                AddNewRow();
                addFooter();
            }

        }

        public MemoryStream GetDocumentStream()
        {
            _document.Close();
            writer.Close();
            return _stream;
        }


        public PdfPTable FormTitle(string competitionName, string date, string place, string disciplineName, string gender, string ages, string url)
        {
            var formPhraseTable = new PdfPTable(9)
            {
                WidthPercentage = 100
            };
            string formType = "";
            bool hasStage = false;
            bool hasInstrumentWeight = false;
            bool hasFillDetails = false;
            switch (_format)
            {
                case 1:
                case 2:
                case 3:
                    formType = Messages.ForRuns;
                    hasStage = true;
                    break;
                case 4:
                case 5:
                    formType = Messages.ForRuns + " " + Messages.Longs;
                    hasStage = true;
                    break;
                case 7:
                    formType = Messages.Throwings;
                    hasInstrumentWeight = true;
                    break;
                case 11:
                case 10:
                    formType = Messages.HorizontalJumps;
                    hasFillDetails = true;
                    break;
                case 6:
                    formType = Messages.VerticalJumps;
                    break;
            }

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"   {Messages.AthleticsDiscipline}: {disciplineName}", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });
            try
            {
                var savePath = _contentPath + "/union/" + url;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(savePath);
                float ratio = logo.Width / logo.Height;
                logo.ScaleAbsoluteHeight(60);
                logo.ScaleAbsoluteWidth(60 * ratio);

                formPhraseTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            catch (System.Net.WebException exception)
            {
                formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            catch (Exception ex)
            {
                formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.RefereeForm} {formType}", clubWordFont) })
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            formPhraseTable.AddCell(
            new PdfPCell(new Phrase { hasStage ? new Chunk($"     {Messages.Stage}: ________", titleWordFont) : hasInstrumentWeight ? new Chunk($"     {Messages.InstrumentWeight}: ________", titleWordFont) : hasFillDetails? new Chunk($"     {Messages.Mark}: {Messages.SuccessfulJumpSignDetail} \n            {Messages.FailedJumpSignDetail} \n            {Messages.SkipJumpSignDetail}", titleWordFont) : new Chunk(" ", titleWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 3
            });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.CompetitionName}: {competitionName}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"     {Messages.Date}: {date}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.Gender}: {gender}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            PdfPCell cell = new PdfPCell(new Phrase { _format <= 3 ? new Chunk($"     {Messages.Wind}: _______", titleWordFont) : new Chunk($"{Messages.PlaceOfCompetition}: {place}", titleWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 3
            };

            if(_format > 3)
            {
                cell.PaddingRight = 21;
            }

            formPhraseTable.AddCell(cell);

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.RawAge}: {ages}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });
            if (_format <= 3)
            {
                formPhraseTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{Messages.PlaceOfCompetition}: {place}", titleWordFont) })
                    {
                        Border = Rectangle.NO_BORDER,
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                        PaddingRight = 21,
                        Colspan = 3
                    });
                formPhraseTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk("", titleWordFont) })
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                        Colspan = 6
                    });
            }
            return formPhraseTable;
        }

        public void FormTitleOld(string competitionName, string date, string place, string disciplineName, string gender, string ages, string url)
        {
            var formPhraseTable = new PdfPTable(9)
            {
                WidthPercentage = 100
            };
            string formType = "";
            bool hasStage = false;
            bool hasInstrumentWeight = false;
            bool hasFillDetails = false;
            switch (_format)
            {
                case 1:
                case 2:
                case 3:
                    formType = Messages.ForRuns;
                    hasStage = true;
                    break;
                case 4:
                case 5:
                    formType = Messages.ForRuns + " " + Messages.Longs;
                    hasStage = true;
                    break;
                case 7:
                    formType = Messages.Throwings;
                    hasInstrumentWeight = true;
                    break;
                case 11:
                case 10:
                    formType = Messages.HorizontalJumps;
                    hasFillDetails = true;
                    break;
                case 6:
                    formType = Messages.VerticalJumps;
                    break;
            }

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"   {Messages.AthleticsDiscipline}: {disciplineName}", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });
            try
            {
                var savePath = _contentPath + "/union/" + url;
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(savePath);
                float ratio = logo.Width / logo.Height;
                logo.ScaleAbsoluteHeight(60);
                logo.ScaleAbsoluteWidth(60 * ratio);

                formPhraseTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            catch (System.Net.WebException exception)
            {
                formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            catch (Exception ex)
            {
                formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", clubWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    Colspan = 3,
                    Rowspan = 3
                });
            }
            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.RefereeForm} {formType}", clubWordFont) })
                {
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            formPhraseTable.AddCell(
            new PdfPCell(new Phrase { hasStage ? new Chunk($"     {Messages.Stage}: ________", titleWordFont) : hasInstrumentWeight ? new Chunk($"     {Messages.InstrumentWeight}: ________", titleWordFont) : hasFillDetails ? new Chunk($"     {Messages.Mark}: {Messages.SuccessfulJumpSignDetail} \n            {Messages.FailedJumpSignDetail} \n            {Messages.SkipJumpSignDetail}", titleWordFont) : new Chunk(" ", titleWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 3
            });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.CompetitionName}: {competitionName}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"     {Messages.Date}: {date}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.Gender}: {gender}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });

            PdfPCell cell = new PdfPCell(new Phrase { _format <= 3 ? new Chunk($"     {Messages.Wind}: _______", titleWordFont) : new Chunk($"{Messages.PlaceOfCompetition}: {place}", titleWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = 3
            };

            if (_format > 3)
            {
                cell.PaddingRight = 21;
            }

            formPhraseTable.AddCell(cell);

            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk(" ", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });


            formPhraseTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{Messages.RawAge}: {ages}", titleWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = 3
                });
            if (_format <= 3)
            {
                formPhraseTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{Messages.PlaceOfCompetition}: {place}", titleWordFont) })
                    {
                        Border = Rectangle.NO_BORDER,
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                        PaddingRight = 21,
                        Colspan = 3
                    });
                formPhraseTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk("", titleWordFont) })
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                        Colspan = 6
                    });
            }
            _document.Add(formPhraseTable);
        }


        public void AddPlayerDataByCompetition(PdfPTable registrationsTable, string place, string athleteNumber, string lane, string fullName, string clubName, bool underline)
        {
            int padding = 5;
            var border = underline ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER;
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{place}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = _format == 10 || _format == 11 || _format == 6 || _format == 4 || _format == 5 || _format == 1 || _format == 2 || _format == 3 || _format == 7 ? 2 : 1,
                        PaddingBottom = padding
                    }
            );
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{fullName}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = _format == 4 || _format == 5 ? 2:3,
                        PaddingBottom = padding
                    }
            );
            registrationsTable.AddCell(
                    new PdfPCell(new Phrase { new Chunk($"{athleteNumber}", tableWordFont) })
                    {
                        HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                        VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                        Colspan = 2,
                        PaddingBottom = padding
                    }
            );
            if (_format == 1 || _format == 2 || _format == 3)
            {
                registrationsTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk($"{lane}", tableWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 2,
                            PaddingBottom = padding
                        }
                );
            }
            registrationsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($"{clubName}", tableWordFont) })
                {
                    HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                    VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                    Colspan = 3,
                    PaddingBottom = padding
                }
            );

            if (!string.IsNullOrWhiteSpace(place))
            {
                switch (_format)
                {
                    case 1:
                    case 2:
                    case 3:
                        AddTableCell(registrationsTable, Messages.FinalResult, 2, 0, true);
                        AddTableCell(registrationsTable, $"{Messages.Time} 1", 2);
                        AddTableCell(registrationsTable, $"{Messages.Time} 2", 2);
                        AddTableCell(registrationsTable, $"{Messages.Time} 3", 2);
                        AddTableCell(registrationsTable, Messages.Comments, 2);
                        AddTableCell(registrationsTable, Messages.ResultScore, 2);
                        break;
                    case 4:
                    case 5:
                        AddTableCell(registrationsTable, Messages.ElectricTime, 2);
                        AddTableCell(registrationsTable, Messages.ManualTime, 2);
                        AddTableCell(registrationsTable, Messages.Comments, 2);
                        AddTableCell(registrationsTable, Messages.ResultScore, 2);
                        break;
                    case 7:
                        AddTableCell(registrationsTable, Messages.FinalResult, 2, 0, true);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 1", 2);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 2", 2);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 3", 2);
                        AddTableCell(registrationsTable, $"{Messages.MiddleSum}", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 4", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 5", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 6", 2);
                        AddTableCell(registrationsTable, Messages.Comments, 2);
                        AddTableCell(registrationsTable, Messages.ResultScore, 2);
                        break;
                    case 11:
                    case 10:
                        AddTableCell(registrationsTable, Messages.FinalResult, 2, 0, true);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 1", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 2", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.SmallAttempt} 3", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.MiddleSum}", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 4", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 5", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, $"{Messages.Final} 6", 2);
                        AddTableCell(registrationsTable, $"{Messages.Wind}", 2);
                        AddTableCell(registrationsTable, Messages.Comments, 2);
                        AddTableCell(registrationsTable, Messages.ResultScore, 2);
                        break;
                    case 6:
                        AddTableCell(registrationsTable, Messages.FinalResult, 3, 0, true);

                        AddTableCell(registrationsTable, $"1", 1, 4);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 1);
                        AddTableCell(registrationsTable, $"1", 1, 2);
                        AddTableCell(registrationsTable, $"2", 1);
                        AddTableCell(registrationsTable, $"3", 1, 3);
                        AddTableCell(registrationsTable, Messages.Comments, 4);
                        AddTableCell(registrationsTable, Messages.ResultScore, 3);
                        break;
                }
            }
            else
            {
                switch (_format)
                {
                    case 1:
                    case 2:
                    case 3:
                        AddTableCell(registrationsTable, "", 2, 0, true);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        break;
                    case 4:
                    case 5:
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        break;
                    case 7:
                        AddTableCell(registrationsTable, "", 2, 0, true);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        break;
                    case 11:
                    case 10:
                        AddTableCell(registrationsTable, "", 2, 0, true);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        AddTableCell(registrationsTable, "", 2);
                        break;
                    case 6:
                        AddTableCell(registrationsTable, "", 3, 0, true);

                        AddTableCell(registrationsTable, $"", 1, 4);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1,1);
                        AddTableCell(registrationsTable, $"", 1,2);
                        AddTableCell(registrationsTable, $"", 1);
                        AddTableCell(registrationsTable, $"", 1, 3);
                        AddTableCell(registrationsTable, "", 4);
                        AddTableCell(registrationsTable, "", 3);
                        break;
                }
            }


        }
        public void AddRegistrationsByHeat(IGrouping<string, CompetitionDisciplineRegistration> registrationByHeat, PdfPTable registrationsTable, int? seasonId)
        {
            if (_format == 6) {
                registrationsTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk(" ", tablePlaceWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 13,
                            Border = Rectangle.NO_BORDER
                        }
                );
                AddTableCell(registrationsTable, "  ", 3, 6);
                AddTableCell(registrationsTable, "  ", 3, 5);
                AddTableCell(registrationsTable, "  ", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 5);
                AddTableCell(registrationsTable, "", 3, 7);
                registrationsTable.AddCell(
                        new PdfPCell(new Phrase { new Chunk("", tablePlaceWordFont) })
                        {
                            HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                            VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                            Colspan = 7,
                            Border = Rectangle.NO_BORDER
                        }
                );
            }
            AddPlayerDataByCompetition(registrationsTable, Messages.Rank, Messages.AthleteNumber, Messages.Lane, Messages.FullName, Messages.ClubName, true);
            for (int i = 0; i < registrationByHeat.Count(); i++)
            {
                var registration = registrationByHeat.ElementAt(i);
                var result = registration.CompetitionResult.FirstOrDefault();
                AddPlayerDataByCompetition(registrationsTable, "", registration.User.AthleteNumbers?.FirstOrDefault(x=>x.SeasonId == seasonId)?.AthleteNumber1.ToString() ?? string.Empty, result?.Lane?.ToString() ?? "", registration.User.FullName, registration.Club.Name, registrationByHeat.Count() == i + 1 ? true : false);
            }
        }

        bool firstRun = true;
        public void AddHeatRegistration(IGrouping<string, CompetitionDisciplineRegistration> registrationByHeat, League league, string disciplineName, bool isLast)
        {
            var headerTable = FormTitle(league.Name, league.LeagueStartDate.HasValue ? league.LeagueStartDate.Value.ToShortDateString() : "", league.PlaceOfCompetition, disciplineName, _gender, _ages, league.Union.Logo);
            writer.PageEvent = new PDFFooter(header, headerTable);
            if (firstRun)
            {
                writer.CloseStream = false;
                _document.Open();
                firstRun = false;

            }
            AddNewRow();

            var heatName = Messages.Without;
            if (!isLast || registrationByHeat.Key != league.Union.Name)
            {
                heatName = registrationByHeat.Key;
            }

            AddNewRow();
            int cols = 0;
            PdfPTable registrationsTable = null;
            switch (_format)
            {
                case 1:
                case 2:
                case 3:
                    registrationsTable = new PdfPTable(24)
                    {
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        WidthPercentage = 100,
                        PaddingTop = 10,
                        HeaderRows = 2
                    };
                    registrationsTable.AddCell(
                        new PdfPCell(new Phrase { !string.IsNullOrWhiteSpace(heatName) ? new Chunk($"{Messages.Heat}: {heatName}", titleWordFont) : new Chunk(" ", titleWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                            HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                            Colspan = 24
                        });
                    AddRegistrationsByHeat(registrationByHeat, registrationsTable, league?.SeasonId);
                    cols = 24;
                    break;
                case 4:
                case 5:
                    registrationsTable = new PdfPTable(17)
                    {
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        WidthPercentage = 100,
                        PaddingTop = 10,
                        HeaderRows = 2
                    };
                    registrationsTable.AddCell(
                        new PdfPCell(new Phrase { !string.IsNullOrWhiteSpace(heatName) ? new Chunk($"{Messages.Heat}: {heatName}", titleWordFont) : new Chunk(" ", titleWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                            HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                            Colspan = 17
                        });
                    AddRegistrationsByHeat(registrationByHeat, registrationsTable, league?.SeasonId);
                    cols = 17;
                    break;
                case 7:
                    registrationsTable = new PdfPTable(30)
                    {
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        WidthPercentage = 100,
                        PaddingTop = 10,
                        HeaderRows = 2
                    };
                    registrationsTable.AddCell(
                        new PdfPCell(new Phrase { !string.IsNullOrWhiteSpace(heatName) ? new Chunk($"{Messages.Heat}: {heatName}", titleWordFont) : new Chunk(" ", titleWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                            HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                            Colspan = 30
                        });
                    AddRegistrationsByHeat(registrationByHeat, registrationsTable, league?.SeasonId);
                    cols = 30;
                    break;
                case 11:
                case 10:
                    registrationsTable = new PdfPTable(44)
                    {
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        WidthPercentage = 100,
                        PaddingTop = 10,
                        HeaderRows = 2
                    };
                    registrationsTable.AddCell(
                        new PdfPCell(new Phrase { !string.IsNullOrWhiteSpace(heatName) ? new Chunk($"{Messages.Heat}: {heatName}", titleWordFont) : new Chunk(" ", titleWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                            HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                            Colspan = 44
                        });
                    AddRegistrationsByHeat(registrationByHeat, registrationsTable, league?.SeasonId);
                    cols = 44;
                    break;
                case 6:
                    registrationsTable = new PdfPTable(50)
                    {
                        RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                        WidthPercentage = 100,
                        PaddingTop = 10,
                        HeaderRows = 3
                    };
                    registrationsTable.AddCell(
                        new PdfPCell(new Phrase { !string.IsNullOrWhiteSpace(heatName) ? new Chunk($"{Messages.Heat}: {heatName}", titleWordFont) : new Chunk(" ", titleWordFont) })
                        {
                            Border = Rectangle.NO_BORDER,
                            RunDirection = _isHebrew ? PdfWriter.RUN_DIRECTION_RTL : PdfWriter.RUN_DIRECTION_LTR,
                            HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                            Colspan = 50
                        });
                    AddRegistrationsByHeat(registrationByHeat, registrationsTable, league?.SeasonId);
                    cols = 50;
                    break;
            }
            // fake invisible rows added to prevent overlapping with footer
            registrationsTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($" ", heatWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = cols
            });
            registrationsTable.AddCell(
            new PdfPCell(new Phrase { new Chunk($" ", heatWordFont) })
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                Colspan = cols
            });
            if (_format == 7)
            {
                registrationsTable.AddCell(
                new PdfPCell(new Phrase { new Chunk($" ", heatWordFont) })
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                    Colspan = cols
                });
            }
            _document.Add(registrationsTable);
            if(!isLast)
                _document.NewPage();
        }

        private void AddTableCellToBottom(PdfPTable table, string content, int cols, bool isBorderHidden = false)
        {
            var cell = new PdfPCell(new Phrase { new Chunk($"{content}", footerWordFont) })
            {
                HorizontalAlignment = PdfPCell.ALIGN_LEFT,
                VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                //Border = Rectangle.NO_BORDER,
                Colspan = cols
            };
            if (isBorderHidden)
            {
                cell.Border = Rectangle.NO_BORDER;
            }
            table.AddCell(cell);
        }

        private void addFooter() {
            PdfPTable tabFot = new PdfPTable(4)
            {
                RunDirection = PdfWriter.RUN_DIRECTION_RTL,
                WidthPercentage = 100,
                PaddingTop = 0
            };
            tabFot.TotalWidth = _document.PageSize.Width - 2 * _document.Left;
            AddTableCellToBottom(tabFot, $"{Messages.MainReferee} - {Messages.Signature}", 1);
            AddTableCellToBottom(tabFot, Messages.IdentNum, 1);
            AddTableCellToBottom(tabFot, $"{Messages.Referees} - {Messages.Signature}", 1);
            AddTableCellToBottom(tabFot, Messages.IdentNum, 1);
            AddTableCellToBottom(tabFot, " ", 1);

            AddTableCellToBottom(tabFot, " ", 1);
            AddTableCellToBottom(tabFot, " ", 1);
            AddTableCellToBottom(tabFot, " ", 1);
            if (_format == 7)
            {
                AddTableCellToBottom(tabFot, $"{Messages.WeightApproval}: ", 4, true);
                AddTableCellToBottom(tabFot, $"{Messages.IdentNum}", 4, true);
            }
            var footerHeight = _format == 7 ? 80 : 40;
            tabFot.WriteSelectedRows(0, -1, _document.Left, _document.Bottom + footerHeight, writer.DirectContent);
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
            _document.Add(new Paragraph(" ",tablePlaceWordFont10));
        }

        private void AddTableCell(PdfPTable table, string content, int cols, int mode = 0, bool isGreyBg = false)
        {
            var cell = new PdfPCell(new Phrase { new Chunk($"{content}", tableWordFont) })
            {
                HorizontalAlignment = PdfPCell.ALIGN_CENTER,
                VerticalAlignment = PdfPCell.ALIGN_MIDDLE,
                Colspan = cols
            };
            if (mode == 0)
            {
                cell.FixedHeight = cellHeight;
                cell.BorderWidthBottom = 0.5f;
            }
            if (mode == 1)
            {
                cell.BorderWidthLeft = 1f;
                cell.FixedHeight = cellHeight;
            }
            if (mode == 4)
            {
                cell.BorderWidthRight = 2f;
                cell.BorderWidthBottom = 0.5f;
            }
            if (mode == 3)
            {
                cell.BorderWidthLeft = 2f;
                cell.BorderWidthBottom = 0.5f;
            }
            if (mode == 2)
            {
                cell.BorderWidthRight = 1f;
                cell.FixedHeight = cellHeight;
                cell.BorderWidthBottom = 0.5f;
            }
            if (mode == 5)
            {
                cell.FixedHeight = cellHeight;
                cell.BorderWidthRight = 1f;
                cell.BorderWidthLeft = 1f;
            }
            if (mode == 6)
            {
                cell.FixedHeight = cellHeight;
                cell.BorderWidthRight = 2f;
                cell.BorderWidthLeft = 1f;
                cell.BorderWidthBottom = 0.5f;
            }
            if (mode == 7)
            {
                cell.FixedHeight = cellHeight;
                cell.BorderWidthRight = 1f;
                cell.BorderWidthLeft = 2f;
                cell.BorderWidthBottom = 0.5f;
            }
            if (isGreyBg)
            {
                cell.BackgroundColor = new iTextSharp.text.BaseColor(230, 230, 230);
            }
            table.AddCell(cell);
        }
    }
}