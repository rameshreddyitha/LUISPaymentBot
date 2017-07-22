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

        [Prompt("Please select profile {&}. {||}")]
        [Describe("type")]
        public ProfileType ProfileType { get; set; }

        [Prompt("Would you like to pay full amount?{||}")]
        public bool FullPayment { get; set; }

        [Prompt("Please enter your {&}.")]
        [Describe("Bill Amount")]
        public string BillAmount { get; set; }
    }
}