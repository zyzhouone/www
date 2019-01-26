﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;


namespace Model
{
    public class tblteamsVew
    {
        public string teamid
        { get; set; }

        public string match_id
        { get; set; }

        public string Teamno
        { get; set; }

        public string Teamname
        { get; set; }

        public string Userid
        { get; set; }

        public string Moblie
        { get; set; }

        public string Username
        { get; set; }

        public string Companyid
        { get; set; }

        public string Companyname
        { get; set; }

        public string Lineid
        { get; set; }

        public string Linename
        { get; set; }

        public DateTime? Createtime
        { get; set; }

        public int? Eventid
        { get; set; }

        public string Eventname
        { get; set; }

        public int? Status
        { get; set; }



    }
}
