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
using System.Text;
using System.Text.RegularExpressions;

namespace CmsApp.Helpers
{
    public class StartListCSVHelper
    {
        private readonly bool _isHebrew;
        TextWriter textWriter;
        MemoryStream memoryStream;
        private bool isNewLine = true;
        public StartListCSVHelper(List<CompetitionDisciplineCSVDto> registrationsByDiscipline, bool isHebrew, int? seasonId)
        {
            _isHebrew = isHebrew;
            memoryStream = new MemoryStream();
            textWriter = new StreamWriter(memoryStream, Encoding.GetEncoding("Windows-1255"));
            addColumnsLine();
            if (registrationsByDiscipline != null) {
                foreach (var registrationsByHeatList in registrationsByDiscipline)
                {
                    AddHeatRegistrationList(registrationsByHeatList, seasonId);
                }
            }

        }

        public void addNewCell(string content)
        {
            if (!isNewLine) {
                content = ";" + content;
            }
            textWriter.Write(content);
            isNewLine = false;
        }


        public void addNewRow()
        {
            textWriter.WriteLine("");
            isNewLine = true;
        }

        public void addColumnsLine()
        {
            addNewCell("Code");
            addNewCell("Date");
            addNewCell("Time");
            addNewCell("Lane/order");
            addNewCell("Bib No");
            addNewCell("Last Name");
            addNewCell("Name");
            addNewCell("Nation");
            addNewCell("Length");
            addNewCell("Title 1");
            addNewCell("Title 2");
            addNewCell("Sponsor");
            addNewCell("Record 1");
            addNewCell("Record 2");
            addNewCell("Record Code 1");
            addNewCell("Record Code 2");
            addNewRow();
        }


        public void addNewLine(string content)
        {
            textWriter.WriteLine(content);
        }

        public byte[] GetDocumentBytes()
        {
            textWriter.Flush();
            textWriter.Close();
            byte[] bytes = memoryStream.ToArray();
            bytes = bytes.ToArray();
            memoryStream.Close();
            return bytes;
        }

        public Stream GetDocumentStream()
        {
            return memoryStream;
        }

        public void AddRegistrationsByHeat(List<CompetitionDisciplineRegistration> registrationByHeat, int? seasonId)
        {
            for (int i = 0; i < registrationByHeat.Count(); i++)
            {
                var registration = registrationByHeat.ElementAt(i);
                var result = registration.CompetitionResult.FirstOrDefault();
                addNewCell("");
                addNewCell("");
                addNewCell("");
                addNewCell(result.Lane.ToString());
                addNewCell(registration.User.AthleteNumbers.FirstOrDefault(x=>x.SeasonId == seasonId)?.AthleteNumber1?.ToString() ?? string.Empty);

                
                var clubName = registration.Club.Name;
                clubName = clubName.Substring(0, Math.Min(clubName.Length, 5));
                if(!string.IsNullOrWhiteSpace(registration.Club.ClubDisplayName))
                {
                    clubName = registration.Club.ClubDisplayName;
                }
                var fullName = registration.User.FullName;
                fullName = fullName.Substring(0, Math.Min(fullName.Length, 15));

                addNewCell(fullName);
                addNewCell("");
                addNewCell(clubName);
                addNewCell("");
                addNewRow();
            }
        }

        public void AddHeatRegistrationList(CompetitionDisciplineCSVDto registrationByHeat, int? seasonId)
        {

            AddHeatRegistration(registrationByHeat.Registrations, registrationByHeat.HeatName, registrationByHeat.CompetitionDesciplineId, registrationByHeat.DisciplineDate, registrationByHeat.DisciplineTime, registrationByHeat.DisciplineLength, registrationByHeat.DisciplineName, registrationByHeat.CategoryName, seasonId);         
        }

        public void AddHeatRegistration(List<CompetitionDisciplineRegistration> registrationByHeat, string heatName, string compId, string date, string time, string length, string disciplineName, string categoryName, int? seasonId)
        {
            AddHeatTitle(compId, heatName, date, time, length, disciplineName, categoryName);
            AddRegistrationsByHeat(registrationByHeat, seasonId);
        }

        string RemoveBetween(string s, char begin, char end)
        {
            Regex regex = new Regex(string.Format("\\{0}.*?\\{1}", begin, end));
            var str = regex.Replace(s, string.Empty);
            str = str.Replace("  "," ");
            str = str.TrimEnd();
            return str;
        }

        private void AddHeatTitle(string compId,string heat, string date, string time, string length, string disciplineName, string categoryName)
        {
            //char t = 0xce;
            addNewCell(compId+heat);
            addNewCell(date);
            addNewCell(time);
            addNewCell("");
            addNewCell("");
            addNewCell("");
            addNewCell("");
            addNewCell("");
            addNewCell(length);
            addNewCell("־");
            //textWriter.Write(disciplineName + " " + categoryName + " ");
            textWriter.Write(RemoveBetween(disciplineName, '(', ')') + " " + categoryName + " ");
            textWriter.Write(heat);
            addNewRow();
        }



    }
}