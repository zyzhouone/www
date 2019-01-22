using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/********************************************
 * tbl_points实体类
 * 
 * *****************************************/
namespace Model
{
    [Table("tbl_points")]
    public class tblpoints
    {
        [Key]
        [Column("id",Order=1)]
        public int? Id
        { get;set; }

        [Column("`eventid`")]
        public int? Eventid
        { get;set; }

        [Column("`lineid`")]
        public int? Lineid
        { get;set; }

        [Column("`pointname`")]
        public string Pointname
        { get;set; }

        [Column("`content`")]
        public string Content
        { get;set; }

        [Column("`sort`")]
        public int? Sort
        { get;set; }

        [Column("`pointtype`")]
        public int? Pointtype
        { get;set; }

        [Column("`status`")]
        public int? Status
        { get;set; }

        [Column("`creater`")]
        public int? Creater
        { get;set; }

        [Column("`pointno`")]
        public string Pointno
        { get;set; }

    }
}
