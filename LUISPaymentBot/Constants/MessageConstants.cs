using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Constants
{
    public static class MessageConstants
    {
        public const string PaymentIntent = "SEDC Account Management";

        public const string LeaveIntent = "Employee Leave Management";

        public const string ServiceSelection = "What service would like to select?";

        public const string IncorrectServiceSelection = "I am sorry, I didn't understand that. Please select one of the options.";

        public const string NotImplemted = "This functionality is not yet implemented!";

        public const string WelcomeMsg = "Welcome to Utility Service Desk";

        public const string Authenticated = "Hello {0}. How can i help you today?";

        public const string ServicesInformation = "You can ask me to provide bill information, make payment and also report an outage";

        public const string LoginError = "We didn't find this account number on our records. Please enter a valid account number";

        public const string NoProfileExist = "No profile available, Please create profile before making payment.";

        public const string PleaseWait = "Please wait we are retreiving the information";

        public const string BillAmount = "Your bill amount {0}";

        public const string PaymentSuccess = "Your payment is successfull. Please save your approval code {0} for future reference";

        public const string PaymentDeclined = "Your payment has been declined";

        public const string DueAmount = "You don't have dues to pay";
    }
}