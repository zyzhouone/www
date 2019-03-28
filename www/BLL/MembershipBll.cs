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
    public class MembershipBll : BaseBll
    {
        /// <summary>
        /// 根据密码获取用户信息
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public tblusers GetUser(string mobile, string pwd)
        {
            using (var db = new BFdbContext())
            {
                return db.tblusers.FirstOrDefault(p => p.Passwd == pwd && p.Status == 0 && p.Mobile == mobile);
                //return db.tblusers.FirstOrDefault(p =>  p.Status == 0 && p.Mobile == mobile);
            }
        }

        public int UpdateUserLastTime(string userid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format("update tbl_users set last_time=now() where userid='{0}'", userid);
                return db.ExecuteSqlCommand(sql);
            }
        }

        public tblusers GetUser(string mobile)
        {
            using (var db = new BFdbContext())
            {
                return db.tblusers.FirstOrDefault(p => p.Status == 0 && p.Mobile == mobile);
            }
        }

        /// <summary>
        /// 用户注册时，生成验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public int GetSMS(string mobile,string ip,ref SMSResponse rep)
        {
            using (var db = new BFdbContext())
            {
                if (db.tblusers.Any(p => p.Mobile == mobile && p.Status == 0))
                    return -1;

                DateTime dt = DateTime.Now.AddDays(-1);
                int cntt = db.tbluserstime.Count(p => p.RomateIp == ip && p.crtdate >= dt);
                if (cntt > 10)
                    return -3;
                int cnt = db.tbluserstime.Count(p => p.Mobile == mobile && p.crtdate >= dt);
                if (cnt > 10)
                    return -2;

                tblusers usr = new tblusers();
                usr.Mobile = mobile;
                usr.Passwd = "-";
                usr.mono = VerifyCode.Get6SzCode();
                usr.Status = 4;
                usr.Playerid = 0;
                usr.userid = Guid.NewGuid().ToString();
                usr.Isupt = "0";
                usr.Type = "0";

                int res = db.Insert<tblusers>(usr);

                tbluserstime tm = new tbluserstime();
                tm.crtdate = DateTime.Now;
                tm.Mobile = mobile;
                tm.tid = Guid.NewGuid().ToString();
                tm.RomateIp = ip;

                db.Insert<tbluserstime>(tm);

                if (res > 0)
                {
                    rep = SMSHepler.SendRegSms(mobile, usr.mono);
                }
                return res;
            }
        }

        /// <summary>
        /// 忘记密码时，生成验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public int GetGSMS(string mobile)
        {
            using (var db = new BFdbContext())
            {
                tblusers usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.Status == 0);
                if (usr == null)
                    return -1;

                usr.mono = VerifyCode.Get6SzCode();

                int res = db.Update<tblusers>(usr);

                if (res > 0)
                    SMSHepler.SendGetSms(mobile, usr.mono);

                return res;
            }
        }

        /// <summary>
        /// 注册用户时，检验验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="vercode"></param>
        /// <param name="usrid"></param>
        /// <returns></returns>
        public int CheckSms(string mobile, string vercode, ref string usrid)
        {
            using (var db = new BFdbContext())
            {
                if (db.tblusers.Any(p => p.Mobile == mobile && p.Status == 0))
                    return -2;

                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.mono == vercode && p.Status == 4);
                if (usr == null)
                    return -1;
                else
                {
                    usrid = usr.userid;
                    return 0;
                }
            }
        }

        /// <summary>
        /// 忘记密码时，检验验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="vercode"></param>
        /// <param name="usrid"></param>
        /// <returns></returns>
        public int CheckGSms(string mobile, string vercode)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.mono == vercode && p.Status == 0);
                if (usr == null)
                    return -1;
                else
                    return 0;
            }
        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="usrid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int SubmitPwd(string usrid, string pwd)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.userid == usrid && p.Status == 4);
                if (usr == null)
                    return -1;
                else
                {
                    usr.Status = 0;
                    usr.Passwd = pwd;

                    return db.Update<tblusers>(usr);
                }
            }
        }

        private int SetYearOld(DateTime birthday)
        {
            try
            {
                string dy = birthday.ToString("yyyyMMdd");
                string nw = DateTime.Now.ToString("yyyyMMdd");
                string m = (int.Parse(nw) - int.Parse(dy) + 1).ToString();

                if (m.Length > 4)
                    return int.Parse(m.Substring(0, m.Length - 4));
                else
                    return 0;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int UpdateUser(tblusers usr)
        {
            using (var db = new BFdbContext())
            {
                if (db.tblusers.Any(p => p.cardno == usr.cardno && p.userid != usr.userid))
                    return -2;

                int yearold = SetYearOld(usr.birthday.Value);
                if (yearold > 60 || yearold < 16)
                    return -3;

                var tusr = db.tblusers.FirstOrDefault(p => p.userid == usr.userid);
                if (tusr == null)
                    return -1;
                else
                {
                    tusr.birthday = usr.birthday;
                    tusr.cardno = usr.cardno;
                    tusr.cardtype = usr.cardtype;
                    tusr.Mobile = usr.Mobile;
                    tusr.Name = usr.Name;
                    tusr.sexy = usr.sexy;
                    tusr.Isupt = "1";
                    tusr.Ismod = "1";
                    tusr.Modtime = DateTime.Now;

                    // or match_id='6a61b95b-2d5d-4373-abaf-bf4e4c438800')
                    string sql = string.Format(@"update tbl_match_users set birthday='{0}',age={1},cardno='{2}',cardtype='{3}',sexy={4},mobile='{5}',nickname='{6}' where userid='{7}' and  (match_id in (select match_id from tbl_match where status in (0,1))  or match_id='c83aa363-873e-489e-ac07-373489c94320')",
                        tusr.birthday.Value.ToString("yyyy-MM-dd"), SetYearOld(tusr.birthday.Value), tusr.cardno, tusr.cardtype, tusr.sexy, tusr.Mobile, tusr.Name, tusr.userid);
                    db.ExecuteSqlCommand(sql);

                    return db.Update<tblusers>(tusr);
                }
            }
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="usrid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public int ResetPwd(string mobile, string pwd)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.Status == 0);
                if (usr == null)
                    return -1;
                else
                {
                    usr.Passwd = pwd;

                    return db.Update<tblusers>(usr);
                }
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
                string sql = string.Format(@"select a.*,b.leader,b.pay,concat(b.teamname,'【',d.linename,'】') as teamname,b.teamid,CAST(c.status as char) as mstatus,c.teamtype 
                                                from tbl_match a,tbl_match_users b,tbl_teams c ,tbl_lines d where a.match_id=b.match_id and 
                                                        b.teamid=c.teamid  and c.linesid=d.lines_id  
                                                and (b.status in ('0','1') or b.leader=1) and b.userid='{0}' order by c.createtime desc", userid);
                return db.SqlQuery<tblmatchentity>(sql).ToList();
            }
        }

        /// <summary>
        /// 我的消息
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<tblinfomation> GetMyinfo(string userid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format(@"select a.* from tbl_infomation a where a.type in ('2','3','4') and a.userid='{0}' order by createtime desc", userid);
                return db.SqlQuery<tblinfomation>(sql).ToList();
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="oldpwd"></param>
        /// <param name="newpwd"></param>
        /// <returns></returns>
        public int UpdatePwd(string userid, string oldpwd, string newpwd)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.userid == userid && p.Passwd == oldpwd && p.Status == 0);
                if (usr == null)
                    return -1;
                else
                {
                    usr.Passwd = newpwd;

                    return db.Update<tblusers>(usr);
                }
            }
        }

        public tblusers GetUserById(string userid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblusers.FirstOrDefault(p => p.userid == userid);
            }
        }

        public tblusers GetUserByTeamId(string tid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format("select a.* from tbl_users a,tbl_teams b where a.userid=b.userid and b.teamid='{0}'", tid);
                return db.SqlQuery<tblusers>(sql).FirstOrDefault();
            }
        }

        public int AcceptMatch(string infoid, string matchuserid)
        {
            using (var db = new BFdbContext())
            {
                var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == matchuserid);
                if (musr == null)
                    return -1;
                else
                {
                    var ur = db.tblusers.FirstOrDefault(p => p.userid == musr.Userid);
                    if (string.IsNullOrEmpty(ur.Isupt) || ur.Isupt == "0")
                        return -3;

                    var lst = db.tblmatchusers.Where(p => p.Match_Id == musr.Match_Id && p.Userid == musr.Userid && p.Matchuserid != musr.Matchuserid);
                    //已经接受别的队伍
                    if (lst.Any(p => p.Status == "1"))
                        return -2;

                    string sql = string.Format("select a.* from tbl_users a,tbl_teams b where a.userid=b.userid and b.teamid='{0}'", musr.Teamid);
                    var leader = db.SqlQuery<tblusers>(sql).FirstOrDefault();

                    tblinfomation info = new tblinfomation();
                    info.Context = string.Format("用户[{0}]已经接受了你的邀请,赶快去报名吧.", musr.Mobile);
                    info.createtime = DateTime.Now;
                    info.Infoid = Guid.NewGuid().ToString();
                    info.Mobile = leader.Mobile;
                    info.Status = "0";
                    info.Type = "3";
                    info.Userid = leader.userid;
                    db.TInsert<tblinfomation>(info);

                    foreach (var item in lst)
                    {
                        item.Status = "9";
                        db.TUpdate<tblmatchusers>(item);
                    }

                    var info2 = db.tblinfomation.FirstOrDefault(p => p.Infoid == infoid);
                    info2.Field2 = "1";
                    db.TUpdate<tblinfomation>(info2);

                    musr.Status = "1";
                    musr.birthday = ur.birthday;
                    SetYearOld(musr);
                    musr.Cardno = ur.cardno;
                    musr.Cardtype = ur.cardtype;
                    musr.Nickname = ur.Name;
                    musr.Mobile = ur.Mobile;

                    int sx = 0;
                    if (int.TryParse(ur.sexy, out sx))
                        musr.Sexy = sx;

                    db.TUpdate<tblmatchusers>(musr);

                    return db.SaveChanges();
                }
            }
        }

        private void SetYearOld(tblmatchusers item)
        {
            try
            {
                //if (item.Cardtype == "1")
                //{
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
                //}
            }
            catch (Exception ex)
            {
                item.Age = 0;
            }
        }

        public int RejectMatch(string infoid, string matchuserid)
        {
            using (var db = new BFdbContext())
            {
                var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == matchuserid);
                if (musr == null)
                    return -1;
                else
                {
                    var info = db.tblinfomation.FirstOrDefault(p => p.Infoid == infoid);
                    info.Field2 = "2";
                    db.TUpdate<tblinfomation>(info);

                    musr.Status = "9";
                    db.TUpdate<tblmatchusers>(musr);

                    return db.SaveChanges();
                }
            }
        }

        public int DelTeam(string tid)
        {
            using (var db = new BFdbContext())
            {
                string sql = "delete from tbl_teams where status='6' and teamid='" + tid + "'";
                int res = db.ExecuteSqlCommand(sql);

                if (res > 0)
                {
                    sql = "delete from tbl_match_users where teamid='" + tid + "'";
                    res = db.ExecuteSqlCommand(sql);

                    sql = "delete from tbl_orders where status=0 and teamid='" + tid + "'";
                    res = db.ExecuteSqlCommand(sql);
                }

                return res;
            }
        }

        public tblorders GetOrderById(string id)
        {
            using (var db = new BFdbContext())
            {
                return db.tblorders.FirstOrDefault(p => p.Orderid == id);
            }
        }

        public tblorders GetOrderByTeamId(string tid)
        {
            using (var db = new BFdbContext())
            {
                return db.tblorders.FirstOrDefault(p => p.Teamid == tid);
            }
        }

        public tblmatchentity GetMatchByOrderId(string oid)
        {
            using (var db = new BFdbContext())
            {
                string sql = string.Format("select a.*,c.mobile from tbl_match a,tbl_orders b,tbl_users c where a.match_id=b.match_id and b.userid=c.userid and b.orderid='{0}'", oid);

                return db.SqlQuery<tblmatchentity>(sql).FirstOrDefault();
            }
        }

        public string CheckPay(string oid, int paycnt, string matchid)
        {
            using (var db = new BFdbContext())
            {
                BFParameters bf = new BFParameters();
                bf.Add("@orderid", oid);
                bf.Add("@tcnt", paycnt, DbType.Int32);
                bf.Add("@matchid", matchid);
                bf.Add("@msg", null, DbType.String, ParameterDirection.Output);
                db.MysqlExecuteProcedure("sp_paycheck", bf);

                return bf.GetOutParameter("@msg").ToString();
            }
        }

        public void PayReturn(string orderno, string tradeno, string buyeremail, string tradestatus)
        {
            using (var db = new BFdbContext())
            {
                BFParameters bf = new BFParameters();
                bf.Add("@orderno", orderno);
                bf.Add("@tradeno", tradeno);
                bf.Add("@buyeremail", buyeremail);
                bf.Add("@tradestatus", tradestatus);
                db.MysqlExecuteProcedure("sp_payreturn", bf);

            }
        }

        public int Replayer(string usrid,string mobile,string mid)
        {
            using (var db = new BFdbContext())
            {
                var usr = db.tblusers.FirstOrDefault(p => p.Mobile == mobile && p.Status == 0);
                if (usr == null)
                    return -4;

                var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == mid);
                if (musr==null||musr.Status != "1")
                    return -3;

                var match = db.tblmatch.FirstOrDefault(p => p.Match_id == musr.Match_Id);
                var leader = db.tblmatchusers.FirstOrDefault(p => p.Teamid == musr.Teamid && p.Userid == usrid);

                if (leader.Leader != 1)
                    return -2;

                if (db.tblmatchusers.Any(p => p.Match_Id == musr.Match_Id && p.Userid == usr.userid&&p.Status=="1"))
                    return -1;

                tblreplace tbl=new tblreplace();
                tbl.Createtime=DateTime.Now;
                tbl.D_Age=musr.Age;
                tbl.D_Birthday=musr.birthday;
                tbl.D_Cardno=musr.Cardno;
                tbl.D_Cardtype=musr.Cardtype;
                tbl.D_Flag="0";
                tbl.D_Matchuserid=musr.Matchuserid;
                tbl.D_Mobile=musr.Mobile;
                tbl.D_Nickname=musr.Nickname;
                tbl.D_Sexy=musr.Sexy;
                tbl.D_Userid=musr.Userid;
                tbl.Id=Guid.NewGuid().ToString();
                tbl.Match_Id=musr.Match_Id;
                tbl.S_Flag="0";
                tbl.S_Userid=usr.userid;
                tbl.Teamid=musr.Teamid;
                
                db.TInsert<tblreplace>(tbl);

                musr.Status="8";
                db.TUpdate<tblmatchusers>(musr);


                tblinfomation tblA = new tblinfomation();
                tblA.Context = string.Format("用户[{0}]邀请你加入[{1}]队伍,参加[{2}],请火速接受邀请吧.", leader.Mobile, leader.Teamname, match.Match_name);
                tblA.createtime = DateTime.Now;
                tblA.Field1 = tbl.Id;
                tblA.Field2 = "0";
                tblA.Infoid = Guid.NewGuid().ToString();
                tblA.Mobile = usr.Mobile;
                tblA.Status = "0";
                tblA.Type = "4";
                tblA.Userid = usr.userid;
                db.TInsert<tblinfomation>(tblA);

                tblinfomation tblB = new tblinfomation();
                tblB.Context = string.Format("你参加的[{0}],已被队长替换;如果接受,请点击【同意】按钮.", match.Match_name);
                tblB.createtime = DateTime.Now;
                tblB.Field1 = tbl.Id;
                tblB.Field2 = "0";
                tblB.Infoid = Guid.NewGuid().ToString();
                tblB.Mobile = musr.Mobile;
                tblB.Status = "0";
                tblB.Type = "4";
                tblB.Userid = musr.Userid;
                db.TInsert<tblinfomation>(tblB);

                return db.SaveChanges();
            }
        }

        public int AcceptReplay(string infoid)
        {
            using (var db = new BFdbContext())
            {
                var info = db.tblinfomation.FirstOrDefault(p => p.Infoid == infoid);
                if (info == null||info.Field2!="0")
                    return -1;

                var rep=db.tblreplace.FirstOrDefault(p=>p.Id==info.Field1);
                var leader = db.tblmatchusers.FirstOrDefault(p => p.Teamid == rep.Teamid && p.Leader == 1);

                string mobile = "";

                //需要替换队员的处理
                if (rep.S_Userid == info.Userid)
                {
                    var usr = db.tblusers.FirstOrDefault(p => p.userid == info.Userid);
                    if (usr.Isupt != "1")
                        return -2;

                    if (db.tblmatchusers.Any(p => p.Match_Id == rep.Match_Id && p.Userid == usr.userid && p.Status == "1"))
                        return -3;

                    var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == rep.D_Matchuserid);
                    if (musr == null)
                        return -8;

                    mobile = usr.Mobile;

                    //被替换队员已经同意
                    if (rep.D_Flag == "1")
                    {
                        tblmatchusers tm = new tblmatchusers();
                        tm.birthday = usr.birthday;
                        tm.Cardno = usr.cardno;
                        tm.Cardtype = usr.cardtype;
                        tm.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        tm.Leader = 0;
                        tm.Match_Id = rep.Match_Id;
                        tm.Matchuserid = Guid.NewGuid().ToString();
                        tm.Mobile = usr.Mobile;
                        tm.Nickname = usr.Name;
                        tm.Pay = 0;
                        tm.Sexy = int.Parse(usr.sexy);
                        tm.Status = "1";
                        tm.Teamid = rep.Teamid;
                        tm.Teamname = musr.Teamname;
                        tm.Userid = usr.userid;
                        SetYearOld(tm);

                        if (tm.Age < 16 || tm.Age > 60)
                            return -9;

                        db.TInsert<tblmatchusers>(tm);
                        db.TDelete<tblmatchusers>(musr);

                        rep.S_Matchuserid = tm.Matchuserid;
                    }

                    rep.S_Agreetime = DateTime.Now;
                    rep.S_Flag = "1";
                    db.TUpdate<tblreplace>(rep);
                }
                else
                {
                    var usr = db.tblusers.FirstOrDefault(p => p.userid == rep.S_Userid);

                    mobile = rep.D_Mobile;

                    if (rep.S_Flag == "1")
                    {
                        var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == rep.D_Matchuserid);
                        if (musr == null)
                            return -8;

                        tblmatchusers tm = new tblmatchusers();
                        tm.birthday = usr.birthday;
                        tm.Cardno = usr.cardno;
                        tm.Cardtype = usr.cardtype;
                        tm.Createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        tm.Leader = 0;
                        tm.Match_Id = rep.Match_Id;
                        tm.Matchuserid = Guid.NewGuid().ToString();
                        tm.Mobile = usr.Mobile;
                        tm.Nickname = usr.Name;
                        tm.Pay = 0;
                        tm.Sexy = int.Parse(usr.sexy);
                        tm.Status = "1";
                        tm.Teamid = rep.Teamid;
                        tm.Teamname = leader.Teamname;
                        tm.Userid = usr.userid;
                        SetYearOld(tm);

                        if (tm.Age < 16 || tm.Age > 60)
                            return -9;

                        db.TInsert<tblmatchusers>(tm);
                        db.TDelete<tblmatchusers>(musr);

                        rep.S_Matchuserid = tm.Matchuserid;
                    }

                    rep.D_Agreetime = DateTime.Now;
                    rep.D_Flag = "1";
                    db.TUpdate<tblreplace>(rep);
                }

                info.Field2 = "1";
                db.TUpdate<tblinfomation>(info);

                tblinfomation tblA = new tblinfomation();
                tblA.Context = string.Format("用户[{0}]接受了你的替换队员请求,请查看.", mobile);
                tblA.createtime = DateTime.Now;
                tblA.Infoid = Guid.NewGuid().ToString();
                tblA.Mobile = leader.Mobile;
                tblA.Status = "0";
                tblA.Type = "2";
                tblA.Userid = leader.Userid;
                db.TInsert<tblinfomation>(tblA);

                return db.SaveChanges();
            }
        }

        public int RejectReplay(string infoid)
        {
            using (var db = new BFdbContext())
            {
                var info = db.tblinfomation.FirstOrDefault(p => p.Infoid == infoid);
                if (info == null || info.Field2 != "0")
                    return -1;

                var rep = db.tblreplace.FirstOrDefault(p => p.Id == info.Field1);
                var musr = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == rep.D_Matchuserid);
                var leader = db.tblmatchusers.FirstOrDefault(p => p.Teamid == rep.Teamid && p.Leader == 1);

                if (musr == null)
                    return -2;

                if (info.Userid == rep.S_Userid)
                {
                    rep.S_Agreetime = DateTime.Now;
                    rep.S_Flag = "2";
                }
                else
                {
                    rep.D_Agreetime = DateTime.Now;
                    rep.D_Flag = "2";
                }

                info.Field2 = "2";
                musr.Status = "1";

                tblinfomation tblA = new tblinfomation();
                tblA.Context = string.Format("用户[{0}]拒绝了你替换队员的请求,请查看.", info.Mobile);
                tblA.createtime = DateTime.Now;
                tblA.Infoid = Guid.NewGuid().ToString();
                tblA.Mobile = leader.Mobile;
                tblA.Status = "0";
                tblA.Type = "2";
                tblA.Userid = leader.Userid;
                db.TInsert<tblinfomation>(tblA);

                db.TUpdate<tblinfomation>(info);
                db.TUpdate<tblreplace>(rep);
                db.TUpdate<tblmatchusers>(musr);

                return db.SaveChanges();
            }
        }

        public int ReLeader(string lid, string mid)
        {
            using (var db = new BFdbContext())
            {
                var player = db.tblmatchusers.FirstOrDefault(p => p.Matchuserid == mid);

                var leader = db.tblmatchusers.FirstOrDefault(p => p.Userid == lid && p.Teamid == player.Teamid);
                if (leader.Leader != 1)
                    return -1;

                var team = db.tblteams.FirstOrDefault(p => p.teamid == leader.Teamid);

                leader.Leader = 0;
                leader.Mono = DateTime.Now.ToString("yyMMddHHmmss");

                player.Leader = 1;
                team.Userid = player.Userid;

                db.TUpdate<tblmatchusers>(leader);
                db.TUpdate<tblmatchusers>(player);
                db.TUpdate<tblteams>(team);

                return db.SaveChanges();
            }
        }

        public int changeteam(tblteams tm)
        {
            using (var db = new BFdbContext())
            {
                var team = db.tblteams.FirstOrDefault(p => p.teamid == tm.teamid);

                if (team == null)
                    return -1;

                if (team.Status.Value == 0)
                    return -2;

                if (db.tblteams.Any(p => p.teamid != tm.teamid && p.match_id == team.match_id && p.Teamname == tm.Teamname))
                    return -3;

                string sql = string.Format("update tbl_match_users set teamname='{0}' where teamid='{1}'", tm.Teamname, tm.teamid);
                db.ExecuteSqlCommand(sql);

                team.Teamname = tm.Teamname;
                return db.Update<tblteams>(team);
            }
        }

        public tbllines GetLineById(string tid)
        {
            using (var db = new BFdbContext())
            {
                var tm = db.tblteams.FirstOrDefault(p => p.teamid == tid);
                return db.tbllines.FirstOrDefault(p => p.Linesid == tm.Linesid);
            }
        }
    }
}
