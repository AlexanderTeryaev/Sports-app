using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Resources;
using AppModel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService;
using Omu.ValueInjecter;


namespace CmsApp.Controllers
{
    [AppAuthorize(Roles = AppRole.Admins)]
    public class UsersController : AdminController
    {
        UsersRepo uRepo = new UsersRepo();
        LeagueRepo leagRepo = new LeagueRepo();
        //
        // GET: /Users/

        public ActionResult Index(int? gridNum)
        {
            ViewBag.PageSize = gridNum ?? GlobVars.GridItems;

            ViewBag.GridNum = AppFunc.GetGridItemsCombo(gridNum);

            var query = uRepo.GetQuery();

            return View(query.OrderByDescending(t => t.UserId));
        }

        [HttpGet]
        public ActionResult PasswordRecovery()
        {
            return View(new PasswordRecoveryViewModel());
        }

        [HttpPost]
        public ActionResult PasswordRecovery(PasswordRecoveryViewModel vm)
        {
            User usr = uRepo.GetByUsername(vm.UserName.Trim());
            if (usr == null)
            {
                ModelState.AddModelError("UserName", "המשתמש לא נמצא במערכת");
                return View(vm);
            }
            else
            {
                vm.Password = Protector.Decrypt(usr.Password);
            }
            return View(vm);
        }


        [NoCache]
        public ActionResult Edit(int id = 0)
        {
            var vm = new UserForm();
            vm.RolesList = new SelectList(uRepo.GetTypes(), "TypeId", "TypeName");

            if (id != 0)
            {
                var user = uRepo.GetById(id);
                if (!string.IsNullOrEmpty(user.Password))
                    user.Password = Protector.Decrypt(user.Password);

                vm.InjectFrom(user);
            }

            return PartialView("_Edit", vm);
        }

        [HttpPost]
        public ActionResult Edit(UserForm frm)
        {
            bool isExists = uRepo.IsUsernameExists(frm.UserName, frm.UserId);
            if (isExists)
            {
                ModelState.AddModelError("UserName", Messages.UserNameExists);
            }

            if (!ModelState.IsValid)
            {
                frm.RolesList = new SelectList(uRepo.GetTypes(), "TypeId", "TypeName");
                return PartialView("_Edit", frm);
            }

            var user = new User();

            if (frm.UserId != 0)
            {
                user = uRepo.GetById(frm.UserId);
                if (frm.IsBlocked)
                {
                    LoginService.RemoveSession(user.SessionId);
                }
            }
            else
            {
                user.IsActive = true;
                uRepo.Create(user);
            }

            UpdateModel(user);

            user.Password = Protector.Encrypt(frm.Password);

            uRepo.Save();

            return Content("ok");
        }
    }
}
