using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUISPaymentBot.Models
{
    [Serializable]
    public class PaymentResponse
    {
        public string AuthorizationCodeField { get; set; }

        public string DescriptionField { get; set; }

        public string ExtensionsListField { get; set; }

        public string ErrorStringField { get; set; }

        public string ObjectIDField { get; set; }

        public string CommentsField { get; set; }

        public int VerbField { get; set; }
    }

}