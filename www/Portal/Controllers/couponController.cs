using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using log4net;

using Model;
using BLL;
using Utls;

namespace Portal.Controllers
{
    public class couponController : BaseController
    {
        //
        // GET: /coupon/

        public ActionResult f880067()
        {
            if (UserInfo == null)
                return RedirectToAction("login", "member", new { url = "/coupon/f880067" });

            // TeamRegBll bll = new TeamRegBll();
            // return View(bll.GetMatchUsersByUserid(UserInfo.userid));

            return View();
        }

        //[HttpPost]
        //public JsonResult coupon(string tid, string couponno)
        //{
        //    if (UserInfo == null)
        //        return RepReurnError("请登录");

        //    TeamRegBll bll = new TeamRegBll();
        //    int res = bll.checkcoupon(tid, couponno, UserInfo.userid, "");

        //    if (res > 0)
        //        return RepReurnOK();
        //    else if (res == -2)
        //        return RepReurnError("邀请码不正确或者已经被使用");
        //    else if (res == -3)
        //        return RepReurnError("你不是队长，不能操作");
        //    else
        //        return RepReurnError("操作中出现错误");
        //}

        [HttpPost]
        public JsonResult ckcoupon(string couponno, string company)
        {
            if (UserInfo == null)
                return RepReurnError("请登录");

            Fmodel fm = new Fmodel();
            TeamRegBll bll = new TeamRegBll();
            int res = bll.ckcoupon(couponno, UserInfo.userid, company, ref fm);

            if (res >= 0)
                return RepReurnOK(fm);
            else if (res == -2)
                return RepReurnError("邀请码不正确或者已经被使用");
            else if (res == -3)
                return RepReurnError("你不是队长，不能操作");
            else if (res == -908)
                return RepReurnError("公司名称不正确");
            else if (res == -909)
            {
                fm.tag = "909";
                //  return RepReurnError("路线人数要求不同，不能使用F码");
                return RepReurnOK(fm);
            }
            else if (res == -910)
                return RepReurnError("邀请码目前还不能使用，请到日期使用");
            else if (res == -911)
                return RepReurnError("你已经正式报名成功，不能使用支付邀请码");
            else if (res == -912)
                return RepReurnError("你已建队伍路线和邀请邀请路线冲突，请先[我的信息]里取消队伍，再使用邀请码");
            else if (res == -913)
                return RepReurnError("邀请码已经被使用");
            else
                return RepReurnError("操作中出现错误");
        }

        [HttpPost]
        public JsonResult couponok(string tmname, string couponno, string company)
        {
            if (UserInfo == null)
                return RepReurnError("请登录");

            TeamRegBll bll = new TeamRegBll();
            string res = bll.checkcoupon(couponno, UserInfo.userid, company, tmname);

            if (res == "-2")
                return RepReurnError("邀请码不正确或者已经被使用");
            else if (res == "-3")
                return RepReurnError("你的信息还没有完善，请先更新个人信息再后续操作");
            else if (res == "-908")
                return RepReurnError("公司名称不正确");
            else if (res == "-910")
                return RepReurnError("邀请码目前还不能使用，请到日期使用");
            else if (res == "-911")
                return RepReurnError("已经存在相同的队伍名称");
            else if (res == "-912")
                return RepReurnError("你已经正式报名成功，不能使用支付邀请码");
            else if (res == "-913")
                return RepReurnError("你已建队伍路线和邀请邀请路线冲突，请先[我的信息]里取消队伍，再使用邀请码");
            else if (res == "-914")
                return RepReurnError("邀请码已经被使用");
            else
                return RepReurn(0, res, null);
        }
    }
}
