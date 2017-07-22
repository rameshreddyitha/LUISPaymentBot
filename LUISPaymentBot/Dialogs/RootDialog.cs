using LUISPaymentBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System.Threading;
using LUISPaymentBot.Constants;

namespace LUISPaymentBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(MessageConstants.WelcomeMsg);
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            //await context.PostAsync(MessageConstants.WelcomeMsg);
            context.Call(new LoginDialog(0), this.ResumeLoginSuccess);
            //PromptDialog.Choice(
            //   context,
            //   this.AfterChoiceSelected,
            //   new[] { MessageConstants.PaymentIntent, MessageConstants.LeaveIntent },
            //   MessageConstants.ServiceSelection,
            //   MessageConstants.IncorrectServiceSelection,
            //   attempts: 2);
        }

        private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                //var selection = await result;

                //switch (selection)
                //{
                //    case MessageConstants.LeaveIntent:
                //        await context.PostAsync(MessageConstants.NotImplemted);
                //        await this.StartAsync(context);
                //        break;

                //    case MessageConstants.PaymentIntent:
                //        await context.PostAsync(MessageConstants.WelcomeMsg);
                //        context.Call(new LoginDialog(0), this.ResumeLoginSuccess);
                //        break;
                //}
            }
            catch (TooManyAttemptsException)
            {
                await this.StartAsync(context);
            }
        }

        private async Task ResumeLoginSuccess(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                bool message = (bool)await result;

                AuthenticateResponse loginInfo = null;
                if (!context.UserData.TryGetValue((context.Activity.Id + context.Activity.From.Id), out loginInfo))
                {
                    context.Call(new LoginDialog(1), this.ResumeLoginSuccess);
                }
                else
                {
                    await context.PostAsync(string.Format(MessageConstants.Authenticated, loginInfo.Name));
                    await context.PostAsync(MessageConstants.ServicesInformation);
                    context.Forward(new AccountDialog(loginInfo), this.ResumeScheduler, context.Activity, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                context.Call(new LoginDialog(1), this.ResumeLoginSuccess);
            }
        }

        private async Task ResumeScheduler(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }
    }
}