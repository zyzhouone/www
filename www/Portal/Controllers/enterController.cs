using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Text;
using log4net;

using Model;
using BLL;
using Utls;
using System.Net;
using Newtonsoft.Json;

namespace Portal.Controllers
{
    public class enterController : BaseController
    {
        //
        // GET: /register/

        public ActionResult Index(string matchid)
        {
            ViewBag.id = matchid;
            return View();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public JsonResult GetSMS(string mobile)
        {
            TeamRegBll bll = new TeamRegBll();
            int res = bll.Step1(mobile);

            if (res == 1)
                return RepReurnOK();
            else
                return RepReurnError("获取验证码时出现错误");
        }

        public JsonResult CheckSMS(string mobile, string vercode, string matchid)
        {
            TeamRegBll bll = new TeamRegBll();

            string res = bll.CheckSms(mobile, vercode, matchid);

            if (res == "-1")
                return RepReurnError("输入的验证码错误");
            else if (res == "-2")
                return RepReurnError("抱歉，这个号码已经被注册过");
            else
                return RepReurn(0, res, null);
        }

        public ActionResult Protocol(string matchid)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            TeamRegBll bll = new TeamRegBll();
            var tm = bll.GetTeamByum(UserInfo.userid, matchid);
            if (tm != null)
            {
                //已经注册过队伍，则跳转到对应步骤
                if (string.IsNullOrEmpty(tm.Lineid))
                    return RedirectToAction("Step3", new { tid = tm.Teamid });
                else
                    return RedirectToAction("Step4", new { tid = tm.Teamid });
            }

            var mt = bll.GetMatchById(matchid);
            if (mt != null)
            {
                ViewBag.matchname = mt.Match_name;
                ViewBag.notice = mt.Notice;
            }

            ViewBag.tid = matchid;
            return View();
        }

        public ActionResult Step2(string uid, string matchid)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            TeamRegBll bll = new TeamRegBll();

            var mt = bll.GetMatchById(matchid);
            if (mt != null)
            {
                ViewBag.matchname = mt.Match_name;
                if (mt.Status != "1")
                    return RedirectToAction("bulletin", "dx");
            }
            else
                return RedirectToAction("bulletin", "dx");

            var tm = bll.GetTeamByum(UserInfo.userid, matchid);
            if (tm != null)
            {
                //已经注册过队伍，则跳转到对应步骤
                if (string.IsNullOrEmpty(tm.Lineid))
                    return RedirectToAction("Step3", new { tid = tm.Teamid });
                else
                    return RedirectToAction("Step4", new { tid = tm.Teamid });
            }

            ViewBag.id = UserInfo.userid;
            ViewBag.tid = matchid;
            return View();
        }

        public JsonResult CheckTname(string matchid, string tname)
        {
            TeamRegBll bll = new TeamRegBll();
            bool res = bll.CheckTname(matchid, tname);

            if (res)
                return RepReurnError("已经存在相同的队伍名称");
            else
                return RepReurnOK();
        }

        public ActionResult RegTname(string id, string tid, string tname, string tcom, string pwd)
        {
            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "MD5");
            string teamid = "";
            TeamRegBll bll = new TeamRegBll();
            string res = bll.RegTname(id, tid, tname, tcom, md5Pwd, ref teamid);

            if (res == "-3")
                return RedirectToAction("Step3", new { tid = teamid });
            else if (res == "-2")
                return RepReurnError("你的基本信息还不完整，请先完善你的信息");
            else if (res == "-1")
                return RepReurnError("已经存在相同的队伍名称");
            else
                return RepReurn(0, res, null);
        }

        public ActionResult Step3(string tid, string tp)
        {
            ViewBag.tid = tid;
            TeamRegBll bll = new TeamRegBll();
            var mt = bll.GetMatchByTeamid(tid);
            if (mt != null)
            {
                ViewBag.matchname = mt.Match_name;
                ViewBag.tname = mt.Teamname;

                if (mt.Status != "1"&&string.IsNullOrEmpty(tp))
                    return RedirectToAction("bulletin", "dx");
            }

            ViewBag.tp = tp;
            return View(bll.GetLines(tid));
        }

        public JsonResult getlines(string lid,string tp)
        {
            TeamRegBll bll = new TeamRegBll();
            var res = bll.getLines(lid);
            return RepReurnOK(res);
        }

        public JsonResult SelLine(string tid, string lid, string tp)
        {
            if (UserInfo == null)
                return RepReurnError("请登录网站");

            TeamRegBll bll = new TeamRegBll();

            if (string.IsNullOrEmpty(tp) || tp != "1")
            {
                int res = bll.SelectLine(tid, lid);

                if (res < 0)
                    return RepReurnError("操作中出现错误");
                else
                    return RepReurnOK();
            }
            else
            {
                int res = bll.ChangeLine(UserInfo.userid, tid, lid);

                if (res == -2)
                    return RepReurnError("路线传递错误");
                else if (res == -3)
                    return RepReurnError("你不是队长,不能操作");
                else if (res == -4)
                    return RepReurnError("目前状态不能更换路线");
                else if (res == -1)
                    return RepReurnError("队伍不存在");
                else if (res == -5)
                    return RepReurnError("这个路线不能修改");
                else if (res == -6)
                    return RepReurnError("你目前路线是此路线，不能再次选择");
                else if (res < 0)
                    return RepReurnError("操作中出现错误");
                else
                    return RepReurnOK();
            }
        }

        public ActionResult Step4(string tid, string tp)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            ViewBag.tp = tp;
            TeamRegBll bll = new TeamRegBll();
            ViewBag.tid = tid;
            var mbll = new MembershipBll();
            var usr = mbll.GetUserByTeamId(tid);
            //var usr = bll.GetMatchuserById(tid, UserInfo.userid);

            var line = bll.GetLineById(tid);
            if (line == null)
                return RedirectToAction("Step3", new { tid = tid });

            ViewBag.cnt = line.Playercount.Value - 1;
            ViewBag.lsts = line.Status;

            if (line.Matchid == "6a61b95b-2d5d-4373-abaf-bf4e4c438800")
                ViewBag.lname = line.Lineno;
            else
            {
                if (string.IsNullOrEmpty(line.Lineno))
                    ViewBag.lname = line.Linename;
                else
                    ViewBag.lname = line.Lineno + "-" + line.Linename;
            }               

            var mt = bll.GetMatchByTeamid(tid);
            if (mt != null)
            {
                ViewBag.tp = ((mt.Status == "1" || mt.Status == "2" || mt.Status == "3") && mt.Paystatus == 6 && mt.chglines == "1") ? "1" : "";

                ViewBag.matchstatus = mt.Status;
                ViewBag.matchname = mt.Match_name;
                ViewBag.tname = mt.Teamname;
                ViewBag.leader = (mt.userid == UserInfo.userid ? "1" : "0");
                ViewBag.date2 = mt.Date2.Value.ToString("yyyy年MM月dd日");
                if (mt.Paystatus == 1 || mt.Paystatus == 0)
                    return RedirectToAction("Step5", new { tid = tid });
            }

            return View(usr);
        }

        [HttpPost]
        public JsonResult inputmb(List<tblmatchusers> mus, string tid)
        {
            TeamRegBll bll = new TeamRegBll();
            string res = bll.InputMb(mus, tid);

            if (res == "-1")
                return RepReurnError("操作中出现错误");
            else if (res == "-2")
                return RepReurnError("请至少邀请1名女性队员");
            else if (res == "-3")
                return RepReurnError("团队中有成员小于16岁");
            else if (res == "-4")
                return RepReurnError("团队中有成员大于60岁");
            else if (res == "-5")
                return RepReurnError("团队中有成员姓名为空");
            else if (res == "-6")
                return RepReurnError("团队中有成员身份证为空");
            else if (res == "-7")
                return RepReurnError("团队中有成员年龄为空");
            else if (res == "-8")
                return RepReurnError("你的队伍没有按规定人数邀请");
            else if (res == "-9")
                return RepReurnError("请输入附加信息");
            else if (res == "-10")
                return RepReurnError("请输入附加信息");
            else if (!string.IsNullOrEmpty(res))
                return RepReurnError(res);
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult addplayer(string tid, string m)
        {
            TeamRegBll bll = new TeamRegBll();
            int res = bll.AddMatchuser(tid, m);
            if (res == -3)
                return RepReurnError("不能邀请自己");
            else if (res == -2)
                return RepReurnError("已经被邀请");
            else if (res == -1)
                return RepReurnError(" 邀请的队员没有注册");
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult delplayer(string uid)
        {
            TeamRegBll bll = new TeamRegBll();
            int res = bll.DelMatchuser(uid);
            if (res <= 0)
                return RepReurnError("删除中出现错误");
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult addextra(tblmatchextra extra)
        {
            TeamRegBll bll = new TeamRegBll();

            int res = bll.AddExtra(extra.extype, extra.teamid, extra.info1, extra.info2, extra.info3);

            if (res > 0)
                return RepReurnOK();
            else if(res==-80)
                return RepReurnError("宝宝年龄需要在7-16周岁之间");
            else
                return RepReurnError("操作中出现错误");
        }

        public JsonResult getextra(string r, string teamid)
        {
            TeamRegBll bll = new TeamRegBll();
            return RepReurnOK(bll.GetExtra(teamid));
        }

        [HttpPost]
        public ActionResult postimg()
        {
            //string path = @"C:\DX\DX_image\uploadimg";// Server.MapPath("~/uploadimg");
            string path = Server.MapPath("~/uploadimg");
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            string filename = string.Format("{0}{1}", Guid.NewGuid(), System.IO.Path.GetExtension(Request.Files[0].FileName));

            Request.Files[0].SaveAs(System.IO.Path.Combine(path, filename));
            //Request.Files[0].SaveAs(System.IO.Path.Combine(Server.MapPath("~/uploadimg"), filename));

            return RedirectToAction("loadimg", new { file = "/uploadimg/" + filename });
        }

        public ActionResult StepFM(string tid)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            TeamRegBll bll = new TeamRegBll();
            ViewBag.tid = tid;
            var mbll = new MembershipBll();
            var usr = mbll.GetUserByTeamId(tid);
            //var usr = bll.GetMatchuserById(tid, UserInfo.userid);

            var line = bll.GetLineById(tid);
            if (line == null)
                return RedirectToAction("Step3", new { tid = tid });

            ViewBag.cnt = line.Playercount.Value - 1;
            ViewBag.lsts = line.Status;

            if (line.Matchid == "6a61b95b-2d5d-4373-abaf-bf4e4c438800")
                ViewBag.lname = line.Lineno;
            else
            {
                if (string.IsNullOrEmpty(line.Lineno))
                    ViewBag.lname = line.Linename;
                else
                    ViewBag.lname = line.Lineno + "-" + line.Linename;
            }

            var mt = bll.GetMatchByTeamid(tid);
            if (mt != null)
            {
                ViewBag.tp = ((mt.Status == "1" || mt.Status == "2" || mt.Status == "3") && mt.Paystatus == 6 && mt.chglines == "1") ? "1" : "";

                ViewBag.matchstatus = mt.Status;
                ViewBag.matchname = mt.Match_name;
                ViewBag.tname = mt.Teamname;
                ViewBag.leader = (mt.userid == UserInfo.userid ? "1" : "0");
                ViewBag.date2 = mt.Date4.Value.ToString("yyyy年MM月dd日");
                if (mt.Paystatus == 1 || mt.Paystatus == 0)
                    return RedirectToAction("Step5", new { tid = tid });
            }

            return View(usr);
        }

        [HttpPost]
        public JsonResult CompFM(List<tblmatchusers> mus, string tid)
        {
            TeamRegBll bll = new TeamRegBll();
            string res = bll.CompFM(tid);

            if (res == "-1")
                return RepReurnError("操作中出现错误");
            else if (res == "-2")
                return RepReurnError("请至少邀请1名女性队员");
            else if (res == "-3")
                return RepReurnError("团队中有成员小于16岁");
            else if (res == "-4")
                return RepReurnError("团队中有成员大于60岁");
            else if (res == "-5")
                return RepReurnError("团队中有成员姓名为空");
            else if (res == "-6")
                return RepReurnError("团队中有成员身份证为空");
            else if (res == "-7")
                return RepReurnError("团队中有成员年龄为空");
            else if (res == "-8")
                return RepReurnError("你的队伍没有按规定人数邀请");
            else if (res == "-9")
                return RepReurnError("请输入附加信息");
            else if (res == "-10")
                return RepReurnError("请输入附加信息");
            else if (res == "-98")
                return RepReurnError("未找到邀请码信息");
            else if (res == "-99")
                return RepReurnError("邀请码目前还不能使用，请到日期使用");
            else if (!string.IsNullOrEmpty(res))
                return RepReurnError(res);
            else
                return RepReurnOK();
        }

        public ActionResult uploadimg()
        {
            return View();
        }

        public ActionResult loadimg(string file)
        {
            ViewBag.src = file;
            return View();
        }

        public JsonResult getplayer(string tid, string r)
        {
            TeamRegBll bll = new TeamRegBll();
            return RepReurnOK(bll.GetMatchuserByTeamId(tid));
        }

       

        /// <summary>
        /// zzy 2019-01-22
        /// 检查是否预约
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public JsonResult CheckReserve(string tid)
        {
            TeamRegBll bll = new TeamRegBll();
            var team = bll.GetTeamById(tid);

            WebClient MyWebClient = new WebClient();
            string strUrl = string.Format("https://demomini.fooddecode.com/order/checkReserve?teamId={0}&userId={1}&matchId={2}", team.teamid, team.Userid, team.match_id);

            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            byte[] pageData = MyWebClient.DownloadData(strUrl);


            String strJson = Encoding.UTF8.GetString(pageData) ?? "";
            ResponseModel rb = JsonConvert.DeserializeObject<ResponseModel>(strJson);
            if(rb.status==1)
            {
                //获取验证码
                strUrl = string.Format("https://demomini.fooddecode.com/order/getVCode?sessionKey={0}", team.Userid);

                pageData = MyWebClient.DownloadData(strUrl);


                strJson = Encoding.UTF8.GetString(pageData) ?? "";
                rb = JsonConvert.DeserializeObject<ResponseModel>(strJson);
            }

            return Json(rb, JsonRequestBehavior.AllowGet);
        }
       
        /// <summary>
        /// zzy 2019-01-22
        /// 抢购订单
        /// </summary>
        /// <param name="tid"></param>
        /// <returns></returns>
        public JsonResult CreatePrepay(string tid, string vCode)
        {
            TeamRegBll bll = new TeamRegBll();
            var team = bll.GetTeamById(tid);

            WebClient MyWebClient = new WebClient();
            string strUrl = string.Format("https://demomini.fooddecode.com/order/createPrepay?sessionKey={0}&userId={1}&teamId={2}&matchId={3}&vcode={4}&linesId={5}&payType={6}", team.Userid, team.Userid, team.teamid, team.match_id, vCode, team.Linesid, "ZFB");

            MyWebClient.Credentials = CredentialCache.DefaultCredentials;
            byte[] pageData = MyWebClient.DownloadData(strUrl);


            String strJson = Encoding.UTF8.GetString(pageData) ?? "";
            ResponseModel rb = JsonConvert.DeserializeObject<ResponseModel>(strJson);
            if (rb.status == 1)
            {
                //查询是否可以支付
                strUrl = string.Format("https://demomini.fooddecode.com/order/queryPrepay?sessionKey={0}&orderNo={1}", team.Userid, rb.data);

                pageData = MyWebClient.DownloadData(strUrl);
                strJson = Encoding.UTF8.GetString(pageData) ?? "";
                rb = JsonConvert.DeserializeObject<ResponseModel>(strJson);
            }
            return Json(rb, JsonRequestBehavior.AllowGet);
        }
       

        public ActionResult Step5(string tid)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            ViewBag.quarty = -1;
            ViewBag.recnt = 0;
            ViewBag.teamid = tid;

            TeamRegBll bll = new TeamRegBll();
            var ms = bll.GetMatchUsersByTeamid(tid);
            if (ms != null && ms.Count > 0)
            {
                ViewBag.match = ms[0].Match_name;
                ViewBag.rq = ms[0].Date3.Value.ToString("yyyy-MM-dd HH:mm");
                ViewBag.rq4 = ms[0].Date4.Value.ToString("yyyy-MM-dd HH:mm");
                ViewBag.lname = ms[0].linename;
                ViewBag.dw = ms[0].Teamname;
                ViewBag.tstatus = ms[0].Teamstatus.Value.ToString();
                ViewBag.begin = (DateTime.Now - ms[0].Date3.Value).TotalSeconds >= 0 ? "1" : "2";
                ViewBag.teamtype = ms[0].teamtype.ToString();

                if (ms[0].Match_Id == "6a61b95b-2d5d-4373-abaf-bf4e4c438800")
                    ViewBag.lname = ms[0].Lineno;
                else
                {
                    if (string.IsNullOrEmpty(ms[0].Lineno))
                        ViewBag.lname = ms[0].linename;
                    else
                        ViewBag.lname = ms[0].Lineno + "-" + ms[0].linename;
                }

                if (ms.Any(p => p.userid == UserInfo.userid && p.Leader == 1))
                    ViewBag.leader = "1";
                ViewBag.mstatus = ms[0].mstatus;
                ViewBag.recnt = ms.Where(p => !string.IsNullOrEmpty(p.Mono)).Count();

                if (ViewBag.mstatus == "3" && ViewBag.tstatus == "0" && ViewBag.leader == "1")
                {
                    ViewBag.recnt = bll.GetReplayerCnt(tid);
                }
                //if (ms[0].mstatus == "3" && ms[0].Teamstatus == 1)
                //{
                //  int cnt=  bll.GetFOrders(ms[0].Match_Id);
                //  ViewBag.quarty = 500 - cnt;              
                //}
            }
            else
            {
                ViewBag.match = "";
                ViewBag.rq = "";
                ViewBag.dw = "";
                ViewBag.leader = "";
                ViewBag.tstatus = "";
                ViewBag.begin = "";
                ViewBag.mstatus = "";
                ViewBag.teamtype = "";
            }

            ViewBag.tid = tid;
            return View(ms);
        }

        [HttpPost]
        public JsonResult editplayer(tblmatchusers tm)
        {
            TeamRegBll bll = new TeamRegBll();
            int res = bll.UpdatelMatchuser(tm);

            if (res > 0)
                return RepReurnOK();
            else if (res == -2)
                return RepReurnError("这个身份证号已经被注册");
            else
                return RepReurnError("编辑中出现错误");
        }

        [HttpPost]
        public JsonResult checkpay(string tid)
        {
            MembershipBll bll = new MembershipBll();
            var order = bll.GetOrderByTeamId(tid);
            if(order.Status==2)
                return RepReurnError("已经支付成功");

            var line = bll.GetLineById(tid);
            var tm = new TeamRegBll().GetTeamById(tid);
            if(tm.Teamtype==1)
                return RepReurnOK();

            if (line.Linesid == "207bfe24-bef3-4e1d-95dc-22228e0e04e8")
                return RepReurnError("抱歉！您所选择的线路类型已经支付结束");
            else if (line.Linesid == "119eeb28-935e-479c-b157-1dd17e6427f6" || line.Linesid == "7e34f192-f909-4f2d-9f37-99a9ad7df092" || line.Linesid == "f0efcd04-10d0-4a96-91ea-1f2e7e374250")
                return RepReurnError("抱歉！您所选择的线路类型目前还没有开始支付");

            string msg = bll.CheckPay(order.Id, line.Paycount.Value, order.Match_Id);

            if (!string.IsNullOrEmpty(msg))
                return RepReurnError(msg);
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult replayer(string mobile, string mid)
        {
            if (UserInfo == null)
                return RepReurnError("请登录");

            MembershipBll bll = new MembershipBll();
            int res = bll.Replayer(UserInfo.userid, mobile, mid);

            if (res > 0)
                return RepReurnOK();
            else if (res == -1)
                return RepReurnError("更换队员已经参加了比赛");
            else if (res == -2)
                return RepReurnError("你不是队长，没有权限更换");
            else if (res == -3)
                return RepReurnError("替换的队员不存在");
            else
                return RepReurnError("输入的手机号没有注册或者信息不完善");
        }

        [HttpPost]
        public JsonResult releader(string mid)
        {
            if (UserInfo == null)
                return RepReurnError("请登录");

            MembershipBll bll = new MembershipBll();
            int res = bll.ReLeader(UserInfo.userid, mid);

            if (res > 0)
                return RepReurnOK();
            else if (res == -1)
                return RepReurnError("你不是队长，没有权限更换");
            else
                return RepReurnError("操作中出现错误");
        }
    }
}
