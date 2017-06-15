namespace TTL.Bot.Models
{
    public class Station
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public Coordinate Coordinate { get; set; }
        public decimal? Distance { get; set; }
    }
}