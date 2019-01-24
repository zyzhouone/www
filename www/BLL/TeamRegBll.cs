using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using DAL;
using Model;
using Utls;
using System.Net;
using Newtonsoft.Json;

namespace BLL
{
    public class TeamRegBll : BaseBll
    {
        /// <summary>
        /// 团队注册1步
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public int Step1(string mobile)
        {
            using (var db = new BFdbContext())
            {
                tblusers usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.Status == 0);
                int res = 0;
                if (usr == null)
                {
                    usr = new tblusers();
                    usr.Mobile = mobile;
                    usr.mono = VerifyCode.Get6SzCode();
                    usr.Status = 6;
                    usr.Isupt = "0";

                    res = db.Insert<tblusers>(usr);
                }
                else
                {
                    usr.mono = VerifyCode.Get6SzCode();
                    res = db.Update<tblusers>(usr);
                }
                if (res > 0)
                    SMSHepler.SendRegSms(mobile, usr.mono);

                return res;
            }
        }

        /// <summary>
        /// 检查验证码是否正确
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="vercode"></param>
        /// <returns></returns>
        public string CheckSms(string mobile, string vercode, string matchid)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.mono == vercode && (p.Status == 6 || p.Status == 0));
                if (usr == null)
                    return "-1";
                else
                {
                    if (db.tblmatchusers.Any(p => p.Match_Id == matchid && p.Mobile == mobile && p.Leader == 1))
                        return "-2";

                    return usr.userid;
                }
            }
        }

        /// <summary>
        /// 检查队伍名称是否重复
        /// </summary>
        /// <param name="matchid"></param>
        /// <param name="tname"></param>
        /// <returns></returns>
        public bool CheckTname(string matchid, string tname)
        {
            using (var db = new BFdbContext())
            {
                return db.tblteams.Any(p => p.match_id == matchid && p.Teamname == tname);
            }
        }

        /// <summary>
        /// 注册队伍
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tid"></param>
        /// <param name="tname"></param>
        /// <param name="tcom"></param>
        /// <returns></returns>
        public string RegTname(string uid, string tid, string tname, string tcom, string pwd, ref string teamid)
        {
            using (var db = new BFdbContext())
            {
                if (db.tblteams.Any(p => p.match_id == tid && p.Teamname == tname))
                    return "-1";
                var tms = db.tblteams.FirstOrDefault(p => p.match_id == tid && p.Userid == uid);
                if (tms != null)
                {
                    teamid = tms.teamid;
                    return "-3";
                }

                var usr = db.tblusers.FirstOrDefault(p => p.userid == uid);
                if (usr == null || usr.Isupt == "0")
                    return "-2";

                tblteams tm = new tblteams();
                tm.Company = string.IsNullOrEmpty(tcom) ? "个人" : tcom;
                tm.Createtime = DateTime.Now;
                tm.Eventid = "1";
                tm.Lineid = "";
                tm.match_id = tid;
                tm.Status = 6;
                tm.Teamname = tname;
                tm.Teamno = "0";
                tm.Userid = uid;
                tm.teamid = Guid.NewGuid().ToString();
                tm.Teamtype = 0;
                tm.Type_ = "0";
                db.TInsert<tblteams>(tm);

                tblmatchusers m = new tblmatchusers();
                m.Cardno = usr.cardno;
                m.Cardtype = usr.cardtype;
                m.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m.Leader = 1;
                m.Match_Id = tid;
                m.Matchuserid = Guid.NewGuid().ToString();
                m.Mobile = usr.Mobile;
                m.Nickname = usr.Name;
                m.Pay = 0;
                m.birthday = usr.birthday;

                SetYearOld(m);

                int sx = 1;
                if (int.TryParse(usr.sexy, out sx))
                    m.Sexy = sx;
                else
                    m.Sexy = 1;
                m.Status = "1";

                m.Teamid = tm.teamid;
                m.Teamname = tm.Teamname;
                m.Userid = usr.userid;
                db.TInsert<tblmatchusers>(m);

                db.SaveChanges();

                return tm.teamid;
            }
        }

        public List<tblline> GetLines(string mid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format("select a.* from tbl_line a,tbl_teams b where a.match_id=b.match_id and b.teamid='{0}' order by sort", mid);
                return db.SqlQuery<tblline>(sql).ToList();
            }
        }

        public tblmatchentity GetMatchByTeamid(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.SqlQuery<tblmatchentity>("select a.*,b.teamname,b.userid,b.status as Paystatus,b.chglines from tbl_match a,tbl_teams b where a.match_id=b.match_id and b.teamid='" + tid + "'").FirstOrDefault();
            }
        }

        public tblmatchentity GetMatchUserByUidMid(string userid, string matchid)
        {
            using (var db = new BFdbContext())
            {
                return db.SqlQuery<tblmatchentity>("select a.*,b.match_name,c.teamno as teamno_t,d.linename as Lineno,b.date4,b.logopic from tbl_match_users a,tbl_match b,tbl_teams c,tbl_lines d where a.match_id=b.match_id and a.teamid=c.teamid and c.linesid=d.lines_id and a.userid='" + userid + "' and a.match_id='" + matchid + "' and c.eventid = 3").FirstOrDefault();
            }
        }

        public List<tblmatchentity> GetMatchUsersByUidMid(string userid, string matchid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format(@"SELECT a.*,b.match_name,b.date4,b.lineno,b.Teamstatus,b.teamno_t from tbl_match_users a,
                            (SELECT a.teamid,a.match_id,b.match_name,b.date4,
                                concat(case when a.match_id='6a61b95b-2d5d-4373-abaf-bf4e4c438800' then d.line_no else CONCAT(d.line_no,'-',d.linename) end,if(c.teamno is null,'',if(c.teamno='0','',if(c.teamno='00000','',concat('[', c.teamno,']'))))) lineno,
                                c.`status` as Teamstatus,c.teamno as teamno_t 
                                from tbl_match_users a,tbl_match b,tbl_teams c,tbl_lines d
                                where a.match_id=b.match_id and a.teamid=c.teamid and c.linesid=d.lines_id and a.userid='{0}' and a.match_id='{1}' and c.status in (0,1)) b
                            WHERE a.teamid=b.teamid and a.match_id=b.match_id and a.status='1'", userid, matchid);

                return db.SqlQuery<tblmatchentity>(sql).ToList();
            }
        }

        public List<tblmatchentity> GetMatchUsersByTeamid(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.SqlQuery<tblmatchentity>("SELECT a.*,c.status as mstatus,c.match_name,c.date3,c.date4,b.teamno as teamno_t,b.status as Teamstatus,d.linename,b.teamtype,d.line_no as lineno FROM tbl_match_users a,tbl_teams b,tbl_match c,tbl_lines d where a.match_id=c.match_id and a.teamid=b.teamid and b.linesid=d.lines_id and a.teamid='" + tid + "' order by a.leader desc").ToList();
            }
        }

        public int GetReplayerCnt(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblreplace.Count(p => p.Teamid == tid);
            }
        }

        public int GetFOrders(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblorders.Count(p => p.Match_Id == tid && p.Status == 2);
            }
        }

        public tblteams GetTeamById(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblteams.FirstOrDefault(p => p.teamid == tid);
            }
        }

        public tblmatch GetMatchById(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblmatch.FirstOrDefault(p => p.Match_id == tid);
            }
        }

        public tblteammatchusers GetTeamByum(string usrid, string matchid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format("select a.*,b.lineid from tbl_match_users a,tbl_teams b where a.teamid=b.teamid and (a.status='1' or a.leader=1) and a.userid='{0}' and a.match_id='{1}'", usrid, matchid);
                return db.SqlQuery<tblteammatchusers>(sql).FirstOrDefault();
            }
        }

        public tbllines GetLineById(string tid)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                return db.tbllines.FirstOrDefault(p => p.Linesid == team.Linesid);
            }
        }

        public List<tblmatchusers> GetMatchuserByTeamId(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblmatchusers.Where(p => p.Teamid == tid && p.Leader == 0).ToList();
            }
        }

        public tblmatchusers GetMatchuserById(string tid, string uid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblmatchusers.FirstOrDefault(p => p.Teamid == tid && p.Userid == uid);
            }
        }

        public int DelMatchuser(string uid)
        {
            using (var db = new BFdbContext())
            {
                tblmatchusers tbl = new tblmatchusers();
                tbl.Matchuserid = uid;

                return db.Delete<tblmatchusers>(tbl);
            }
        }

        public int UpdatelMatchuser(tblmatchusers user)
        {
            using (var db = new BFdbContext())
            {
                if (db.tblusers.Any(p => p.userid != user.Userid && p.cardno == user.Cardno))
                    return -2;

                var ur = db.tblmatchusers.FirstOrDefault(p => p.Userid == user.Userid && p.Teamid == user.Teamid && p.Leader == 1);

                ur.Mobile = user.Mobile;
                ur.Cardno = user.Cardno;
                ur.Cardtype = user.Cardtype;
                ur.Nickname = user.Nickname;
                ur.Sexy = user.Sexy;
                ur.Leader = user.Leader;

                DateTime dt;
                if (DateTime.TryParse(user.Pnov, out dt))
                    ur.birthday = dt;
                else
                    ur.birthday = null;

                SetYearOld(ur);

                db.TUpdate<tblmatchusers>(ur);

                var bur = db.tblusers.FirstOrDefault(p => p.userid == user.Userid);
                bur.Name = user.Nickname;
                bur.birthday = ur.birthday;
                bur.cardno = user.Cardno;
                bur.cardtype = user.Cardtype;
                bur.Mobile = user.Mobile;
                bur.sexy = user.Sexy.ToString();
                db.TUpdate<tblusers>(bur);

                return db.SaveChanges();
            }
        }

        public int AddMatchuser(string tid, string mobile)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.Status == 0);
                if (usr == null)
                    return -1;

                if (db.tblmatchusers.Any(p => p.Userid == usr.userid && p.Teamid == tid && p.Status != "9"))
                    return -2;

                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team.Userid == usr.userid)
                    return -3;

                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);
                var dusr = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);

                tblmatchusers m = new tblmatchusers();
                m.Cardno = usr.cardno;
                m.Cardtype = usr.cardtype;
                m.birthday = usr.birthday;

                SetYearOld(m);

                m.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m.Leader = 0;
                m.Match_Id = team.match_id;
                m.Matchuserid = Guid.NewGuid().ToString();
                m.Mobile = usr.Mobile;
                m.Nickname = usr.Name;
                m.Pay = 0;

                int sx = 1;
                if (int.TryParse(usr.sexy, out sx))
                    m.Sexy = sx;
                else
                    m.Sexy = 1;
                m.Status = "2";
                m.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m.Teamid = team.teamid;
                m.Teamname = team.Teamname;
                m.Userid = usr.userid;
                db.TInsert<tblmatchusers>(m);

                tblinfomation info = new tblinfomation();
                info.Context = string.Format("用户[{0}]邀请你加入[{1}]队伍,参加[{2}],赶快去看看并接受邀请吧.", dusr.Mobile, team.Teamname, match.Match_name);
                info.createtime = DateTime.Now;
                info.Infoid = Guid.NewGuid().ToString();
                info.Mobile = m.Mobile;
                info.Status = "0";
                info.Type = "3";
                info.Userid = m.Userid;
                info.Field1 = m.Matchuserid;
                info.Field2 = "0";
                db.TInsert<tblinfomation>(info);

                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 选择路线
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lid"></param>
        /// <returns></returns>
        public int SelectLine(string id, string lid)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == id);
                if (team == null)
                    return -1;
                var lns = db.tbllines.FirstOrDefault(p => p.Linesid == lid);
                if (lns == null)
                    return -1;

                team.Lineid = lns.Lineid;
                team.Linesid = lid;//int.Parse(lid);
                return db.Update<tblteams>(team);
            }
        }

        public int ChangeLine(string userid, string teamid, string lid)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == teamid);
                if (team == null)
                    return -1;

                if (team.Userid != userid)
                    return -3;

                if (!(team.Status.Value == 1 || (team.Status.Value == 6 && team.ChgLines == "1")))
                    return -4;

                var lines = db.tbllines.FirstOrDefault(p => p.Linesid == lid);
                if (lines == null)
                    return -2;

                if (team.Linesid == lid)
                    return -6;

                if (string.IsNullOrEmpty(lines.CanChange) || lines.CanChange == "0")
                    return -5;

                team.Linesid = lid;
                team.Lineid = lines.Lineid;

                //重置状态
                if (team.Status.Value == 1)
                {
                    tblorders order = db.tblorders.FirstOrDefault(p => p.Teamid == teamid);
                    //order.Lineid = team.Lineid;
                    //order.Match_Id = team.match_id;
                    //order.Ordertotal = lines.Price;
                    //order.Status = 0;
                    if (order != null)
                        db.Delete<tblorders>(order);

                    tblmatchextra extra = db.tblmatchextra.FirstOrDefault(p => p.teamid == teamid);
                    if (extra != null)
                        db.Delete<tblmatchextra>(extra);
                }

                team.Status = 6;
                team.ChgLines = "1";
                return db.Update<tblteams>(team);
            }
        }

        public List<tbllines> getLines(string lineid)
        {
            using (var db = new BFdbContext())
            {
                return db.tbllines.Where(p => p.Lineid == lineid).OrderBy(p => p.Lineno).ToList();
            }
        }

        /// <summary>
        /// 录入队员信息
        /// </summary>
        /// <param name="mus"></param>
        /// <param name="tid"></param>
        /// <returns></returns>
        public string InputMb_old(List<tblmatchusers> mus, string tid)
        {
            string resmsg = "";

            using (var db = new BFdbContext())
            {
                //var team = db.tblteams.FirstOrDefault(p => p.Id == tid);
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team == null)
                    return "-1";

                var user = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);
                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);

                int nov = 1;

                foreach (var item in mus)
                {
                    SetYearOld(item);

                    item.Createtime = DateTime.Now.ToString("yyyy=MM-dd HH:mm:ss");
                    item.Match_Id = team.match_id;
                    item.Teamid = tid;
                    item.Teamname = team.Teamname;
                    item.Matchuserid = Guid.NewGuid().ToString();

                    int tn = 0;
                    if (int.TryParse(team.Teamno, out tn))
                        item.Teamno = tn;
                    else
                        item.Teamno = 0;

                    item.Pay = 1;
                    item.Pnov = team.Teamno + nov;

                    if (item.Mobile == user.Mobile)
                    {
                        item.Userid = user.userid;
                        item.Leader = 1;
                        db.TInsert<tblmatchusers>(item);
                    }
                    else
                    {
                        tblusers tbusr = db.tblusers.FirstOrDefault(p => p.Mobile == item.Mobile && p.Status == 0);
                        if (tbusr == null)
                            resmsg += string.Format("手机号[{0}]用户没有注册或者没有完成个人信息;", item.Mobile);
                        else
                        {
                            item.Userid = tbusr.userid;
                            tblinfomation info = new tblinfomation();
                            info.Context = string.Format("用户[{0}]邀请你加入[{1}]队伍,参加[{2}],赶快去看看并接受邀请吧.", item.Mobile, team.Teamname, match.Match_name);
                            info.createtime = DateTime.Now;
                            info.Infoid = Guid.NewGuid().ToString();
                            info.Mobile = item.Mobile;
                            info.Status = "1";
                            info.Type = "3";
                            info.Userid = tbusr.userid;
                            info.Field1 = item.Matchuserid;
                            db.TInsert<tblinfomation>(info);
                        }
                        item.Leader = 0;
                    }

                    db.TInsert<tblmatchusers>(item);
                }

                team.Status = 0;
                db.TUpdate<tblteams>(team);

                if (user != null && user.Status == 6)
                {
                    user.Status = 0;
                    db.TUpdate<tblusers>(user);
                }

                if (!string.IsNullOrEmpty(resmsg))
                {
                    db.Dispose();
                    return resmsg;
                }

                db.SaveChanges();
                return "";
            }
        }

        private void SetYearOld(tblmatchusers item)
        {
            try
            {
                //if (item.Cardtype == "1")
                {
                    if (item.birthday.HasValue)
                    {
                        string dy = item.birthday.Value.ToString("yyyyMMdd");
                        string nw = DateTime.Now.ToString("yyyyMMdd");
                        string m = (int.Parse(nw) - int.Parse(dy) + 1).ToString();

                        if (m.Length > 4)
                            item.Age = int.Parse(m.Substring(0, m.Length - 4));
                        else
                            item.Age = 0;
                    }
                    else
                        item.Age = 0;
                }
            }
            catch (Exception ex)
            {
                item.Age = 0;
            }
        }

        private void SetYearOld_Card(tblmatchusers item)
        {
            if (item.Cardtype == "1")
            {
                if (string.IsNullOrEmpty(item.Cardno))
                    item.Age = 0;
                else if (item.Cardno.Length == 18)
                {
                    string dy = item.Cardno.Substring(6, 8);
                    string nw = DateTime.Now.ToString("yyyyMMdd");
                    string m = (int.Parse(nw) - int.Parse(dy)).ToString();

                    if (m.Length > 4)
                        item.Age = int.Parse(m.Substring(0, m.Length - 4));
                    else
                        item.Age = 0;
                }
                else if (item.Cardno.Length == 15)
                {
                    string dy = item.Cardno.Substring(6, 6);
                    if (dy.StartsWith("0") || dy.StartsWith("1") || dy.StartsWith("2"))
                        dy = "20" + dy;
                    else
                        dy = "19" + dy;

                    string nw = DateTime.Now.ToString("yyyyMMdd");
                    string m = (int.Parse(nw) - int.Parse(dy)).ToString();

                    if (m.Length > 4)
                        item.Age = int.Parse(m.Substring(0, m.Length - 4));
                    else
                        item.Age = 0;
                }
                else
                    item.Age = 0;
            }
        }

        public string InputMb(List<tblmatchusers> mus, string tid)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team == null)
                    return "-1";

                var tblmusers = db.tblmatchusers.Where(p => p.Teamid == team.teamid && p.Status == "1");

                var line = db.tbllines.FirstOrDefault(p => p.Linesid == team.Linesid);
                if (!tblmusers.Any(p => p.Sexy == 2 && p.Teamid == team.teamid) && line.Playercount>1)
                    return "-2";

                if (tblmusers.Any(p => p.Age < 16 && p.Teamid == team.teamid))
                    return "-3";

                if (tblmusers.Any(p => p.Age > 60 && p.Teamid == team.teamid))
                    return "-4";

                if (tblmusers.Any(p => p.Nickname == null && p.Teamid == team.teamid))
                    return "-5";

                if (tblmusers.Any(p => p.Cardno == null && p.Teamid == team.teamid))
                    return "-6";

                if (tblmusers.Any(p => p.Age == null))
                    return "-7";

                //var user = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);
                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);

                if (tblmusers.Count() != line.Playercount.Value)
                    return "-8";

                if (line.Status.Value == 2 && !db.tblmatchextra.Any(p => p.teamid == tid && p.extype == "1"))
                    return "-9";

                if (line.Status.Value == 1 && !db.tblmatchextra.Any(p => p.teamid == tid && p.extype == "2"))
                    return "-10";

                team.Status = 1;
                db.TUpdate<tblteams>(team);

                //添加订单
                tblorders order = new tblorders();
                order.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                order.Id = Guid.NewGuid().ToString();
                order.Lineid = team.Lineid;
                order.Match_Id = team.match_id;
                order.Orderid = IDGenerator.GetIdF();
                order.Ordertotal = line.Price;
                order.Status = 0;
                order.Teamid = team.teamid;
                order.Title = string.Format("[{0}]报名费用", match.Match_name);
                order.Userid = team.Userid;

                var od = db.tblorders.FirstOrDefault(p => p.Teamid == team.teamid);
                if (od != null)
                    db.TDelete<tblorders>(od);

                db.TInsert<tblorders>(order);

                db.SaveChanges();
               
                    return "";
               
            }
        }

        public string CompFM(string tid)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team == null)
                    return "-1";

                var tblmusers = db.tblmatchusers.Where(p => p.Teamid == team.teamid && p.Status == "1");

                if (!tblmusers.Any(p => p.Sexy == 2 && p.Teamid == team.teamid))
                    return "-2";

                if (tblmusers.Any(p => p.Age < 16 && p.Teamid == team.teamid))
                    return "-3";

                if (tblmusers.Any(p => p.Age > 60 && p.Teamid == team.teamid))
                    return "-4";

                if (tblmusers.Any(p => p.Nickname == null && p.Teamid == team.teamid))
                    return "-5";

                if (tblmusers.Any(p => p.Cardno == null && p.Teamid == team.teamid))
                    return "-6";

                if (tblmusers.Any(p => p.Age == null))
                    return "-7";

                //var user = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);
                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);
                if (match.Status != "8")
                    return "-99";

                var line = db.tbllines.FirstOrDefault(p => p.Linesid == team.Linesid);

                if (tblmusers.Count() != line.Playercount.Value)
                    return "-8";

                if (line.Status.Value == 2 && !db.tblmatchextra.Any(p => p.teamid == tid && p.extype == "1"))
                    return "-9";

                if (line.Status.Value == 1 && !db.tblmatchextra.Any(p => p.teamid == tid && p.extype == "2"))
                    return "-10";

                var coupon = db.tblcoupon.FirstOrDefault(p => p.matchid == team.match_id && p.teamid == team.teamid);
                if (coupon == null || coupon.status == "1")
                    return "-98";

                if (coupon.type == "1")
                    team.Status = 0;
                else
                    team.Status = 1;
                db.TUpdate<tblteams>(team);

                coupon.status = "1";
                db.TUpdate<tblcoupon>(coupon);

                //添加订单
                tblorders order = new tblorders();
                order.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                order.Id = Guid.NewGuid().ToString();
                order.Lineid = team.Lineid;
                order.Match_Id = team.match_id;
                order.Orderid = IDGenerator.GetIdF();
                order.Ordertotal = line.Price;
                if (coupon.type == "1")
                    order.Status = 2;
                else
                    order.Status = 0;
                order.Teamid = team.teamid;
                order.Title = string.Format("[{0}]报名费用", match.Match_name);
                order.Userid = team.Userid;

                var od = db.tblorders.FirstOrDefault(p => p.Teamid == team.teamid);
                if (od != null)
                    db.TDelete<tblorders>(od);

                db.TInsert<tblorders>(order);

                db.SaveChanges();
                return "";
            }
        }

        public string InputMb_old3(List<tblmatchusers> mus, string tid)
        {
            string resmsg = "";

            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team == null)
                    return "-1";

                var user = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);
                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);

                int nov = 1;

                foreach (var item in mus)
                {
                    if (item.Leader == 1)
                    {
                        var lusr = db.tblmatchusers.FirstOrDefault(p => p.Teamid == team.teamid && p.Userid == user.userid);
                        if (lusr != null)
                        {
                            lusr.Nickname = item.Nickname;
                            lusr.Cardtype = item.Cardtype;
                            lusr.Cardno = item.Cardno;
                            lusr.Sexy = item.Sexy;
                            lusr.Mobile = item.Mobile;

                            db.TUpdate<tblmatchusers>(lusr);
                        }
                        else
                        {
                            SetYearOld(item);

                            item.Createtime = DateTime.Now.ToString("yyyy=MM-dd HH:mm:ss");
                            item.Match_Id = team.match_id;
                            item.Teamid = tid;
                            item.Teamname = team.Teamname;
                            item.Matchuserid = Guid.NewGuid().ToString();

                            int tn = 0;
                            if (int.TryParse(team.Teamno, out tn))
                                item.Teamno = tn;
                            else
                                item.Teamno = 0;

                            item.Pay = 1;
                            item.Pnov = team.Teamno + nov;

                            item.Userid = user.userid;
                            item.Leader = 1;
                            item.Status = "1";
                            db.TInsert<tblmatchusers>(item);
                        }
                    }
                    else
                    {
                        //tblmatchusers tbusr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == item.Matchuserid && p.Status == "2");
                        //if (tbusr != null)
                        //{
                        //    //tbusr.Status = "2";
                        //    //db.TUpdate<tblmatchusers>(tbusr);

                        //    tblinfomation info = new tblinfomation();
                        //    info.Context = string.Format("用户[{0}]邀请你加入[{1}]队伍,参加[{2}],赶快去看看并接受邀请吧.", user.Mobile, team.Teamname, match.Match_name);
                        //    info.createtime = DateTime.Now;
                        //    info.Infoid = Guid.NewGuid().ToString();
                        //    info.Mobile = item.Mobile;
                        //    info.Status = "0";
                        //    info.Type = "3";
                        //    info.Userid = tbusr.Userid;
                        //    info.Field1 = item.Matchuserid;
                        //    info.Field2 = "0";
                        //    db.TInsert<tblinfomation>(info);
                        //}
                    }
                }

                if (!string.IsNullOrEmpty(resmsg))
                {
                    db.Dispose();
                    return resmsg;
                }

                team.Status = 1;
                db.TUpdate<tblteams>(team);

                //添加订单
                tblorders order = new tblorders();
                order.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                order.Id = Guid.NewGuid().ToString();
                order.Lineid = team.Lineid;
                order.Match_Id = team.match_id;
                order.Orderid = IDGenerator.GetIdF();
                order.Ordertotal = "300";
                order.Status = 0;
                order.Teamid = team.teamid;
                order.Title = string.Format("[{0}]报名费", match.Match_name);
                order.Userid = team.Userid;

                db.TInsert<tblorders>(order);

                db.SaveChanges();
                return "";
            }
        }

        public string InputMb_old2(List<tblmatchusers> mus, string tid)
        {
            string resmsg = "";

            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                if (team == null)
                    return "-1";

                var user = db.tblusers.FirstOrDefault(p => p.userid == team.Userid);
                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == team.match_id);

                int nov = 1;

                foreach (var item in mus)
                {
                    if (item.Mobile == user.Mobile)
                    {
                        if (item.Cardtype == "1")
                        {
                            if (string.IsNullOrEmpty(item.Cardno))
                                item.Age = 0;
                            else if (item.Cardno.Length == 18)
                            {
                                string dy = item.Cardno.Substring(6, 8);
                                string nw = DateTime.Now.ToString("yyyyMMdd");
                                string m = (int.Parse(nw) - int.Parse(dy)).ToString();

                                if (m.Length > 4)
                                    item.Age = int.Parse(m.Substring(0, m.Length - 4));
                                else
                                    item.Age = 0;
                            }
                            else if (item.Cardno.Length == 15)
                            {
                                string dy = item.Cardno.Substring(6, 6);
                                if (dy.StartsWith("0") || dy.StartsWith("1") || dy.StartsWith("2"))
                                    dy = "20" + dy;
                                else
                                    dy = "19" + dy;

                                string nw = DateTime.Now.ToString("yyyyMMdd");
                                string m = (int.Parse(nw) - int.Parse(dy)).ToString();

                                if (m.Length > 4)
                                    item.Age = int.Parse(m.Substring(0, m.Length - 4));
                                else
                                    item.Age = 0;
                            }
                            else
                                item.Age = 0;

                        }
                        item.Createtime = DateTime.Now.ToString("yyyy=MM-dd HH:mm:ss");
                        item.Match_Id = team.match_id;
                        item.Teamid = tid;
                        item.Teamname = team.Teamname;
                        item.Matchuserid = Guid.NewGuid().ToString();

                        int tn = 0;
                        if (int.TryParse(team.Teamno, out tn))
                            item.Teamno = tn;
                        else
                            item.Teamno = 0;

                        item.Pay = 1;
                        item.Pnov = team.Teamno + nov;

                        item.Userid = user.userid;
                        item.Leader = 1;
                        db.TInsert<tblmatchusers>(item);
                    }
                    else
                    {
                        tblusers tbusr = db.tblusers.FirstOrDefault(p => p.Mobile == item.Mobile && p.Status == 0);
                        if (tbusr == null)//|| string.IsNullOrEmpty(tbusr.Name) || string.IsNullOrEmpty(tbusr.cardno))
                            resmsg += string.Format("手机号[{0}]用户没有注册或者没有完成个人信息;", item.Mobile);
                        else
                        {
                            tblinfomation info = new tblinfomation();
                            info.Context = string.Format("用户[{0}]邀请你加入[{1}]队伍,参加[{2}],赶快去看看并接受邀请吧.", item.Mobile, team.Teamname, match.Match_name);
                            info.createtime = DateTime.Now;
                            info.Infoid = Guid.NewGuid().ToString();
                            info.Mobile = item.Mobile;
                            info.Status = "0";
                            info.Type = "3";
                            info.Userid = tbusr.userid;
                            info.Field1 = item.Matchuserid;
                            info.Field2 = "0";
                            db.TInsert<tblinfomation>(info);
                        }
                    }
                }


                if (!string.IsNullOrEmpty(resmsg))
                {
                    db.Dispose();
                    return resmsg;
                }

                team.Status = 0;
                db.TUpdate<tblteams>(team);

                //添加订单
                tblorders order = new tblorders();
                order.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                order.Id = Guid.NewGuid().ToString();
                order.Lineid = team.Lineid;
                order.Match_Id = team.match_id;
                order.Orderid = IDGenerator.GetIdF();
                order.Ordertotal = "300";
                order.Status = 0;
                order.Teamid = team.teamid;
                order.Title = string.Format("[{0}]报名费", match.Match_name);
                order.Userid = team.Userid;

                db.TInsert<tblorders>(order);

                db.SaveChanges();
                return "";
            }
        }

        /// <summary>
        /// 团队导入
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ImpTeams(List<tblmatchentity> mus, ref int count)
        {
            using (var db = new BFdbContext())
            {
                var sns = mus.Distinct(new Comparint());
                count = sns.Count();

                foreach (var sn in sns)
                {
                    var teams = mus.Where(p => p.Pnov == sn.Pnov);
                    string tmid = Guid.NewGuid().ToString();
                    foreach (var team in teams)
                    {
                        tblmatchusers musr = new tblmatchusers();
                        musr.Leader = 0;
                        musr.Matchuserid = Guid.NewGuid().ToString();
                        musr.Age = team.Age;
                        musr.Cardno = team.Cardno;
                        musr.Cardtype = team.Cardtype;
                        musr.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        musr.Match_Id = team.Match_Id;
                        musr.Mobile = team.Mobile;
                        musr.Nickname = team.Nickname;
                        musr.Pay = 0;
                        musr.Pnov = team.Pnov;
                        musr.Sexy = team.Sexy;
                        musr.Teamname = team.Teamname;
                        musr.Teamno = team.Teamno;
                        musr.Teamid = tmid;

                        var usr = db.tblusers.FirstOrDefault(p => p.Mobile == team.Mobile && p.Status == 0);

                        if (usr == null)
                        {
                            usr = new tblusers();
                            usr.cardno = team.Cardno;
                            usr.cardtype = team.Cardtype;
                            usr.Mobile = team.Mobile;
                            usr.Name = team.Nickname;
                            usr.Passwd = "";
                            usr.Playerid = 0;
                            usr.sexy = team.Sexy.ToString();
                            usr.Status = 0;
                            usr.userid = Guid.NewGuid().ToString();

                            db.TInsert<tblusers>(usr);
                        }
                        if (team.Leader == 1)
                        {
                            tblteams tm = new tblteams();
                            tm.Company = team.Passwd;
                            tm.Createtime = DateTime.Now;
                            tm.Eventid = "1";
                            tm.Lineid = team.Lineno;
                            tm.match_id = team.Match_Id;
                            tm.Status = 0;
                            tm.teamid = tmid;
                            tm.Teamname = team.Teamname;
                            tm.Teamno = team.Teamno.ToString();
                            tm.Userid = usr.userid;

                            db.TInsert<tblteams>(tm);

                            tblorders ord = new tblorders();
                            ord.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            ord.Id = Guid.NewGuid().ToString();
                            ord.Lineid = team.Lineno;
                            ord.Match_Id = team.Match_Id;
                            ord.Orderid = IDGenerator.GetIdG();
                            ord.Ordertotal = "200";
                            ord.Status = 0;
                            ord.Teamid = tmid;
                            ord.Title = string.Format("[{0}]报名费用", team.Match_name);
                            ord.Userid = usr.userid;

                            db.TInsert<tblorders>(ord);

                            musr.Leader = 1;
                        }

                        musr.Userid = usr.userid;
                        db.TInsert<tblmatchusers>(musr);
                    }

                }
                return db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class Comparint : IEqualityComparer<tblmatchentity>
        {
            public bool Equals(tblmatchentity x, tblmatchentity y)
            {
                return x.Pnov == y.Pnov;
            }

            public int GetHashCode(tblmatchentity obj)
            {
                return obj.ToString().GetHashCode();
            }
        }

        public int AddExtra(string type, string teamid, string info1, string info2, string info3)
        {
            using (var db = new BFdbContext())
            {
                //判断首字符是否为字母，是，则认为护照
                int std = 0;
                //检查年龄
                if (type == "1" && int.TryParse(info2.Substring(0, 1), out std))
                {
                    string dy = info2.Substring(6, 8);
                    string nw = DateTime.Now.ToString("yyyyMMdd");
                    string m = (int.Parse(nw) - int.Parse(dy) + 1).ToString();

                    if (m.Length > 4)
                    {
                        int y = int.Parse(m.Substring(0, m.Length - 4));
                        if (y < 7 || y > 16)
                            return -80;
                    }
                    else
                        return -80;
                }

                var ex = db.tblmatchextra.FirstOrDefault(p => p.teamid == teamid);
                if (ex == null)
                {
                    ex = new tblmatchextra();
                    ex.Id = Guid.NewGuid().ToString();
                    ex.updt = DateTime.Now;
                    ex.extype = type;
                    ex.teamid = teamid;
                    ex.info1 = info1;
                    ex.info2 = info2;
                    ex.info3 = info3;

                    return db.Insert<tblmatchextra>(ex);
                }
                else
                {
                    ex.updt = DateTime.Now;
                    ex.extype = type;
                    ex.info1 = info1;
                    ex.info2 = info2;
                    ex.info3 = info3;

                    return db.Update<tblmatchextra>(ex);
                }
            }
        }

        public tblmatchextra GetExtra(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblmatchextra.FirstOrDefault(p => p.teamid == tid);
            }
        }

        public string checkcoupon(string coupon, string userid, string company, string teamname)
        {
            using (var db = new BFdbContext())
            {
                tblcoupon cpn = db.tblcoupon.FirstOrDefault(p => p.couponchar == coupon && p.status == "0");
                if (cpn == null)
                    return "-2";

                if (cpn.company != company)
                    return "-908";

                if (!string.IsNullOrEmpty(cpn.teamid))
                    return "-914";

                var tmatch = db.tblmatch.FirstOrDefault(p => p.Match_id == cpn.matchid);
                if (tmatch.Status != "8")
                    return "-910";

                var tm = db.tblteams.FirstOrDefault(p => p.match_id == cpn.matchid && p.Userid == userid);
                if (tm == null)
                {
                    if (db.tblteams.Any(p => p.match_id == cpn.matchid && p.Teamname == teamname))
                        return "-911";

                    tblusers usr = db.tblusers.FirstOrDefault(p => p.userid == userid);
                    if(usr.Isupt!="1")
                        return "-3";

                    tblteams tbl = new tblteams();
                    tbl.Company = cpn.company;
                    tbl.Createtime = DateTime.Now;
                    tbl.Lineid = cpn.lineid;
                    tbl.Linesid = cpn.linesid;
                    tbl.match_id = cpn.matchid;
                    tbl.Status = 2;
                    tbl.teamid = Guid.NewGuid().ToString();
                    tbl.Teamname = teamname;
                    tbl.Teamno = "0";
                    tbl.Eventid = "1";
                    tbl.ChgLines = "0";
                    if (cpn.type == "0")
                        tbl.Teamtype = 1;
                    else
                        tbl.Teamtype = 2;
                    tbl.Type_ = "0";
                    tbl.Userid = userid;


                    tblmatchusers m = new tblmatchusers();
                    m.Cardno = usr.cardno;
                    m.Cardtype = usr.cardtype;
                    m.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    m.Leader = 1;
                    m.Match_Id = cpn.matchid;
                    m.Matchuserid = Guid.NewGuid().ToString();
                    m.Mobile = usr.Mobile;
                    m.Nickname = usr.Name;
                    m.Pay = 0;
                    m.birthday = usr.birthday.Value;

                    SetYearOld(m);

                    int sx = 1;
                    if (int.TryParse(usr.sexy, out sx))
                        m.Sexy = sx;
                    else
                        m.Sexy = 1;
                    m.Status = "1";

                    m.Teamid = tbl.teamid;
                    m.Teamname = tbl.Teamname;
                    m.Userid = userid;

                    //cpn.status = "1";
                    cpn.teamid = tbl.teamid;
                    cpn.usedtime = DateTime.Now;
                    cpn.userid = tbl.Userid;

                    db.TInsert<tblmatchusers>(m);
                    db.TInsert<tblteams>(tbl);
                    db.TUpdate<tblcoupon>(cpn);

                    db.SaveChanges();

                    return tbl.teamid;
                }

                if (tm.Status.Value == 0 && cpn.type == "0")
                    return "-912";

                if (!string.IsNullOrEmpty(tm.Linesid) && !string.IsNullOrEmpty(cpn.linesid) && tm.Linesid != cpn.linesid)
                {
                    int cnt1 = db.tbllines.FirstOrDefault(p => p.Linesid == tm.Linesid).Playercount.Value;
                    int cnt2 = db.tbllines.FirstOrDefault(p => p.Linesid == cpn.linesid).Playercount.Value;

                    if (cnt1 != cnt2)
                    {
                        tm.Status = 2;
                        db.ExecuteSqlCommand(string.Format("delete from tbl_match_users where teamid='{0}'", tm.teamid));
                        db.ExecuteSqlCommand(string.Format("delete from tbl_orders where teamid='{0}'", tm.teamid));

                        tblusers usr2 = db.tblusers.FirstOrDefault(p => p.userid == userid);
                        tblmatchusers m2 = new tblmatchusers();
                        m2.Cardno = usr2.cardno;
                        m2.Cardtype = usr2.cardtype;
                        m2.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        m2.Leader = 1;
                        m2.Match_Id = cpn.matchid;
                        m2.Matchuserid = Guid.NewGuid().ToString();
                        m2.Mobile = usr2.Mobile;
                        m2.Nickname = usr2.Name;
                        m2.Pay = 0;
                        m2.birthday = usr2.birthday;

                        SetYearOld(m2);

                        int sx2 = 1;
                        if (int.TryParse(usr2.sexy, out sx2))
                            m2.Sexy = sx2;
                        else
                            m2.Sexy = 1;
                        m2.Status = "1";

                        m2.Teamid = tm.teamid;
                        m2.Teamname = tm.Teamname;
                        m2.Userid = userid;

                        db.Insert<tblmatchusers>(m2);
                    }
                }

                //if (!string.IsNullOrEmpty(tm.Linesid) && tm.Linesid != cpn.linesid)
                //    return "-913";

                if (tm.Status == 1 || tm.Status == 0)
                    cpn.status = "1";
                cpn.teamid = tm.teamid;
                cpn.usedtime = DateTime.Now;
                cpn.userid = tm.Userid;

                db.Update<tblcoupon>(cpn);

                if (cpn.type == "0")
                    tm.Teamtype = 1;
                else
                {
                    if (tm.Status.Value == 1)
                        tm.Status = 0;
                    tm.Teamtype = 2;
                }
                tm.Lineid = cpn.lineid;
                tm.Linesid = cpn.linesid;
                tm.Company = cpn.company;

                db.Update<tblteams>(tm);

                var ord = db.tblorders.FirstOrDefault(p => p.Teamid == tm.teamid);
                if (ord != null)
                {
                    ord.Lineid = tm.Lineid;
                    ord.Ordertotal = db.tbllines.FirstOrDefault(p => p.Linesid == cpn.linesid).Price;
                    db.Update<tblorders>(ord);
                }
                return tm.teamid;
            }
        }

        public int ckcoupon(string coupon, string userid, string company, ref Fmodel fm)
        {
            using (var db = new BFdbContext())
            {
                tblcoupon cpn = db.tblcoupon.FirstOrDefault(p => p.couponchar == coupon && p.status == "0");
                if (cpn == null)
                    return -2;

                if (cpn.company != company)
                    return -908;

                if (!string.IsNullOrEmpty(cpn.teamid))
                    return -913;

                var tmatch= db.tblmatch.FirstOrDefault(p => p.Match_id == cpn.matchid);
                if (tmatch.Status != "8")
                    return -910;

                fm.matchname = tmatch.Match_name;
                fm.linetype = db.tblline.FirstOrDefault(p => p.Lineid == cpn.lineid).Name;
                fm.linename = db.tbllines.FirstOrDefault(p => p.Linesid == cpn.linesid).Linename;
                fm.ftype = cpn.type;

                var tm = db.tblteams.FirstOrDefault(p => p.match_id == cpn.matchid && p.Userid == userid);
                if (tm == null)
                    return 0;

                if (tm.Status.Value == 0 && cpn.type == "0")
                    return -911;

                fm.teamname = tm.Teamname;
                fm.tmstatus = tm.Status.ToString();

                if (!string.IsNullOrEmpty(tm.Linesid) && !string.IsNullOrEmpty(cpn.linesid) && tm.Linesid != cpn.linesid)
                {
                    int cnt1 = db.tbllines.FirstOrDefault(p => p.Linesid == tm.Linesid).Playercount.Value;
                    int cnt2 = db.tbllines.FirstOrDefault(p => p.Linesid == cpn.linesid).Playercount.Value;

                    if (cnt1 != cnt2)
                        return -909;
                }

                //if (!string.IsNullOrEmpty(tm.Linesid) && tm.Linesid != cpn.linesid)
                //    return -912;
            }
            return 0;
        }

        public tblmatchentity GetMatchUsersByUserid(string userid)
        {
            using (var db = new BFdbContext())
            {
                tblteams tm = db.tblteams.FirstOrDefault(p => p.Userid == userid && p.Status == 1);

                if (tm == null)
                    return null;

                return db.SqlQuery<tblmatchentity>("SELECT b.teamname,b.teamid,c.status as mstatus,c.match_name,c.date3,c.date4,b.teamno as teamno_t,b.status as Teamstatus,d.linename,b.teamtype FROM tbl_teams b,tbl_match c,tbl_lines d where b.match_id=c.match_id and b.linesid=d.lines_id and b.teamid='" + tm.teamid + "'").FirstOrDefault();
            }
        }
    }
}
