using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

/********************************************
 * tbl_coupon实体类
 * 
 * *****************************************/
namespace Model
{
    [Table("tbl_coupon")]
    public class tblcoupon
    {
        [Key]
        [Column("couponid", Order = 1)]
        public string couponid
        { get; set; }

        [Column("`couponchar`")]
        public string couponchar
        { get; set; }

        [Column("`matchid`")]
        public string matchid
        { get; set; }

        [Column("`userid`")]
        public string userid
        { get; set; }

        [Column("`teamid`")]
        public string teamid
        { get; set; }

        [Column("`createtime`")]
        public DateTime? createtime
        { get; set; }

        [Column("`usedtime`")]
        public DateTime? usedtime
        { get; set; }

        [Column("`status`")]
        public string status
        { get; set; }

        [Column("`type`")]
        public string type
        { get; set; }

        [Column("`lineid`")]
        public string lineid
        { get; set; }

        [Column("`linesid`")]
        public string linesid
        { get; set; }

        [Column("`company`")]
        public string company
        { get; set; }

        [Column("`mobile`")]
        public string mobile
        { get; set; }
    }
}
