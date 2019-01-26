using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using DAL;
using Model;
using Utls;

namespace BLL
{
    public class GroupBll : BaseBll
    {
        /// <summary>
        /// 获取团队信息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public tblcompany GetCompanyByUserid(string userid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblcompany.FirstOrDefault(p => p.userid == userid);
            }
        }

        public tblline GetLineByName(string name)
        {
            using (var db = new BFdbContext())
            {
                return db.tblline.FirstOrDefault(p => p.Name == name);
            }
        }

        /// <summary>
        /// 获取我的比赛
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<tblmatchentity> GetMymatch(string userid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format(@"SELECT a.*,b.match_name,b.date1,b.date2,b.date3,b.status as mstatus,c.status as paystatus FROM tbl_teams a,tbl_match b,tbl_orders c
                                            where a.match_id=b.match_id and a.teamid=c.teamid and a.userid ='{0}'", userid);
                return db.SqlQuery<tblmatchentity>(sql).ToList();
            }
        }

        /// <summary>
        /// 更新团队
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        public int UpdateCompany(tblcompany com)
        {
            using (var db = new BFdbContext())
            {
                var tcom = db.tblcompany.FirstOrDefault(p => p.userid == com.userid);
                if (tcom == null)
                    return -1;

                tcom.Area = com.Area;
                tcom.Contacts = com.Contacts;
                tcom.Moblie = com.Moblie;
                tcom.Name = com.Name;
        
                return db.Update<tblcompany>(tcom);
            }
        }

        public List<tblmatch> GetMatch(string status)
        {
            using (var db = new BFdbContext())
            {
                //if (!string.IsNullOrEmpty(status))
                //    return db.tblmatch.Where(p => p.Status == status).ToList();
                //else
                //    return db.tblmatch.ToList();

                string sql = "select * from tbl_match order by date1 desc";
                return db.SqlQuery<tblmatch>(sql).ToList();
            }
        }

        public tblmatch GetMatchById(string id)
        {
            using (var db = new BFdbContext())
            {
                return db.tblmatch.FirstOrDefault(p => p.Match_id == id);
            }
        }

        public tblteams GetTeamByUidMid(string uid,string mid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblteams.FirstOrDefault(p => p.match_id == mid && p.Userid == uid);
            }
        }
    }
}
