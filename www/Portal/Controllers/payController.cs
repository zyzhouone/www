using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;

using Portal.Alipay;

using Model;
using BLL;
using Utls;

namespace Portal.Controllers
{
    public class payController : BaseController
    {
        public ActionResult test()
        {
            return View();
        }

        public ActionResult create(string id)
        {
            ViewBag.msg = "";
            ViewBag.title="";
            ViewBag.total = "";

            createpay(id);
            return View();
        }

        public ActionResult payskip(string id)
        {
            ViewBag.tid = id;
            return View();
        }

        public void createpay(string teamid)
        {
            MembershipBll bll = new MembershipBll();
            var order = bll.GetOrderByTeamId(teamid);
            var cnt = new TeamBll().GetPaycountByTeamid(teamid);

            var tm = new TeamRegBll().GetTeamById(teamid);
            if (tm.Teamtype != 1)
            {
                string msg = bll.CheckPay(order.Id, cnt, order.Match_Id);

                if (!string.IsNullOrEmpty(msg) && msg != "正在支付，请等待")
                {
                    ViewBag.msg = msg;
                    return;
                }
            }

            //商户订单号，商户网站订单系统中唯一订单号，必填
            string out_trade_no = order.Orderid;

            //订单名称，必填
            string subject = order.Title;

            //付款金额，必填
            string total_fee = order.Ordertotal;

            //商品描述，可空
            string body = order.Title;


            ViewBag.title = order.Title;
            ViewBag.total = order.Ordertotal;

            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", Config.service);
            sParaTemp.Add("partner", Config.partner);
            sParaTemp.Add("seller_id", Config.seller_id);
            sParaTemp.Add("_input_charset", Config.input_charset.ToLower());
            sParaTemp.Add("payment_type", Config.payment_type);
            sParaTemp.Add("notify_url", Config.notify_url);
            sParaTemp.Add("return_url", Config.return_url);
            sParaTemp.Add("anti_phishing_key", Config.anti_phishing_key);
            sParaTemp.Add("exter_invoke_ip", Config.exter_invoke_ip);
            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("total_fee", total_fee);
            sParaTemp.Add("body", body);
            sParaTemp.Add("it_b_pay", Config.it_b_pay);

            //其他业务参数根据在线开发文档，添加参数.文档地址:https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.O9yorI&treeId=62&articleId=103740&docType=1
            //如sParaTemp.Add("参数名","参数值");

            //建立请求
            string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认");
            Response.Write(sHtmlText);
        }

        private void WriteLog(string msg, SortedDictionary<string, string> dict)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(this.GetType());

            foreach (var item in dict)
            {
                msg += string.Format("[{0}:{1}]", item.Key, item.Value);
            }
            log.Fatal(msg);
        }
        //
        // GET: /pay/
        public ActionResult success_return()
        {
            SortedDictionary<string, string> sPara = GetRequestGet();

            WriteLog("success_return:", sPara);

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.QueryString["notify_id"], Request.QueryString["sign"]);

                if (verifyResult)//验证成功
                {
                    ViewBag.flag = "0";

                    //商户订单号
                    string out_trade_no = Request.QueryString["out_trade_no"];

                    //支付宝交易号
                    string trade_no = Request.QueryString["trade_no"];

                    //交易状态
                    string trade_status = Request.QueryString["trade_status"];

                    string buyer_email = Request.QueryString["buyer_email"];

                    MembershipBll bll = new MembershipBll();
                    bll.PayReturn(out_trade_no, trade_no, buyer_email, trade_status);

                    if (trade_status == "TRADE_FINISHED" || trade_status == "TRADE_SUCCESS" || trade_status == "9000")
                    {
                        var m = bll.GetMatchByOrderId(out_trade_no);

                        ViewBag.price = Request.QueryString["price"];
                        ViewBag.title = Request.QueryString["subject"];
                        ViewBag.no = trade_no;
                        ViewBag.match = m.Match_name;
                        ViewBag.date = m.Date4.Value.ToString("yyyy-MM-dd");
                        SMSHepler.SendCommonSms(m.Mobile, string.Format("[{0}]报名费用已经成功支付,感谢你的参与!请妥善保管帐号信息,等待查询比赛编组.", m.Match_name));
                    }
                }
                else//验证失败
                {
                    //Response.Write("验证失败");
                    ViewBag.flag = "1";

                }
            }
            else
            {
                //Response.Write("无返回参数");
                ViewBag.flag = "1";
            }

            return View();
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        private SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }

        [HttpPost]
        public ContentResult notify_return()
        {
            SortedDictionary<string, string> sPara = GetRequestPost();

            WriteLog("notify_return:", sPara);

            if (sPara.Count > 0)//判断是否有带返回参数
            {
                Notify aliNotify = new Notify();
                bool verifyResult = aliNotify.Verify(sPara, Request.Form["notify_id"], Request.Form["sign"]);

                //if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码


                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表

                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];

                    string buyer_email = Request.Form["buyer_email"];

                    if (trade_status == "TRADE_FINISHED" || trade_status == "TRADE_SUCCESS" || trade_status == "9000")
                    {
                        MembershipBll bll = new MembershipBll();
                        bll.PayReturn(out_trade_no, trade_no, buyer_email, trade_status);

                        var m = bll.GetMatchByOrderId(out_trade_no);

                        //log4net.ILog log = log4net.LogManager.GetLogger(this.GetType());
                        //log.Fatal(trade_status);
                        SMSHepler.SendCommonSms(m.Mobile, string.Format("[{0}]报名费用已经成功支付,感谢你的参与!请妥善保管帐号信息,等待查询比赛编组.", m.Match_name));
                    }

                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    //Response.Write("success");  //请不要修改或删除

                    return this.Content("success");
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                //else//验证失败
                //{
                //Response.Write("fail");
                //    return this.Content("fail");
                //}
            }
            else
            {
                //Response.Write("无通知参数");
                return this.Content("无通知参数");
            }
        }

        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        private SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
    }
}
