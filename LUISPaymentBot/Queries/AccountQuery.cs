using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models.Queries
{
    [Serializable]
    public class AccountQuery
    {
        [Prompt("Please enter your {&}.")]
        [Describe("Account Number")]
        public string Mbrsep { get; set; }
    }
}