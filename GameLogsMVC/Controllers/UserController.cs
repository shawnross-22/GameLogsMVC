using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.GameData;
using GameLogsMVC.Models.PullData;
using GameLogsMVC.Models.ViewData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;

namespace GameLogsMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDBContext _dbContext;
        private readonly BadgeCheckerFactory _badgeCheckerFactory;

        public UserController(ILogger<UserController> logger, ApplicationDBContext dbContext, BadgeCheckerFactory badgeCheckerFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _badgeCheckerFactory = badgeCheckerFactory;
        }
        public async Task<IActionResult> Index(string userName)
        {
            UserView userView = new UserView();
            userView.ID = userName;
            if(userName == Request.Cookies["userID"])
            {
                userView.Change = true;
            }
            else
            {
                userView.Change = false;
            }
            
            userView.Count = await _dbContext.UserGame.Where(u => u.UserID == userView.ID).CountAsync();

            var countCurrent = await _dbContext.UserGame
                .Join(
                _dbContext.Game,
                userGame => userGame.GameID,  
                game => game.ID,              
                (userGame, game) => new { UserGame = userGame, Game = game }
                )
                .Where(joined => joined.UserGame.UserID == userView.ID && joined.Game.Date.Substring(0, 4) == DateTime.Now.Year.ToString())
                .CountAsync();

            userView.CountCurrent = countCurrent;

            if(userView.ID != Request.Cookies["userID"])
            {
                var userGameIDs = _dbContext.UserGame
                .Where(ug => ug.UserID == userName || ug.UserID == Request.Cookies["userID"])
                .GroupBy(ug => ug.GameID)
                .Where(group => group.Count() == 2)
                .Select(group => group.Key)
                .ToList();

                userView.SameGames = _dbContext.Game.Where(g => userGameIDs.Contains(g.ID)).ToList();
                userView.SameGames = userView.SameGames.OrderByDescending(g => g.Date).ToList();
            }

            Dictionary<string, string> favTeams = _dbContext.User.Where(u => u.ID == userView.ID).Select(u => new Dictionary<string, string> { { "MLB", u.FavMLB },{ "NBA", u.FavNBA },{ "NFL", u.FavNFL },{ "NCAAF", u.FavNCAAF }}).FirstOrDefault();
            if (!favTeams.IsNullOrEmpty())
            {
                foreach (var key in favTeams.Keys)
                {
                    if (!string.IsNullOrEmpty(favTeams[key]))
                    {
                        userView.FavTeams.Add(key, _dbContext.Team.FirstOrDefault(u => u.League == key && u.Name == favTeams[key]));
                    }
                    else
                    {
                        userView.FavTeams.Add(key, null);
                    }
                }
            }

            return View(userView);
        }

        public IActionResult EditProfile(string userName)
        {
            EditProfileView editProfileView = new EditProfileView();
            editProfileView.User = _dbContext.User.Where(u => u.ID == userName).FirstOrDefault();
            editProfileView.MLBTeams.AddRange(_dbContext.Team.Where(t => t.League == "MLB").ToList());
            editProfileView.NFLTeams.AddRange(_dbContext.Team.Where(t => t.League == "NFL").ToList());
            editProfileView.NBATeams.AddRange(_dbContext.Team.Where(t => t.League == "NBA").ToList());
            editProfileView.NCAAFTeams.AddRange(_dbContext.Team.Where(t => t.League == "NCAAF").ToList());
            List<string> favTeams = _dbContext.User.Where(u => u.ID == editProfileView.User.ID).Select(u => new List<string> { u.FavMLB, u.FavNBA, u.FavNFL, u.FavNCAAF }).FirstOrDefault();
            if (!favTeams.IsNullOrEmpty())
            {
                foreach (var team in favTeams)
                {
                    if (team != "None")
                    {
                        editProfileView.FavTeams.Add(_dbContext.Team.Where(u => u.Name == team).FirstOrDefault());
                    }
                    else
                    {
                        editProfileView.FavTeams.Add(new Models.DBData.Team());
                    }
                }
            }
            return View(editProfileView);
        }

        public IActionResult Diary(string userName, string league)
        {
            DiaryView diaryView = new DiaryView();
            diaryView.User = userName;
            diaryView.League = league;
            diaryView.GameResult = GetUserGames(league , userName);
            return View(diaryView);
        }

        public IActionResult Stats(string userName, string league)
        {
            StatsView statsView = new StatsView();
            statsView.League = league;
            statsView.User = userName;
            if (league != "Favorites")
            {
                GameResult results = GetUserGames(league, userName);
                if (results.games.Count() != 0)
                {
                    List<string> gameIds = new List<string>();
                    foreach (var g in results.games)
                    {
                        gameIds.Add(g.ID);
                    }
                    switch (league)
                    {
                        case "MLB":
                            statsView.Player = GetFrequentPlayer("batting", gameIds);
                            statsView.Pitcher = GetFrequentPlayer("pitching", gameIds);
                            statsView.LongGame = GetShortLongGame(results.games, "L");
                            statsView.ShortGame = GetShortLongGame(results.games, "S");
                            List<int> MLBindex = new List<int> { 3, 2, 5, 4, 0, 5 };
                            List<string> MLBpositions = new List<string> { "batting", "batting", "batting", "batting", "pitching", "pitching" };
                            statsView.Stats = GetHighStat(MLBindex, gameIds, MLBpositions);
                            List<Dictionary<string, string>> MLBaccStats = GetAccumulatedStat(MLBindex, gameIds, MLBpositions);
                            statsView.Stats.AddRange(MLBaccStats);
                            break;
                        case "NBA":
                            statsView.Player = GetFrequentPlayer("", gameIds);
                            List<int> NBAindex = new List<int> { 13, 6, 7, 8, 9 };
                            List<string> NBApositions = new List<string> { "", "", "", "", "" };
                            statsView.Stats = GetHighStat(NBAindex, gameIds, NBApositions);
                            List<Dictionary<string, string>> NBAaccStats = GetAccumulatedStat(NBAindex, gameIds, NBApositions);
                            statsView.Stats.AddRange(NBAaccStats);
                            break;
                        case "NCAAB":
                            statsView.Player = GetFrequentPlayer("", gameIds);
                            List<int> NCAABindex = new List<int> { 12, 6, 7, 8, 9 };
                            List<string> NCAABpositions = new List<string> { "", "", "", "", "" };
                            statsView.Stats = GetHighStat(NCAABindex, gameIds, NCAABpositions);
                            List<Dictionary<string, string>> NCAABaccStats = GetAccumulatedStat(NCAABindex, gameIds, NCAABpositions);
                            statsView.Stats.AddRange(NCAABaccStats);
                            break;
                        case "NFL":
                        case "NCAAF":
                            statsView.Player = GetFrequentPlayer("", gameIds);
                            List<int> NFLindex = new List<int> { 1, 1, 4, 1, 4, 2 };
                            List<string> NFLpositions = new List<string> { "passing", "rushing", "rushing", "receiving", "receiving", "kicking" };
                            List<int> NFLaccindex = new List<int> { 1, 3, 1, 3, 1, 3, 0, 0, 2, 0 };
                            List<string> NFLaccpositions = new List<string> { "passing", "passing", "rushing", "rushing", "receiving", "receiving", "interceptions", "defensive", "defensive", "kicking" };
                            statsView.Stats = GetHighStat(NFLindex, gameIds, NFLpositions);
                            List<Dictionary<string, string>> NFLaccStats = GetAccumulatedStat(NFLaccindex, gameIds, NFLaccpositions);
                            statsView.Stats.AddRange(NFLaccStats);
                            break;

                    }

                    statsView.Games = gameIds.Count();
                    statsView.TeamsSeen = results.games.SelectMany(g => new[] { g.Away, g.Home }).Distinct().Count();
                    statsView.TeamsVisited = results.games.Where(g => g.NeutralSite == "False").Select(g => g.Home).Distinct().Count();

                    Game impactGame = results.games
                    .Where(g => !string.IsNullOrEmpty(g.ImpactPlay))
                    .ToList()
                    .OrderByDescending(g => double.Parse(g.ImpactPlay.Substring(g.ImpactPlay.LastIndexOf(' ') + 1).Trim('%')) / 100.0)
                    .FirstOrDefault();
                    statsView.Play = impactGame.ImpactPlay;

                    Game highestScoring = results.games
                    .OrderByDescending(g => int.Parse(g.Score.Substring(0, g.Score.IndexOf('-'))) + int.Parse(g.Score.Substring(g.Score.IndexOf('-') + 1)))
                    .FirstOrDefault();
                    statsView.HighScoring.Add(highestScoring.Date + ", " + highestScoring.Home + " vs. " + highestScoring.Away, highestScoring.Score);

                }
            }
            else
            {
                List<string> favTeams = _dbContext.User.Where(u => u.ID == userName).Select(u => new List<string> { u.FavMLB, u.FavNBA, u.FavNFL, u.FavNCAAF }).FirstOrDefault();
                List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userName).Select(ug => ug.GameID).ToList();
                foreach (var team in favTeams)
                {
                    if (team != "")
                    {
                        int count = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.Away == team || g.Home == team)).Count();
                        Dictionary<string, int> favCount = new Dictionary<string, int> { { team, count } };
                        statsView.FavsCount.Add(favCount);
                        var wins = _dbContext.Game
                        .Where(g => userGames.Contains(g.ID))
                        .AsEnumerable() // Switch to client-side evaluation
                        .Count(g =>
                        {
                            var parts = g.Score.Split('-');
                            var homeScore = int.Parse(parts[0]);
                            var awayScore = int.Parse(parts[1]);
                            return (g.Home == team && homeScore > awayScore) ||
                                   (g.Away == team && awayScore > homeScore);
                        });
                        var losses = _dbContext.Game
                        .Where(g => userGames.Contains(g.ID))
                        .AsEnumerable() // Switch to client-side evaluation
                        .Count(g =>
                        {
                            var parts = g.Score.Split('-');
                            var homeScore = int.Parse(parts[0]);
                            var awayScore = int.Parse(parts[1]);
                            return (g.Home == team && homeScore < awayScore) ||
                                   (g.Away == team && awayScore < homeScore);
                        });
                        Dictionary<string, string> favRecord = new Dictionary<string, string> { { team, wins.ToString() + "-" + losses.ToString()} };
                        statsView.FavsRecord.Add(favRecord);
                    }

                }
            }
            return View(statsView);
        }

        public async Task<IActionResult> Badges(string userName)
        {
            UserBadgeView badgeView = new UserBadgeView();
            badgeView.UserID = userName;
            badgeView.Badges = _dbContext.Badge.ToList();
            foreach (var badge in badgeView.Badges)
            {
                IBadgeChecker badgeChecker = _badgeCheckerFactory.GetBadgeChecker(badge.ID);
                if (badgeChecker != null)
                {
                    var (progress, completion) = badgeChecker.CheckCompletion(userName, false);
                    badgeView.Completion.Add(completion);
                    badgeView.Progress.Add(progress);
                }
            }

            return View(badgeView);
        }

        public async Task<IActionResult> Followers(string userName)
        {
            FollowerView followerView = new FollowerView();
            followerView.User = userName;
            followerView.userFollows = _dbContext.UserFollow.Where(uf => uf.FollowingID == userName).ToList();
            foreach (var user in followerView.userFollows)
            {
                int games = _dbContext.UserGame.Where(u => u.UserID == user.UserID).Count();
                followerView.followGames.Add(games);
            }
            return View(followerView);
        }

        public async Task<IActionResult> Following(string userName)
        {
            FollowerView followerView = new FollowerView();
            followerView.User = userName;
            followerView.userFollows = _dbContext.UserFollow.Where(uf => uf.UserID == userName).ToList();
            foreach (var user in followerView.userFollows)
            {
                int games = _dbContext.UserGame.Where(u => u.UserID == user.FollowingID).Count();
                followerView.followGames.Add(games);
            }
            return View(followerView);
        }

        public async Task<IActionResult> ChangeFavTeam([FromBody] FavTeam favTeam)
        {
            User user = _dbContext.User.Where(u => u.ID == favTeam.ID).FirstOrDefault();

            // Use reflection to set the value of the corresponding property
            user.FavMLB = favTeam.FavMLB;
            user.FavNBA = favTeam.FavNBA;
            user.FavNFL = favTeam.FavNFL;
            user.FavNCAAF = favTeam.FavNCAAF;
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }

        public GameResult GetUserGames(string league, string userId)
        {
            GameResult gameResults = new GameResult();
            List<Game> gameIDs = new List<Game>();
            List<bool> attendance = new List<bool>();
            string viewerId = Request.Cookies["userID"];
            List<string> userGames = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userId)
            .Select(UserGame => UserGame.GameID)
            .ToList();

            for (int i = 0; i < userGames.Count(); i++)
            {
                Game userGame = _dbContext.Game.Where(Game => Game.ID == userGames[i]).FirstOrDefault();
                if (userGame.League == league)
                {
                    gameIDs.Add(userGame);
                }
            }

            gameIDs = gameIDs.OrderByDescending(g => g.Date).ToList();

            foreach (var game in gameIDs)
            {
                if (userId == viewerId)
                {
                    attendance.Add(true);
                }
                else if(viewerId == null)
                {
                    attendance = null;
                }
                else
                {
                    UserGame match = _dbContext.UserGame.Where(ug => ug.GameID == game.ID && ug.UserID == viewerId).FirstOrDefault();
                    if (match != null)
                    {
                        attendance.Add(true);
                    }
                    else
                    {
                        attendance.Add(false);
                    }
                }
            }
            

            gameResults.games = gameIDs;
            gameResults.attended = attendance;

            return gameResults;
        }

        public Dictionary<string, int> GetFrequentPlayer(string position, List<string> gameIds)
        {
            List<PlayerGame> playerGames = _dbContext.PlayerGame.Where(pg => gameIds.Contains(pg.GameID) && pg.Position == position && pg != null).ToList();
            Dictionary<string, int> playerGameCountDictionary = _dbContext.PlayerGame
            .Where(pg => gameIds.Contains(pg.GameID) && (string.IsNullOrEmpty(position) || pg.Position == position))
            .ToList()
            .GroupBy(pg => pg.PlayerID)
            .OrderByDescending(group => group.Select(pg => pg.GameID).Distinct().Count())
            .Take(3)
            .Join
            (
                _dbContext.Player,
                pg => pg.Key,
                p => p.ID,
                (pg, p) => new { PlayerID = pg.Key, PlayerName = p.Name, GameCount = pg.Select(x => x.GameID).Distinct().Count() }
            )
            .ToDictionary(
                item => item.PlayerName,   // Key selector (PlayerName)
                item => item.GameCount     // Value selector (GameCount)
            );

            return playerGameCountDictionary;
        }

        public Dictionary<string, string> GetShortLongGame (List<Game> games, string length)
        {
            Dictionary<string, string> gameLengthDictionary = new Dictionary<string, string>();
            Game game = new Game();
            if(length == "L")
            {
                game = games
                .Where(g => !string.IsNullOrEmpty(g.Duration))
                .Select(g => g)
                .OrderByDescending(g => g.Duration)
                .FirstOrDefault();
            }
            else
            {
                game = games
                .Where(g => !string.IsNullOrEmpty(g.Duration))
                .Select(g => g)
                .OrderBy(g => g.Duration)
                .FirstOrDefault();
            }

            gameLengthDictionary.Add(game.Date + ", " + game.Away + " @ " + game.Home, game.Duration);
            return gameLengthDictionary;
        }

        public List<Dictionary<string, string>> GetAccumulatedStat(List<int> indexes, List<string> gameIds, List<string> positions)
        {
            List<Dictionary<string, string>> statsList = new List<Dictionary<string, string>>();
            for (int i = 0; i < indexes.Count(); i++)
            {
                Dictionary<string, string> highStat = _dbContext.PlayerGame
                .Where(pg => gameIds.Contains(pg.GameID) && !string.IsNullOrEmpty(pg.Stats))
                .ToList()
                .Where(pg => pg.Stats.Split(',').Length > indexes[i] && pg.Position == positions[i])
                .GroupBy(pg => pg.PlayerID)
                .OrderByDescending(group =>
                    positions[i] == "pitching" && indexes[i] == 0
                        ? group.Sum(pg => ConvertToDecimalInnings(pg.Stats, indexes[i]))
                        : (positions[i] == "kicking"
                            ? group.Sum(pg => pg.Stats.Split(',')[indexes[i]][0] - '0') // Sum of the first character as integer
                            : group.Sum(pg => double.Parse(pg.Stats.Split(',')[indexes[i]])))) // Sum as integer
                .Take(1)
                .Join(
                    _dbContext.Player,
                    pg => pg.Key,
                    p => p.ID,
                    (pg, p) => new
                    {
                        PlayerName = p.Name,
                        HRSum = positions[i] == "pitching" && indexes[i] == 0
                            ? pg.Sum(item => ConvertToDecimalInnings(item.Stats, indexes[i]))
                            : (positions[i] == "kicking"
                                ? pg.Sum(item => item.Stats.Split(',')[indexes[i]][0] - '0') // Sum of the first character as integer
                                : pg.Sum(item => int.Parse(item.Stats.Split(',')[indexes[i]])))
                    })
                .ToDictionary(
                    item => item.PlayerName,
                    item => (positions[i] == "pitching" && indexes[i] == 0)
                            ? ConvertToBaseballInningsString(item.HRSum)
                            : item.HRSum.ToString()
                );

                statsList.Add(highStat);

            }
            
            return statsList;
        }

        public List<Dictionary<string, string>> GetHighStat(List<int> indexes, List<string> gameIds, List<string> positions)
        {
            List<Dictionary<string, string>> statsList = new List<Dictionary<string, string>>();
            for (int i = 0; i < indexes.Count(); i++)
            {
                Dictionary<string, string> highStat = _dbContext.PlayerGame
                    .Where(pg => gameIds.Contains(pg.GameID) && !string.IsNullOrEmpty(pg.Stats) && pg.Position == positions[i])
                    .ToList()
                    .Where(pg => !string.IsNullOrEmpty(pg.Stats.Split(',')[indexes[i]]))
                    .Select(pg => new
                    {
                        PlayerID = pg.PlayerID,
                        MaxStat = positions[i] == "pitching" && indexes[i] == 0
                            ? ConvertToDecimalInnings(pg.Stats, indexes[i])
                            : int.Parse(pg.Stats.Split(',')[indexes[i]]),
                        GameID = pg.GameID,
                        TeamID = pg.Team
                     })
                    .OrderByDescending(result => result.MaxStat)
                    .Take(1)
                    .Join(
                        _dbContext.Player,
                        result => result.PlayerID,
                        p => p.ID,
                        (result, p) => new
                        {
                            PlayerName = p.Name,
                            Team = result.TeamID,
                            GameID = result.GameID,
                            MaxStat = result.MaxStat
                        })
                    .Select(result => new
                    {
                        PlayerName = result.PlayerName + ", " + result.Team, // Concatenate PlayerName and Team
                        result.MaxStat,
                        result.GameID
                    })
                    .Join(
                        _dbContext.Game,
                        result => result.GameID,
                        g => g.ID,
                        (result, g) => new
                        {
                            PlayerName = result.PlayerName + ", " + DateTime.Parse(g.Date).ToString("yyyy-MM-dd") + " " + g.Away + " @ " + g.Home,
                            result.MaxStat
                        })
                    .ToDictionary(
                        item => item.PlayerName,
                        item => (positions[i] == "pitching" && indexes[i] == 0)
                            ? ConvertToBaseballInningsString(item.MaxStat)
                            : item.MaxStat.ToString()
                    );
                statsList.Add(highStat);
            }

            return statsList;
        }

        // Function to convert innings from baseball notation to decimal notation
        double ConvertToDecimalInnings(string stats, int index)
        {
            string[] statsArray = stats.Split(',');
            double innings = double.Parse(statsArray[index]);
            double converted = Math.Floor(innings) + ((innings - Math.Floor(innings)) * 10) / 3;
            return converted;
        }

        private string ConvertToBaseballInningsString(double innings)
        {
            int wholeInnings = (int)Math.Floor(innings);
            int outs = (int)((innings - wholeInnings) * 3);
            return $"{wholeInnings}.{outs}";
        }
    }
}
