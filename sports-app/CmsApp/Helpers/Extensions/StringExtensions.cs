using System;

namespace CmsApp.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ReplaceHostToNgrok(this string url, string ngrokHost)
        {
            var builder = new UriBuilder(url) { Host = ngrokHost, Port = 80 };
            return builder.Uri.ToString();
        }
    }
}