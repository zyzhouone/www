using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/********************************************
 * tbl_pay实体类
 * 
 * *****************************************/
namespace Model
{
    [Table("tbl_match_extra")]
    public class tblmatchextra
    {
        [Key]
        [Column("id", Order = 1)]
        public string Id
        { get; set; }

        [Column("`updt`")]
        public DateTime? updt
        { get; set; }

        [Column("`extype`")]
        public string extype
        { get; set; }

        [Column("`teamid`")]
        public string teamid
        { get; set; }

        [Column("`info1`")]
        public string info1
        { get; set; }

        [Column("`info2`")]
        public string info2
        { get; set; }

        [Column("`info3`")]
        public string info3
        { get; set; }

        [Column("`info4`")]
        public string info4
        { get; set; }
        [Column("`info5`")]
        public string info5
        { get; set; }
        [Column("`info6`")]
        public string info6
        { get; set; }

        [Column("`cardtype`")]
        public string cardtype
        { get; set; }

        [Column("`birthday`")]
        public string birthday
        { get; set; }

        [Column("`sexy`")]
        public string sexy
        { get; set; }
    }
}
