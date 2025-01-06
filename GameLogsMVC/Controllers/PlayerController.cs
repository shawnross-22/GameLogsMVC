using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.GameData;
using GameLogsMVC.Models.ViewData;
using GameLogsMVC.Models.PullData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace GameLogsMVC.Controllers
{
    public class PlayerController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext _dbContext;

        public PlayerController(ILogger<HomeController> logger, ApplicationDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public IActionResult Index(string league, string playerID)
        {
            string userName = Request.Cookies["userID"];
            PlayerView playerView = new PlayerView();
            playerView.UserID = userName;
            playerView.League = league;
            string url = "";
            switch (league)
            {
                case "MLB":
                    url = "https://site.web.api.espn.com/apis/common/v3/sports/baseball/mlb/athletes/" + playerID;
                    break;
                case "NCAAF":
                    url = "https://site.web.api.espn.com/apis/common/v3/sports/football/college-football/athletes/" + playerID;
                    break;
                case "NFL":
                    url = "https://site.web.api.espn.com/apis/common/v3/sports/football/nfl/athletes/" + playerID;
                    break;
                case "NBA":
                    url = "https://site.web.api.espn.com/apis/common/v3/sports/basketball/nba/athletes/" + playerID;
                    break;
                case "NCAAB":
                    url = "https://site.web.api.espn.com/apis/common/v3/sports/basketball/mens-college-basketball/athletes/" + playerID;
                    break;
            }
            using (var client = new HttpClient())
            {
                string playerResults = client.GetStringAsync(url).Result;

                playerView.Player = JsonConvert.DeserializeObject<Players>(playerResults);
            }
            List<string> userGamesWatched = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userName)
            .Select(UserGame => UserGame.GameID)
            .ToList();

            List<string> userGamesAttended = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userName && UserGame.Attended == "A")
            .Select(UserGame => UserGame.GameID)
            .ToList();

            playerView.PlayerViewInfoAttended = GetPlayerViewInfo(userGamesAttended, playerView.Player, league);
            playerView.PlayerViewInfoWatched = GetPlayerViewInfo(userGamesWatched, playerView.Player, league);

            return View(playerView);
        }

        public PlayerViewInfo GetPlayerViewInfo (List<string> userGames, Players player, string league)
        {
            PlayerViewInfo playerView = new PlayerViewInfo();
            for (int i = 0; i < userGames.Count(); i++)
            {
                List<PlayerGame> playerGames = _dbContext.PlayerGame.Where(pg => pg.GameID == userGames[i] && pg.PlayerID == player.Athlete.ID && pg.League == league).ToList();
                if (playerGames.Count != 0)
                {
                    playerView.PlayerGames.AddRange(playerGames);
                    var game = _dbContext.Game.FirstOrDefault(g => g.ID == playerGames[0].GameID);

                    if (!playerView.Games.ContainsKey(game))
                    {
                        playerView.Games.Add(game, playerGames);
                    }
                }
            }

            playerView.Games = playerView.Games
            .OrderByDescending(kvp => kvp.Key.Date)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var gameIdToOrder = playerView.Games
            .Select((kvp, index) => new { kvp.Key.ID, Order = index })
            .ToDictionary(x => x.ID, x => x.Order);

            playerView.PlayerGames = playerView.PlayerGames
            .OrderBy(pg => gameIdToOrder[pg.GameID])
            .ToList();

            List<string> positions = new List<string>();
            List<string> order = new List<string>();

            int start = 0;
            int end = 0;
            switch (league)
            {
                case "MLB":
                    positions.AddRange(new List<string> { "pitching", "batting" });
                    foreach (var pos in positions)
                    {
                        if (playerView.PlayerGames.Any(pg => pg.Position == pos))
                        {
                            order.AddRange(new List<string> { pos, pos, pos, pos, pos, pos, pos, pos });
                            end += 8;
                        }
                    }
                    break;
                case "NBA":
                    start = 0;
                    end = 14;
                    break;
                case "NCAAB":
                    start = 0;
                    end = 13;
                    break;
                case "NFL":
                case "NCAAF":
                    switch (player.Athlete.Position.Abbreviation)
                    {
                        case "QB":
                            positions.AddRange(new List<string> { "passing", "rushing", "fumbles", "receiving", "kickReturns", "puntReturns", "defensive", "kicking", "punting", "interceptions" });
                            break;
                        case "RB":
                            positions.AddRange(new List<string> { "rushing", "receiving", "fumbles", "kickReturns", "puntReturns", "passing", "defensive", "kicking", "punting", "interceptions" });
                            break;
                        case "WR":
                        case "TE":
                            positions.AddRange(new List<string> { "receiving", "rushing", "fumbles", "kickReturns", "puntReturns", "passing", "defensive", "kicking", "punting", "interceptions" });
                            break;
                        case "OL":
                        case "LS":
                            positions.AddRange(new List<string> { "passing", "receiving", "rushing", "fumbles", "kickReturns", "puntReturns", "defensive", "kicking", "punting", "interceptions" });
                            break;
                        case "DE":
                        case "DL":
                        case "DT":
                        case "LB":
                        case "DB":
                        case "CB":
                        case "S":
                            positions.AddRange(new List<string> { "defensive", "interceptions", "fumbles", "passing", "rushing", "receiving", "kickReturns", "puntReturns", "kicking", "punting" });
                            break;
                        case "PK":
                            positions.AddRange(new List<string> { "kicking", "punting", "passing", "receiving", "rushing", "fumbles", "defensive", "kickReturns", "puntReturns", "interceptions" });
                            break;
                        case "P":
                            positions.AddRange(new List<string> { "punting", "kicking", "passing", "receiving", "rushing", "fumbles", "defensive", "kickReturns", "puntReturns", "interceptions" });
                            break;
                    }
                    foreach (var pos in positions)
                    {
                        if (playerView.PlayerGames.Any(pg => pg.Position == pos))
                        {
                            switch (pos)
                            {
                                case "passing":
                                    order.AddRange(new List<string> { pos, pos, pos, pos });
                                    end += 4;
                                    break;
                                case "receiving":
                                case "rushing":
                                case "fumbles":
                                case "kickReturns":
                                case "puntReturns":
                                case "interceptions":
                                    order.AddRange(new List<string> { pos, pos, pos });
                                    end += 3;
                                    break;
                                case "kicking":
                                case "punting":
                                    order.AddRange(new List<string> { pos, pos });
                                    end += 2;
                                    break;
                                case "defensive":
                                    order.AddRange(new List<string> { pos, pos, pos, pos, pos, pos, pos });
                                    end += 7;
                                    break;
                            }
                        }
                    }
                    break;
            }

            List<string> sums = new List<string>(new string[end - start]);

            for (int i = 0; i < sums.Count; i++)
            {
                sums[i] = "0"; // Initialize with zero values
            }

            foreach (var playerGame in playerView.PlayerGames)
            {
                var statsArray = playerGame.Stats.Split(',');
                List<int> js = new List<int>();
                if (league == "NFL" ||  league == "NCAAF")
                {
                    switch (playerGame.Position)
                    {
                        case "passing":
                            js.AddRange(new List<int> { 0, 1, 3, 4 });
                            break;
                        case "rushing":
                        case "receiving":
                            js.AddRange(new List<int> { 0, 1, 3 });
                            break;
                        case "fumbles":
                        case "interceptions":
                            js.AddRange(new List<int> { 0, 1, 2 });
                            break;
                        case "defensive":
                            js.AddRange(new List<int> { 0, 1, 2, 3, 4, 5, 6 });
                            break;
                        case "kickReturns":
                        case "puntReturns":
                            js.AddRange(new List<int> { 0, 1, 4 });
                            break;
                        case "kicking":
                            js.AddRange(new List<int> { 0, 3 });
                            break;
                        case "punting":
                            js.AddRange(new List<int> { 0, 1 });
                            break;
                    }
                    for (int i = 0; i < js.Count; i++)
                    {
                        if (double.TryParse(statsArray[js[i]], out double value))
                        {
                            sums[order.IndexOf(playerGame.Position) + i] = (Convert.ToDouble(sums[order.IndexOf(playerGame.Position) + i]) + value).ToString();
                        }
                        else if (statsArray[js[i]].Contains("-") || statsArray[js[i]].Contains("/"))
                        {
                            string splitter = "";
                            if (statsArray[js[i]].Contains("-"))
                            {
                                splitter = "-";
                            }
                            else
                            {
                                splitter = "/";
                            }
                            var parts = statsArray[js[i]].Split(splitter).ToList();
                            var currentParts = sums[order.IndexOf(playerGame.Position) + i].Split(splitter).ToList();

                            int beforeDash = Convert.ToInt32(currentParts[0]) + Convert.ToInt32(parts[0]);
                            int afterDash = Convert.ToInt32(currentParts.Count > 1 ? currentParts[1] : "0") + Convert.ToInt32(parts[1]);

                            sums[order.IndexOf(playerGame.Position) + i] = beforeDash + splitter + afterDash;
                        }
                    }
                }
                else if (league == "MLB")
                {
                    switch (playerGame.Position)
                    {
                        case "batting":
                            js.AddRange(new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 });
                            break;
                        case "pitching":
                            js.AddRange(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 });
                            break;
                    }
                    for (int i = 0; i < js.Count; i++)
                    {
                        if (int.TryParse(statsArray[js[i]], out int value))
                        {
                            sums[order.IndexOf(playerGame.Position) + i] = (Convert.ToInt32(sums[order.IndexOf(playerGame.Position) + i]) + value).ToString();
                        }
                        else if (statsArray[js[i]].Contains('.'))
                        {
                            double sumsinnings = double.Parse(sums[order.IndexOf(playerGame.Position) + i]);
                            double innings = double.Parse(statsArray[js[i]]);
                            double converted = Math.Floor(innings) + ((innings - Math.Floor(innings)) * 10) / 3 + Math.Floor(sumsinnings) + ((sumsinnings - Math.Floor(sumsinnings)) * 10) / 3;
                            int wholeInnings = (int)Math.Floor(converted);
                            int outs = (int)((converted - wholeInnings) * 3);
                            sums[order.IndexOf(playerGame.Position) + i] = $"{wholeInnings}.{outs}";
                        }
                    }
                }
                else
                {
                    for (int i = start; i <= end && i < statsArray.Length; i++)
                    {
                        if (int.TryParse(statsArray[i], out int value))
                        {
                            sums[i - start] = (Convert.ToInt32(sums[i - start]) + value).ToString();
                        }
                        else if (statsArray[i].Contains("-") || statsArray[i].Contains("/"))
                        {
                            string splitter = "";
                            if (statsArray[i].Contains("-"))
                            {
                                splitter = "-";
                            }
                            else
                            {
                                splitter = "/";
                            }
                            var parts = statsArray[i].Split(splitter).ToList();
                            var currentParts = sums[i - start].Split(splitter).ToList();

                            int beforeDash = Convert.ToInt32(currentParts[0]) + Convert.ToInt32(parts[0]);
                            int afterDash = Convert.ToInt32(currentParts.Count > 1 ? currentParts[1] : "0") + Convert.ToInt32(parts[1]);

                            sums[i - start] = beforeDash + splitter + afterDash;
                        }
                    }
                }
            }

            playerView.Sums = sums.Select(s => s.ToString()).ToList();
            return playerView;
        }
    }
}
