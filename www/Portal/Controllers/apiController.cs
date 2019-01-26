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
    public class apiController : BaseController
    {
        //
        // GET: /api/

        public ActionResult regprotocol()
        {
            return View();
        }

        public ActionResult matchcert(string userid, string matchid)
        {
            if (matchid == "91083e8d-2b3a-428e-97c9-50ca9ba4fb35")
                return RedirectToAction("matchcert_ht", new { userid = userid, matchid = matchid });

            TeamRegBll bll = new TeamRegBll();
            tblmatchentity ent = bll.GetMatchUserByUidMid(userid, matchid);
            if (ent == null)
            {
                ent = new tblmatchentity();
                ent.Mono = "NONE";
            }
            return View(ent);
        }

        public ActionResult matchcert_ht(string userid, string matchid)
        {
            TeamRegBll bll = new TeamRegBll();
            List<tblmatchentity> ent = bll.GetMatchUsersByUidMid(userid, matchid);
            if (ent == null)
            {
                ent = new List<tblmatchentity>();                
            }
            return View(ent);
        }

        public ActionResult matchdetail(string userid, string matchid)
        {
            TeamRegBll bll = new TeamRegBll();
            List<tblmatchentity> lst = bll.GetMatchUsersByUidMid(userid, matchid);
            if (lst == null)
                lst = new List<tblmatchentity>();

            return View(lst);
        }

        /// <summary>
        /// 检查字符串的日期格式
        /// </summary>
        /// <param name="datestr">格式：yyyy-MM-dd HH:mm:ss</param>
        /// <returns></returns>
        public bool CheckDate(string datestr)
        {
            if (string.IsNullOrEmpty(datestr))
                return false;

            DateTime dtOut;

            if (!DateTime.TryParseExact(datestr, "yyyy-MM-dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dtOut))
                return false;

            return true;
        }

        public ActionResult question(string userid, string matchid)
        {
            if (string.IsNullOrEmpty(matchid) || string.IsNullOrEmpty(userid))
            {
                ViewBag.result = "-2";
            }
            else
            {
                ViewBag.result = "1";
                ViewBag.mid = matchid;
                ViewBag.uid = userid;
            }

            return View();
        }

        [HttpPost]
        public ActionResult question(string hidmid, string hiduid, FormCollection fc)
        {
            string mid = fc["hidmid"];
            string uid = fc["hiduid"];

            if (string.IsNullOrEmpty(mid) || string.IsNullOrEmpty(uid))
            {
                ViewBag.result = "-1";
            }
            else
            {
                List<tblanswer> lst = new List<tblanswer>();
                for (int i = 1; i <= 8; i++)
                {
                    tblanswer tbl = new tblanswer();
                    tbl.answerid = Guid.NewGuid().ToString();
                    tbl.matchid = mid;
                    tbl.questionid = "q" + i.ToString();
                    tbl.result = fc["q" + i.ToString()];
                    tbl.updtime = DateTime.Now;
                    tbl.userid = uid;
                    lst.Add(tbl);
                }

                ApiBll bll = new ApiBll();

                bll.Answer(lst);

                ViewBag.result = "0";
            }

            return View();
        }
    }
}
