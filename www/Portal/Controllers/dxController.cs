using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Text;
using System.IO;
using log4net;

using Model;
using BLL;
using Utls;

namespace Portal.Controllers
{
    public class dxController : BaseController
    {
        //
        // GET: /home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult bulletin()
        {
            //GroupBll bll = new GroupBll();

            //return View(bll.GetMatch(""));

            if (UserInfo != null)
            { 
            
            }

            return View();
        }

        public ActionResult competition()
        {
            return View();
        }

        public ActionResult about()
        {
            return View();
        }

        public ActionResult notice()
        {
            return View();
        }

        public ActionResult board()
        {
            return View();
        }

        public ActionResult detail(string id)
        {
            ViewBag.id = id;

            GroupBll bll = new GroupBll();
            var m = bll.GetMatchById(id);

            return View(m);
        }

        public ActionResult d3335b57()
        {
            return View();
        }

        public ActionResult d3335b58()
        {
            return View();
        }

        public ActionResult d3335b59()
        {
            return View();
        }

        public ActionResult d3335b60()
        {
            return View();
        }

        public ActionResult d3335b61()
        {
            return View();
        }

        public ActionResult d3335b62()
        {
            return View();
        }

        public ActionResult d3335b63()
        {
            return View();
        }

        public ActionResult d3335b64()
        {
            return View();
        }

        public ActionResult d3335b65()
        {
            return View();
        }
        public ActionResult d3335b66()
        {
            return View();
        }
        public ActionResult d3335b67()
        {
            return View();
        }
        public ActionResult d3335b68()
        {
            return View();
        }
        public ActionResult d3335b69()
        {
            return View();
        }
        public ActionResult d3335b70()
        {
            return View();
        }
        public ActionResult d3335b71()
        {
            return View();
        }
        public ActionResult jsgz()
        {
            return View();
        }

        public FileStreamResult DownFile()
        {
            string file = Server.MapPath("~/templates/ybm.docx");
            return File(new FileStream(file, FileMode.Open), "application/octet-stream", Server.UrlEncode("定向赛预报名流程.docx"));
        }

        public ActionResult liucheng()
        {
            return View();
        }
        public ActionResult notice_taian()
        {
            return View();
        }
    }
}
