using GameLogsMVC.Models.PullData;

namespace GameLogsMVC.Models.ViewData
{
    public class TeamsView
    {
        public TeamsView()
        {
            teams = new List<DBData.Team>();
        }

        public List<DBData.Team> teams { get; set; }
    }
}
