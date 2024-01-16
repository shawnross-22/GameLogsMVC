using GameLogsMVC.Models.PullData;

namespace GameLogsMVC.Models.ViewData
{
    public class TeamsView
    {
        public TeamsView()
        {
            teams = new List<Team>();
        }

        public List<PullData.Team> teams { get; set; }
    }
}
