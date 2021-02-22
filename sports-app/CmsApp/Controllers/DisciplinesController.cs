using AppModel;
using ClosedXML.Excel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService.DTO;
using Omu.ValueInjecter;
using Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace CmsApp.Controllers
{
    public class DisciplinesController : AdminController
    {
        public ActionResult ListBySection(int sectionId, int unionId)
        {
            var vm = new DisciplineTabViewModel();
            var disciplines = disciplinesRepo.GetBySection(sectionId, unionId);
            vm.DisciplineViewModelsList = disciplines.Select(x => new DisciplineViewModel
            {
                DisciplineId = x.DisciplineId,
                Class = x.Class,
                DisciplineType = x.DisciplineType,
                Format = x.Format,
                Name = x.Name,
                NumberOfSportsmen = x.NumberOfSportsmen,
                RoadHeat = x.RoadHeat ?? false,
                MountainHeat = x.MountainHeat ?? false,
                Coxwain = x.Coxwain ?? false

            }).ToList();
            ViewBag.SectionId = sectionId;
            ViewBag.UnionId = unionId;
            string sectionAlias = secRepo.GetById(sectionId).Alias;
            ViewBag.SectionAlias = sectionAlias;

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);

            return PartialView("_List", vm);
        }

        public ActionResult ListFriendshipTypesBySection(int sectionId, int unionId)
        {
            var vm = new DisciplineTabViewModel();
            var disciplines = friendshipTypesRepo.GetBySection(sectionId, unionId);
            vm.FriendshipTypeViewModelsList = disciplines.Select(x => new FriendshipTypeViewModel
            {
                FriendshipsTypesId = x.FriendshipsTypesId,
                Name = x.Name,
                UnionId = x.UnionId,
                SeasonId = x.SeasonId,
                Hierarchy = x.Hierarchy
            }).ToList();
            ViewBag.SectionId = sectionId;
            ViewBag.UnionId = unionId;
            string sectionAlias = secRepo.GetById(sectionId).Alias;
            ViewBag.SectionAlias = sectionAlias;

            ViewBag.JobRole = usersRepo.GetTopLevelJob(AdminId);
            return PartialView("_FriendshipTypesList", vm);
        }

        [HttpPost]
        public void ExportDisciplineInfo(int disciplineId, int seasonId)
        {
            Discipline d = disciplinesRepo.GetById(disciplineId);
            List<GymnasticExportDto> export_list = playersRepo.GetDisciplinePlayers(disciplineId, seasonId)
                                                          .GroupBy(g => g.IdentNum).Select(g => g.First())
                                                          .OrderBy(g => g.Route)
                                                          .ThenBy(g => g.Rank)
                                                          .ThenBy(g => g.ClubName)
                                                          .ThenBy(g => g.LastName)
                                                          .ThenBy(g => g.FirstName)
                                                          .ToList();
            using (var workbook =
                new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet(Messages.Players);

                var columnCounter = 1;
                var rowCounter = 1;
                var addCell = new Action<string>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });
                addCell("#");
                addCell(Messages.FirstName);
                addCell(Messages.LastName);
                addCell(Messages.IdentNum);
                addCell(Messages.ClubName);
                addCell(Messages.Route);
                addCell(Messages.RankGym);

                rowCounter++;
                columnCounter = 1;

                ws.Columns().AdjustToContents();

                foreach (var row in export_list)
                {
                    addCell((rowCounter - 1).ToString());
                    addCell(row.FirstName);
                    addCell(row.LastName);
                    addCell(row.IdentNum);
                    addCell(row.ClubName);
                    addCell(row.Route);
                    addCell(row.Rank);

                    rowCounter++;
                    columnCounter = 1;
                }

                ws.Columns().AdjustToContents();

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition",
                    $"attachment;filename= {d.Name.Replace(' ', '_')}_{Messages.DisciplineReport.Replace(" ", "_")}_{DateTime.Today.ToString("dd/MM/yyyy")}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        [HttpPost]
        public ActionResult Save(string name, int sectionId, int unionId, int? _class, int? NumberOfSportsmen, bool? RoadHeat, bool? MountainHeat, bool? Coxwain)
        {
            var discipline = new Discipline
            {
                Name = name,
                UnionId = unionId,
                Class = _class,
                NumberOfSportsmen = NumberOfSportsmen
            };

            string sectionAlias = secRepo.GetById(sectionId).Alias;
            if (string.Equals(sectionAlias, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase))
            {
                discipline.RoadHeat = RoadHeat ?? false;
                discipline.MountainHeat = MountainHeat ?? false;
            }
            if (string.Equals(sectionAlias, GamesAlias.Rowing, StringComparison.CurrentCultureIgnoreCase))
            {
                discipline.Coxwain = Coxwain ?? false;
            }

            disciplinesRepo.Create(discipline);

            return RedirectToAction(nameof(ListBySection), new { sectionId, unionId });
        }

        [HttpPost]
        public ActionResult SaveFriendshipType(string name, int sectionId, int unionId, int? hierarchy)
        {
            var seasonId = seasonsRepository.GetCurrentByUnionId(unionId);
            if (string.IsNullOrEmpty(name) || hierarchy == null)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                Response.StatusDescription = "Required";
                return Json(new { Message = Messages.Error });
            }

            //validate hierarchy
            var list = friendshipTypesRepo.GetAllByUnionId(unionId);
            if(hierarchy > 0 && list.Any(x=> x.Hierarchy == hierarchy))
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                Response.StatusDescription = "NotUnique";
                return Json(new { Message = Messages.Error });
            }

            if (seasonId.HasValue)
            {
                var friendshipsType = new FriendshipsType
                {
                    Name = name,
                    UnionId = unionId,
                    SeasonId = seasonId.Value,
                    Hierarchy = hierarchy
                };

                friendshipTypesRepo.Create(friendshipsType);

                return RedirectToAction(nameof(ListFriendshipTypesBySection), new { sectionId, unionId });
            }

            return null;
        }

        public ActionResult Edit(int id, int? seasonId = null, string roleType = null)
        {
            if (!string.IsNullOrWhiteSpace(roleType))
            {
                this.SetWorkerSession(roleType);
            }

            var disciline = disciplinesRepo.GetById(id);
            if (disciline.IsArchive)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var seasons = seasonsRepository.GetSeasonsByUnion(disciline.UnionId, false).ToList();
            Session["CurrentUnionId"] = disciline.UnionId;
            var model = new EditDisciplineViewModel
            {
                Id = id,
                UnionId = disciline.UnionId,
                SeasonId = seasonId ?? GetUnionCurrentSeasonFromSession(),
                Seasons = seasons,
                Name = disciline.Name,
                UnionName = disciline.Union.Name,
                SectionAlias = disciline?.Union?.Section?.Alias ?? string.Empty
            };

            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var item = disciplinesRepo.GetByIdWithUnion(id);
            item.IsArchive = true;

            disciplinesRepo.Save();
            return RedirectToAction(nameof(ListBySection), new { sectionId = item.Union.SectionId, unionId = item.UnionId });
        }

        public ActionResult DeleteFriendship(int id)
        {
            var item = friendshipTypesRepo.GetByIdWithUnion(id);

            if (item.TeamsPlayers.Count() > 0)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { Message = Messages.Error }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                item.IsArchive = true;
                friendshipTypesRepo.Save();
            }

            return RedirectToAction(nameof(ListFriendshipTypesBySection), new { sectionId = item.Union.SectionId, unionId = item.UnionId});
        }

        public ActionResult Details(int id, int seasonId, bool hasErrors = false, int? routeId = null, int? teamRouteId = null)
        {
            var item = disciplinesRepo.GetById(id);
            var vm = new DisciplineDetailsForm();
            vm.InjectFrom(item);
            vm.SeasonId = seasonId;
            var doc = disciplinesRepo.GetTermsDoc(id);
            if (doc != null)
            {
                vm.DocId = doc.DocId;
            }

            if (TempData["ViewData"] != null)
            {
                ViewData = (ViewDataDictionary)TempData["ViewData"];
            }

            string alias = item.Union.Section.Alias;

            if (alias == GamesAlias.Gymnastic)
            {
                ViewBag.Alias = alias;
                vm.Routes = item.DisciplineRoutes.OrderBy(p => p.Hierarchy.GetValueOrDefault(int.MaxValue))
                    .Select(x => new RouteViewModel
                    {
                        Id = x.Id,
                        Route = x.Route,
                        Ranks = x.RouteRanks.Where(r => r.IsArchived != true).Select(p => new RankViewModel(p)).ToList(),
                        Hierarchy = x.Hierarchy,

                        RelationCount = x.UsersRoutes.Count + x.RouteRanks.Count
                    })
                    .ToList();
                vm.TeamRoutes = item.DisciplineTeamRoutes.OrderBy(p => p.Hierarchy.GetValueOrDefault(int.MaxValue))
                    .Select(x => new RouteViewModel
                    {
                        Id = x.Id,
                        Route = x.Route,
                        TeamRanks = x.RouteTeamRanks.Select(p => new RankViewModel(p)).ToList(),
                        Hierarchy = x.Hierarchy,

                        RelationCount = x.TeamsRoutes.Count + x.RouteTeamRanks.Count
                    })
                    .ToList();
                vm.UnionId = item.UnionId;
                vm.IntrumentForm = new InstrumentForm
                {
                    DisciplineId = id,
                    SeasonId = seasonId,
                    InstrumentsList = disciplinesRepo.GetAllInstruments(id, seasonId)
                };
            }

            if (string.Equals(alias, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase))
            {
                vm.MountainHeat = item.MountainHeat ?? false;
                vm.RoadHeat = item.RoadHeat ?? false;
            }

            ViewBag.Alias = alias;

            ViewBag.HasErrors = hasErrors;
            ViewBag.RouteId = routeId;
            ViewBag.TeamRouteId = teamRouteId;

            return PartialView("_Details", vm);
        }

        [HttpPost]
        public ActionResult Details(DisciplineDetailsForm frm)
        {
            int maxFileSize = GlobVars.MaxFileSize * 1000;
            var savePath = Server.MapPath(GlobVars.ContentPath + "/discipline/");

            var item = disciplinesRepo.GetById(frm.DisciplineId);
            UpdateModel(item);
            item.Name = frm.Name;

            string alias = item.Union.Section.Alias;
            if (alias == GamesAlias.Gymnastic)
            {
                ViewBag.Alias = alias;

                #region Individual

                if (frm.Routes != null)
                {
                    if (frm.Routes.Where(p => p.Hierarchy != null).GroupBy(p => p.Hierarchy).Where(p => p.Count() > 1).Any())
                    {
                        ModelState.AddModelError("_individual_", Messages.DisciplineNotAllHaveUniqueHierarchy);
                    }
                    var ind = 0;
                    foreach (var route in frm.Routes)
                    {
                        var itemRoute = item.DisciplineRoutes.First(x => x.Id == route.Id);
                        itemRoute.Route = route.Route;
                        if (route.Hierarchy == null && itemRoute.Hierarchy != null)
                        {
                            ModelState.AddModelError($"Routes[{ind}].Hierarchy", Messages.DisciplineRouteHierarchyRequired);
                        }
                        else
                        {
                            itemRoute.Hierarchy = route.Hierarchy;
                            db.Entry(itemRoute).State = System.Data.Entity.EntityState.Modified;
                        }
                        ind++;
                    }

                    if (frm.NewHierarchy != null && item.DisciplineRoutes.Where(p => p.Hierarchy == frm.NewHierarchy).Any())
                    {
                        ModelState.AddModelError("NewHierarchy", Messages.DisciplineUniqueHierarchy);
                    }
                }

                if (!string.IsNullOrEmpty(frm.NewRote?.Trim()))
                {
                    if (frm.NewHierarchy == null)
                    {
                        ModelState.AddModelError("NewHierarchy", Messages.DisciplineRouteHierarchyRequired);
                    }
                    else
                    {
                        item.DisciplineRoutes.Add(new DisciplineRoute { Route = frm.NewRote, Hierarchy = frm.NewHierarchy });
                    }
                }

                #endregion

                #region Team
                if (frm.TeamRoutes != null)
                {
                    if (frm.TeamRoutes.Where(p => p.Hierarchy != null).GroupBy(p => p.Hierarchy).Where(p => p.Count() > 1).Any())
                    {
                        ModelState.AddModelError("_team_", Messages.DisciplineNotAllHaveUniqueHierarchy);
                    }
                    int ind = 0;
                    foreach (var route in frm.TeamRoutes)
                    {
                        var itemRoute = item.DisciplineTeamRoutes.First(x => x.Id == route.Id);
                        itemRoute.Route = route.Route;

                        if (route.Hierarchy == null && itemRoute.Hierarchy != null)
                        {
                            ModelState.AddModelError($"TeamRoutes[{ind}].Hierarchy", Messages.DisciplineRouteHierarchyRequired);
                        }
                        else
                        {
                            itemRoute.Hierarchy = route.Hierarchy;
                            db.Entry(itemRoute).State = System.Data.Entity.EntityState.Modified;
                        }
                        ind++;
                    }

                    if (frm.NewTeamHierarchy != null && item.DisciplineTeamRoutes.Where(p => p.Hierarchy == frm.NewTeamHierarchy).Any())
                    {
                        ModelState.AddModelError("NewTeamHierarchy", Messages.DisciplineUniqueHierarchy);
                    }
                }

                if (!string.IsNullOrEmpty(frm.NewTeamRote?.Trim()))
                {
                    if (frm.NewTeamHierarchy == null)
                    {
                        ModelState.AddModelError("NewTeamHierarchy", Messages.DisciplineRouteHierarchyRequired);
                    }
                    else
                    {
                        item.DisciplineTeamRoutes.Add(new DisciplineTeamRoute { Route = frm.NewTeamRote, Hierarchy = frm.NewTeamHierarchy });
                    }
                }
                #endregion
            }

            var imageFile = GetPostedFile("PrimaryImageFile");
            if (imageFile != null)
            {
                if (imageFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("PrimaryImageFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(imageFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("PrimaryImageFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.PrimaryImage))
                            FileUtil.DeleteFile(savePath + item.PrimaryImage);

                        item.PrimaryImage = newName;
                    }
                }
            }

            var logoFile = GetPostedFile("LogoFile");
            if (logoFile != null)
            {
                if (logoFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("LogoFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(logoFile, "logo");
                    if (newName == null)
                    {
                        ModelState.AddModelError("LogoFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.Logo))
                            FileUtil.DeleteFile(savePath + item.Logo);

                        item.Logo = newName;
                    }
                }
            }

            var indexFile = GetPostedFile("IndexFile");
            if (indexFile != null)
            {
                if (indexFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("IndexFile", Messages.FileSizeError);
                }
                else
                {
                    var newName = SaveFile(indexFile, "img");
                    if (newName == null)
                    {
                        ModelState.AddModelError("IndexFile", Messages.FileError);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.IndexImage))
                            FileUtil.DeleteFile(savePath + item.IndexImage);

                        item.IndexImage = newName;
                    }
                }
            }

            var docFile = GetPostedFile("DocFile");
            if (docFile != null)
            {
                if (docFile.ContentLength > maxFileSize)
                {
                    ModelState.AddModelError("DocFile", Messages.FileSizeError);
                }
                bool isValid = SaveDocument(docFile, frm.DisciplineId);
                if (!isValid)
                {
                    ModelState.AddModelError("DocFile", Messages.FileError);
                }
            }

            if (ModelState.IsValid)
            {
                disciplinesRepo.Save();
                TempData["Saved"] = true;

                return RedirectToAction(nameof(Details), new { id = item.DisciplineId, seasonId = frm.SeasonId });
            }
            else
            {
                //TempData["ViewData"] = ViewData;
                //string alias = item.Union.Section.Alias;

                foreach (var r in frm.Routes)
                {
                    var itemR = item.DisciplineRoutes.FirstOrDefault(p => p.Id == r.Id);
                    if (itemR != null)
                    {
                        r.Ranks = itemR.RouteRanks.Where(x => x.IsArchived != true).Select(p => new RankViewModel(p)).ToList();
                    }
                }

                frm.IntrumentForm = new InstrumentForm
                {
                    DisciplineId = item.DisciplineId,
                    SeasonId = frm.SeasonId.Value,
                    InstrumentsList = disciplinesRepo.GetAllInstruments(item.DisciplineId, frm.SeasonId.Value)
                };

                return PartialView("_Details", frm);
            }
        }

        public ActionResult DeleteImage(int disciplineId, string image)
        {
            DataEntities db = new DataEntities();
            var item = db.Disciplines.FirstOrDefault(x => x.DisciplineId == disciplineId);
            if (item == null || string.IsNullOrEmpty(image))
                return RedirectToAction("Edit", new { id = disciplineId });
            if (image == "PrimaryImage")
            {
                item.PrimaryImage = null;
            }
            if (image == "Logo")
            {
                item.Logo = null;
            }
            if (image == "IndexImage")
            {
                item.IndexImage = null;
            }
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = disciplineId });
        }

        public ActionResult DeleteDoc(int disciplineId)
        {
            DataEntities db = new DataEntities();
            var item = db.DisciplinesDocs.FirstOrDefault(i => i.DisciplineId == disciplineId);
            if (item == null)
                return RedirectToAction("Edit", new { id = disciplineId });
            db.DisciplinesDocs.Remove(item);
            db.SaveChanges();
            return RedirectToAction("Edit", new { id = disciplineId });
        }

        public ActionResult ShowDoc(int id)
        {
            var doc = disciplinesRepo.GetDocById(id);

            Response.AddHeader("content-disposition", "inline;filename=" + doc.FileName + ".pdf");

            return this.File(doc.DocFile, "application/pdf");
        }


        [HttpPost]
        public ActionResult SetIsForScore(int Id, bool IsForScore)
        {
            disciplinesRepo.SetIsForScore(Id, IsForScore);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult SetIsMultiBattle(int Id, bool IsMultiBattle)
        {
            disciplinesRepo.SetIsMultiBattle(Id, IsMultiBattle);
            return Json(new { Success = true });
        }

        [HttpPost]
        public ActionResult SetCorrectionForClubCompetition(int ClubId, int SeasonId, int LeagueId, decimal Correction, int GenderId, int? TypeId)
        {
            disciplinesRepo.SetCorrectionForClubCompetition(ClubId, LeagueId, SeasonId, Correction, GenderId, TypeId);
            return Json(new { Success = true });
        }



        [NonAction]
        private bool SaveDocument(HttpPostedFileBase file, int disciplineId)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (ext != ".pdf")
            {
                return false;
            }

            var doc = disciplinesRepo.GetTermsDoc(disciplineId);
            if (doc == null)
            {
                doc = new DisciplinesDoc { DisciplineId = disciplineId };
                disciplinesRepo.CreateDoc(doc);
            }

            doc.FileName = file.FileName;

            byte[] docData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                docData = reader.ReadBytes(file.ContentLength);
            }

            //var req = new MetascanHelper.MetadataRequest(Guid.NewGuid().ToString(),
            //    file.FileName,
            //    docData,
            //    MetascanHelper.MetaScanAction.PostFileToMetaScan
            //);

            //string dataid = req.PostFileForScanning();
            //MetaScanScanStatus metaScanStatus;
            //req.CheckFileScan(dataid, out metaScanStatus);

            //int tries = 3;
            //while (metaScanStatus == MetaScanScanStatus.GeneralError && dataid != "" && tries > 0)
            //{
            //    tries--;
            //    System.Threading.Thread.Sleep(2000);
            //    req.CheckFileScan(dataid, out metaScanStatus);
            //}

            //if (metaScanStatus == MetaScanScanStatus.Valid)
            //{
            doc.DocFile = docData;
            disciplinesRepo.Save();
            return true;
            //}
        }

        [NonAction]
        private HttpPostedFileBase GetPostedFile(string name)
        {
            if (Request.Files[name] == null)
                return null;

            if (Request.Files[name].ContentLength == 0)
                return null;

            return Request.Files[name];
        }

        [NonAction]
        private string SaveFile(HttpPostedFileBase file, string name)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
                return null;

            string newName = name + "_" + AppFunc.GetUniqName() + ext;

            var savePath = Server.MapPath(GlobVars.ContentPath + "/discipline/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            System.IO.File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }

        public ActionResult DeleteRoute(int routeId, int disciplineId, int seasonId, bool deleteRelation = false)
        {
            var hasErrors = disciplinesRepo.DeleteRouteById(routeId, deleteRelation);
            //return RedirectToAction("Details", new { id = disciplineId, seasonId, hasErrors, routeId });
            if (!hasErrors && deleteRelation)
            {
                return RedirectToAction("Details", new { id = disciplineId, seasonId });
            }
            else
            {
                return RedirectToAction("Details", new { id = disciplineId, seasonId, hasErrors, routeId });
            }
        }


        [HttpPost]
        public ActionResult CreateDistance(int seasonId, string name)
        {
            disciplinesRepo.CreateDistance(name, seasonId);
            return RedirectToAction("DistanceTable", new { seasonId });
        }

        [HttpPost]
        public ActionResult EditDistance(int Id, string name)
        {
            disciplinesRepo.EditDistance(Id, name);
            return Json(new { Success = true });
        }


        public ActionResult DeleteDistance(int Id, int seasonId)
        {
            disciplinesRepo.DeleteDistance(Id);
            ViewBag.SeasonId = seasonId;
            return RedirectToAction("DistanceTable", new { seasonId });
        }


        public ActionResult DistanceTable(int seasonId)
        {
            var distanceList = disciplinesRepo.GetDistanceTable(seasonId);
            ViewBag.SeasonId = seasonId;
            return PartialView("_DistanceTable", distanceList);
        }


        public ActionResult DeleteTeamRoute(int routeId, int disciplineId, int seasonId, bool deleteRelation = false)
        {
            var hasErrors = disciplinesRepo.DeleteTeamRouteById(routeId, deleteRelation);
            if (!hasErrors && deleteRelation)
            {
                return RedirectToAction("Edit", new { id = disciplineId, seasonId });
            }
            else
            {
                return RedirectToAction("Details", new { id = disciplineId, seasonId, hasErrors, teamRouteId = routeId });
            }
        }

        [HttpPost]
        public ActionResult AddRank(int routeId, string rank, 
            DateTime? fromAge, DateTime? toAge,
            int disciplineId)
        {
            var newRankId = disciplinesRepo.AddRankToRoute(routeId, rank, fromAge, toAge);
            disciplinesRepo.Save();

            var route = disciplinesRepo.GetRouteById(routeId);
            var model = new RouteViewModel
            {
                Id = route.Id,
                Ranks = route.RouteRanks.Where(r => r.IsArchived != true).Select(p => new RankViewModel(p)).ToList(),
                Route = route.Route
            };
            return PartialView("_Ranks", model);
        }

        [HttpPost]
        public ActionResult AddTeamRank(int routeId, string rank, 
            DateTime? fromAge, DateTime? toAge, 
            int disciplineId)
        {
            var newRankId = disciplinesRepo.AddTeamRankToRoute(routeId, rank, fromAge, toAge);
            disciplinesRepo.Save();

            var route = disciplinesRepo.GetTeamRouteById(routeId);
            var model = new RouteViewModel
            {
                Id = route.Id,
                TeamRanks = route.RouteTeamRanks.Select(p => new RankViewModel(p)).ToList(),
                Route = route.Route
            };
            return PartialView("_TeamRanks", model);
        }

        [HttpPost]
        public IEnumerable GetRouteRanks(int routeId)
        {
            return disciplinesRepo.GetRanksByRouteId(routeId)
                .Select(x => new
                {
                    rankId = x.Id,
                    rank = x.Rank
                })
                .AsEnumerable();
        }

        [HttpPost]
        public ActionResult DeleteRank(int rankId, int disciplineId)
        {
            var rank = disciplinesRepo.GetRankById(rankId);

            var model = new RouteViewModel();
            
            disciplinesRepo.DeleteRank(rank);
            disciplinesRepo.Save();

            var route = disciplinesRepo.GetRouteById(rank.RouteId);

            model.Id = route.Id;
            model.Ranks = route.RouteRanks.Where(x => x.IsArchived != true).Select(p => new RankViewModel(p)).ToList();
            model.Route = route.Route;

            return PartialView("_Ranks", model);
        }

        [HttpPost]
        public ActionResult DeleteTeamRank(int rankId, int disciplineId, bool deleteRelation = false)
        {
            var rank = disciplinesRepo.GetTeamRankById(rankId);

            var model = new RouteViewModel();

            disciplinesRepo.DeleteTeamRank(rank);
            disciplinesRepo.Save();

            var route = disciplinesRepo.GetTeamRouteById(rank.TeamRouteId);

            model.Id = route.Id;
            model.TeamRanks = route.RouteTeamRanks.Select(p => new RankViewModel(p)).ToList();
            model.Route = route.Route;

            return PartialView("_TeamRanks", model);
        }

        [HttpPost]
        public void UpdateRank(int rankId, string newValue, DateTime? fromAge, DateTime? toAge)
        {
            var rank = disciplinesRepo.GetRankById(rankId);
            rank.Rank = newValue;
            rank.FromAge = fromAge;
            rank.ToAge = toAge;
            disciplinesRepo.Save();
        }

        [HttpPost]
        public void UpdateTeamRank(int rankId, string newValue, DateTime? fromAge, DateTime? toAge)
        {
            var rank = disciplinesRepo.GetTeamRankById(rankId);
            rank.Rank = newValue;
            rank.FromAge = fromAge;
            rank.ToAge = toAge;
            disciplinesRepo.Save();
        }

        [HttpPost]
        public ActionResult CreateInstrument(InstrumentForm form)
        {
            if (form.InstrumentName != null)
            {
                disciplinesRepo.CreateInsturment(new InstrumentDto
                {
                    DisciplineId = form.DisciplineId,
                    SeasonId = form.SeasonId,
                    Name = form.InstrumentName
                });
                disciplinesRepo.Save();
            }
            form.InstrumentsList = disciplinesRepo.GetAllInstruments(form.DisciplineId, form.SeasonId);
            return PartialView("_Instruments", form);
        }

        [HttpPost]
        public ActionResult DeleteInstrument(int id, int seasonId, int disciplineId)
        {
            disciplinesRepo.DeleteInstrument(id);
            disciplinesRepo.Save();
            var form = new InstrumentForm
            {
                DisciplineId = disciplineId,
                SeasonId = seasonId,
                InstrumentsList = disciplinesRepo.GetAllInstruments(disciplineId, seasonId)
            };
            return PartialView("_Instruments", form);
        }

        [HttpPost]
        public void UpdateInstrument(int id, string instrumentName)
        {
            disciplinesRepo.UpdateInstrumentName(id, instrumentName);
            disciplinesRepo.Save();
        }


        public ActionResult Update(DisciplineDetailsForm data)
        {
            var item = disciplinesRepo.GetById(data.DisciplineId);
            string alias = item.Union.Section.Alias;
            bool isBicycleSection = string.Equals(alias, GamesAlias.Bicycle, StringComparison.CurrentCultureIgnoreCase);
            bool isRowingSection = string.Equals(alias, GamesAlias.Rowing, StringComparison.CurrentCultureIgnoreCase);
            disciplinesRepo.UpdateDiscipline(data.DisciplineId, data.Name, data.Format, data.Class, data.DisciplineType, data.NumberOfSportsmen, isBicycleSection, data.RoadHeat, data.MountainHeat, isRowingSection, data.Coxwain);
            return RedirectToAction(nameof(ListBySection), new { SectionId = data.SeasonId, UnionId = data.UnionId });
        }

        public ActionResult UpdateFriendshipType(FriendshipTypeViewModel data)
        {
            var list = friendshipTypesRepo.GetAllByUnionId(data.UnionId);
            if (data.Hierarchy > 0 && list.Any(x => x.Hierarchy == data.Hierarchy && x.FriendshipsTypesId != data.FriendshipsTypesId))
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                Response.StatusDescription = "NotUnique";
                return Json(new { Message = Messages.Error });
            }

            friendshipTypesRepo.UpdateFriendshipType(data.FriendshipsTypesId, data.Name, data.Hierarchy);
            return RedirectToAction(nameof(ListFriendshipTypesBySection), new { SectionId = data.SeasonId, UnionId = data.UnionId });
        }

        [HttpPost]
        public void ExportDisciplinesToExcel(int id)
        {
            var disciplinesList = disciplinesRepo.GetDisciplineStatistics(id);
            using (var workbook = new XLWorkbook(XLEventTracking.Disabled) { RightToLeft = getCulture() == CultEnum.He_IL })
            {
                var ws = workbook.AddWorksheet($"{Messages.Disciplines}");
                var columnCounter = 1;
                var rowCounter = 1;

                var addCell = new Action<object>(value =>
                {
                    ws.Cell(rowCounter, columnCounter).Value = value;
                    columnCounter++;
                });

                addCell(Messages.Name);
                addCell(Messages.Approved);
                addCell(Messages.ActivePlayers);
                addCell(Messages.NumberOfClubs);

                rowCounter++;
                columnCounter = 1;
                ws.Columns().AdjustToContents();


                foreach (var row in disciplinesList)
                {
                    addCell(row.Name);
                    addCell(row.Approved);
                    addCell(row.Active);
                    addCell(row.NumberOfClubs);

                    rowCounter++;
                    columnCounter = 1;
                    ws.Columns().AdjustToContents();
                }

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", $"attachment;filename={Messages.Disciplines}_{Messages.Information}.xlsx");

                using (var myMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(myMemoryStream);
                    myMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public ActionResult ModifyResultCompetitionHeat(int regId, string heat)
        {
            disciplinesRepo.ModifyAthleteCompetitionHeat(regId, heat);
            return Json(new { Success = true });
        }
        public ActionResult ModifyResultCompetitionLane(int regId, int? lane)
        {
            disciplinesRepo.ModifyAthleteCompetitionLane(regId, lane);
            return Json(new { Success = true });
        }

        public ActionResult CreateCompetitionExpertiseHeat(int leagueId, int expId, int compHeatId, int[] unionHeatIds)
        {
            var isExist = disciplinesRepo.CheckCompetitionExpertiesHeatByExpId(expId, compHeatId);

            if(isExist)
            {
                return Json(new { Success = false, Message = Messages.SelectedCompetitionHeatAlreadyAdded });
            }

            var league = leagueRepo.GetById(leagueId);
            var ages = disciplinesRepo.GetAllByUnionIdWithAges(unionHeatIds);
            var agesList = ages.SelectMany(x => x.CompetitionAges);
            var agesIds = agesList.Select(x => x.id).Distinct();
            var compExpHeatId = disciplinesRepo.CreateCompetitionExpertiseHeat(expId, compHeatId, unionHeatIds, league.LevelId, agesIds.ToArray());            
            var uHeats = disciplinesRepo.GetAllByUnionId(league.UnionId.Value);
            var compHeatName = db.BicycleCompetitionHeats.FirstOrDefault(x => x.Id == compHeatId)?.Name;
            var levels = new SelectList(db.CompetitionLevels.Where(x => x.UnionId == league.UnionId && x.SeasonId == league.SeasonId).ToList(), "id", "level_name", league.LevelId);
            var agesSel = new MultiSelectList(agesList.Distinct(), "id", "age_name", agesIds);
            return Json(new { Success = true, CompExpHeatId = compExpHeatId, CompHeatName = compHeatName, UnionHeats = uHeats, CompAges = agesSel, CompAgesIds = agesIds.Select(x => x.ToString()).ToArray(), Levels = levels });
        }

        public ActionResult UpdateCompetitionExpertiseHeat(int leagueId, int expId, int compHeatId, int[] unionHeatIds)
        {
            disciplinesRepo.RemoveCompetitionExpertiseDisciplineHeat(expId, compHeatId, unionHeatIds);
            disciplinesRepo.AddCompetitionExpertiseDisciplineHeat(expId, compHeatId, unionHeatIds);
            return Json(new { Success = true });
        }

        public ActionResult DeleteCompetitionExpertiseHeat(int leagueId, int expId, int compHeatId)
        {
            disciplinesRepo.RemoveCompetitionExpertiseHeat(expId, compHeatId, null);
            return Json(new { Success = true });
        }

        public ActionResult UpdateCompetitionExpertiseHeatLevel(int leagueId, int expId, int compHeatId, int? levelId)
        {
            disciplinesRepo.UpdateCompetitionExpertiseHeatLevel(expId, compHeatId, levelId);
            return Json(new { Success = true });
        }

        public ActionResult UpdateCompetitionExpertiseHeatAges(int leagueId, int expId, int compHeatId, int[] agesIds)
        {
            disciplinesRepo.RemoveCompetitionExpertiseHeatAges(expId, compHeatId, agesIds);
            disciplinesRepo.AddCompetitionExpertiseHeatAges(expId, compHeatId, agesIds);
            return Json(new { Success = true });
        }
    }
}
