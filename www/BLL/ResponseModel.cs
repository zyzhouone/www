using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLL
{
   public class ResponseModel
    {
       public int status { get; set; }
       public string desc { get; set; }
       public object data { get; set; }
    }

   public class ResultModel
   {
       public int code { get; set; }
       public string msg { get; set; }
   }
}
