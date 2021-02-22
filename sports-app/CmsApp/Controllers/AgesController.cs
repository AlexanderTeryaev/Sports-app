using AutoMapper;
using CmsApp.Models;
using DataService.DTO;
using Resources;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CmsApp.Controllers
{
    public class AgesController : AdminController
    {
        public ActionResult List()
        {
            var ages = agesRepo.GetAll();
            Mapper.Initialize(cfg => cfg.CreateMap<AgeDto, AgeViewModel>());
            var model = Mapper.Map<IEnumerable<AgeDto>, IEnumerable<AgeViewModel>>(ages);
            return PartialView("_List", model);
        }

        [HttpPost]
        public ActionResult Create(AgeViewModel age)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<AgeViewModel, AgeDto>());
            var ageDto = Mapper.Map<AgeViewModel, AgeDto>(age);
            agesRepo.Create(ageDto);
            agesRepo.Save();
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public void Update(AgeViewModel age)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<AgeViewModel, AgeDto>());
            var ageDto = Mapper.Map<AgeViewModel, AgeDto>(age);
            agesRepo.Update(ageDto);
            agesRepo.Save();
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                agesRepo.Delete(id);
                agesRepo.Save();
                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                var leaguesRelated = secRepo.GetRelatedLeaguesString(id);
                if (!string.IsNullOrEmpty(leaguesRelated))
                    message = Messages.ErrorDeleteAge.Replace("[leagues]", leaguesRelated);

                return Json(new { Success = false, Message = message });
            }
        }
    }
}