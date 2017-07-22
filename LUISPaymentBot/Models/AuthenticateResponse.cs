using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models
{
    [Serializable]
    public class AuthenticateResponse
    {
        public string Name { get; set; }
        public string MbrSep { get; set; }
        public bool CreditCard { get; set; }
        public bool Echeck { get; set; }
        public string AmountDue { get; set; }
        public string DueDate { get; set; }
        public string Location { get; set; }
        public string LastPayment { get; set; }
        public string PaidDate { get; set; }
    }
}