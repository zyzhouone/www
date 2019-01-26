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
    public class MatchController : BaseController
    {
        //
        // GET: /Main/Match/

        public ActionResult Index(string matchname, string area2, int? pageIndex)
        {
            var teams = new List<tblmatch>();
            try
            {
                teams = new MatchBll().GetMatchs(matchname, area2, pageIndex.GetValueOrDefault(1));

            }
            catch (Exception e)
            {

            }
            return View(teams);
        }

        public ActionResult Create()
        {

            List<SelectListItem> Status = new MemberBll().GetDict(3);
            foreach (SelectListItem r in Status)
            {
                ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection fc)
        {
            var model = new tblmatch();
            model.Match_id = Guid.NewGuid().ToString();
            model.Match_name = fc["Match_name"].ToString();
            model.Content = fc["Content"].ToString();
            model.Area1 = fc["Area1"].ToString();
            model.Area2 = fc["Area2"].ToString();
            if (fc["Date1"] != null)
            {
                model.Date1 = DateTime.Parse(fc["Date1"].ToString());
            }
            if (fc["Date2"] != null)
            {
                model.Date2 = DateTime.Parse(fc["Date2"].ToString());
            }
            if (fc["Date3"] != null)
            {
                model.Date3 = DateTime.Parse(fc["Date3"].ToString());
            }
            if (fc["Date4"] != null)
            {
                model.Date4 = DateTime.Parse(fc["Date4"].ToString());
            }
            string filename = "";
            // 上传文件
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files["Pic1"];
            if (file != null)
            {
                filename = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if ((filename.ToUpper() == "PNG" || filename.ToUpper() == "JPG") && file.ContentLength / 1024 < 2000)
                {
                    string path = Server.MapPath("~/UploadFiles/");
                    filename = "productfile_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filename;
                    file.SaveAs(path + filename);
                }
                else
                {
                    ViewBag.ErrorMsg = "<font color='red'>只能上传PNG或JPG格式的图片且大小不超过2M</font>";
                    return View();
                }
            }
            else
            {
                ViewBag.ErrorMsg = "<font color='red'>获取文件错误</font>";
                return View(model);
            }
            model.Pic1 = filename;

            //------
            HttpPostedFileBase file2 = files["Pic2"];
            if (file2 != null)
            {
                string filename2 = file2.FileName.Substring(file2.FileName.LastIndexOf(".") + 1);
                if ((filename2.ToUpper() == "PNG" || filename2.ToUpper() == "JPG") && file2.ContentLength / 1024 < 2000)
                {
                    string path = Server.MapPath("~/UploadFiles/");
                    filename = "thumbnailfile_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filename2;
                    file2.SaveAs(path + filename);
                }
                else
                {
                    ViewBag.ErrorMsg = "<font color='red'>只能上传PNG或JPG格式的图片且大小不超过2M</font>";
                    return View();
                }
                model.Pic2 = filename;
            }
            model.Status = fc["optStatus"].ToString();

            var bll = new MatchBll();

            try
            {
                bll.AddMatch(model);
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
            var bll = new MatchBll();
            var model = bll.GetMatchById(id);

            List<SelectListItem> Status = new MemberBll().GetDict(3);

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
            var bll = new MatchBll();
            var model = bll.GetMatchById(id);

            model.Match_name = fc["Match_name"].ToString();
            model.Content = fc["Content"].ToString();
            model.Area1 = fc["Area1"].ToString();
            model.Area2 = fc["Area2"].ToString();
            if (fc["Date1"] != null)
            {
                model.Date1 = DateTime.Parse(fc["Date1"].ToString());
            }
            if (fc["Date2"] != null)
            {
                model.Date2 = DateTime.Parse(fc["Date2"].ToString());
            }
            if (fc["Date3"] != null)
            {
                model.Date3 = DateTime.Parse(fc["Date3"].ToString());
            }
            if (fc["Date4"] != null)
            {
                model.Date4 = DateTime.Parse(fc["Date4"].ToString());
            }
            ViewBag.ErrorMsg = "";
            string filename = "";
            // 上传文件
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files["Pic1"];
            if (file != null && !string.IsNullOrEmpty(file.FileName))
            {
                filename = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if ((filename.ToUpper() == "PNG" || filename.ToUpper() == "JPG") && file.ContentLength / 1024 < 2000)
                {
                    string path = Server.MapPath("~/UploadFiles/");
                    filename = "productfile_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filename;
                    file.SaveAs(path + filename);
                }
                else
                {
                    ViewBag.ErrorMsg = "<font color='red'>只能上传PNG或JPG格式的图片且大小不超过2M</font>";
                    return View();
                }
                model.Pic1 = filename;
            }


            HttpPostedFileBase file2 = files["Pic2"];
            if (file2 != null && !string.IsNullOrEmpty(file2.FileName))
            {
                string filename2 = file2.FileName.Substring(file2.FileName.LastIndexOf(".") + 1);
                if ((filename2.ToUpper() == "PNG" || filename2.ToUpper() == "JPG") && file2.ContentLength / 1024 < 2000)
                {
                    string path = Server.MapPath("~/UploadFiles/");
                    filename = "thumbnailfile_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + filename2;
                    file2.SaveAs(path + filename);
                }
                else
                {
                    ViewBag.ErrorMsg = "<font color='red'>只能上传PNG或JPG格式的图片且大小不超过2M</font>";
                    return View();
                }
                model.Pic2 = filename;
            }

            model.Status = fc["optStatus"].ToString();
            try
            {
                bll.EditMatch(model);
            }
            catch (ValidException ex)
            {
                this.ModelState.AddModelError(ex.Name, ex.Message);
                List<SelectListItem> Status = new MemberBll().GetDict(3);
                foreach (SelectListItem r in Status)
                {
                    if (model.Status.ToString() == r.Value)
                        ViewBag.Status += "<option value='" + r.Value.ToString() + "'selected>" + r.Text.ToString() + "</option>";
                    else
                        ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
                }
                return View(model);
            }

            return this.RefreshParent();
        }

        [HttpPost]
        public ActionResult Delete(List<string> ids)
        {
            var bll = new MatchBll();

            try
            {
                bll.DeleteMatch(ids);
            }
            catch (ValidException ex)
            {
                return Alert(ex.Message);
            }

            return RedirectToAction("Index");
        }

        public ActionResult line(string linename, int? pageIndex)
        {
            var lines = new List<tblline>();
            try
            {
                lines = new MatchBll().GetLines(linename, pageIndex.GetValueOrDefault(1));

            }
            catch (Exception e)
            {

            }
            return View(lines);
        }

        public ActionResult LineCreate()
        {
            List<SelectListItem> Status = new MemberBll().GetDict(3);
            foreach (SelectListItem r in Status)
            {
                ViewBag.Status += "<option value='" + r.Value.ToString() + "'>" + r.Text.ToString() + "</option>";
            }

            return View();
        }

        [HttpPost]
        public ActionResult LineCreate(FormCollection fc)
        {
            var model = new tblline();
            var bll = new MatchBll();
            model.Lineid = Guid.NewGuid().ToString();
            model.Name = fc["Name"].ToString().Trim();
            model.Content = fc["Content"].ToString().Trim();
            model.Players = Int32.Parse(fc["Players"].ToString().Trim());
            model.Count = Int32.Parse(fc["Count"].ToString().Trim());
            model.Conditions = "{players:\"" + model.Players.ToString() + "\",count:\"" + model.Count.ToString() + "\"}";
            model.Createtime = DateTime.Now;
            model.Status = Int32.Parse(fc["optStatus"].ToString());
            try
            {
                bll.AddLine(model);
            }
            catch (ValidException ex)
            {
                this.ModelState.AddModelError(ex.Name, ex.Message);
                return View(model);
            }

            return this.RefreshParent();
        }

        public ActionResult LineEdit(string id)
        {
            var bll = new MatchBll();
            var model = bll.GetLineById(id);
            List<SelectListItem> Status = new MemberBll().GetDict(3);
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
        public ActionResult LineEdit(string id,FormCollection fc)
        {
            var bll = new MatchBll();
            var model = bll.GetLineById(id);
            model.Name = fc["Name"].ToString().Trim();
            model.Content = fc["Content"].ToString().Trim();
            model.Players = Int32.Parse(fc["Players"].ToString().Trim());
            model.Count = Int32.Parse(fc["Count"].ToString().Trim());
            model.Conditions = "{players:\"" + model.Players.ToString() + "\",count:\"" + model.Count.ToString() + "\"}";
            model.Status = Int32.Parse(fc["optStatus"].ToString());
            try
            {
                bll.EditLine(model);
            }
            catch (ValidException ex)
            {
                this.ModelState.AddModelError(ex.Name, ex.Message);
                return View(model);
            }

            return this.RefreshParent();
        }

        [HttpPost]
        public ActionResult LineDelete(List<string> ids)
        {
            var bll = new MatchBll();

            try
            {
                bll.DeleteLine(ids);
            }
            catch (ValidException ex)
            {
                return Alert(ex.Message);
            }

            return RedirectToAction("line");
        }
    }
}
