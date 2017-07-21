using AdaptiveCards;
using MeetingRoomManagerLUIS.Models;
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

namespace MeetingRoomManagerLUIS.Dialogs
{
    [LuisModel("7d838588-89ab-4cb4-92f9-e8863918bc95", "d1b7382449a8439cb46e7a60ff846e6b", LuisApiVersion.V2)]
    [Serializable]
    public class CreateSchedulerDialog : LuisDialog<ScheduleInformation>
    {
        private readonly BuildFormDelegate<ScheduleInformation> MakeScheduleForm;
        private readonly BuildFormDelegate<CancelSchedule> CancelScheduleForm;
        internal CreateSchedulerDialog(BuildFormDelegate<ScheduleInformation> makeScheduleForm)
        {
            this.MakeScheduleForm = makeScheduleForm;
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Booking Room")]
        public async Task ProcessSchedulerForm(IDialogContext context, LuisResult result)
        {
            if (result.Entities != null && result.Entities.Count > 0)
            {
                List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
                ScheduleInformation scheduleInfo = new ScheduleInformation();
                long duration = 0;
                result.Entities.ToList().ForEach(c =>
                {
                    switch (c.Type)
                    {
                        case "Location":
                            scheduleInfo.Location = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Location });
                            break;
                        case "Subject":
                            scheduleInfo.Subject = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Subject });
                            break;
                        case "builtin.datetimeV2.datetime":
                        case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                scheduleInfo.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Start.ToString() });
                                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = duration.ToString() });
                            }
                            break;
                        default:
                            break;
                    }
                });

                if (duration > 0)
                {
                    scheduleInfo.Start = TimeZone.CurrentTimeZone.ToLocalTime(scheduleInfo.Start.HasValue ? DateTime.Now : scheduleInfo.Start.Value);
                    AddOrUpdateStartEntity(entityRecommendation, scheduleInfo);
                    scheduleInfo.End = TimeZone.CurrentTimeZone.ToLocalTime(duration > 0 ? scheduleInfo.Start.Value.AddSeconds(duration) : scheduleInfo.End.Value);
                    entityRecommendation.Add(new EntityRecommendation() { Type = "End", Entity = scheduleInfo.End.ToString() });
                }


                var createscheduleform = new FormDialog<ScheduleInformation>(scheduleInfo, this.MakeScheduleForm, FormOptions.PromptInStart, entityRecommendation);
                context.Call<ScheduleInformation>(createscheduleform, CompleteCreateSchedule);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }
        }

        [LuisIntent("Cancel Room")]
        public async Task CancelSchedulerForm(IDialogContext context, LuisResult result)
        {
            if (result.Entities != null && result.Entities.Count > 0)
            {
                List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
                CancelSchedule info = new CancelSchedule();
                long duration = 0;
                result.Entities.ToList().ForEach(c =>
                {
                    switch (c.Type)
                    {
                        case "Location":
                            info.Location = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Location });
                            break;
                        case "builtin.datetimeV2.datetime":
                        case "builtin.datetimeV2.date":
                            //Start
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                info.Start = (DateTime)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = scheduleInfo.Start.ToString() });
                                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
                            }
                            break;
                        case "builtin.datetimeV2.duration":
                            //End
                            if (((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value") != null)
                            {
                                duration = (long)((Newtonsoft.Json.Linq.JArray)c.Resolution.Values.FirstOrDefault()).FirstOrDefault().SelectToken("value");
                                entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = duration.ToString() });
                            }
                            break;
                        case "AppointmentName":
                            info.AppointmentName = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = info.AppointmentName });
                            break;
                        default:
                            break;
                    }
                });

                if (duration > 0)
                {
                    info.Start = TimeZone.CurrentTimeZone.ToLocalTime(info.Start.HasValue ? DateTime.Now : info.Start.Value);
                    AddOrUpdateStartEntity(entityRecommendation, info);
                    info.End = TimeZone.CurrentTimeZone.ToLocalTime(duration > 0 ? info.Start.Value.AddSeconds(duration) : info.End.Value);
                    entityRecommendation.Add(new EntityRecommendation() { Type = "End", Entity = info.End.ToString() });
                }

                var cancelAppointment = new FormDialog<CancelSchedule>(info, this.CancelScheduleForm, FormOptions.PromptInStart, entityRecommendation);
                context.Call<CancelSchedule>(cancelAppointment, CompleteCancelAppointment);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }
        }

        private static void AddOrUpdateStartEntity(List<EntityRecommendation> entityRecommendation, ScheduleInformation scheduleInfo)
        {
            if (!entityRecommendation.Any(c => c.Type.Equals("Start")))
            {
                entityRecommendation.Add(new EntityRecommendation() { Type = "Start", Entity = scheduleInfo.Start.ToString() });
            }
            else
            {
                EntityRecommendation startEntity = entityRecommendation.Find(c => c.Type.Equals("Start"));
                startEntity.Entity = scheduleInfo.Start.ToString();
            }
        }

        private async Task CompleteCreateSchedule(IDialogContext context, IAwaitable<ScheduleInformation> result)
        {
            ScheduleInformation scheduleInfo = null;
            try
            {
                scheduleInfo = await result;
            }
            catch (OperationCanceledException ex)
            {
                await context.PostAsync(ex.Message);
                await context.PostAsync("Form cancelled.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }

            if (scheduleInfo != null)
            {
                #region Adaptive Card to get rich output
                IMessageActivity message = context.MakeMessage();
                message.Attachments = new List<Attachment>();

                #region Skype wont support Adaptive Cards - For Ref: https://github.com/Microsoft/BotBuilder/issues/2803

                //AdaptiveCard card = new AdaptiveCard();
                //card.Body.Add(new TextBlock()
                //{
                //    Text = "Thanks for submitting...",
                //    Wrap = true,
                //    Size = TextSize.ExtraLarge,
                //    Weight = TextWeight.Bolder
                //});

                //card.Body.Add(new TextBlock()
                //{
                //    Text = "Below are the details are under process...",
                //    Wrap = true,
                //    Size = TextSize.Large,
                //    Weight = TextWeight.Bolder
                //});

                //card.Body.Add(new TextBlock() { Text = $"Employee Id: {scheduleInfo.EmployeeId}", Weight = TextWeight.Normal });
                //card.Body.Add(new TextBlock() { Text = $"Subject: {scheduleInfo.Subject}", Weight = TextWeight.Normal });
                //card.Body.Add(new TextBlock() { Text = $"Location: {scheduleInfo.Location}", Weight = TextWeight.Normal });
                //card.Body.Add(new TextBlock() { Text = $"Start: {scheduleInfo.Start.Value.ToString("dd-MMM-yyyy hh:mm tt")}", Weight = TextWeight.Normal });
                //card.Body.Add(new TextBlock() { Text = $"End: {scheduleInfo.End.Value.ToString("dd-MMM-yyyy hh:mm tt")}", Weight = TextWeight.Normal }); 
                #endregion

                string cardOutput = $"Employee: {scheduleInfo.EmployeeId}{Environment.NewLine}" +
                    $"Subject: {scheduleInfo.Subject}{Environment.NewLine}" +
                    $"Location: {scheduleInfo.Location}{Environment.NewLine}" +
                    $"Start: {scheduleInfo.Start.Value.ToString("dd-MMM-yyyy hh:mm tt")}{Environment.NewLine}" +
                    $"End: {scheduleInfo.End.Value.ToString("dd-MMM-yyyy hh:mm tt")}";

                HeroCard plCard = new HeroCard()
                {
                    Title = $"Thanks for submitting...",
                    Subtitle = $"Details are under process...",
                    Text = cardOutput
                };


                message.Attachments.Add(plCard.ToAttachment());
                #endregion
                await context.PostAsync(message);
            }
            //context.Wait(MessageReceived);
            context.Done<ScheduleInformation>(scheduleInfo);
        }

        private async Task CompleteCancelAppointment(IDialogContext context, IAwaitable<CancelSchedule> result)
        {
            CancelSchedule cancelInfo = null;
            try
            {
                cancelInfo = await result;
            }
            catch (OperationCanceledException ex)
            {
                await context.PostAsync(ex.Message);
                await context.PostAsync("Form cancelled.");
                context.Done<CancelSchedule>(new CancelSchedule());
            }

            if (cancelInfo != null)
            {
                #region Adaptive Card to get rich output
                IMessageActivity message = context.MakeMessage();
                message.Attachments = new List<Attachment>();

                string cardOutput = $"Appointment Name: {cancelInfo.AppointmentName}{Environment.NewLine}";

                HeroCard plCard = new HeroCard()
                {
                    Title = $"Thanks for submitting...",
                    Subtitle = $"Details are under process...",
                    Text = cardOutput
                };


                message.Attachments.Add(plCard.ToAttachment());
                #endregion
                await context.PostAsync(message);
            }
            //context.Wait(MessageReceived);
            context.Done<CancelSchedule>(cancelInfo);
        }

        [LuisIntent("Available Rooms")]
        public async Task SearchRooms(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Welcome to the rooms finder! We are analyzing your message: '{message.Text}'...");
            if (result.Entities != null && result.Entities.Count > 0)
            {
                List<EntityRecommendation> entityRecommendation = new List<EntityRecommendation>();
                AvailableRooms info = new AvailableRooms();
                result.Entities.ToList().ForEach(c =>
                {
                    switch (c.Type)
                    {
                        case "Area":
                            info.Area = c.Entity;
                            entityRecommendation.Add(new EntityRecommendation() { Type = c.Type, Entity = info.Area });
                            break;
                        default:
                            break;
                    }
                });
                var showRoomsFormDialog = new FormDialog<AvailableRooms>(info, this.BuildHotelsForm, FormOptions.PromptInStart, result.Entities);
                context.Call<AvailableRooms>(showRoomsFormDialog, ResumeAfterRoomsFormDialog);
            }
            else
            {
                await context.PostAsync("I'm sorry. I didn't understand you.");
                context.Done<ScheduleInformation>(new ScheduleInformation());
            }
            
        }

        private IForm<AvailableRooms> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<AvailableRooms> processHotelsSearch = async (context, state) =>
            {
                var message = "Searching for rooms";
                if (!string.IsNullOrEmpty(state.Area))
                {
                    message += $" in {state.Area}...";
                }
                await context.PostAsync(message);
            };

            return new FormBuilder<AvailableRooms>()
                .Field(nameof(AvailableRooms.Area), (state) => string.IsNullOrEmpty(state.Area))
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterRoomsFormDialog(IDialogContext context, IAwaitable<AvailableRooms> result)
        {
            try
            {
                var searchQuery = await result;

                var rooms = await this.GetRoomsAsync(searchQuery);

                await context.PostAsync($"I found {rooms.Count()} rooms:");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var room in rooms)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = room.Location,
                        Subtitle = room.Area,
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

        private async Task<IEnumerable<Room>> GetRoomsAsync(AvailableRooms searchQuery)
        {
            var rooms = new List<Room>();

            // Filling the rooms results manually just for demo purposes
            for (int i = 1; i <= 5; i++)
            {
                var random = new Random(i);
                Room hotel = new Room()
                {
                    Area = $"{searchQuery.Area ?? searchQuery.Area} Room {i}",
                    Location = $"Conference {i}"
                };

                rooms.Add(hotel);
            }
            
            return rooms;
        }
    }
}