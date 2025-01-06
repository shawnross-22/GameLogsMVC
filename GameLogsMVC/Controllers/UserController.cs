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
            
            List<string> userGames = _dbContext.UserGame.Where(u => u.UserID == userView.ID && u.Attended == "A").Select(u => u.GameID).ToList();

            userView.Count = userGames.Count();

            var countCurrent = _dbContext.Game.Where(g => userGames.Contains(g.ID) && g.Date.Substring(0, 4) == DateTime.Now.Year.ToString()).Count();

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

            Dictionary<string, string> favTeams = _dbContext.User.Where(u => u.ID == userView.ID).Select(u => new Dictionary<string, string> { { "MLB", u.FavMLB },{ "NBA", u.FavNBA },{ "NFL", u.FavNFL },{ "NCAAF", u.FavNCAAF },{ "NCAAB", u.FavNCAAB } }).FirstOrDefault();
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
            editProfileView.NCAABTeams.AddRange(_dbContext.Team.Where(t => t.League == "NCAAB").ToList());
            List<string> favTeams = _dbContext.User.Where(u => u.ID == editProfileView.User.ID).Select(u => new List<string> { u.FavMLB, u.FavNBA, u.FavNFL, u.FavNCAAF, u.FavNCAAB }).FirstOrDefault();
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

        public IActionResult Log(string userName, string league)
        {
            DiaryView diaryView = new DiaryView();
            diaryView.User = userName;
            diaryView.League = league;
            diaryView.GameResult = GetUserGames(league , userName)[1];
            return View(diaryView);
        }

        public IActionResult Stats(string userName, string league)
        {
            StatsView statsView = new StatsView();
            statsView.League = league;
            statsView.User = userName;

            GameResult resultWatched = GetUserGames(league, userName)[1];
            GameResult resultAttended = GetUserGames(league, userName)[0];
            List<string> gameIdsWatched = new List<string>();
            List<string> gameIdsAttended = new List<string>();
            foreach (var games in resultWatched.games)
            {
                gameIdsWatched.Add(games.ID);
            }
            foreach (var games in resultAttended.games)
            {
                gameIdsAttended.Add(games.ID);
            }
            if (resultWatched.games.Count()!=0 || league == "Favorites")
            {
                statsView.StatsWatched = GetStatsForView(gameIdsWatched, resultWatched, league, userName, "");
                statsView.StatsAttended = GetStatsForView(gameIdsAttended, resultAttended, league, userName, "A");
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
                    var (progress, completion, gameID, index, leagues, homeIDs, awayIDs, playerIDs) = badgeChecker.CheckCompletion(userName, false);
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
            user.FavNCAAB = favTeam.FavNCAAB;
            await _dbContext.SaveChangesAsync();
            return Ok("Success");
        }

        public List<GameResult> GetUserGames(string league, string userId)
        {
            GameResult gameResultAttended = new GameResult();
            GameResult gameResultWatched = new GameResult();
            List<GameResult> gameResults = new List<GameResult> { gameResultAttended, gameResultWatched };
            List<Game> gameIDs = new List<Game>();
            List<bool> attendance = new List<bool>();
            List<bool> watched = new List<bool>();
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
                if(viewerId == null)
                {
                    attendance = null;
                }
                else
                {
                    UserGame match = _dbContext.UserGame.Where(ug => ug.GameID == game.ID && ug.UserID == viewerId).FirstOrDefault();
                    if (match != null)
                    {
                        if (match.Attended == "A")
                        {
                            attendance.Add(true);
                        }
                        else
                        {
                            attendance.Add(false);
                        }

                        if (match.Attended == "W")
                        {
                            watched.Add(true);
                        }
                        else
                        {
                            watched.Add(false);
                        }
                    }                  
                    else
                    {
                        attendance.Add(false);
                        watched.Add(false);
                    }
                }
            }

            gameResults[1].games = gameIDs;
            gameResults[1].attended = attendance;
            gameResults[1].watched = watched;

            List<string> userGamesAttended = _dbContext.UserGame
            .Where(UserGame => UserGame.UserID == userId && UserGame.Attended == "A")
            .Select(UserGame => UserGame.GameID)
            .ToList();

            List<Game> gameIDsAttended = new List<Game>();

            for (int i = 0; i < userGamesAttended.Count(); i++)
            {
                Game userGame = _dbContext.Game.Where(Game => Game.ID == userGamesAttended[i]).FirstOrDefault();
                if (userGame.League == league)
                {
                    gameIDsAttended.Add(userGame);
                }
            }

            gameResults[0].games = gameIDsAttended;

            return gameResults;
        }

        public Dictionary<string, List<string>> GetFrequentPlayer(string position, List<string> gameIds)
        {
            List<PlayerGame> playerGames = _dbContext.PlayerGame.Where(pg => gameIds.Contains(pg.GameID) && pg.Position == position && pg != null).ToList();
            Dictionary<string, List<string>> playerGameCountDictionary = _dbContext.PlayerGame
            .Where(pg => gameIds.Contains(pg.GameID) && (string.IsNullOrEmpty(position) || pg.Position == position) && pg.Stats != "")
            .ToList()
            .GroupBy(pg => pg.PlayerID)
            .OrderByDescending(group => group.Select(pg => pg.GameID).Distinct().Count())
            .Take(10)
            .Join
            (
                _dbContext.Player,
                pg => pg.Key,
                p => p.ID,
                (pg, p) => new { PlayerID = pg.Key, PlayerName = p.Name, GameCount = pg.Select(x => x.GameID).Distinct().Count() }
            )
            .ToDictionary(
                item => item.PlayerID,
                item => new List<string> { item.PlayerName, item.GameCount.ToString() }
            );

            return playerGameCountDictionary;
        }

        public Game GetShortLongGame (List<Game> games, string length)
        {
            List<string> gameLength = new List<string>();
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
  
            return game;
        }

        public List<Dictionary<List<string>, string>> GetAccumulatedStat(List<int> indexes, List<string> gameIds, List<string> positions)
        {
            List<Dictionary<List<string>, string>> statsList = new List<Dictionary<List<string>, string>>();
            for (int i = 0; i < indexes.Count(); i++)
            {
                Dictionary<List<string>, string> highStat = _dbContext.PlayerGame
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
                        PlayerID = p.ID,
                        HRSum = positions[i] == "pitching" && indexes[i] == 0
                            ? pg.Sum(item => ConvertToDecimalInnings(item.Stats, indexes[i]))
                            : (positions[i] == "kicking"
                                ? pg.Sum(item => item.Stats.Split(',')[indexes[i]][0] - '0') // Sum of the first character as integer
                                : pg.Sum(item => double.Parse(item.Stats.Split(',')[indexes[i]])))
                    })
                .ToDictionary(
                    item => new List<string> { item.PlayerName, item.PlayerID },
                    item => (positions[i] == "pitching" && indexes[i] == 0)
                            ? ConvertToBaseballInningsString(item.HRSum)
                            : item.HRSum.ToString()
                );

                statsList.Add(highStat);

            }
            
            return statsList;
        }

        public List<Dictionary<List<string>, string>> GetHighStat(List<int> indexes, List<string> gameIds, List<string> positions)
        {
            List<Dictionary<List<string>, string>> statsList = new List<Dictionary<List<string>, string>>();
            for (int i = 0; i < indexes.Count(); i++)
            {
                Dictionary<List<string>, string> highStat = _dbContext.PlayerGame
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
                            PlayerID = result.PlayerID,
                            Team = result.TeamID,
                            GameID = result.GameID,
                            MaxStat = result.MaxStat
                        })
                    .Select(result => new
                    {
                        PlayerName = result.PlayerName,
                        PlayerID = result.PlayerID,
                        result.Team,// Concatenate PlayerName and Team
                        result.MaxStat,
                        result.GameID
                    })
                    .Join(
                        _dbContext.Game,
                        result => result.GameID,
                        g => g.ID,
                        (result, g) => new
                        {
                            PlayerName = new List<string> { g.ID, g.Home != result.Team ? g.AwayTeamID : g.HomeTeamID, g.Home != result.Team ? g.HomeTeamID : g.AwayTeamID, result.PlayerID, result.PlayerName, DateTime.Parse(g.Date).ToString("yyyy-MM-dd"), result.Team, g.Away != result.Team ? g.Away : g.Home },
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

        public StatsView.Stats GetStatsForView (List<string> gameIds, GameResult results, string league, string userName, string attended)
        {
            StatsView.Stats statsView = new StatsView.Stats();
            if (league != "Favorites")
            {
                switch (league)
                {
                    case "MLB":
                        statsView.Player = GetFrequentPlayer("batting", gameIds);
                        statsView.Pitcher = GetFrequentPlayer("pitching", gameIds);
                        statsView.GameStats.Add(GetShortLongGame(results.games, "L"));
                        statsView.GameStats.Add(GetShortLongGame(results.games, "S"));
                        List<int> MLBindex = new List<int> { 3, 2, 5, 4, 0, 5 };
                        List<string> MLBpositions = new List<string> { "batting", "batting", "batting", "batting", "pitching", "pitching" };
                        statsView.SingleStats = GetHighStat(MLBindex, gameIds, MLBpositions);
                        statsView.AccStats = GetAccumulatedStat(MLBindex, gameIds, MLBpositions);
                        break;
                    case "NBA":
                        statsView.Player = GetFrequentPlayer("", gameIds);
                        List<int> NBAindex = new List<int> { 13, 6, 7, 8, 9 };
                        List<string> NBApositions = new List<string> { "", "", "", "", "" };
                        statsView.SingleStats = GetHighStat(NBAindex, gameIds, NBApositions);
                        statsView.AccStats = GetAccumulatedStat(NBAindex, gameIds, NBApositions);
                        break;
                    case "NCAAB":
                        statsView.Player = GetFrequentPlayer("", gameIds);
                        List<int> NCAABindex = new List<int> { 12, 6, 7, 8, 9 };
                        List<string> NCAABpositions = new List<string> { "", "", "", "", "" };
                        statsView.SingleStats = GetHighStat(NCAABindex, gameIds, NCAABpositions);
                        statsView.AccStats = GetAccumulatedStat(NCAABindex, gameIds, NCAABpositions);
                        break;
                    case "NFL":
                    case "NCAAF":
                        statsView.Player = GetFrequentPlayer("", gameIds);
                        List<int> NFLindex = new List<int> { 1, 1, 4, 1, 4, 2 };
                        List<string> NFLpositions = new List<string> { "passing", "rushing", "rushing", "receiving", "receiving", "kicking" };
                        List<int> NFLaccindex = new List<int> { 1, 3, 1, 3, 1, 3, 0, 0, 2, 0 };
                        List<string> NFLaccpositions = new List<string> { "passing", "passing", "rushing", "rushing", "receiving", "receiving", "interceptions", "defensive", "defensive", "kicking" };
                        statsView.SingleStats = GetHighStat(NFLindex, gameIds, NFLpositions);
                        statsView.AccStats = GetAccumulatedStat(NFLaccindex, gameIds, NFLaccpositions);
                        break;

                }

                statsView.Games = gameIds.Count();
                statsView.TeamsSeen = results.games.SelectMany(g => new[] { g.Away, g.Home }).Distinct().Count();
                statsView.TeamsVisited = results.games.Where(g => g.NeutralSite == "False").Select(g => g.Home).Distinct().Count();

                Game highestScoring = results.games
                .OrderByDescending(g => int.Parse(g.Score.Substring(0, g.Score.IndexOf('-'))) + int.Parse(g.Score.Substring(g.Score.IndexOf('-') + 1)))
                .FirstOrDefault();
                statsView.GameStats.Add(highestScoring);

                Game impactGame = results.games
                .Where(g => !string.IsNullOrEmpty(g.ImpactPlay))
                .ToList()
                .OrderByDescending(g => double.Parse(g.ImpactPlay.Substring(g.ImpactPlay.LastIndexOf(' ') + 1).Trim('%')) / 100.0)
                .FirstOrDefault();
                statsView.GameStats.Add(impactGame);


            }
            else
            {
                Dictionary<string, string> favTeams = _dbContext.User.Where(u => u.ID == userName).Select(u => new Dictionary<string, string> { { "MLB", u.FavMLB }, { "NBA", u.FavNBA }, { "NFL", u.FavNFL }, { "NCAAF", u.FavNCAAF }, { "NCAAB", u.FavNCAAB } }).FirstOrDefault();
                List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userName && ug.Attended.Contains(attended)).Select(ug => ug.GameID).ToList();
                foreach (var favTeam in favTeams.Keys)
                {
                    if (favTeams[favTeam] != "")
                    {
                        int count = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.Away == favTeams[favTeam] || g.Home == favTeams[favTeam]) && g.League == favTeam).Count();
                        Dictionary<string, List<string>> favCount = new Dictionary<string, List<string>> { { favTeam, new List<string> { favTeams[favTeam], count.ToString() } } };
                        statsView.FavsCount.Add(favCount);
                        var wins = _dbContext.Game
                        .Where(g => userGames.Contains(g.ID) && g.League == favTeam)
                        .AsEnumerable() // Switch to client-side evaluation
                        .Count(g =>
                        {
                            var parts = g.Score.Split('-');
                            var homeScore = int.Parse(parts[0]);
                            var awayScore = int.Parse(parts[1]);
                            return (g.Home == favTeams[favTeam] && homeScore > awayScore) ||
                                   (g.Away == favTeams[favTeam] && awayScore > homeScore);
                        });
                        var losses = _dbContext.Game
                        .Where(g => userGames.Contains(g.ID) && g.League == favTeam)
                        .AsEnumerable() // Switch to client-side evaluation
                        .Count(g =>
                        {
                            var parts = g.Score.Split('-');
                            var homeScore = int.Parse(parts[0]);
                            var awayScore = int.Parse(parts[1]);
                            return (g.Home == favTeams[favTeam] && homeScore < awayScore) ||
                                   (g.Away == favTeams[favTeam] && awayScore < homeScore);
                        });
                        Dictionary<string, List<string>> favRecord = new Dictionary<string, List<string>> { { favTeam, new List<string> { favTeams[favTeam], wins.ToString() + "-" + losses.ToString() } } };
                        statsView.FavsRecord.Add(favRecord);
                    }

                }
            }
            return statsView;
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
