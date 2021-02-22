namespace CmsApp.Models
{
    public class OfficialsSettingsModel
    {
        public decimal? RefereeFeePerGame { get; set; }
        public CurrencyUnits RefereePaymentCurrencyUnits { get; set; }
        public decimal? RefereePaymentForTravel { get; set; }
        public MetricUnits RefereeTravelMetricUnits { get; set; }
        public CurrencyUnits RefereeTravelCurrencyUnits { get; set; }

        public decimal? SpectatorFeePerGame { get; set; }
        public CurrencyUnits SpectatorPaymentCurrencyUnits { get; set; }
        public decimal? SpectatorPaymentForTravel { get; set; }
        public MetricUnits SpectatorTravelMetricUnits { get; set; }
        public CurrencyUnits SpectatorTravelCurrencyUnits { get; set; }

        public decimal? DeskFeePerGame { get; set; }
        public CurrencyUnits DeskPaymentCurrencyUnits { get; set; }
        public decimal? DeskPaymentForTravel { get; set; }
        public MetricUnits DeskTravelMetricUnits { get; set; }
        public CurrencyUnits DeskTravelCurrencyUnits { get; set; }
        public bool IsOfficialsFeatureEnabled { get; internal set; }
        public string Section { get; set; }

        public decimal? RateAPerGame { get; set; }
        public decimal? RateBPerGame { get; set; }
        public decimal? RateCPerGame { get; set; }
        public decimal? RateAForTravel { get; set; }
        public decimal? RateBForTravel { get; set; }
        public decimal? RateCForTravel { get; set; }
    }
}