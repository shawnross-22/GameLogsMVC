using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class UserGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string UserID { get; set; }
        public string GameID { get; set; }
        public string League { get; set; }
        public string Attended { get; set; }
    }
}
