using GameLogsMVC.Models.DBData;

namespace GameLogsMVC.Models.ViewData
{
    public class UsersView
    {
        public List<DBData.User> users { get; set; }
        public List<string> follows { get; set; }
        public string? ID { get; set; }
        public UsersView() 
        { 
            users = new List<DBData.User>();
            follows = new List<string>();
        }
    }
}
