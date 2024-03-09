using GameLogsMVC.Models.DBData;

namespace GameLogsMVC.Models.ViewData
{
    public class FollowerView
    {
        public string User { get; set; }
        public List<UserFollow> userFollows { get; set; }
        public List<int> followGames { get; set; }

        public FollowerView() 
        { 
            userFollows = new List<UserFollow>();
            followGames = new List<int>();
        }   
    }
}
