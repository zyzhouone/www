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
    public class groupController : BaseController
    {
        //
        // GET: /group/

        public ActionResult my()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            GroupBll bll = new GroupBll();
            return View(bll.GetCompanyByUserid(UserInfo.userid));
        }

        [HttpPost]
        public ActionResult my(FormCollection fc)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            var model = new tblcompany();

            model.Area = fc["Area"];
            model.Contacts = fc["Contacts"];
            model.Moblie = fc["Moblie"];
            model.Name = fc["Name"];
            model.userid = UserInfo.userid;

            GroupBll bll = new GroupBll();
            int res = bll.UpdateCompany(model);
            if (res < 0)
                model.Status = -999;
            else
                model.Status = -888;

            return View(model);
        }

        public ActionResult import()
        {
            GroupBll bll = new GroupBll();
            var list = bll.GetMatch("1");

            StringBuilder sbt = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0)
                    sbt.AppendFormat("<option value='{0}' selected>{1}</option>", list[i].Match_id, list[i].Match_name);
                else
                    sbt.AppendFormat("<option value='{0}'>{1}</option>", list[i].Match_id, list[i].Match_name);
            }
            ViewBag.match = sbt.ToString();
            ViewBag.flag = "0";

            return View();
        }

        public FileStreamResult DownFile()
        {
            string file = Server.MapPath("~/templates/group_import.xlsx");
            return File(new FileStream(file, FileMode.Open), "application/octet-stream", Server.UrlEncode("定向赛团体报名统计表.xlsx"));
        }

        [HttpPost]
        public ActionResult import(FormCollection fc, HttpPostedFileBase file)
        {
            if (file == null)
            {
                GroupBll bll = new GroupBll();
                var list = bll.GetMatch("1");

                StringBuilder sbt = new StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Match_id == fc["match"])
                        sbt.AppendFormat("<option value='{0}' selected>{1}</option>", list[i].Match_id, list[i].Match_name);
                    else
                        sbt.AppendFormat("<option value='{0}'>{1}</option>", list[i].Match_id, list[i].Match_name);
                }
                ViewBag.match = sbt.ToString();
                ViewBag.flag = "-1";

                return View();
            }
            else
            {
                string filename = string.Format("{0}{1}", Guid.NewGuid().ToString(), System.IO.Path.GetExtension(file.FileName));
                file.SaveAs(System.IO.Path.Combine(Server.MapPath("~/upload/file"), filename));
                return RedirectToAction("confirm", new { matchid = fc["match"], fid = filename });
            }
        }

        /// <summary>
        /// 导入确认
        /// </summary>
        /// <param name="matchid"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        public ActionResult confirm(string matchid, string fid)
        {
            try
            {
                ViewBag.error = "0";
                ViewBag.matchid = matchid;
                ViewBag.fid = fid;

                List<tblmatchentity> lstMatchusers = new List<tblmatchentity>();

                DataTable data = NpoiHelper.XlSToDataTable(System.IO.Path.Combine(Server.MapPath("~/upload/file"), fid), "TTBM", 0);

                if (data == null || data.Rows.Count < 1)
                    return View(lstMatchusers);

                GroupBll bll = new GroupBll();
                tblmatch match = bll.GetMatchById(matchid);

                ViewBag.matchname = HttpUtility.HtmlEncode(match.Match_name);

                StringBuilder sbtError = new StringBuilder();

                int sn = 0;
                string lineid = "";
                string teamno = "";
                string teamname = "";
                string company = "";
                int year = 0;

                foreach (DataRow row in data.Rows)
                {
                    sbtError.Clear();
                    year = 0;

                    if (string.IsNullOrEmpty(row["队员姓名"].ToString().Trim()))
                        continue;

                    //记录序号，以标记团队
                    if (!string.IsNullOrEmpty(row["序号"].ToString().Trim()))
                        sn = int.Parse(row["序号"].ToString().Trim());

                    if (!string.IsNullOrEmpty(row["路线名称"].ToString().Trim()))
                    {
                        lineid = row["路线名称"].ToString().Trim();

                        var d = bll.GetLineByName(lineid);
                        if (d == null)
                            sbtError.AppendFormat("[路线名称:{0}]不存在;", lineid);
                    }
                    if (!string.IsNullOrEmpty(row["队列号"].ToString().Trim()))
                        teamno = row["队列号"].ToString().Trim();

                    if (!string.IsNullOrEmpty(row["队名(6个字符以内)"].ToString().Trim()))
                        teamname = row["队名(6个字符以内)"].ToString().Trim();

                    if (!string.IsNullOrEmpty(row["单位名称"].ToString().Trim()))
                        company = row["单位名称"].ToString().Trim();


                    if (string.IsNullOrEmpty(row["性别"].ToString().Trim()))
                        sbtError.Append("[性别]不能为空;");

                    if (string.IsNullOrEmpty(row["身份证/护照"].ToString().Trim()))
                        sbtError.Append("[身份证/护照]不能为空;");
                    else
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(row["身份证/护照"].ToString().Trim(), @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$"))
                            sbtError.Append("[身份证/护照]格式错误;");
                    }
                    if (!System.Text.RegularExpressions.Regex.IsMatch(row["手机号码"].ToString().Trim(), @"^[1]+[0-9]+\d{9}"))
                        sbtError.AppendFormat("[手机号码:{0}]格式错误;", row["手机号码"]);

                    if (!int.TryParse(row["年龄"].ToString().Trim(), out year))
                    {
                        sbtError.AppendFormat("[年龄:{0}]是否输入及正确;", row["年龄"]);
                    }
                    if (sbtError.Length > 0)
                        ViewBag.error = "-1";

                    tblmatchentity muser = new tblmatchentity();
                    muser.Pnov = sn.ToString();
                    muser.Teamname = teamname;
                    muser.Cardno = row["身份证/护照"].ToString().Trim();
                    muser.Cardtype = "1";
                    muser.Createtime = DateTime.Now;//.ToString("yyyy-MM-dd");
                    //muser.Leader = row["队员编号"].ToString().Trim() == "队长" ? 1 : 0;
                    muser.Match_Id = "";
                    muser.Matchuserid = Guid.NewGuid().ToString();
                    muser.Mobile = row["手机号码"].ToString().Trim();
                    muser.Nickname = row["队员姓名"].ToString().Trim();
                    muser.Lineno = lineid;
                    muser.LeaderM = row["队员编号"].ToString().Trim() == "队长" ? "是" : "";
                    muser.Sexy = row["性别"].ToString().Trim() == "男" ? 1 : 0;
                    muser.Age = year;
                    muser.Mono = row["是否健康"].ToString().Trim();
                    muser.Content = HttpUtility.HtmlEncode(sbtError.ToString());

                    lstMatchusers.Add(muser);
                }

                return View(lstMatchusers);
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(this.GetType());
                log.Error(ex);

                ViewBag.error = "-2";
                return View();
            }
        }

        /// <summary>
        /// 开始导入数据
        /// </summary>
        /// <param name="matchid"></param>
        /// <param name="fid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult beginimp(string matchid, string fid)
        {
            try
            {
                List<tblmatchentity> lstMatchusers = new List<tblmatchentity>();

                DataTable data = NpoiHelper.XlSToDataTable(System.IO.Path.Combine(Server.MapPath("~/upload/file"), fid), "TTBM", 0);

                GroupBll bll = new GroupBll();
                tblmatch match = bll.GetMatchById(matchid);

                int sn = 0;
                string lineid = "";
                string teamno = "";
                string teamname = "";
                string company = "";
                int year = 0;
                int dm = 0;

                foreach (DataRow row in data.Rows)
                {
                    year = 0;

                    if (string.IsNullOrEmpty(row["队员姓名"].ToString().Trim()))
                        continue;

                    //记录序号，以标记团队
                    if (!string.IsNullOrEmpty(row["序号"].ToString().Trim()))
                        sn = int.Parse(row["序号"].ToString().Trim());

                    if (!string.IsNullOrEmpty(row["路线名称"].ToString().Trim()))
                    {
                        var d = bll.GetLineByName(row["路线名称"].ToString().Trim());
                        lineid = d.Lineid;
                    }

                    if (!string.IsNullOrEmpty(row["队列号"].ToString().Trim()))
                    {
                        teamno = row["队列号"].ToString().Trim();
                        int.TryParse(teamno, out dm);
                    }
                    if (!string.IsNullOrEmpty(row["队名(6个字符以内)"].ToString().Trim()))
                        teamname = row["队名(6个字符以内)"].ToString().Trim();

                    if (!string.IsNullOrEmpty(row["单位名称"].ToString().Trim()))
                        company = row["单位名称"].ToString().Trim();

                    tblmatchentity muser = new tblmatchentity();
                    muser.Pnov = sn.ToString();
                    muser.Teamname = teamname;
                    muser.Teamno = dm;
                    muser.Cardno = row["身份证/护照"].ToString().Trim();
                    muser.Cardtype = "1";
                    muser.Leader = row["队员编号"].ToString().Trim() == "队长" ? 1 : 0;
                    muser.Match_Id = matchid;
                    muser.Mobile = row["手机号码"].ToString().Trim();
                    muser.Nickname = row["队员姓名"].ToString().Trim();
                    muser.Lineno = lineid;
                    muser.Sexy = row["性别"].ToString().Trim() == "男" ? 1 : 0;
                    muser.Passwd = company;
                    int.TryParse(row["年龄"].ToString().Trim(), out year);
                    muser.Age = year;
                    muser.Mono = row["是否健康"].ToString().Trim();

                    lstMatchusers.Add(muser);
                }

                int count = 0;
                TeamRegBll tbll = new TeamRegBll();
                int res = tbll.ImpTeams(lstMatchusers, ref count);
                return RedirectToAction("importsuccess", new { m = HttpUtility.UrlEncode(match.Match_name), s = HttpUtility.UrlEncode(string.Format("已成功导入{0}个队伍，{1}个队员信息", count, res)) });
            }
            catch (Exception ex)
            {
                ILog log = LogManager.GetLogger(this.GetType());
                log.Error(ex);
                return new EmptyResult();
            }
        }

        public ActionResult importsuccess(string m, string s)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            GroupBll bll = new GroupBll();
            var com = bll.GetCompanyByUserid(UserInfo.userid);
            if (com != null)
                ViewBag.com = com.Name;
            else
                ViewBag.com = "";

            ViewBag.match = m;
            ViewBag.msg = s;
            return View();
        }

        public ActionResult matchlist()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            GroupBll bll = new GroupBll();
            return View(bll.GetMymatch(UserInfo.userid));
        }

        public ActionResult myfix()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            ViewBag.flag = 0;
            return View();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult myfix(FormCollection fc)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(fc["password"], "MD5");
            string md5newPwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(fc["npassword"], "MD5");

            MembershipBll bll = new MembershipBll();
            int res = bll.UpdatePwd(UserInfo.userid, md5Pwd, md5newPwd);
            ViewBag.flag = res;
            return View();
        }

        public ActionResult myinfo()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            MembershipBll bll = new MembershipBll();
            return View(bll.GetMyinfo(UserInfo.userid));
        }
    }
}
