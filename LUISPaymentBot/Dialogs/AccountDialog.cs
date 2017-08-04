using AdaptiveCards;
using LUISPaymentBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUISPaymentBot.Queries;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;
using LUISPaymentBot.Enums;
using LUISPaymentBot.Constants;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using System.Xml;

namespace LUISPaymentBot.Dialogs
{
    [LuisModel("4d67f828-282c-4681-a408-1a01eb542e49", "d66d1dec2e2e48668a60c62775661a04", LuisApiVersion.V2)]
    [Serializable]
    public class AccountDialog : LuisDialog<object>
    {
        private AuthenticateResponse loginInformation;
        public AccountDialog(AuthenticateResponse loginInformation)
        {
            this.loginInformation = loginInformation;
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            if (result.Query.Equals("Stop", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Halt", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Quit", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("End", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Break", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Bye", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Thank you", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Thank u", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Thanks", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Close", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Terminate", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Finish", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Cancel", StringComparison.CurrentCultureIgnoreCase)
                || result.Query.Equals("Exit", StringComparison.CurrentCultureIgnoreCase))
            {
                await context.PostAsync(MessageConstants.ThankYou);
                context.Done<object>(null);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Wait(MessageReceived);
            }
        }

        #region GetDueDate

        [LuisIntent("GetDueDate")]
        public async Task GetDueDate(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            //await context.PostAsync(MessageConstants.PleaseWait);

            EntityRecommendation entityRecommendation;
            AccountQuery accountQuery = new AccountQuery();
            if (result.TryFindEntity("Mbrsep", out entityRecommendation))
            {
                entityRecommendation.Type = "Mbrsep";
                entityRecommendation.Entity = accountQuery.Mbrsep;
            }
            else
            {
                accountQuery.Mbrsep = this.loginInformation.MbrSep;
                accountQuery.Location = this.loginInformation.Location;
            }

            var formDailog = new FormDialog<AccountQuery>(accountQuery, null, FormOptions.PromptInStart, result.Entities);

            context.Call(formDailog, this.ResumeAfterDueAmountFormDialog);

        }

        private IForm<AccountQuery> BuildDueAmountForm()
        {
            OnCompletionAsyncDelegate<AccountQuery> processSearch = async (context, state) =>
            {
                var message = "Please wait getting data...";
                if (!string.IsNullOrEmpty(state.Mbrsep))
                {
                    message += $" for {state.Mbrsep}...";
                }
                await context.PostAsync(message);
            };

            return new FormBuilder<AccountQuery>()
                .Field(nameof(AccountQuery.Mbrsep), (state) => string.IsNullOrEmpty(state.Mbrsep))
                .OnCompletion(processSearch)
                .Build();
        }

        private async Task<AccountInfo> GetDueDateAsync(AccountQuery searchQuery)
        {
            HttpClient client = GetClient();
            HttpResponseMessage response = await client.GetAsync(ConfigurationManager.AppSettings["GetAccountDue"].ToString() + searchQuery.Mbrsep);
            AccountInfo account = null;
            if (response.IsSuccessStatusCode)
            {
                object result = await response.Content.ReadAsAsync<object>();
                account = JsonConvert.DeserializeObject<AccountInfo>(result.ToString());
            }
            if (account == null)
            {
                account = new AccountInfo();
                account.Error = "Sorry...,details not found our database.";
            }
            account.Mbrsep = searchQuery.Mbrsep;


            return account;
        }

        private async Task ResumeAfterDueAmountFormDialog(IDialogContext context, IAwaitable<AccountQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var accountInfo = await this.GetDueDateAsync(searchQuery);

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                if (accountInfo != null && string.IsNullOrEmpty(accountInfo.Error))
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = accountInfo.Mbrsep,
                        Text = $"Due Amount: {accountInfo.AmountDue}, Due Date: {accountInfo.DueDate}"
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
                else if (!string.IsNullOrEmpty(accountInfo.Error))
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = accountInfo.Mbrsep,
                        Subtitle = $"Information of {accountInfo.Mbrsep }.",
                        Text = $"Details: {accountInfo.Error}"
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
                //context.Done<object>(null);
            }
        }

        #endregion GetDueDate

        #region GetLastPaid

        [LuisIntent("GetLastPaidBill")]
        public async Task GetLastPaidBill(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            //await context.PostAsync($"Please wait we are retreiving the information");

            EntityRecommendation entityRecommendation;
            AccountQuery accountQuery = new AccountQuery();
            if (result.TryFindEntity("Mbrsep", out entityRecommendation))
            {
                entityRecommendation.Type = "Mbrsep";
                entityRecommendation.Entity = accountQuery.Mbrsep;
            }
            else
            {
                accountQuery.Mbrsep = this.loginInformation.MbrSep;
                accountQuery.Location = this.loginInformation.Location;
            }

            var formDailog = new FormDialog<AccountQuery>(accountQuery, null, FormOptions.PromptInStart, result.Entities);

            context.Call(formDailog, this.ResumeAfterLastPaidBillFormDialog);

        }

        private IForm<AccountQuery> BuildLastPaidBillForm()
        {
            OnCompletionAsyncDelegate<AccountQuery> processSearch = async (context, state) =>
            {
                var message = "Please wait getting data...";
                if (!string.IsNullOrEmpty(state.Mbrsep))
                {
                    message += $" for {state.Mbrsep}...";
                }
                await context.PostAsync(message);
            };

            return new FormBuilder<AccountQuery>()
                .Field(nameof(AccountQuery.Mbrsep), (state) => string.IsNullOrEmpty(state.Mbrsep))
                .OnCompletion(processSearch)
                .Build();
        }

        private async Task<AccountInfo> GetLastPaidBillAsync(AccountQuery searchQuery)
        {
            HttpClient client = GetClient();
            HttpResponseMessage response = await client.GetAsync(ConfigurationManager.AppSettings["GetLastPaidBill"].ToString() + searchQuery.Mbrsep);
            AccountInfo account = null;
            if (response.IsSuccessStatusCode)
            {
                object result = await response.Content.ReadAsAsync<object>();
                account = JsonConvert.DeserializeObject<AccountInfo>(result.ToString());
            }
            if (account == null)
            {
                account = new AccountInfo();
                account.Error = "Sorry...,details not found our database.";
            }
            account.Mbrsep = searchQuery.Mbrsep;


            return account;
        }

        private async Task ResumeAfterLastPaidBillFormDialog(IDialogContext context, IAwaitable<AccountQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var accountInfo = await this.GetLastPaidBillAsync(searchQuery);

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                if (accountInfo != null && string.IsNullOrEmpty(accountInfo.Error))
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = accountInfo.Mbrsep,
                        Text = $"Last Paid Amount: {accountInfo.LastPayment}, Last Paid Date: {accountInfo.PaidDate}"
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }
                else if (!string.IsNullOrEmpty(accountInfo.Error))
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = accountInfo.Mbrsep,
                        Subtitle = $"Information of {accountInfo.Mbrsep }.",
                        Text = $"Details: {accountInfo.Error}"
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
                //context.Done<object>(null);
            }
        }

        #endregion GetLastPaid

        #region MakePayment

        [LuisIntent("MakePayment")]
        public async Task MakePayment(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            decimal amountDue = 0;
            PaymentQuery paymentQuery = new PaymentQuery();

            if (!this.loginInformation.Echeck && !this.loginInformation.CreditCard)
            {
                await context.PostAsync(MessageConstants.NoProfileExist);
                context.Wait(MessageReceived);
            }
            else if (decimal.TryParse(this.loginInformation.AmountDue, out amountDue) && amountDue <= 0)
            {
                await context.PostAsync(MessageConstants.DueAmount);
                context.Wait(MessageReceived);
            }
            else
            {
                paymentQuery.Mbrsep = this.loginInformation.MbrSep;
                paymentQuery.BillAmount = this.loginInformation.AmountDue;
                paymentQuery.ProfileType = (this.loginInformation.Echeck && this.loginInformation.CreditCard) ? paymentQuery.ProfileType : this.loginInformation.Echeck ? ProfileType.ECheck : ProfileType.CreditCard;
                //await context.PostAsync(MessageConstants.PleaseWait);
                await context.PostAsync(string.Format(MessageConstants.BillAmount, this.loginInformation.AmountDue, this.loginInformation.DueDate));

                EntityRecommendation entityRecommendation;

                if (result.TryFindEntity("Mbrsep", out entityRecommendation))
                {
                    entityRecommendation.Type = "Mbrsep";
                    entityRecommendation.Entity = paymentQuery.Mbrsep;
                }
                if (result.TryFindEntity("BillAmount", out entityRecommendation))
                {
                    entityRecommendation.Type = "BillAmount";
                    entityRecommendation.Entity = paymentQuery.BillAmount;
                }
                if (result.TryFindEntity("ProfileType", out entityRecommendation))
                {
                    entityRecommendation.Type = "ProfileType";
                    entityRecommendation.Entity = paymentQuery.ProfileType.ToString();
                }

                var formDailog = new FormDialog<PaymentQuery>(paymentQuery, this.PaymentForm, FormOptions.PromptInStart, result.Entities);

                context.Call(formDailog, this.ResumeAfterPaymentFormDialog);
            }
        }

        private IForm<PaymentQuery> PaymentForm()
        {
            return new FormBuilder<PaymentQuery>()
                .AddRemainingFields()
                .Field(nameof(PaymentQuery.Mbrsep), (state) => string.IsNullOrEmpty(state.Mbrsep))
                .Field(nameof(PaymentQuery.ProfileType))
                .Field(new FieldReflector<PaymentQuery>("FullPayment")
                .SetType(typeof(bool))
                .SetValidate(FullPaymentValidate))
                .Field(nameof(PaymentQuery.BillAmount), (state) => string.IsNullOrEmpty(state.BillAmount))
                .Build();
        }

        private Task<ValidateResult> billAmountValidate(PaymentQuery state, object value)
        {
            ValidateResult result = new ValidateResult() { IsValid = true };
            state.BillAmount = this.loginInformation.AmountDue;
            return Task.FromResult(result);
        }

        private Task<ValidateResult> FullPaymentValidate(PaymentQuery state, object value)
        {
            bool result = (bool)value;
            state.BillAmount = result ? this.loginInformation.AmountDue : string.Empty;
            return Task.FromResult(new ValidateResult() { IsValid = true });
        }

        private async Task<PaymentResponse> DoPaymentAsync(PaymentQuery paymentQuery)
        {
            Payment payment = new Payment()
            {
                Mbrsep = paymentQuery.Mbrsep.Replace("-", string.Empty),
                PaymentType = paymentQuery.ProfileType.Equals(ProfileType.ECheck) ? "EC" : "CC",
                Amount = paymentQuery.BillAmount
            };

            HttpClient client = GetClient();
            HttpResponseMessage response = await client.PostAsJsonAsync(ConfigurationManager.AppSettings["PostPayment"].ToString(), payment);
            PaymentResponse paymentResponse = null;
            if (response.IsSuccessStatusCode)
            {
                object result = await response.Content.ReadAsAsync<object>();
                paymentResponse = JsonConvert.DeserializeObject<PaymentResponse>(result == null ? string.Empty : result.ToString());

            }
            return paymentResponse;
        }

        private async Task ResumeAfterPaymentFormDialog(IDialogContext context, IAwaitable<PaymentQuery> result)
        {
            try
            {
                var paymentQuery = await result;

                var paymentResponse = await this.DoPaymentAsync(paymentQuery);

                if (paymentResponse != null)
                {
                    if (!string.IsNullOrEmpty(paymentResponse.ErrorStringField))
                    {
                        string response = "No {0} profile available, Please create profile before making payment.";
                        if (paymentResponse.ErrorStringField.Equals("No E-Check profile available", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await context.PostAsync(string.Format(response, "E-Check"));
                        }
                        if (paymentResponse.ErrorStringField.Equals("Use profile selected, but no CC profile available.", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await context.PostAsync(string.Format(response, "Credit Card"));
                        }
                    }
                    if (paymentResponse.DescriptionField.Equals("Declined", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.PostAsync(MessageConstants.PaymentDeclined);
                    }
                    if (!string.IsNullOrEmpty(paymentResponse.AuthorizationCodeField) &&
                        paymentResponse.DescriptionField.Equals("Approved", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.PostAsync(string.Format(MessageConstants.PaymentSuccess, paymentResponse.AuthorizationCodeField));
                    }
                }
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
                //context.Done<object>(null);
            }
        }

        #endregion

        #region Outage

        [LuisIntent("Outage")]
        public async Task DoOutage(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            //await context.PostAsync(MessageConstants.PleaseWait);

            EntityRecommendation entityRecommendation;
            AccountQuery accountQuery = new AccountQuery();
            if (result.TryFindEntity("Mbrsep", out entityRecommendation))
            {
                entityRecommendation.Type = "Mbrsep";
                entityRecommendation.Entity = accountQuery.Mbrsep;
            }
            else
            {
                accountQuery.Mbrsep = this.loginInformation.MbrSep;
            }
            if (result.TryFindEntity("Location", out entityRecommendation))
            {
                entityRecommendation.Type = "Location";
                entityRecommendation.Entity = accountQuery.Location;
            }
            else
            {
                accountQuery.Location = this.loginInformation.Location;
            }

            var formDailog = new FormDialog<AccountQuery>(accountQuery, null, FormOptions.PromptInStart, result.Entities);

            context.Call(formDailog, this.ResumeAfterOutageFormDialog);
        }

        private async Task ResumeAfterOutageFormDialog(IDialogContext context, IAwaitable<AccountQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var outageInfo = await this.DoOutageAsync(searchQuery);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(outageInfo);
                string response = doc.SelectSingleNode("//edit") != null ? doc.SelectSingleNode("//edit").InnerText : doc.SelectSingleNode("//output") != null
                    ? MessageConstants.OutageSuccess : string.Empty;
                response = response.Equals("Outage already exists.", StringComparison.CurrentCultureIgnoreCase) ? MessageConstants.OutageAlreadyExist : response;

                await context.PostAsync(response);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "You have canceled the operation.";
                }
                else
                {
                    reply = $"Oops! Something went wrong   Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Wait(MessageReceived);
                //context.Done<object>(null);
            }
        }

        private async Task<string> DoOutageAsync(AccountQuery outageQuery)
        {
            HttpClient client = GetClient();
            HttpResponseMessage response = await client.GetAsync(ConfigurationManager.AppSettings["Outage"].ToString() +
                string.Format(MessageConstants.OutageQurey, outageQuery.Mbrsep, outageQuery.Location));
            object result = null;
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<object>();
            }
            return result.ToString();
        }


        #endregion

        #region Common
        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"].ToString());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
        #endregion
    }
}