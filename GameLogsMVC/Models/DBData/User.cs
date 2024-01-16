using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class User
    {
        [Key] public string ID { get; set; }
        public string Password { get; set; }
    }
}
