using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DAL;
using Model;
using Utls;

namespace BLL
{
    /// <summary>
    /// API接口信息
    /// </summary>
    public class ApiBll : BaseBll
    {
        /// <summary>
        /// 获取队伍信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public tblteams GetTeamById(string id)
        {
            using (var db = new BFdbContext())
            {
                return db.tblteams.FirstOrDefault(p => p.teamid == id);
            }
        }

        public List<tblmatch> GetMatcheByDt(DateTime dt)
        {
            using (var db = new BFdbContext())
            {
                string sql = "select * from tbl_match";
                return db.SqlQuery<tblmatch>(sql).ToList();


            }
        }

        public int Answer(List<tblanswer> lst)
        {
            using (var db = new BFdbContext())
            {
                foreach (var item in lst)
                {
                    db.TInsert<tblanswer>(item);
                }
                return db.SaveChanges();
            }
        }
    }
}
