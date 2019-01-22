using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/********************************************
 * 
 *  
 * *****************************************/
namespace Model
{
    public class Fmodel
    {
        public string teamname
        { get;set; }

        public string linetype
        { get; set; }

        public string linename
        { get;set; }

        public string ftype
        { get; set; }

        public string matchname
        { get; set; }

        public string tmstatus
        { get; set; }

        public string tag
        { get; set; }
    }
}
