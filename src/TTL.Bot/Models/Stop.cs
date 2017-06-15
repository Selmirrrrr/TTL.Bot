namespace TTL.Bot.Models
{
    using System;

    public class Stop
    {
        public DateTime Departure { get; set; }
        public int? Delay { get; set; }
        public int? Platform { get; set; }
    }
}