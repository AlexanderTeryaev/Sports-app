using HtmlAgilityPack;

namespace DataService.Utils
{
    public static class ScrapperHelper
    {
        /// <summary>
        /// Return text from link in specific html node
        /// </summary>
        /// <param name="nodeCollection"></param>
        /// <param name="index"></param>
        /// <param name="xPathArg">by default it will take from A tag</param>
        /// <returns></returns>
        public static string GetLinkTextFromHtmlNode(this HtmlNodeCollection nodeCollection, int index, string xPathArg = ".//a")
        {
            return nodeCollection[index]?.SelectSingleNode(xPathArg).InnerText.Trim();
        }

        /// <summary>
        /// Get text from specific node
        /// </summary>
        /// <param name="nodeCollection"></param>
        /// <param name="index">index of node</param>
        /// <returns></returns>
        public static string GetTextFromHtmlNode(this HtmlNodeCollection nodeCollection, int index)
        {
            return nodeCollection[index]?.InnerText.Trim();
        }

        public static string GetTeam1Score(this HtmlNodeCollection nodeCollection, int index)
        {
            var scores = nodeCollection[index].SelectNodes(".//a");
            if (scores.Count != 2)
            {
                return "0";
            }
            return scores[0].InnerText.Trim();
        }

        public static string GetTeam2Score(this HtmlNodeCollection nodeCollection, int index)
        {
            var scores = nodeCollection[index].SelectNodes(".//a");
            if (scores.Count != 2)
            {
                return "0";
            }
            return scores[1].InnerText.Trim();
        }
    }
}
