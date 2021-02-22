using System;
using System.IO;
using System.Configuration;

public static class GlobVars
{
    public const int UkraineGymnasticUnionId = 52;

    public static readonly int GridItems = 15;

    private static string GetValue(string name)
    {
        return ConfigurationManager.AppSettings[name];
    }

    public static int ActivityStatusTruncateThreshold
    {
        get
        {
            var value = GetValue("ActivityStatusTruncateThreshold");

            int threshold;
            return int.TryParse(value, out threshold) ? threshold : 100;
        }
    }

    public static string ContentPath
    {
        get { return GetValue("ContentPath"); }
    }

    public static string SiteUrl
    {
        get {
            string envFront = Environment.GetEnvironmentVariable("LOGLIG_DEVELOPMENT_FRONT_URL", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrWhiteSpace(envFront))
            {
                return envFront;
            }
            return GetValue("SiteUrl");
        }
    }

    public static string[] ValidImages
    {
        get { return GetValue("ValidImages").Split('|'); }
    }

    public static int MaxFileSize
    {
        get { return int.Parse(GetValue("MaxFileSize")); }
    }

    public static bool IsTest
    {
        get { return bool.Parse(GetValue("IsTestEvironment")); }
    }

    public static string PdfRoute
    {
        get { return GetValue("PdfRoute"); }
    }

    public static string PdfUrl
    {
        get { return GetValue("PdfUrl"); }
    }

    public static string ClubContentPath
    {
        get { return Path.Combine(ContentPath, "Clubs/"); }
    }
    public static string TeamContentPath
    {
        get { return Path.Combine(ContentPath, "Teams/"); }
    }
    public static string RegionalContentPath
    {
        get { return Path.Combine(ContentPath, "Regionals/"); }
    }

    public static string EventContentPath
    {
        get { return Path.Combine(ContentPath, "Events/"); }
    }

    public static string BenefitContentPath
    {
        get { return Path.Combine(ContentPath, "Benefits/"); }
    }
}