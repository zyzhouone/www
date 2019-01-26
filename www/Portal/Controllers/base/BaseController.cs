using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using log4net;
using Model;

namespace Portal.Controllers
{
    public class BaseController : Controller
    {
        //
        // GET: /Base/

        private tblusers userinfo;

        protected tblusers UserInfo
        {
            get { return userinfo; }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie cookie = filterContext.HttpContext.Request.Cookies["coc_cookie_info"];
            if (cookie != null)
            { 
                //已经登录
                userinfo = new tblusers();
                userinfo.userid = cookie.Values.Get("uuid");
                userinfo.Name = cookie.Values.Get("uunm");

                ViewBag.uuid = userinfo.userid;
                ViewBag.uunm = HttpUtility.HtmlEncode(userinfo.Name);
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnException(ExceptionContext filterContext)
        {
            ILog log = LogManager.GetLogger(filterContext.GetType());
            log.Error(filterContext.Exception);

            base.OnException(filterContext);
        }

        protected JsonResult RepReurnOK()
        {
            RepMsg rg = new RepMsg() { Code = 0 };
            return base.Json(rg, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult RepReurnOK(object data)
        {
            RepMsg rg = new RepMsg() { Code = 0, Data = data };
            return base.Json(rg, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult RepReurnError(string msg)
        {
            RepMsg rg = new RepMsg() { Code = -1, Message = msg };
            return base.Json(rg, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult RepReurn(int code, string msg, object data)
        {
            RepMsg rg = new RepMsg() { Code = code, Message = msg, Data = data };
            return base.Json(rg, JsonRequestBehavior.AllowGet);
        }
    }
}
