using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.GameData;

namespace GameLogsMVC.Models.ViewData
{
    public class UserView
    {
        public bool Change { get; set; }
        public string ID { get; set; }
        public int Count { get; set; }
        public int CountCurrent { get; set; }
        public List<Game> SameGames { get; set; }
        public Dictionary<string, DBData.Team> FavTeams { get; set; }
        

        public UserView()
        {
            FavTeams = new Dictionary<string, DBData.Team>();
        }

    }
}
