using LUISPaymentBot.Constants;
using LUISPaymentBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace LUISPaymentBot.Dialogs
{
    [Serializable]
    public class LoginDialog : IDialog<object>
    {
        private int occurance = 0;
        public LoginDialog(int occurance)
        {
            this.occurance = occurance;
        }
        private string UserName { get; set; }
        private string Password { get; set; }
        public async Task StartAsync(IDialogContext context)
        {
            //await context.PostAsync("Please authenticate yourself.");

            if (occurance == 0)
            {
                await context.PostAsync("Please enter your account number");
            }
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (string.IsNullOrEmpty(message.Text))
            {
                await context.PostAsync("Please enter your account number");
                context.Wait(this.MessageReceivedAsync);
            }
            else
            {
                this.UserName = message.Text.Replace("-", string.Empty);

                await context.PostAsync(MessageConstants.PleaseWait);
                HttpClient client = GetClient();
                AuthenticateInput userInfo = new AuthenticateInput() { mbrsep = this.UserName, tokenpwd = "1" };
                HttpResponseMessage response = await client.PostAsJsonAsync<AuthenticateInput>(ConfigurationManager.AppSettings["ValidateLogin"], userInfo);
                if (response.IsSuccessStatusCode)
                {
                    #region Commented to hold state with in converstaion
                    //************************************************To maintain data with in conversation************************************************
                    //StateClient stateClient = context.Activity.GetStateClient();
                    //BotState botState = new BotState(stateClient);
                    //BotData botData = await botState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                    //botData.SetProperty<LoginInformation>("accessInformation", new LoginInformation() { UserName = this.UserName, Name = string.Empty });
                    //LoginInformation loginInfo = botData.GetProperty<LoginInformation>("accessInformation"); 
                    #endregion

                    //************************************************To maintain data with all conversation************************************************

                    object serviceResult = response.Content.ReadAsAsync<object>().Result;
                    AuthenticateResponse authResponse = JsonConvert.DeserializeObject<AuthenticateResponse>(serviceResult.ToString());
                    authResponse.MbrSep = this.UserName;
                    context.UserData.SetValue((context.Activity.Id + context.Activity.From.Id), authResponse);
                    context.Done(true);
                }
                else
                {
                    string errorMessage = response.Content.ReadAsStringAsync().Result;
                    await context.PostAsync(MessageConstants.LoginError);
                    context.Fail(new Exception(errorMessage));
                }

                //await context.PostAsync("Enter your password");
                //context.Wait(this.PasswordReceivedAsync);
            }
        }

        private async Task PasswordReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            if (string.IsNullOrEmpty(message.Text))
            {
                await context.PostAsync("I'm sorry, I don't understand your reply. Enter your password?");
                context.Wait(this.PasswordReceivedAsync);
            }
            else
            {
                await context.PostAsync("Please wait we are retreiving the information.");
                this.Password = message.Text;
                HttpClient client = GetClient();
                AuthenticateInput userInfo = new AuthenticateInput() { mbrsep = this.UserName, tokenpwd = this.Password };
                HttpResponseMessage response = await client.PostAsJsonAsync<AuthenticateInput>(ConfigurationManager.AppSettings["ValidateLogin"], userInfo);
                if (response.IsSuccessStatusCode)
                {
                    #region Commented to hold state with in converstaion
                    //************************************************To maintain data with in conversation************************************************
                    //StateClient stateClient = context.Activity.GetStateClient();
                    //BotState botState = new BotState(stateClient);
                    //BotData botData = await botState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
                    //botData.SetProperty<LoginInformation>("accessInformation", new LoginInformation() { UserName = this.UserName, Name = string.Empty });
                    //LoginInformation loginInfo = botData.GetProperty<LoginInformation>("accessInformation"); 
                    #endregion

                    //************************************************To maintain data with all conversation************************************************

                    string serviceResult = response.Content.ReadAsStringAsync().Result;
                    context.UserData.SetValue((context.Activity.Id + context.Activity.From.Id), new AuthenticateResponse() { MbrSep = this.UserName, Name = serviceResult.Trim() });
                    context.Done(true);
                }
                else
                {
                    string errorMessage = response.Content.ReadAsStringAsync().Result;
                    await context.PostAsync(MessageConstants.LoginError);
                    context.Fail(new Exception(errorMessage));
                }
            }
        }

        private HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["BaseAddress"].ToString());
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}