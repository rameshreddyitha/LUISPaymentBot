using LUISPaymentBot.Enums;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Queries
{
    [Serializable]
    public class PaymentQuery
    {
        [Prompt("Please enter your {&}.")]
        [Describe("Account Number")]
        public string Mbrsep { get; set; }

        [Prompt("Pay by {||}")]
        public ProfileType ProfileType { get; set; }

        [Prompt("Tap Yes for full payment. To make partial payment tap No and enter the amount {||}")]
        public bool FullPayment { get; set; }

        [Prompt("Please enter your {&}.")]
        [Describe("Bill Amount")]
        public string BillAmount { get; set; }
    }
}