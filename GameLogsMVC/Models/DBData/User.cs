using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class User
    {
        [Key] public string ID { get; set; }
        public string Password { get; set; }
        public string FavMLB { get; set; }
        public string FavNBA { get; set; }
        public string FavNFL { get; set; }
        public string FavNCAAF { get; set; }
    }
}
