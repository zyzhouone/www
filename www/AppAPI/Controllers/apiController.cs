using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Utls;
using Model;
using BLL;

namespace AppAPI.Controllers
{
    /// <summary>
    /// API接口
    /// </summary>
    public class apiController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult createteam()
        {
            return RepReurnOK();
        }

        /// <summary>
        ///队伍详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /*public JsonResult getteam(string id)
        {
            int intId = 0;

            if (!int.TryParse(id, out intId))
                return RepReurnError("id传入错误");


            ApiBll bll = new ApiBll();
            var team = bll.GetTeamById(intId);
            if (team == null)
                return RepReurnError("查询的队伍信息不存在");

            var data = new { id = team.Id, teamname = team.Teamname, userid = team.Userid, company = team.Company, lineid = team.Lineid, createtime = team.Createtime };
            return RepReurnOK(data);
        }*/

        public JsonResult getmatchlist(string dt)
        {
            
            ApiBll bll = new ApiBll();
            var match = bll.GetMatcheByDt(DateTime.Now);
            foreach (var m in match)
            {
                if (m.Status == "0")
                    m.Status = "预报名中";
                else if (m.Status == "1")
                    m.Status = "报名中";
                else if(m.Status == "2")
                    m.Status="已结束";
            }

            if (match == null)
                return RepReurnError("没有比赛信息");
                        
            return RepReurnOK(match);
        }
    }
}
