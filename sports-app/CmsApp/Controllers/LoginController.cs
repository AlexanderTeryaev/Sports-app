using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CmsApp.Models;
using CmsApp.Helpers;
using DataService;
using System.Threading.Tasks;
using System.Net;
using CmsApp.Services;
using System.Configuration;
using AppModel;
using Resources;

namespace CmsApp.Controllers
{
    public class PlanController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(PlanSendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            var emailService = new EmailService();
            var body = string.Empty;
            using (var reader = new StreamReader(Server.MapPath("~/Views/Plan/EmailTeamplate.cshtml")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Field1}", model.Field1);
            body = body.Replace("{Field2}", model.Field2);
            body = body.Replace("{Field3}", model.Field3);
            body = body.Replace("{Field4}", model.Field4);
            body = body.Replace("{Field5}", model.Field5);
            body = body.Replace("{Field7}", model.Field7);
            body = body.Replace("{Field8}", model.Field8);

            try
            {
                emailService.SendAsync(ConfigurationManager.AppSettings["MailAdminEmailAddress"], body);
            }
            catch (Exception ex)
            {
                TempData["EmailError"] = ex.Message;
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", "Thanks");
        }


        public class PlanSendViewModel
        {
            [Required]
            public string Field1 { get; set; }

            [Required]
            public string Field2 { get; set; }

            [Required]
            public string Field3 { get; set; }

            [Required]
            public string Field4 { get; set; }

            [Required]
            public string Field5 { get; set; }

            [Required]
            public string Field7 { get; set; }


            public string Field8 { get; set; }
        }
    }

    public class ThanksController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

    }

    public class LoginengController : Controller
    {
        public ActionResult Index()
        {
            this.SetCulture(CultEnum.En_US);

            var vm = new LoginForm
            {
                IsSecure = IsCaptchaCookie(),
                Culture = this.GetRequestCulture()
            };

            return View(vm);
        }

        [NoCache]
        public ActionResult Captcha()
        {
            return new Helpers.ImageResult();
        }

        [HttpPost]
        public ActionResult Index(LoginForm frm, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("LgnErr", "נא מלא את פרטי ההתחברות");
                return View(frm);
            }

            var isCaptcha = IsCaptchaCookie();
            if (isCaptcha && !IsValidCaptcha(frm.Captcha))
            {
                ModelState.AddModelError("LgnErr", "קוד אבטחה שגוי");
                ModelState.AddModelError("Captcha", "Err");
                frm.IsSecure = true;

                return View(frm);
            }

            var tries = AnonymousTries();
            if (tries == 1)
            {
                SetCaptchaCookie(true);
            }

            var uRep = new UsersRepo();
            var user = uRep.GetByUsername(frm.UserName.Trim());

            if (user == null)
            {
                user = uRep.GetByIdentityNumber(frm.UserName.Trim());
            }

            if (user == null)
            {
                AnonymousTriesAdd();

                ModelState.AddModelError("LgnErr", "שם משתמש או סיסמה שגויים");
                ModelState.AddModelError("Password", "Err");
                return View(frm);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("LgnErr", "משתמש זה נחסם, יש לפנות להנהלת האתר");
                return View(frm);
            }

            var usrPass = Protector.Decrypt(user.Password);
            if (frm.Password != usrPass)
            {
                ModelState.AddModelError("LgnErr", "שם משתמש או סיסמה שגויים");
                user.TriesNum += 1;

                if (user.TriesNum > 2)
                {
                    frm.IsSecure = true;
                    SetCaptchaCookie(true);
                }

                if (user.TriesNum >= 10)
                {
                    user.TriesNum = 0;
                    user.IsBlocked = true;
                }

                uRep.Save();
                return View(frm);
            }

            SetCaptchaCookie(false);

            var currSession = user.SessionId;

            user.TriesNum = 0;
            user.SessionId = Session.SessionID;
            uRep.Save();

            var userData = user.UsersType.TypeRole + "^" + user.SessionId;
            var userId = user.UserId.ToString();

            LoginService.UpdateSessions(user.SessionId, currSession);

            int expirationTime;
            var envExpirationTime = Environment.GetEnvironmentVariable("LOGLIG_DEVELOPMENT", EnvironmentVariableTarget.Machine);
            var isNum = int.TryParse(envExpirationTime, out expirationTime);
            if (!isNum)
            {
                expirationTime = 8;
            }

            var ticket = new FormsAuthenticationTicket(1, userId,
                DateTime.Now,
                DateTime.Now.AddHours(expirationTime),
                frm.IsRemember,
                userData,
                FormsAuthentication.FormsCookiePath);

            var encTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = ticket.Expiration;
            Response.Cookies.Add(cookie);

            SetAdminCookie(user.FullName, user.UsersType.TypeRole);

            this.SetWorkerSession("");

            this.SetLastUserCulture(user);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Redirect(returnUrl);
            }

            //For Testing
            //var test = uRep.GetById(54);
            //System.Diagnostics.Debug.WriteLine("*" + test.UserName + "*");
            //System.Diagnostics.Debug.WriteLine("*" + Protector.Decrypt(test.Password) + "*");

            //test = uRep.GetById(13);
            //System.Diagnostics.Debug.WriteLine("*" + test.UserName + "*");
            //System.Diagnostics.Debug.WriteLine("*" + Protector.Decrypt(test.Password) + "*");

            return Redirect(FormsAuthentication.DefaultUrl);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            this.SetCulture(CultEnum.En_US);

            return View("Index", new LoginForgotPasswordModel { LoginAction = LoginAction.ForgotPassword });
        }

        private void SetAdminCookie(string name, string role)
        {
            var c = new HttpCookie("cmsdata");
            c.Values.Add("uname", HttpUtility.UrlEncode(name));
            c.Values.Add("utype", HttpUtility.UrlEncode(role));
            c.Expires = DateTime.Now.AddDays(1);

            Response.Cookies.Add(c);
        }

        private void SetCaptchaCookie(bool isAdd)
        {
            var day = isAdd ? 1 : -1;
            var c = new HttpCookie("capterr", "true");
            c.Expires = DateTime.Now.AddDays(day);

            Response.Cookies.Add(c);
        }

        public bool IsCaptchaCookie()
        {
            return Request.Cookies["capterr"] != null;
        }

        private bool IsValidCaptcha(string captcha)
        {
            if (string.IsNullOrWhiteSpace(captcha))
                return false;

            if (TempData["Captcha"] == null)
                return false;

            var captTxt = TempData["Captcha"].ToString();

            return captcha.ToUpper() == captTxt;
        }

        private int AnonymousTries()
        {
            if (Session["lgntries"] == null)
                return 0;

            return (int)Session["lgntries"];
        }

        private void AnonymousTriesAdd()
        {
            var tries = AnonymousTries();

            Session["lgntries"] = ++tries;
        }
    }

    public class TermsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

    }

    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            this.SetCulture(CultEnum.He_IL);

            var vm = new LoginForm
            {
                IsSecure = IsCaptchaCookie(),
                Culture = this.GetRequestCulture()
            };

            return View(vm);
        }

        [NoCache]
        public ActionResult Captcha()
        {
            return new Helpers.ImageResult();
        }

        [HttpPost]
        public ActionResult Index(LoginForm frm, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("LgnErr", "נא מלא את פרטי ההתחברות");
                return View(frm);
            }

            var isCaptcha = IsCaptchaCookie();
            if (isCaptcha && !IsValidCaptcha(frm.Captcha))
            {
                ModelState.AddModelError("LgnErr", "קוד אבטחה שגוי");
                ModelState.AddModelError("Captcha", "Err");
                frm.IsSecure = true;

                return View(frm);
            }

            var tries = AnonymousTries();
            if (tries == 1)
            {
                SetCaptchaCookie(true);
            }

            var uRep = new UsersRepo();
            var user = uRep.GetByIdentityNumber(frm.UserName.Trim());

            if (user == null)
            {
                user = uRep.GetByUsername(frm.UserName.Trim());
            }

            if (user == null)
            {
                AnonymousTriesAdd();

                ModelState.AddModelError("LgnErr", "שם משתמש או סיסמה שגויים");
                ModelState.AddModelError("Password", "Err");
                return View(frm);
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("LgnErr", "משתמש זה נחסם, יש לפנות להנהלת האתר");
                return View(frm);
            }            

            var usrPass = Protector.Decrypt(user.Password);
            if (frm.Password != usrPass)
            {
                ModelState.AddModelError("LgnErr", "שם משתמש או סיסמה שגויים");
                user.TriesNum += 1;

                if (user.TriesNum > 2)
                {
                    frm.IsSecure = true;
                    SetCaptchaCookie(true);
                }

                if (user.TriesNum >= 10)
                {
                    user.TriesNum = 0;
                    user.IsBlocked = true;
                }

                uRep.Save();
                return View(frm);
            }

            var jobIsBlocked = user.UsersJobs?.Any(uj => uj.IsBlocked);

            if (jobIsBlocked == true)
            {
                ModelState.AddModelError("LgnErr", Messages.BlockadeMessage);
                return View(frm);
            }

            SetCaptchaCookie(false);

            var currSession = user.SessionId;

            user.TriesNum = 0;
            user.SessionId = Session.SessionID;
            uRep.Save();

            var userData = new List<string> { user.UsersType.TypeRole, user.SessionId };
            if (user.UsersJobs.Any() && !userData.Contains(AppRole.Workers))
            {
                userData.Add(AppRole.Workers);
            }
            var userId = user.UserId.ToString();

            LoginService.UpdateSessions(user.SessionId, currSession);

            int expirationTime;
            var envExpirationTime = Environment.GetEnvironmentVariable("LOGLIG_DEVELOPMENT", EnvironmentVariableTarget.Machine);
            var isNum = int.TryParse(envExpirationTime, out expirationTime);
            if (!isNum)
            {
                expirationTime = 8;
            }

            var ticket = new FormsAuthenticationTicket(1, userId,
                DateTime.Now,
                DateTime.Now.AddHours(expirationTime),
                frm.IsRemember,
                string.Join("^", userData),
                FormsAuthentication.FormsCookiePath);

            var encTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            cookie.Expires = ticket.Expiration;
            Response.Cookies.Add(cookie);

            SetAdminCookie(user.FullName, user.UsersType.TypeRole);

            this.SetWorkerSession("");

            this.SetLastUserCulture(user);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                Redirect(returnUrl);
            }

            //For Testing
            //var test = uRep.GetById(54);
            //System.Diagnostics.Debug.WriteLine("*" + test.UserName + "*");
            //System.Diagnostics.Debug.WriteLine("*" + Protector.Decrypt(test.Password) + "*");

            //test = uRep.GetById(13);
            //System.Diagnostics.Debug.WriteLine("*" + test.UserName + "*");
            //System.Diagnostics.Debug.WriteLine("*" + Protector.Decrypt(test.Password) + "*");

            return Redirect(FormsAuthentication.DefaultUrl);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            this.SetCulture(CultEnum.He_IL);

            return View("Index", new LoginForgotPasswordModel{ LoginAction = LoginAction.ForgotPassword});
        }

        [HttpPost]
        public ActionResult ForgotPassword(string restoreIdentNum, string restoreEmail)
        {
            if (string.IsNullOrWhiteSpace(restoreIdentNum) || string.IsNullOrWhiteSpace(restoreEmail))
            {
                return HttpNotFound();
            }

            var usersRepo = new UsersRepo();

            var user = usersRepo.GetCollection<AppModel.User>(x => x.Email == restoreEmail &&
                                                                   x.IdentNum == restoreIdentNum)
                .FirstOrDefault();

            if (user != null)
            {
                var resetGuid = Guid.NewGuid();
                var emailService = new EmailService();

                using (var db = new DataEntities())
                {
                    db.ResetPasswordRequests.Add(new ResetPasswordRequest
                    {
                        ResetGuid = resetGuid,
                        UserId = user.UserId,
                        DateCreated = DateTime.Now
                    });
                    db.SaveChanges();
                }

#if DEBUG
                var sendToEmail = "info@loglig.com";
#else
                var sendToEmail = restoreEmail;
#endif
                var resetPasswordUrl = Url.Action("ResetPassword", "Login", new {id = resetGuid}, Request.Url?.Scheme);

                var emailBody = string.Format(Messages.Login_RestorePassword_EmailBody, user.IdentNum, user.Email,
                    DateTime.Now, resetPasswordUrl);

                emailService.SendAsync(sendToEmail, emailBody, Messages.Login_RestorePassword_EmailSubject);
            }

            return View("Index", new LoginForgotPasswordModel { LoginAction = LoginAction.ForgotPassword, RestoreEmailSent = true});
        }

        [HttpGet]
        public ActionResult ResetPassword(Guid id)
        {
            using (var db = new DataEntities())
            {
                var resetRequest = db.ResetPasswordRequests.FirstOrDefault(x => x.ResetGuid == id && !x.IsCompleted);

                if (resetRequest == null)
                {
                    return RedirectToAction("Index");
                }

                return View("Index", new LoginResetPasswordModel { LoginAction = LoginAction.ResetPassword, ResetId = id});
            }
        }

        [HttpPost]
        public ActionResult ResetPassword(LoginResetPasswordModel model)
        {
            using (var db = new DataEntities())
            {
                var resetRequest =
                    db.ResetPasswordRequests.FirstOrDefault(x => x.ResetGuid == model.ResetId && !x.IsCompleted);

                if (resetRequest == null)
                {
                    return RedirectToAction("Index");
                }

                if (model.ResetPassword != model.ConfirmResetPassword)
                {
                    return View("Index",
                        new LoginResetPasswordModel {LoginAction = LoginAction.ResetPassword, ResetId = model.ResetId});
                }

                resetRequest.User.Password = Protector.Encrypt(model.ResetPassword);
                resetRequest.IsCompleted = true;

                db.SaveChanges();

                return View("Index", new LoginResetPasswordModel { LoginAction = LoginAction.ResetPassword, ResetCompleted = true});
            }
        }

        private void SetAdminCookie(string name, string role)
        {
            var c = new HttpCookie("cmsdata");
            c.Values.Add("uname", HttpUtility.UrlEncode(name));
            c.Values.Add("utype", HttpUtility.UrlEncode(role));

            int expirationTime;
            var envExpirationTime = Environment.GetEnvironmentVariable("LOGLIG_DEVELOPMENT", EnvironmentVariableTarget.Machine);
            var isNum = int.TryParse(envExpirationTime, out expirationTime);
            if (!isNum)
            {
                expirationTime = 24;
            }

            c.Expires = DateTime.Now.AddDays(expirationTime);

            Response.Cookies.Add(c);
        }

        private void SetCaptchaCookie(bool isAdd)
        {
            var day = isAdd ? 1 : -1;
            var c = new HttpCookie("capterr", "true");
            c.Expires = DateTime.Now.AddDays(day);

            Response.Cookies.Add(c);
        }

        public bool IsCaptchaCookie()
        {
            return Request.Cookies["capterr"] != null;
        }

        private bool IsValidCaptcha(string captcha)
        {
            if (string.IsNullOrWhiteSpace(captcha))
                return false;

            if (TempData["Captcha"] == null)
                return false;

            var captTxt = TempData["Captcha"].ToString();

            return captcha.ToUpper() == captTxt;
        }

        private int AnonymousTries()
        {
            if (Session["lgntries"] == null)
                return 0;

            return (int)Session["lgntries"];
        }

        private void AnonymousTriesAdd()
        {
            var tries = AnonymousTries();

            Session["lgntries"] = ++tries;
        }
    }
}
