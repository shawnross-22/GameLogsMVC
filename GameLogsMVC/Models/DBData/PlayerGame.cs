using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class PlayerGame
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string PlayerID { get; set; }
        public string GameID { get; set; }
        public string? Stats { get; set; }
        public string Position { get; set; }
        public string League { get; set; }
        public string Team { get; set; }
    }
}
