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
using System.Numerics;
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

        public async Task<IActionResult> Index()
        {

            return View();
        }

        public async Task<IActionResult> MLB()
        {
            TeamsView MLBView = new TeamsView();
            MLBView.teams.AddRange(_dbContext.Team.Where(t => t.League == "MLB").ToList());
            return View(MLBView);
        }

        public async Task<IActionResult> NCAAF()
        {
            TeamsView NCAAFView = new TeamsView();
            NCAAFView.teams.AddRange(_dbContext.Team.Where(t => t.League == "NCAAF").OrderBy(t=>t.Name).ToList());
            return View(NCAAFView);
        }

        public async Task<IActionResult> NFL()
        {
            TeamsView NFLView = new TeamsView();
            NFLView.teams.AddRange(_dbContext.Team.Where(t => t.League == "NFL").ToList());
            return View(NFLView);
        }

        public async Task<IActionResult> NBA()
        {
            TeamsView NBAView = new TeamsView();
            NBAView.teams.AddRange(_dbContext.Team.Where(t => t.League == "NBA").ToList());
            return View(NBAView);
        }

        public async Task<IActionResult> NCAAB()
        {
            TeamsView NCAABView = new TeamsView();
            NCAABView.teams.AddRange(_dbContext.Team.Where(t => t.League == "NCAAB").ToList());
            return View(NCAABView);
        }

        public async Task<IActionResult> Users()
        {
            UsersView usersView = new UsersView();
            usersView.users = _dbContext.User.ToList();
            usersView.ID = Request.Cookies["userID"];
            usersView.follows = _dbContext.UserFollow.Where(u => u.UserID == usersView.ID).Select(u => u.FollowingID).ToList();

            return View(usersView);
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
                case "NCAAB":
                    url = "https://site.api.espn.com/apis/site/v2/sports/basketball/mens-college-basketball/summary?event=" + id;
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
            team = team.Replace("amp;", "");
            ScheduleView regularSeasonSchedule = GetScheduleView(league, id.Replace(league,""), dates, team, "&seasontype=2");
            fullScheduleView.regularSchedule = regularSeasonSchedule;
            ScheduleView postSeasonSchedule = GetScheduleView(league, id.Replace(league, ""), dates, team, "&seasontype=3");
            fullScheduleView.postSchedule = postSeasonSchedule;
            if(league == "NBA")
            {
                ScheduleView playinSchedule = GetScheduleView(league, id.Replace(league, ""), dates, team, "&seasontype=5");
                fullScheduleView.playInSchedule = playinSchedule;
            }
            fullScheduleView.League = league;
            fullScheduleView.Team = team;
            fullScheduleView.Logo = _dbContext.Team.Where(t => t.Name == team).Select(t => t.Url).FirstOrDefault();
            fullScheduleView.Dates = dates;
            fullScheduleView.ID = id.Replace(league, "");
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
                case "NCAAB":
                    scheduleUrl = "https://site.api.espn.com/apis/site/v2/sports/basketball/mens-college-basketball/teams/" + id + "/schedule?season=" + dates;
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
                    if (userId == null)
                    {
                        result.attended = null;
                    }
                    else
                    {
                        if (userGames.Contains(boxscore.ID))
                        {
                            result.attended = true;
                        }
                        else
                        {
                            result.attended = false;
                        }
                    }
                    
                    scheduleView.Results.Add(result);
                }
            }
            return scheduleView;
        }

        public async Task<IActionResult> SignUp([FromBody] User userID)
        {
            if (_dbContext.User.Any(u => u.ID == userID.ID))
            {
                return Ok("ID already exists.");
            }
            else
            {
                await _dbContext.User.AddAsync(userID);
                await _dbContext.SaveChangesAsync();
                Response.Cookies.Append("userID", userID.ID);
                return Ok("Success.");
            }
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
                case "NCAAB":
                    url = "https://site.api.espn.com/apis/site/v2/sports/basketball/mens-college-basketball/summary?event=" + game.ID;
                    break;
            }
            using (var client = new HttpClient())
            {
                string gameResults = client.GetStringAsync(url).Result;

                try
                {
                    // Your code
                    gameEvent = JsonConvert.DeserializeObject<GameEvent>(gameResults);
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Error: {ex.Message}");
                }

                
            }

            if (await _dbContext.Game.AnyAsync(g => g.ID == game.ID))
            {
                if(!await _dbContext.UserGame.AnyAsync(g => g.GameID == game.ID && g.UserID == userId))
                {
                    UserGame userGame = new UserGame { GameID=game.ID, UserID=userId, League=game.League};
                    await _dbContext.UserGame.AddAsync(userGame);
                    try
                    {
                        // Your code
                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log or handle the exception
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            else
            {
                if (gameEvent.GameInfo.Venue != null)
                {
                    game.Location = gameEvent.GameInfo.Venue.FullName + ", " + gameEvent.GameInfo.Venue.Address.City + ", " + gameEvent.GameInfo.Venue.Address.State;
                }
                else
                {
                    game.Location = "";
                }
                game.Home = gameEvent.Boxscore.Teams[1].Team.DisplayName;
                game.Away = gameEvent.Boxscore.Teams[0].Team.DisplayName;
                game.NeutralSite = gameEvent.Header.Competitions[0].NeutralSite.ToString();
                if (!string.IsNullOrEmpty(gameEvent.Header.GameNote))
                {
                    game.GameNote = gameEvent.Header.GameNote;
                }
                else
                {
                    game.GameNote = "";
                }
                if(gameEvent.GameInfo.GameDuration != null)
                {
                    game.Duration = gameEvent.GameInfo.GameDuration;
                }
                else
                {
                    game.Duration = "";
                }
                Dictionary<string, double> plays = new Dictionary<string, double>();
                if (gameEvent.WinProbability != null)
                {
                    foreach (var wpPlay in gameEvent.WinProbability)
                    {
                        for (int i = 1; i < gameEvent.WinProbability.Count(); i++)
                        {
                            if (!plays.ContainsKey(gameEvent.WinProbability[i].PlayID))
                            {
                                plays.Add(gameEvent.WinProbability[i].PlayID, Math.Abs(gameEvent.WinProbability[i].HomeWinPercentage - gameEvent.WinProbability[i - 1].HomeWinPercentage));

                            }
                            else
                            {
                                if (Math.Abs(gameEvent.WinProbability[i].HomeWinPercentage - gameEvent.WinProbability[i - 1].HomeWinPercentage) > plays[gameEvent.WinProbability[i].PlayID])
                                {
                                    plays[gameEvent.WinProbability[i].PlayID] = Math.Abs(gameEvent.WinProbability[i].HomeWinPercentage - gameEvent.WinProbability[i - 1].HomeWinPercentage);
                                }
                            }
                        }

                    }
                    var sortedDictionary = plays.OrderByDescending(x => x.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                    var firstKeyValuePair = sortedDictionary.FirstOrDefault();
                    if (game.League != "NFL" && game.League != "NCAAF")
                    {
                        foreach (var play in gameEvent.Plays)
                        {
                            if (play.ID == firstKeyValuePair.Key)
                            {
                                if (game.League == "MLB")
                                {
                                    game.ImpactPlay = DateTime.Parse(gameEvent.Header.Competitions[0].Date).ToString("yyyy-MM-dd") + " " + gameEvent.Boxscore.Teams[0].Team.DisplayName + " @ " + gameEvent.Boxscore.Teams[1].Team.DisplayName + " " + play.AwayScore.ToString() + "-" + play.HomeScore.ToString() + play.Period.Type + " " + play.Period.Number.ToString() + " " + play.Outs + " Out(s), " + play.Text + ", " + string.Format("{0:P2}", firstKeyValuePair.Value);
                                }
                                else
                                {
                                    game.ImpactPlay = DateTime.Parse(gameEvent.Header.Competitions[0].Date).ToString("yyyy-MM-dd") + " " + gameEvent.Boxscore.Teams[0].Team.DisplayName + " @ " + gameEvent.Boxscore.Teams[1].Team.DisplayName + " " + play.AwayScore.ToString() + "-" + play.HomeScore.ToString() + ", Q" + play.Period.Number.ToString() + " " + play.Clock.DisplayValue + ", " + play.Text + ", " + string.Format("{0:P2}", firstKeyValuePair.Value);
                                }
                                
                            }
                        }
                    }
                    else
                    {
                        foreach (var drive in gameEvent.Drives.Previous)
                        {
                            foreach (var play in drive.Plays)
                            {
                                if (play.ID == firstKeyValuePair.Key)
                                {
                                    game.ImpactPlay = DateTime.Parse(gameEvent.Header.Competitions[0].Date).ToString("yyyy-MM-dd") + " " + gameEvent.Boxscore.Teams[0].Team.DisplayName + " @ " + gameEvent.Boxscore.Teams[1].Team.DisplayName + " " + play.AwayScore.ToString() + "-" + play.HomeScore.ToString() + ", Q" + play.Period.Number.ToString() + " " + play.Clock.DisplayValue + ", " + play.Text + ", " + string.Format("{0:P2}", firstKeyValuePair.Value);
                                }
                            }
                        }
                    }
                    
                }
                else
                {
                    game.ImpactPlay = "";
                }
                if (gameEvent.Header.Competitions[0].Competitors[0].HomeAway == "home")
                {
                    game.Score = gameEvent.Header.Competitions[0].Competitors[0].Score + "-" + gameEvent.Header.Competitions[0].Competitors[1].Score;
                }
                else
                {
                    game.Score = gameEvent.Header.Competitions[0].Competitors[1].Score + "-" + gameEvent.Header.Competitions[0].Competitors[0].Score;
                }
                await _dbContext.Game.AddAsync(game);
                try
                {
                    // Your code
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Error: {ex.Message}");
                }
                UserGame userGame = new UserGame { GameID = game.ID, UserID = userId, League = game.League };
                await _dbContext.UserGame.AddAsync(userGame);
                try
                {
                    // Your code
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception
                    Console.WriteLine($"Error: {ex.Message}");
                }

                foreach (var team in gameEvent.Boxscore.Players)
                {
                    foreach (var category in team.Statistics)
                    {
                        foreach(var athlete in category.Athletes)
                        {
                            if (!await _dbContext.Player.AnyAsync(p => p.ID == athlete.Athlete.ID))
                            {
                                Models.DBData.Player player = new Models.DBData.Player { ID = athlete.Athlete.ID, Name = athlete.Athlete.DisplayName };
                                await _dbContext.Player.AddAsync(player);
                                try
                                {
                                    // Your code
                                    await _dbContext.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle the exception
                                    Console.WriteLine($"Error: {ex.Message}");
                                }
                            }
                            if (athlete.Athlete.ID[0] != '-')
                            {
                                PlayerGame pg = new PlayerGame { GameID = game.ID, PlayerID = athlete.Athlete.ID, Stats = string.Join(",", athlete.Stats), League = game.League, Team = team.Team.DisplayName };
                                if (category.Type == null)
                                {
                                    if (category.Name != null)
                                    {
                                        pg.Position = category.Name;
                                    }
                                    else
                                    {
                                        pg.Position = "";
                                    }
                                }
                                else
                                {
                                    pg.Position = category.Type;

                                }
                                await _dbContext.PlayerGame.AddAsync(pg);
                                try
                                {
                                    // Your code
                                    await _dbContext.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    // Log or handle the exception
                                    Console.WriteLine($"Error: {ex.Message}");
                                }
                            }
                        }
                    }
                }
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

        public async Task<IActionResult> Follow([FromBody] UserFollow userFollow)
        {
            _dbContext.UserFollow.Add(userFollow);
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }

        public async Task<IActionResult> Unfollow([FromBody] UserFollow userFollow)
        {
            UserFollow foundUserFollow = _dbContext.UserFollow.Where(uf => uf.UserID == userFollow.UserID && uf.FollowingID == userFollow.FollowingID).FirstOrDefault();
            _dbContext.UserFollow.Remove(foundUserFollow);
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }


        //public TeamsView GetTeams(string url, string view)
        //{
        //    TeamsView teamsView = new TeamsView();
        //    string[] lines = new string[] {};
        //    if(view == "NCAAF")
        //    {
        //        string teamsFilePath = Path.Combine("Files", "NCAAFTeams.txt");
        //        lines = System.IO.File.ReadAllLines(teamsFilePath);
        //    }
        //    Root root;
        //    using (var client = new HttpClient())
        //    {
        //        string teamsResults = client.GetStringAsync(url).Result;

        //        root = JsonConvert.DeserializeObject<Root>(teamsResults);
        //    }

        //    // Access the list of sports and extract team names
        //    foreach (var sport in root.sports)
        //    {
        //        foreach (var league in sport.leagues)
        //        {
        //            foreach (var team in league.teams)
        //            {

        //                if (view == "NCAAF")
        //                {
        //                    if (lines.Contains(team.team.displayName))
        //                    {
        //                        teamsView.teams.Add(team);
        //                    }
        //                }
        //                else
        //                {
        //                    teamsView.teams.Add(team);
        //                }                       
        //            }
        //        }
        //    }
        //    return teamsView;
        //}
    }
}