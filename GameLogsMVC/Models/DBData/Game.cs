using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class Game
    {
        [Key] public string ID { get; set; }
        public string Home { get; set; }
        public string Away { get; set; }
        public string Date { get; set; }
        public string Score { get; set; }
        public string? League { get; set; }
        public string Location { get; set; }
        public string Duration { get; set; }
        public string ImpactPlay { get; set; }
        public string GameNote { get; set; }
        public string NeutralSite { get; set; }
        public string Season { get; set; }
        public string HomeTeamID { get; set; }
        public string AwayTeamID {get; set; }
    }
}
