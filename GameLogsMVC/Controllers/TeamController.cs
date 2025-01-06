using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.GameData;
using GameLogsMVC.Models.PullData;
using GameLogsMVC.Models.ViewData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GameLogsMVC.Controllers
{
    public class TeamController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext _dbContext;

        public TeamController(ILogger<HomeController> logger, ApplicationDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            
        }

        public IActionResult Index(string league, string teamID)
        {
            string userName = Request.Cookies["userID"];
            TeamView teamView = new TeamView();
            teamView.UserID = userName;
            teamView.League = league;
            string url = "";
            switch (league)
            {
                case "MLB":
                    url = "https://site.api.espn.com/apis/site/v2/sports/baseball/mlb/teams/" + teamID;
                    break;
                case "NCAAF":
                    url = "https://site.api.espn.com/apis/site/v2/sports/football/college-football/teams/" + teamID;
                    break;
                case "NFL":
                    url = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams/" + teamID;
                    break;
                case "NBA":
                    url = "https://site.api.espn.com/apis/site/v2/sports/basketball/nba/teams/" + teamID;
                    break;
                case "NCAAB":
                    url = "https://site.api.espn.com/apis/site/v2/sports/basketball/mens-college-basketball/teams/" + teamID;
                    break;
            }
            using (var client = new HttpClient())
            {
                string teamResults = client.GetStringAsync(url).Result;

                teamView.Team = JsonConvert.DeserializeObject<Models.PullData.Teams>(teamResults);
            }

            List<string> userGamesWatched = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userName)
            .Select(UserGame => UserGame.GameID)
            .ToList();

            List<string> userGamesAttended = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userName && UserGame.Attended == "A")
            .Select(UserGame => UserGame.GameID)
            .ToList();

            int winsWatched = 0;
            int winsAttended = 0;
            int lossesWatched = 0;
            int lossesAttended = 0;

            for (int i = 0; i < userGamesWatched.Count(); i++)
            {
                Game userGame = _dbContext.Game.Where(Game => Game.ID == userGamesWatched[i]).FirstOrDefault();
                if ((userGame.Home == teamView.Team.Team.DisplayName || userGame.Away == teamView.Team.Team.DisplayName) && userGame.League == league)
                {
                    teamView.GamesWatched.Add(userGame);
                    if (int.Parse(userGame.Score.Split("-")[0]) < int.Parse(userGame.Score.Split("-")[1]))
                    {
                        if (userGame.Home == teamView.Team.Team.DisplayName)
                        {
                            lossesWatched += 1;
                        }
                        else
                        {
                            winsWatched += 1;
                        }
                    }
                    else
                    {
                        if (userGame.Home == teamView.Team.Team.DisplayName)
                        {
                            winsWatched += 1;
                        }
                        else
                        {
                            lossesWatched += 1;
                        }
                    }
                }
                
            }

            for (int i = 0; i < userGamesAttended.Count(); i++)
            {
                Game userGame = _dbContext.Game.Where(Game => Game.ID == userGamesAttended[i]).FirstOrDefault();
                if ((userGame.Home == teamView.Team.Team.DisplayName || userGame.Away == teamView.Team.Team.DisplayName) && userGame.League == league)
                {
                    teamView.GamesAttended.Add(userGame);
                    if (Convert.ToInt32(userGame.Score.Split("-")[0]) < Convert.ToInt32(userGame.Score.Split("-")[1]))
                    {
                        if (userGame.Home == teamView.Team.Team.DisplayName)
                        {
                            lossesAttended += 1;
                        }
                        else
                        {
                            winsAttended += 1;
                        }
                    }
                    else
                    {
                        if (userGame.Home == teamView.Team.Team.DisplayName)
                        {
                            winsAttended += 1;
                        }
                        else
                        {
                            lossesAttended += 1;
                        }
                    }
                }
                
            }

            teamView.RecordWatched = winsWatched.ToString() + "-" + lossesWatched.ToString();
            teamView.RecordAttended = winsAttended.ToString() + "-" + lossesAttended.ToString();

            teamView.GamesWatched = teamView.GamesWatched.OrderByDescending(g => g.Date).ToList();
            teamView.GamesAttended = teamView.GamesAttended.OrderByDescending(g => g.Date).ToList();


            return View(teamView);
        }
    }
}
