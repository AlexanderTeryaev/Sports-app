using AppModel;
using System;

namespace CmsApp.Models.Mappers
{
    public static class BenefitMapper
    {

        public static Benefit ToBenefit(this BenefitForm bf)
        {
            return new Benefit
            {
                BenefitId = bf.BenefitId,
                IsPublished = bf.IsPublished,
                Company = bf.Company,
                Title = bf.Title,
                Description = bf.Description,
                UnionId = bf.UnionId,
                SeasonId = bf.SeasonId,
                Code = bf.Code
            };
        }

        public static BenefitForm ToBenefitForm(this Benefit bn)
        {
            return new BenefitForm
            {
                BenefitId = bn.BenefitId,
                IsPublished = bn.IsPublished,
                SeasonId = bn.SeasonId,
                Description = bn.Description,
                Company = bn.Company,
                Title = bn.Title,
                Code = bn.Code
            };
        } 
    }
}