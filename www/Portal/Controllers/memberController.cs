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

namespace Portal.Controllers
{
    public class memberController : BaseController
    {
        //
        // GET: /login/
        public ActionResult login(string r,string url)
        {
            if (!string.IsNullOrEmpty(r) && r == "c")
            {
                //保存用户信息
                HttpCookie cookie = HttpContext.Request.Cookies["coc_cookie_info"];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddDays(-1);
                    HttpContext.Response.SetCookie(cookie);

                    return RedirectToAction("login", "member");
                }
            }

            return View();
        }

        public ActionResult my()
        {
            MembershipBll bll = new MembershipBll();
            return View(bll.GetUserById(UserInfo.userid));
        }

        [HttpPost]
        public ActionResult my(FormCollection fc)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            var model = new tblusers();

            DateTime sr;
            if (DateTime.TryParse(fc["txtsr"], out sr))
                model.birthday = sr;
            model.cardno = fc["txtcard"];
            model.cardtype = fc["cbxcardtype"];
            model.Mobile = fc["txtmobile"];
            model.Name = fc["txtname"];
            model.sexy = fc["cbxsexy"];
            model.Isupt = "1";

            model.userid = UserInfo.userid;

            MembershipBll bll = new MembershipBll();
            int res = bll.UpdateUser(model);
            if (res < 0)
                model.Status = -999;
            else
                model.Status = -888;

            return View(model);
        }

        public ActionResult mymatch()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            MembershipBll bll = new MembershipBll();

            return View(bll.GetMymatch(UserInfo.userid));
        }

        public ActionResult myinfo()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            MembershipBll bll = new MembershipBll();
            return View(bll.GetMyinfo(UserInfo.userid));
        }

        public ActionResult myfix()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            ViewBag.flag = 0;
            return View();
        }

        public ActionResult information(string id)
        {
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

        public JsonResult checkuser(string acc, string pwd)
        {
            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "MD5");

            MembershipBll bll = new MembershipBll();
            tblusers user = bll.GetUser(acc, md5Pwd);

            string ip;
            GetWebClientIp(out ip);

            if (user == null)
            {
                bll.OptLog(1, "", ip, string.Format("用户[{0}]登录失败",acc));
                return RepReurnError("用户名或者密码输入错误");
            }
            else
            {
                bll.UpdateUserLastTime(user.userid);

                bll.OptLog(1, user.userid, ip, string.Format("用户[{0}]成功登录", acc));

                //保存用户信息
                HttpCookie cookie = new HttpCookie("coc_cookie_info");
                cookie.HttpOnly = false;
                //cookie.Expires = DateTime.Now.AddDays(7);
                System.Collections.Specialized.NameValueCollection nv = new System.Collections.Specialized.NameValueCollection();
                nv.Add("uuid", user.userid);
                nv.Add("uunm", user.Name);
                cookie.Values.Add(nv);

                HttpContext.Response.SetCookie(cookie);

                return RepReurn(0, user.Type, null);
            }
        }

        public ActionResult register()
        {
            return View();
        }

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public JsonResult GetSMS(string mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Trim().Length < 1)
                return RepReurnError("请输入手机号");

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"^1(3|5|7|8)\d{9}$");
            if (!regex.IsMatch(mobile))
                return RepReurnError("手机号错误");

            if (mobile.Trim().Length > 15)
                return RepReurnError("手机号输入错误");
            if (mobile.Trim() == "4654984351984156")
                return RepReurnError("兄弟请收手吧！这样不好.");

            string ip;

            if (!GetWebClientIp(out ip))
                return RepReurnError("错误访问");

            MembershipBll bll = new MembershipBll();
            SMSResponse rep = new SMSResponse();
            int res = bll.GetSMS(mobile.Trim(), ip, ref rep);

            ILog log = LogManager.GetLogger(this.GetType());
            log.Error(string.Format("[{0}-{1}]", rep.error, rep.msg));

            if (res == 1)
                return RepReurnOK();
            else if (res == -1)
                return RepReurnError("这个手机号已经被注册");
            else if (res == -2)
                return RepReurnError("获取验证码次数太频繁，请于明日再进行");
            else if (res == -3)
                return RepReurnError("今日获取验证码次数太频繁，已被限制");
            else
                return RepReurnError("获取验证码时出现错误");
        }

        /// <summary>
        /// 判断接口是否网站域调用
        /// </summary>
        /// <param name="CustomerIP"></param>
        /// <returns></returns>
        private bool GetWebClientIp(out string CustomerIP)
        {
            CustomerIP = "";

            try
            {
                if (System.Web.HttpContext.Current == null
            || System.Web.HttpContext.Current.Request == null
            || System.Web.HttpContext.Current.Request.ServerVariables == null)
                    return false;

                bool normal = false;

                string allhttp = System.Web.HttpContext.Current.Request.ServerVariables.Get("ALL_HTTP");
                if (!string.IsNullOrEmpty(allhttp) && allhttp.Contains("HTTP_REFERER"))
                    normal = true;
                else
                {
                    string allraw = System.Web.HttpContext.Current.Request.ServerVariables.Get("ALL_RAW");
                    if (!string.IsNullOrEmpty(allraw) && allraw.Contains("Referer"))
                        normal = true;
                }

                //CDN加速后取到的IP simone 090805
                CustomerIP = System.Web.HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                    return normal;

                CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!String.IsNullOrEmpty(CustomerIP))
                    return normal;

                if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                if (string.Compare(CustomerIP, "unknown", true) == 0)
                    CustomerIP = System.Web.HttpContext.Current.Request.UserHostAddress;

                return normal;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 验证码检验
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public JsonResult CheckSMS(string mobile, string vercode)
        {
            MembershipBll bll = new MembershipBll();
            string usrid = "";
            int res = bll.CheckSms(mobile, vercode, ref usrid);

            if (res >= 0)
                return RepReurn(0, usrid, null);
            else if (res == -2)
                return RepReurnError("抱歉，这个号码已经被注册过");
            else
                return RepReurnError("输入的验证码错误");
        }

        public ActionResult Setpassword(string id, string m)
        {
            ViewBag.id = id;
            ViewBag.m = m;

            return View();
        }

        public JsonResult Submitpwd(string id, string pwd)
        {
            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "MD5");

            MembershipBll bll = new MembershipBll();
            int res = bll.SubmitPwd(id, md5Pwd);

            if (res < 0)
                return RepReurnError("提交的数据不正确");
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult updpwd(string id, string pwd, string npwd)
        {
            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "MD5");
            string md5NPwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(npwd, "MD5");

            MembershipBll bll = new MembershipBll();
            int res = bll.UpdatePwd(id, md5Pwd, md5NPwd);

            if (res < 0)
                return RepReurnError("请检查输入的旧密码是否正确");
            else
                return RepReurnOK();
        }

        public ActionResult Success(string id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult forget()
        {
            return View();
        }

        public ActionResult modpassword(string m)
        {
            ViewBag.id = m;
            return View();
        }

        public ActionResult passwordsuccess()
        {
            return View();
        }

        /// <summary>
        /// 忘记密码，获取验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public JsonResult GetGSMS(string mobile)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.GetGSMS(mobile);

            if (res == -1)
                return RepReurnError("这个手机号不存在");
            else
                return RepReurnOK();
        }

        public JsonResult CheckGSMS(string mobile, string vercode)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.CheckGSms(mobile, vercode);

            if (res >= 0)
                return RepReurnOK();
            else
                return RepReurnError("输入的验证码错误");
        }

        public JsonResult repwd(string m, string pwd)
        {
            string md5Pwd = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(pwd, "MD5");

            MembershipBll bll = new MembershipBll();
            int res = bll.ResetPwd(m, md5Pwd);

            if (res < 0)
                return RepReurnError("不存在这个账户");
            else
                return RepReurnOK();
        }

        [HttpPost]
        public JsonResult Accept(string id, string uid)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.AcceptMatch(uid, id);

            if (res >= 0)
                return RepReurnOK();
            else if(res==-1)
                return RepReurnError("您已经接受了其余队伍或删除了报名数据,不能再次操作,请忽略此条信息");
            else if(res==-2)
                return RepReurnError("你已经参加了本次比赛的一个队伍，不能接受其他队伍");
            else
                return RepReurnError("请先完善基本信息再参加比赛");
        }

        [HttpPost]
        public JsonResult Reject(string id, string uid)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.RejectMatch(uid, id);

            if (res >= 0)
                return RepReurnOK();
            else
                return RepReurnError("您已经接受了其余队伍或删除了报名数据,不能再次操作,请忽略此条信息");
        }

        [HttpPost]
        public JsonResult delteam(string tid)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.DelTeam(tid);

            if (res >= 0)
                return RepReurnOK();
            else
                return RepReurnError("操作中出现错误");
        }

        public ActionResult center(string m)
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member");

            MembershipBll bll = new MembershipBll();
            //我的赛事
            ViewBag.mtlst = bll.GetMymatch(UserInfo.userid);
            //我的消息
            ViewBag.infolst = bll.GetMyinfo(UserInfo.userid);

            var mbll = new MembershipBll();
            var usr = mbll.GetUserById(UserInfo.userid);

            return View(usr);
        }

        [HttpPost]
        public JsonResult changeteam(tblteams tm)
        {
            if (UserInfo == null)
                return RepReurnError("请登录系统");

            MembershipBll bll = new MembershipBll();
            int res = bll.changeteam(tm);

            if (res >= 0)
                return RepReurnOK();
            else if (res == -1)
                return RepReurnError("队伍不存在");
            else if (res == -2)
                return RepReurnError("报名已经完成，不能修改");
            else if (res == -3)
                return RepReurnError("队伍名称重复，不能修改");
            else
                return RepReurnError("操作中出现错误");
        }

        [HttpPost]
        public JsonResult edituser(tblusers user)
        {
            if (!user.birthday.HasValue)
                return RepReurnError("请检查出生日期格式是否正确");

            MembershipBll bll = new MembershipBll();
            int res = bll.UpdateUser(user);

            if (res >= 0)
                return RepReurnOK();
            else if (res == -2)
                return RepReurnError("这个身份证号已经被注册");
            else if (res == -3)
                return RepReurnError("你的年龄不是在16-60之间");
            else
                return RepReurnError("操作中出现错误");
        }

        [HttpPost]
        public JsonResult AcceptRp(string id)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.AcceptReplay(id);

            if (res >= 0)
                return RepReurnOK();
            else if (res == -1)
                return RepReurnError("此消息已经过期");
            else if (res == -2)
                return RepReurnError("请先完善基本信息再参加比赛");
            else if (res == -3)
                return RepReurnError("你已经参加了本次比赛的一个队伍，不能接受其他队伍");
            else if (res == -9)
                return RepReurnError("参赛的队员年龄不能大于60岁或者小于16岁");
            else
                return RepReurnError("操作中出现错误");
        }

        [HttpPost]
        public JsonResult RejectRp(string id, string uid)
        {
            MembershipBll bll = new MembershipBll();
            int res = bll.RejectReplay(id);

            if (res >= 0)
                return RepReurnOK();
            else
                return RepReurnError("此消息已经过期");
        }

    }
}
