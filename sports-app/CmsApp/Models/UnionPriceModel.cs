using System;

namespace CmsApp.Models
{
    public class UnionPriceModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public DateTime? FromBirthday { get; set; }
        public DateTime? ToBirthday { get; set; }
        public string CardComProductId { get; set; }

    }

    public enum UnionPriceType
    {
        UnionToClubCompetingRegistrationPrice = 0,
        UnionToClubRegularRegistrationPrice,
        UnionToClubInsurancePrice,
        UnionToClubTenicardPrice,
    }
}