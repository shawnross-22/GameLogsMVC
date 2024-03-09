using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GameLogsMVC.Models.DBData
{
    public class UserFollow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string UserID { get; set; }
        public string FollowingID { get; set; }
    }
}
