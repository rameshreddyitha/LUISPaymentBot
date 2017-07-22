using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models
{
    public class AuthenticateInput
    {
        public string mbrsep { get; set; }
        public string tokenpwd { get; set; }
    }
}