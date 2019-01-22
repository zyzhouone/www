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
    [Table("tbl_answer")]
    public class tblanswer
    {
        [Key]
        [Column("answerid", Order = 1)]
        public string answerid
        { get; set; }

        [Column("`questionid`")]
        public string questionid
        { get; set; }

        [Column("`userid`")]
        public string userid
        { get; set; }

        [Column("`matchid`")]
        public string matchid
        { get; set; }

        [Column("`result`")]
        public string result
        { get; set; }

        [Column("`updtime`")]
        public DateTime? updtime
        { get; set; }
    }
}
