namespace TTL.Bot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Models;
    using Newtonsoft.Json;
    using Reponses;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity == null) return;
            if (activity.Text.Contains("/start"))
            {
                await context.PostAsync($"Welcome ! Enter a station name to get next 10 departures board.");
                return;
            }
            await StationboardReponse(context, result);
        }

        private async Task StationboardReponse(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            List<Station> stations;

            var query = activity?.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(query)) return;

            var request = (HttpWebRequest) WebRequest.Create($"http://transport.opendata.ch/v1/locations?query={activity?.Text ?? string.Empty}");

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream())) stations = JsonConvert.DeserializeObject<LocationsResponse>(reader.ReadToEnd()).Stations;
            }

            switch (stations.Count)
            {
                case 0:
                    await context.PostAsync($"No stations found with your query : \" {activity?.Text}\"");
                    break;
                case 1:
                    await context.PostAsync(await GetNextRelations(activity?.Text ?? string.Empty));
                    break;
                default:
                    PromptDialog.Choice(context, OnOptionSelected, stations.Select(s => s.Name), "Please select the station", "Error, please try again.");
                    break;
            }
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                await context.PostAsync(await GetNextRelations(await result));
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
                context.Wait(MessageReceivedAsync);
            }

        }

        private async Task<string> GetNextRelations(string station)
        {
            var responseBuilder = new StringBuilder();

            var request = (HttpWebRequest)WebRequest.Create($"http://transport.opendata.ch/v1/stationboard?station={ station }&limit=10");

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var t = JsonConvert.DeserializeObject<StationboardReponse>(reader.ReadToEnd());
                    foreach (var s in t.Stationboards)
                    {
                        var diff = (int)Math.Abs((s.Stop.Departure - DateTime.Now).TotalMinutes);
                        var minutes = diff >= 0 ? diff : 0;
                        responseBuilder.AppendLine($"Departure in { minutes } minutes at { s.Stop.Departure.ToShortTimeString() } - " +
                                                   (s.Stop.Delay > 0 ? $"Delay : { s.Stop.Delay } - " : "") +
                                                   $"{ s.Name } - " +
                                                   $"{ s.To }" +
                                                   (s.Stop.Platform.HasValue ? $" - Platform : { s.Stop.Platform }" : "") +
                                                   $"{ Environment.NewLine }"
                                                   );
                    }
                }
            }
            return responseBuilder.ToString();
        }
    }
}