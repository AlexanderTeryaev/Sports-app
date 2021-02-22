using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web.Hosting;
using DataService.DTO;
using DataService.Utils;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;

namespace DataService.Services
{
    public class ScrapperService : IDisposable
    {
        #region Fields & constructor
        private readonly IWebDriver _driver;
        private string Url { get; set; }

        public ScrapperService(string url)
        {
            if (_driver == null)
                _driver = new PhantomJSDriver(PhantomJSDriverService.CreateDefaultService(HostingEnvironment.MapPath("/")));

            _driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));

            Url = url;
        }

        public ScrapperService()
        {
            ProcessHelper.StartProcessIfNotStarted();
            if (_driver == null)
                _driver = new PhantomJSDriver();
            //_driver = new PhantomJSDriver(PhantomJSDriverService.CreateDefaultService(HostingEnvironment.MapPath("/")));

            _driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 40));
        }
        #endregion

        public List<SchedulerDTO> SchedulerScraper(string url)
        {
            var doc = GetDocumentAndClick(url);

            var model = new List<SchedulerDTO>();

            ReadShceduleScrapperFromHTML(doc, model, url);
            //var tableRows = doc.DocumentNode.SelectSingleNode("//table[@id='mbt-v2-team-schedule-and-results-tab']")?.SelectNodes(".//tbody//tr");
            //if (tableRows != null)
            //{
            //    foreach (var tableRow in tableRows)
            //    {
            //        var tableCell = tableRow.SelectNodes(".//td");

            //        try
            //        {
            //            model.Add(new SchedulerDTO
            //            {
            //                Time = tableCell.GetLinkTextFromHtmlNode(0),
            //                HomeTeam = tableCell.GetLinkTextFromHtmlNode(1),
            //                HomeTeamScore = tableCell.GetTeam1Score(2),
            //                GuestTeamScore = tableCell.GetTeam2Score(2),
            //                GuestTeam = tableCell.GetLinkTextFromHtmlNode(3),
            //                Auditorium = tableCell.GetTextFromHtmlNode(4),
            //                Url = url

            //            });
            //        }
            //        catch (Exception ex)
            //        {

            //            Trace.WriteLine(ex.Message);
            //        }
                   
            //    }
              
                
            //}

            var nextPage = NextPage();
            do
            {
                if (nextPage != null)
                {
                    ReadShceduleScrapperFromHTML(nextPage, model, url);
                    //tableRows = nextPage.DocumentNode.SelectSingleNode("//table[@id='mbt-v2-team-schedule-and-results-tab']")?.SelectNodes(".//tbody//tr");
                    //if (tableRows != null)
                    //{
                    //    foreach (var row in tableRows)
                    //    {
                    //        var tableCell = row.SelectNodes(".//td");
                    //        try
                    //        {
                    //            model.Add(new SchedulerDTO
                    //            {
                    //                Time = tableCell.GetLinkTextFromHtmlNode(0),
                    //                HomeTeam = tableCell.GetLinkTextFromHtmlNode(1),
                    //                HomeTeamScore = tableCell.GetTeam1Score(2),
                    //                GuestTeamScore = tableCell.GetTeam2Score(2),
                    //                GuestTeam = tableCell.GetLinkTextFromHtmlNode(3),
                    //                Auditorium = tableCell.GetTextFromHtmlNode(4),
                    //                Url = url

                    //            });
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            Trace.WriteLine(ex.Message);
                    //        }
                    //    }
                    //}
                    nextPage = NextPage();
                }
            } while (nextPage != null);

            return model;

        }

        public List<SchedulerDTO> FootballSchedulerScraper(string url)
        {
            var model = new List<SchedulerDTO>();
            GetFootballDocumentAndClick(url, model);
            //ReadFootballShceduleScrapperFromHTML(doc, model, url);

            return model;

        }

        public void ReadShceduleScrapperFromHTML(HtmlDocument doc, List<SchedulerDTO> model, string url)
        {
            var tableRows = doc.DocumentNode.SelectSingleNode("//table[@id='mbt-v2-team-schedule-and-results-tab']")?.SelectNodes(".//tbody//tr");
            if (tableRows != null)
            {
                foreach (var tableRow in tableRows)
                {
                    var tableCell = tableRow.SelectNodes(".//td");
                    try
                    {
                        model.Add(new SchedulerDTO
                        {
                            Time = tableCell.GetLinkTextFromHtmlNode(0),
                            HomeTeam = tableCell.GetLinkTextFromHtmlNode(1),
                            HomeTeamScore = tableCell.GetTeam1Score(2),
                            GuestTeamScore = tableCell.GetTeam2Score(2),
                            GuestTeam = tableCell.GetLinkTextFromHtmlNode(3),
                            Auditorium = tableCell.GetTextFromHtmlNode(4),
                            Url = url

                        });
                    }
                    catch (Exception ex)
                    {
                        
                       Trace.WriteLine(ex.Message);
                    }
                }
            }
        }


        public void ReadFootballShceduleScrapperFromHTML(HtmlDocument doc, List<SchedulerDTO> model, string url)
        {
            var tableRows = doc.DocumentNode.SelectNodes(".//a");
            if (tableRows != null)
            {
                foreach (var tableRow in tableRows)
                {
                    var tableCell = tableRow.SelectNodes(".//div");
                    Console.Out.Write(tableCell);
                }
            }
        }

        public List<StandingDTO> StandingScraper(string url)
        {
            var doc = GetHtmlDocument(url);

            var model = new List<StandingDTO>();

            var tableRows = doc.DocumentNode.SelectNodes(".//table//tbody//tr");
            if (tableRows != null)
            {
                int counter = tableRows.Count;

                for (int i = 0; i < counter; i++)
                {
                    var tableCells = tableRows[i].SelectNodes(".//td");

                    try
                    {
                        if (tableCells.Count == 20)
                        {
                            model.Add(new StandingDTO
                            {
                                Rank = tableCells.GetTextFromHtmlNode(0),
                                Team = tableCells.GetLinkTextFromHtmlNode(1),
                                Games = tableCells.GetTextFromHtmlNode(2),
                                Win = tableCells.GetTextFromHtmlNode(3),
                                Lost = tableCells.GetTextFromHtmlNode(4),
                                Pts = tableCells.GetTextFromHtmlNode(6),
                                PaPf = tableCells.GetTextFromHtmlNode(7),
                                PlusMinusField = tableCells.GetTextFromHtmlNode(8),
                                Home = tableCells.GetTextFromHtmlNode(9),
                                Road = tableCells.GetTextFromHtmlNode(10),
                                ScoreHome = tableCells.GetTextFromHtmlNode(11),
                                ScoreRoad = tableCells.GetTextFromHtmlNode(12),
                                Last5 = tableCells.GetTextFromHtmlNode(13),
                                Url = url
                            });
                        }
                        else
                        {
                            model.Add(new StandingDTO
                            {
                                Rank = tableCells.GetTextFromHtmlNode(0),
                                Team = tableCells.GetLinkTextFromHtmlNode(1),
                                Games = tableCells.GetTextFromHtmlNode(2),
                                Win = tableCells.GetTextFromHtmlNode(3),
                                Lost = tableCells.GetTextFromHtmlNode(4),
                                Pts = tableCells.GetTextFromHtmlNode(5),
                                PaPf = tableCells.GetTextFromHtmlNode(6),
                                PlusMinusField = tableCells.GetTextFromHtmlNode(7),
                                Home = tableCells.GetTextFromHtmlNode(8),
                                Road = tableCells.GetTextFromHtmlNode(9),
                                ScoreHome = tableCells.GetTextFromHtmlNode(10),
                                ScoreRoad = tableCells.GetTextFromHtmlNode(11),
                                Last5 = tableCells.GetTextFromHtmlNode(12),
                                Url = url
                            });
                        }
                    }
                    catch (Exception e)
                    {

                        Trace.WriteLine(e.Message);
                    }
                }
            }

            return model;
        }

        public List<StandingDTO> SoccerStandingScraper(string url)
        {
            var model = new List<StandingDTO>();

            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var sectionWithStandings = _driver.FindElement(By.ClassName("playoff-container"));

            var standings = sectionWithStandings.FindElements(By.TagName("a"));

            for (int i = 0; i < standings.Count; i++)
            {
                var standing = standings[i];
                var items = standing.FindElements(By.TagName("div"));

                string[] stringSeparators = new string[] { "\r\n" };

                var Rank = items[0].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var TeamName = items[1].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Games = items[2].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Wins = items[3].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Ties = items[4].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Loss = items[5].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Goals = items[6].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var Points = items[7].Text.Split(stringSeparators, StringSplitOptions.None)[1];

                string[] goals = Goals.Split('-');
                int goalsScorred;
                int goalsConceeded;
                int Diff = 0;
                if (int.TryParse(goals[1], out goalsScorred) && int.TryParse(goals[0], out goalsConceeded))
                {
                    Diff = goalsScorred - goalsConceeded;
                }

                var Url = url;

                model.Add(new StandingDTO
                {
                    Rank = Rank,
                    Team = TeamName,
                    Games = Games,
                    Win = Wins,
                    Lost = Loss,
                    Pts = Points,
                    PaPf = Goals,
                    PlusMinusField = "",
                    Home = Ties,
                    Road = Diff.ToString(),
                    ScoreHome = "",
                    ScoreRoad = "",
                    Last5 = "",
                    Url = url
                });

            }

            return model;
        }

        public void Quit()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }

        private HtmlDocument GetHtmlDocument()
        {
            _driver.Navigate().GoToUrl(Url);
            var doc = new HtmlDocument();
            doc.LoadHtml(_driver.PageSource);
            return doc;
        }

        private HtmlDocument GetHtmlDocument(string url)
        {
            _driver.Navigate().GoToUrl(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(_driver.PageSource);
            return doc;
        }

        private HtmlDocument GetDocumentAndClick(string url)
        {
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            

            var divToClick = _driver.FindElement(By.ClassName("mbt-v2-navigation")).FindElements(By.TagName("div"))[2];

            var el = wait.Until(ExpectedConditions.ElementToBeClickable(divToClick));

            el.Click();

            var divsWithSelect = _driver.FindElement(By.ClassName("mbt-v2-filters-block"));

            var select = divsWithSelect.FindElements(By.TagName("select"))[1];

            var selectElement = new SelectElement(select);
            selectElement.SelectByValue("all");

            wait.Until(x => x.FindElement(By.Id("mbt-v2-team-schedule-and-results-tab")));

            
            
            
            var doc = new HtmlDocument();

            doc.LoadHtml(_driver.PageSource);
            return doc;
        }

        private void GetFootballDocumentAndClick(string url, List<SchedulerDTO> model)
        {
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(url);

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            var divsWithSchedules = _driver.FindElement(By.ClassName("table_row_group"));

            var schedules = divsWithSchedules.FindElements(By.TagName("a"));

            for(int i = 0; i < schedules.Count; i++)
            {
                var schedule = schedules[i];
                var items = schedule.FindElements(By.TagName("div"));

                string[] stringSeparators = new string[] { "\r\n" };
                var Time = items[0].Text.Split(stringSeparators, StringSplitOptions.None)[1] + " " + items[3].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                var HomeTeam = items[1].Text.Split(stringSeparators, StringSplitOptions.None)[1].Split('-')[0];
                var HomeTeamScore = "0";
                var GuestTeamScore = "0";
                if(items[4].Text.Split(stringSeparators, StringSplitOptions.None)[1].Split('-').Length >= 2)
                {
                    HomeTeamScore = items[4].Text.Split(stringSeparators, StringSplitOptions.None)[1].Split('-')[1];
                    GuestTeamScore = items[4].Text.Split(stringSeparators, StringSplitOptions.None)[1].Split('-')[0];
                }
                    
                var GuestTeam = items[1].Text.Split(stringSeparators, StringSplitOptions.None)[1].Split('-')[1];
                var Auditorium = "";
                if (items[2].Text.Split(stringSeparators, StringSplitOptions.None).Length >= 2)
                {
                    Auditorium = items[2].Text.Split(stringSeparators, StringSplitOptions.None)[1];
                }
                var Url = url;

                model.Add(new SchedulerDTO
                {
                    Time = Time,
                    HomeTeam = HomeTeam,
                    HomeTeamScore = HomeTeamScore,
                    GuestTeamScore = GuestTeamScore,
                    GuestTeam = GuestTeam,
                    Auditorium = Auditorium,
                    Url = url

                });

            }

        }

        private HtmlDocument NextPage()
        {
            //if no next button presented stop search
            if (_driver.FindElements(By.ClassName("mbt-v2-pagination-next")).Count == 0)
                return null;

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            var navElement = _driver.FindElement(By.ClassName("mbt-v2-pagination"));
            if (navElement != null)
            {
                var li = navElement.FindElements(By.TagName("li")).LastOrDefault();
                if (li != null)
                {
                    li.FindElement(By.TagName("a")).Click();
                    wait.Until(x => x.FindElement(By.Id("mbt-v2-team-schedule-and-results-tab")));

                    //var nextPageTable = _driver.FindElement(By.Id("mbt-v2-team-schedule-and-results-tab"));
                    var doc = new HtmlDocument();
                    doc.LoadHtml(_driver.PageSource);
                    return doc;
                }
            }
            return null;
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }


    }
}
