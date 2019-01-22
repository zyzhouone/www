using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Model;
using Utls;
using BLL;

namespace Web.Areas.Main.Controllers
{
    public class TeamController : BaseController
    {
        //
        // GET: /Main/Team/

        public ActionResult Index(string teamname, string company, int? pageIndex)
        {
            var teams = new List<tblteamsVew>();
            try
            {
                teams = new TeamBll().GetTeams(teamname, company, pageIndex.GetValueOrDefault(1));

            }
            catch(Exception e)
            {

            }
            return View(teams);
        }


        public ActionResult Create()
        {
            var bll = new TeamBll();
            List<SelectListItem> company = bll.GetCompany();
            List<SelectListItem> line = bll.GetLine();
            List<SelectListItem> Status = new MemberBll().GetDict(3);

            foreach (SelectListItem r in company)
            {
                ViewBag.company += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }

            foreach (SelectListItem r in line)
            {
                ViewBag.line += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }

            foreach (SelectListItem r in Status)
            {
                ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }

            return View();
        }

        public JsonResult CheckTeamName(string Teamname)
        {
            bool isValidate = true;
            var tblteam = new TeamBll().GetTeamByName(Teamname);
            if (tblteam != null)
            {
                isValidate = false;
            }
            return Json(isValidate, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult Create(FormCollection fc)
        {
            var model = new tblteams();
            model.teamid = Guid.NewGuid().ToString();
            model.Teamname = fc["Teamname"].ToString();
            model.Userid = fc["Userid"];
            model.Company = fc["optCompany"].ToString();
            model.Lineid = fc["optLine"].ToString();
            model.Createtime = DateTime.Now;
            model.Status = Int32.Parse(fc["optStatus"].ToString());
            var bll = new TeamBll();

            try
            {
                bll.AddTeam(model);
            }
            catch (ValidException ex)
            {
                this.ModelState.AddModelError(ex.Name, ex.Message);
                return View(model);
            }

            return this.RefreshParent();
        }


        public ActionResult Edit(string id)
        {
            var bll = new TeamBll();
            var model = bll.GetTeamById(id);

            List<SelectListItem> company = bll.GetCompany();
            List<SelectListItem> line = bll.GetLine();
            List<SelectListItem> Status = new MemberBll().GetDict(3);

            foreach (SelectListItem r in company)
            {
                if (model.Company != null)
                {
                      if (model.Company.ToString() == r.Value)
                        ViewBag.company += "<option value='" + r.Value.ToString() + "'selected>" + r.Text.ToString() + "</option>";
                    else
                        ViewBag.company += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
                }
                else
                {
                    if (r.Value == "3cd53ba4-5c76-11e6-a2c5-6c92bf314f0b")
                        ViewBag.company += "<option value='" + r.Value.ToString() + "'selected>" + r.Text.ToString() + "</option>";
                    else
                        ViewBag.company += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>"; 
                }
              
            }

            foreach (SelectListItem r in line)
            {
                if (model.Lineid.ToString() == r.Value)
                    ViewBag.line += "<option value='" + r.Value.ToString() + "'selected>" + r.Text.ToString() + "</option>";
                else
                    ViewBag.line += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }

            foreach (SelectListItem r in Status)
            {
                if (model.Status.ToString() == r.Value)
                    ViewBag.Status += "<option value='" + r.Value.ToString() + "'selected>" + r.Text.ToString() + "</option>";
                else
                    ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }


            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(string id, FormCollection fc)
        {
            var bll = new TeamBll();
            var model = bll.GetTeamById(id);
            model.Userid = fc["Userid"];
            model.Company = fc["optCompany"].ToString();
            model.Lineid = fc["optLine"].ToString();
            model.Status = Int32.Parse(fc["optStatus"].ToString());

            try
            {
                bll.EditTeam(model);
            }
            catch (ValidException ex)
            {
                this.ModelState.AddModelError(ex.Name, ex.Message);
                List<SelectListItem> company = bll.GetCompany();
                List<SelectListItem> line = bll.GetLine();
                List<SelectListItem> Status = new MemberBll().GetDict(3);

                foreach (SelectListItem r in company)
                {
                    ViewBag.company += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
                }

                foreach (SelectListItem r in line)
                {
                    ViewBag.line += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
                }

                foreach (SelectListItem r in Status)
                {
                    ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
                }

                return View(model);
            }

            return this.RefreshParent();
        }

        [HttpPost]
        public ActionResult Delete(List<string> ids)
        {
            var bll = new TeamBll();

            try
            {
                bll.DeleteTeam(ids);
            }
            catch (ValidException ex)
            {
                return Alert(ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
