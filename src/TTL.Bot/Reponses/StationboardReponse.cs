namespace TTL.Bot.Reponses
{
    using System;
    using System.Collections.Generic;
    using Models;
    using Newtonsoft.Json;

    [Serializable]
    public class StationboardReponse
    {
        [JsonProperty(PropertyName = "Stationboard")]
        public List<Stationboard> Stationboards { get; set; }

    }
}