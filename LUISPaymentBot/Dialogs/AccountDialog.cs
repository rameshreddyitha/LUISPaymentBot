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
using LUISPaymentBot.Models.Queries;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;

namespace LUISPaymentBot.Dialogs
{
    [LuisModel("4d67f828-282c-4681-a408-1a01eb542e49", "d66d1dec2e2e48668a60c62775661a04", LuisApiVersion.V2)]
    [Serializable]
    public class AccountDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        #region GetDueDate

        [LuisIntent("GetDueDate")]
        public async Task GetDueDate(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Please wait we are providing requested information.");         
            
            EntityRecommendation entityRecommendation;
            AccountQuery accountQuery = new AccountQuery();
            if (result.TryFindEntity("Mbrsep", out entityRecommendation))
            {
                entityRecommendation.Type = "Mbrsep";
                entityRecommendation.Entity = accountQuery.Mbrsep;
            }

            var formDailog = new FormDialog<AccountQuery>(accountQuery, this.BuildDueAmountForm, FormOptions.PromptInStart, result.Entities);

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
            if(account==null)
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
                        Subtitle = $"Due Amount details.",
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
                context.Done<object>(null);
            }
        }

        #endregion GetDueDate

        #region GetLastPaid

        [LuisIntent("GetLastPaidBill")]
        public async Task GetLastPaidBill(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Please wait we are providing requested information.");

            EntityRecommendation entityRecommendation;
            AccountQuery accountQuery = new AccountQuery();
            if (result.TryFindEntity("Mbrsep", out entityRecommendation))
            {
                entityRecommendation.Type = "Mbrsep";
                entityRecommendation.Entity = accountQuery.Mbrsep;
            }

            var formDailog = new FormDialog<AccountQuery>(accountQuery, this.BuildLastPaidBillForm, FormOptions.PromptInStart, result.Entities);

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
                        Subtitle = $"Last paid bill details.",
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
                context.Done<object>(null);
            }
        }

        #endregion GetLastPaid

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