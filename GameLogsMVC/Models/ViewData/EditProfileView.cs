using GameLogsMVC.Models.DBData;

namespace GameLogsMVC.Models.ViewData
{
    public class EditProfileView
    {
        public User User { get; set; }
        public List<DBData.Team> MLBTeams { get; set; }
        public List<DBData.Team> NBATeams { get; set; }
        public List<DBData.Team> NFLTeams { get; set; }
        public List<DBData.Team> NCAAFTeams { get; set; }
        public List<DBData.Team> NCAABTeams { get; set; }
        public List<DBData.Team> FavTeams { get; set; }

        public EditProfileView()
        {
            MLBTeams = new List<DBData.Team>();
            NBATeams = new List<DBData.Team>();
            NFLTeams = new List<DBData.Team>();
            NCAAFTeams = new List<DBData.Team>();
            NCAABTeams = new List<DBData.Team>();
            FavTeams = new List<DBData.Team>();
        }
    }
}
