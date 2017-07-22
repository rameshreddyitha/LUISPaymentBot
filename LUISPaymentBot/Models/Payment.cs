using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models
{
    public class Payment
    {
        public string Mbrsep { get; set; }

        public string PaymentType { get; set; }

        public string Amount { get; set; }
    }
}