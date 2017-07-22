using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Constants
{
    public static class MessageConstants
    {
        //public const string PaymentIntent = "SEDC Account Management";

        //public const string LeaveIntent = "Employee Leave Management";

        //public const string ServiceSelection = "What service would like to select?";

        //public const string IncorrectServiceSelection = "I am sorry, I didn't understand that. Please select one of the options.";

        //public const string NotImplemted = "This functionality is not yet implemented!";

        public const string WelcomeMsg = "Welcome to Utility Service Desk";

        public const string Authenticated = "Hello {0}. How can i help you today?";

        public const string ServicesInformation = "You can ask me to provide bill information, make payment and also report an outage";

        public const string LoginError = "We didn't find this account number on our records. Please enter a valid account number";

        public const string NoProfileExist = "We don't find any payment profile(s) for your account. Please create an E-Check or Credit Card Profile for making payment";

        public const string PleaseWait = "Please wait... We are retreiving the information";

        public const string BillAmount = "Your bill amount is {0} and Due Date is {1}";

        public const string PaymentSuccess = "Your payment is successfull. Please save the approval code '{0}' for future reference";

        public const string PaymentDeclined = "Your payment is declined";

        public const string DueAmount = "You don't have any dues";

        public const string OutageSuccess = "Thank you for reporting the outage at your location.  We will try to have your service back on as soon as possible.";

        public const string OutageAlreadyExist = "An outage has already been declared at your location.";

        public const string ThankYou = "Thanks for using Utility Service Desk";

        public const string OutageQurey = "?mbrsep={0}&location={1}";
    }
}