using GameLogsMVC.Models;
using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.GameData;
using GameLogsMVC.Models.PullData;
using GameLogsMVC.Models.ViewData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace GameLogsMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext _dbContext;

        public HomeController(ILogger<HomeController> logger, ApplicationDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult MLB()
        {
            string teamsUrl = "https://site.api.espn.com/apis/site/v2/sports/baseball/mlb/teams";
            TeamsView mlbView = GetTeams(teamsUrl, "MLB");
            return View(mlbView);
        }

        public IActionResult NCAAF()
        {
            string teamsUrl = "https://site.api.espn.com/apis/site/v2/sports/football/college-football/teams?limit=1000";
            TeamsView ncaafView = GetTeams(teamsUrl, "NCAAF");           
            return View(ncaafView);
        }

        public IActionResult NFL()
        {
            string teamsUrl = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams";
            TeamsView nflView = GetTeams(teamsUrl, "NFL");
            return View(nflView);
        }

        public IActionResult NBA()
        {
            string teamsUrl = "http://site.api.espn.com/apis/site/v2/sports/basketball/nba/teams";
            TeamsView nbaView = GetTeams(teamsUrl, "NBA");
            return View(nbaView);
        }
        public async Task<IActionResult> User()
        {
            UserView userView = new UserView();
            userView.ID = Request.Cookies["userID"];
            userView.Count = await _dbContext.UserGame.Where(u => u.UserID == userView.ID).CountAsync();
            var countCurrent = await _dbContext.UserGame
            .Join(
            _dbContext.Game,
            userGame => userGame.GameID,  // Assuming UserGame has a foreign key column named GameID
            game => game.ID,              // Assuming Game has a primary key column named ID
            (userGame, game) => new { UserGame = userGame, Game = game }
            )
            .Where(joined => joined.UserGame.UserID == userView.ID && joined.Game.Date.Substring(0,4) == DateTime.Now.Year.ToString())
            .CountAsync();
            return View(userView);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult BoxScore(string id, string league)
        {
            BoxScoreView boxScoreView = new BoxScoreView();
            GameEvent gameEvent = new GameEvent(); 
            string url = "";
            switch (league)
            {
                case "MLB":
                    url = "https://site.api.espn.com/apis/site/v2/sports/baseball/mlb/summary?event=" + id;
                    break;
                case "NCAAF":
                    url = "https://site.api.espn.com/apis/site/v2/sports/football/college-football/summary?event=" + id;
                    break;
                case "NFL":
                    url = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/summary?event=" + id;
                    break;
                case "NBA":
                    url = "https://site.api.espn.com/apis/site/v2/sports/basketball/nba/summary?event=" + id;
                    break;
            }
            using (var client = new HttpClient())
            {
                string gameResults = client.GetStringAsync(url).Result;

                gameEvent = JsonConvert.DeserializeObject<GameEvent>(gameResults);
            }
            return View(gameEvent);
        }

        public IActionResult Schedule(string league, string team, string id, string dates)
        {
            FullScheduleView fullScheduleView = new FullScheduleView();
            ScheduleView regularSeasonSchedule = GetScheduleView(league, id, dates, team, "&seasontype=2");
            fullScheduleView.regularSchedule = regularSeasonSchedule;
            ScheduleView postSeasonSchedule = GetScheduleView(league, id, dates, team, "&seasontype=3");
            fullScheduleView.postSchedule = postSeasonSchedule;
            if(league == "NBA")
            {
                ScheduleView playinSchedule = GetScheduleView(league, id, dates, team, "&seasontype=5");
                fullScheduleView.playInSchedule = playinSchedule;
            }
            fullScheduleView.League = league;
            fullScheduleView.Team = team;
            fullScheduleView.Dates = dates;
            fullScheduleView.ID = id;
            return View(fullScheduleView);              
        }

        public ScheduleView GetScheduleView(string league, string id, string dates, string team, string seasontype)
        {
            ScheduleView scheduleView = new ScheduleView();
            string scheduleUrl = "";
            switch (league)
            {
                case "MLB":
                    scheduleUrl = "https://site.api.espn.com/apis/site/v2/sports/baseball/mlb/teams/" + id + "/schedule?season=" + dates;
                    break;
                case "NCAAF":
                    scheduleUrl = "https://site.api.espn.com/apis/site/v2/sports/football/college-football/teams/" + id + "/schedule?season=" + dates;
                    break;
                case "NFL":
                    scheduleUrl = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/teams/" + id + "/schedule?season=" + dates;
                    break;
                case "NBA":
                    scheduleUrl = "https://site.api.espn.com/apis/site/v2/sports/basketball/nba/teams/" + id + "/schedule?season=" + dates;
                    break;
            }
            scheduleUrl = scheduleUrl + seasontype;

            BoxScoreItem boxscores;
            using (var client = new HttpClient())
            {
                string scheduleResults = client.GetStringAsync(scheduleUrl).Result;

                boxscores = JsonConvert.DeserializeObject<BoxScoreItem>(scheduleResults);
            }

            foreach (var boxscore in boxscores.Events)
            {
                Result result = new Result();
                result.id = boxscore.ID;
                DateTime dateTime = DateTime.Parse(boxscore.Date, null, System.Globalization.DateTimeStyles.RoundtripKind);

                if (dateTime.Hour < 5)
                {
                    dateTime = dateTime.AddDays(-1);
                }
                result.date = dateTime.ToString("yyyy-MM-dd");


                if (result.date != null)
                {
                    foreach (var competition in boxscore.Competitions)
                    {
                        if (competition.Competitors[0].Team.DisplayName == team)
                        {
                            result.opponent = competition.Competitors[1].Team.DisplayName;
                            if (competition.Competitors[0].Score != null)
                            {
                                if (Convert.ToInt32(competition.Competitors[0].Score.DisplayValue) < Convert.ToInt32(competition.Competitors[1].Score.DisplayValue))
                                {
                                    result.score = "L " + competition.Competitors[0].Score.DisplayValue + "-" + competition.Competitors[1].Score.DisplayValue;
                                }
                                else
                                {
                                    result.score = "W " + competition.Competitors[0].Score.DisplayValue + "-" + competition.Competitors[1].Score.DisplayValue;
                                }
                            }
                        }
                        else
                        {
                            if (!competition.neutralSite)
                            {
                                result.opponent = "@ " + competition.Competitors[0].Team.DisplayName;
                            }
                            else
                            {
                                result.opponent = competition.Competitors[0].Team.DisplayName;
                            }
                            if (competition.Competitors[0].Score != null)
                            {
                                if (Convert.ToInt32(competition.Competitors[0].Score.DisplayValue) < Convert.ToInt32(competition.Competitors[1].Score.DisplayValue))
                                {
                                    result.score = "W " + competition.Competitors[1].Score.DisplayValue + "-" + competition.Competitors[0].Score.DisplayValue;
                                }
                                else
                                {
                                    result.score = "L " + competition.Competitors[1].Score.DisplayValue + "-" + competition.Competitors[0].Score.DisplayValue;
                                }
                            }
                        }
                    }
                    string userId = Request.Cookies["userID"];
                    List<string> userGames = _dbContext.UserGame.Where(UserGame => UserGame.UserID == userId).Select(UserGame => UserGame.GameID).ToList();
                    if (userGames.Contains(boxscore.ID))
                    {
                        result.attended = true;
                    }
                    else
                    {
                        result.attended = false;
                    }
                    scheduleView.Results.Add(result);
                }
            }
            return scheduleView;
        }

        public async Task<IActionResult> Login([FromBody] User userID)
        {
            string message = "";
            User user = null;
            try
            {
                user = await _dbContext.User.SingleOrDefaultAsync(u => u.ID == userID.ID);
                // Other logic...
            }
            catch (Exception ex)
            {
                // Log or print exception details
                Console.WriteLine(ex.Message);
            }

            // Check if the user exists
            if (user == null)
            {
                message = "ID or Password is incorrect. Please try again.";
            }
            else
            {
                if(user.Password.TrimEnd() != userID.Password)
                {
                    message = "ID or Password is incorrect. Please try again.";
                }
                else
                {
                    message = "Success.";
                    Response.Cookies.Append("userID", user.ID);
                }
            }
            

            return Ok(message);
        }

        public async Task<IActionResult> Log([FromBody] Game game)
        {
            string userId = Request.Cookies["userID"];
            if (await _dbContext.Game.AnyAsync(g => g.ID == game.ID))
            {
                if(!await _dbContext.UserGame.AnyAsync(g => g.GameID == game.ID && g.UserID == userId))
                {
                    UserGame userGame = new UserGame { GameID=game.ID, UserID=userId};
                    await _dbContext.UserGame.AddAsync(userGame);
                }
            }
            else
            {
                GameEvent gameEvent = new GameEvent();
                string url = "";
                switch (game.League)
                {
                    case "MLB":
                        url = "https://site.api.espn.com/apis/site/v2/sports/baseball/mlb/summary?event=" + game.ID;
                        break;
                    case "NCAAF":
                        url = "https://site.api.espn.com/apis/site/v2/sports/football/college-football/summary?event=" + game.ID;
                        break;
                    case "NFL":
                        url = "https://site.api.espn.com/apis/site/v2/sports/football/nfl/summary?event=" + game.ID;
                        break;
                    case "NBA":
                        url = "https://site.api.espn.com/apis/site/v2/sports/basketball/nba/summary?event=" + game.ID;
                        break;
                }
                using (var client = new HttpClient())
                {
                    string gameResults = client.GetStringAsync(url).Result;

                    gameEvent = JsonConvert.DeserializeObject<GameEvent>(gameResults);
                }
                game.Location = gameEvent.GameInfo.Venue.FullName + ", " + gameEvent.GameInfo.Venue.Address.City + ", " + gameEvent.GameInfo.Venue.Address.State;
                game.Home = gameEvent.Boxscore.Teams[1].Team.DisplayName;
                game.Away = gameEvent.Boxscore.Teams[0].Team.DisplayName;
                if (gameEvent.Header.Competitions[0].Competitors[0].HomeAway == "home")
                {
                    game.Score = gameEvent.Header.Competitions[0].Competitors[0].Score + "-" + gameEvent.Header.Competitions[0].Competitors[1].Score;
                }
                else
                {
                    game.Score = gameEvent.Header.Competitions[0].Competitors[1].Score + "-" + gameEvent.Header.Competitions[0].Competitors[0].Score;
                }
                await _dbContext.Game.AddAsync(game);
                UserGame userGame = new UserGame { GameID = game.ID, UserID = userId };
                await _dbContext.UserGame.AddAsync(userGame);
            }
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }

        public async Task<IActionResult> Unlog([FromBody] UserGame userGame)
        {
            string userId = Request.Cookies["userID"];
            userGame.UserID = userId;
            UserGame foundUserGame = _dbContext.UserGame.Where(ug => ug.GameID == userGame.GameID && ug.UserID == userId).FirstOrDefault();
            _dbContext.UserGame.Remove(foundUserGame);
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }

        public TeamsView GetTeams(string url, string view)
        {
            TeamsView teamsView = new TeamsView();
            string[] lines = new string[] {};
            if(view == "NCAAF")
            {
                string teamsFilePath = Path.Combine("Files", "NCAAFTeams.txt");
                lines = System.IO.File.ReadAllLines(teamsFilePath);
            }
            Root root;
            using (var client = new HttpClient())
            {
                string teamsResults = client.GetStringAsync(url).Result;

                root = JsonConvert.DeserializeObject<Root>(teamsResults);
            }

            // Access the list of sports and extract team names
            foreach (var sport in root.sports)
            {
                foreach (var league in sport.leagues)
                {
                    foreach (var team in league.teams)
                    {
                        if (view == "NCAAF")
                        {
                            if (lines.Contains(team.team.displayName))
                            {
                                teamsView.teams.Add(team);
                            }
                        }
                        else
                        {
                            teamsView.teams.Add(team);
                        }                       
                    }
                }
            }
            return teamsView;
        }
    }
}