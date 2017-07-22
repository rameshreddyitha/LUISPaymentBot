using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models
{
    [Serializable]
    public class AccountInfo
    {
        public string Mbrsep { get; set; }

        public string DueDate { get; set; }

        public string AmountDue { get; set; }

        public string PaidDate { get; set; }

        public string LastPayment { get; set; }

        public bool CCExists { get; set; }

        public bool ECExists { get; set; }

        public string Error { get; set; }

    }
}