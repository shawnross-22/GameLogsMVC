using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.PullData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace GameLogsMVC.Models.ViewData
{
    public class UserBadgeView
    {
        public string UserID { get; set; }
        public List<Badge> Badges { get; set; }
        public List<double> Completion { get; set; }
        public List<List<List<string>>> Progress { get; set; }

        public UserBadgeView()
        {
            Badges = new List<Badge>();
            Completion = new List<double>();
            Progress = new List<List<List<string>>> { };
        }
    }

    public interface IBadgeChecker
    {
        (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails);
        ApplicationDBContext DbContext { get; set; }
    }

    public abstract class BaseBadgeChecker : IBadgeChecker
    {
        protected ApplicationDBContext _dbContext;

        public BaseBadgeChecker(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public abstract (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails);

        // Implement the DbContext property
        public ApplicationDBContext DbContext
        {
            get { return _dbContext; }
            set { _dbContext = value; }
        }
    }

    public class AllTeams : BaseBadgeChecker
    {
        private readonly string _league;

        public AllTeams(ApplicationDBContext dbContext, string league) : base(dbContext)
        {
            _league = league;
        }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.League == _league && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<string> homeTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Home).Distinct().ToList();
            List<string> awayTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Away).Distinct().ToList();
            List<string> allUniqueTeams = homeTeams.Concat(awayTeams).Distinct().ToList();
            List<DBData.Team> allTeams = _dbContext.Team.Where(t => t.League == _league).ToList();
            List<List<string>> progress = new List<List<string>> { new List<string> { "Team", "Seen", "First Time Seen"} };
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            if (getDetails) {
                foreach (var team in allTeams)
                {
                    string seen = allUniqueTeams.Contains(team.Name).ToString();
                    var firstVisitGame = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.AwayTeamID == team.ID || g.HomeTeamID == team.ID)).OrderBy(g => g.Date).FirstOrDefault();
                    string firstVisit = firstVisitGame != null ? firstVisitGame.Date.ToString() : "N/A";
                    string firstVisitID = firstVisitGame != null ? firstVisitGame.ID : "N/A";
                    List<string> teamInfo = new List<string> { team.Name, seen, firstVisit };
                    progress.Add(teamInfo);
                    gameIDs.Add(firstVisitID);
                    leagues.Add(_league);
                    homeIDs.Add(team.ID);
                }
            }           
            double completionPercent = (double)allUniqueTeams.Count() / (double)allTeams.Count();          
            int index = 2;         

            return (progress, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class GameType : BaseBadgeChecker
    {
        private readonly string _league;
        private readonly string _note;

        public GameType(ApplicationDBContext dbContext, string league, string note) : base(dbContext)
        {
            _league = league;
            _note = note;
        }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<List<string>> progress = new List<List<string>>();
            List<List<string>> ordered = new List<List<string>>();
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            List<List<string>> gameID = _dbContext.Game.Where(g => g.GameNote.Contains(_note) && g.League.Contains(_league) &&userGames.Contains(g.ID)).Select(g => new List<string> { g.ID, g.League, g.HomeTeamID, g.AwayTeamID, g.Date, g.Home, g.Away, g.Score, g.Location, g.GameNote }).ToList();
            if (getDetails)
            {
                ordered.AddRange(gameID.Select(g => g.ToList()).ToList());
            }          
            double completionPercent;
            if(gameID.Count() > 0)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }

            List<string> titles = new List<string> { "Date", "Home", "Away", "Score", "Location", "Note" };
            ordered = ordered.OrderByDescending(item => item[0]).ToList();

            foreach (var game in ordered)
            {
                gameIDs.Add(game[0]);
                leagues.Add(game[1]);
                homeIDs.Add(game[2]);
                awayIDs.Add(game[3]);
            }

            progress.AddRange(ordered.Select(g => g.Skip(4).ToList()).ToList());

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);

            int index = 5;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class HOF : BaseBadgeChecker
    {
        private readonly string _league;
        private readonly string _note;

        public HOF(ApplicationDBContext dbContext, string league) : base(dbContext)
        {
            _league = league;
        }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.League == _league && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<string> HOF = _dbContext.Player.Where(p => p.HOF == "True" && p.League == _league).Select(p => p.ID).ToList();
            List<string> leagues = new List<string>();
            var pgs = _dbContext.PlayerGame
            .Where(pg => userGames.Contains(pg.GameID) && HOF.Contains(pg.PlayerID) && pg.Stats != "")
            .Join(
                _dbContext.Player,
                pg => pg.PlayerID,
                p => p.ID,
                (pg, p) => new { PlayerGame = pg, Player = p })
            .Join(
                _dbContext.Game,
                pgp => pgp.PlayerGame.GameID,
                g => g.ID,
                (pgp, g) => new
                {
                    pgp.PlayerGame.GameID,
                    PlayerName = pgp.Player.Name,
                    PlayerID = pgp.Player.ID,
                    pgp.PlayerGame.Team,
                    GameDate = g.Date,
                    TeamID = pgp.PlayerGame.Team == g.Home ? g.HomeTeamID : g.AwayTeamID
                })
            .ToList();

            List<List<string>> progress = pgs
            .GroupBy(pg => pg.PlayerName)
            .Select(group => group.OrderBy(pg => pg.GameDate).First())
            .Select(pg => new List<string> { pg.PlayerName, pg.Team, pg.GameDate.ToString() })
            .ToList();

            List<string> gameIDs = pgs
            .GroupBy(pg => pg.PlayerName)
            .Select(group => group.OrderBy(pg => pg.GameDate).First())
            .Select(pg => pg.GameID)
            .ToList();

            List<string> homeIDs = pgs
            .GroupBy(pg => pg.PlayerName)
            .Select(group => group.OrderBy(pg => pg.GameDate).First())
            .Select(pg => pg.TeamID)
            .ToList();

            List<string> playerIDs = pgs
            .GroupBy(pg => pg.PlayerName)
            .Select(group => group.OrderBy(pg => pg.GameDate).First())
            .Select(pg => pg.PlayerID)
            .ToList();

            List<string> awayIDs = new List<string>();
            foreach (var id in gameIDs)
            {
                leagues.Add(_league);
            }


            double completionPercent;
            if (progress.Count() > 0)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }

            List<string> titles = new List<string> { "Player", "Team", "Date Seen" };
            progress = progress.OrderByDescending(item => item[2]).ToList();

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);
            int index = 2;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class MVP : BaseBadgeChecker
    {

        public MVP(ApplicationDBContext dbContext) : base(dbContext){}

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<Player> mvpPlayers = _dbContext.Player
            .Where(p => !string.IsNullOrEmpty(p.MVP))
            .ToList();

            List<PlayerGame> playerGames = _dbContext.PlayerGame
                .Where(pg => userGames.Contains(pg.GameID) && !string.IsNullOrEmpty(pg.Stats))
                .ToList();

            List<Game> games = _dbContext.Game
                .ToList();

            // Step 3: Perform in-memory filtering
            var mvpPlayersWithSeasons = mvpPlayers
                .Select(p => new
                {
                    Player = p,
                    MVPSeasons = p.MVP.Split(',').Select(s => s.Trim()).ToHashSet()
                })
                .ToList();

            var pgs = playerGames
                .Join(
                    mvpPlayersWithSeasons,
                    pg => pg.PlayerID,
                    p => p.Player.ID,
                    (pg, p) => new { PlayerGame = pg, MVPSeasons = p.MVPSeasons, Player = p.Player })
                .Join(
                    games,
                    pgp => pgp.PlayerGame.GameID,
                    g => g.ID,
                    (pgp, g) => new { pgp.PlayerGame, pgp.MVPSeasons, pgp.Player, Game = g })
                .Where(pgp =>
                    pgp.MVPSeasons.Contains(pgp.Game.Season))
                .Select(pgp => new
                {
                    pgp.PlayerGame.GameID,
                    PlayerName = pgp.Player.Name,
                    PlayerID = pgp.Player.ID,
                    pgp.PlayerGame.Team,
                    League = pgp.Game.League,
                    GameDate = pgp.Game.Date,
                    TeamID = pgp.PlayerGame.Team == pgp.Game.Home ? pgp.Game.HomeTeamID : pgp.Game.AwayTeamID

                })
                .ToList();

            List<List<string>> progress = pgs
            .GroupBy(pg => pg.PlayerName)
            .Select(group => group.OrderBy(pg => pg.GameDate).First())
            .Select(pg => new List<string> { pg.PlayerName, pg.Team, pg.GameDate.ToString(), pg.GameID, pg.League, pg.TeamID, pg.PlayerID })
            .ToList();

            double completionPercent;
            if (progress.Count() > 0)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }

            List<string> titles = new List<string> { "Player", "Team", "Date Seen" };
            progress = progress.OrderByDescending(item => item[2]).ToList();

            List<string> gameIDs = progress.Select(item => item[3]).ToList();
            List<string> leagues = progress.Select(item => item[4]).ToList();
            List<string> homeIDs = progress.Select(item => item[5]).ToList();
            List<string> playerIDs = progress.Select(item => item[6]).ToList();
            progress = progress
            .Select(item => new List<string> { item[0], item[1], item[2] })
            .ToList();

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);
            int index = 2;
            List<string> awayIDs = new List<string>();
            

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class SameDay : BaseBadgeChecker
    {

        public SameDay(ApplicationDBContext dbContext) : base(dbContext){}

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<List<string>> progress = new List<List<string>>();
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            var games = _dbContext.Game
            .Where(game => userGames.Contains(game.ID))
            .ToList();
            var groupedGames = games.GroupBy(game => game.Date).Where(group => group.Count() > 1);
            foreach (var group in groupedGames)
            {
                foreach (var game in group)
                {
                    progress.Add(new List<string> { game.Date, game.Away, game.Home, game.Score, game.Location, game.GameNote });
                    gameIDs.Add(game.ID);
                    leagues.Add(game.League);
                    homeIDs.Add(game.HomeTeamID);
                    awayIDs.Add(game.AwayTeamID);
                }
            }
            double completionPercent;
            if (progress.Count() > 1)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }

            List<string> titles = new List<string> { "Date", "Away", "Home", "Score", "Location", "Note" };
            progress = progress.OrderByDescending(item => item[0]).ToList();

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);
            int index = 3;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class Favs : BaseBadgeChecker
    {

        public Favs(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            Dictionary<string, string> favTeams = _dbContext.User.Where(u => u.ID == userID).Select(u => new Dictionary<string, string> { { "MLB", u.FavMLB }, { "NBA", u.FavNBA }, { "NFL", u.FavNFL }, { "NCAAF", u.FavNCAAF }, { "NCAAB", u.FavNCAAB } }).FirstOrDefault();
            favTeams = favTeams.Where(kv => kv.Value != "None").ToDictionary(kv => kv.Key, kv => kv.Value);
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<List<string>> progress = new List<List<string>>();
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            bool foundAll = true;
            int count = 0;
            double completionPercent;
            if (!favTeams.IsNullOrEmpty())
            {
                foreach (var key in favTeams.Keys)
                {
                    Game game = _dbContext.Game.Where(g => g.League == key && userGames.Contains(g.ID) && (g.Away == favTeams[key] || g.Home == favTeams[key])).OrderBy(g => g.Date).FirstOrDefault();
                    if (game == null)
                    {
                        progress.Add(new List<string> { key, favTeams[key], "N/A" });
                        foundAll = false;
                        gameIDs.Add("");
                        leagues.Add("");
                        homeIDs.Add("");
                    }
                    else
                    {
                        progress.Add(new List<string> { key, favTeams[key], game.Date });
                        count++;
                        gameIDs.Add(game.ID);
                        leagues.Add(key);
                        homeIDs.Add(game.HomeTeamID);
                    }
                }
                completionPercent = (double)count / (double)favTeams.Keys.Count();
            }
            else
            {
                completionPercent = 0;
            }
            

            List<string> titles = new List<string> { "League", "Team", "Date Seen"};

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);
            int index = 2;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }


    public class Stat : BaseBadgeChecker
    {
        private readonly int _index;
        private readonly string _league;
        private readonly int _threshold;
        private readonly string _position;
        private readonly string _stat;

        public Stat(ApplicationDBContext dbContext, int index, string league, int threshold, string position, string stat) : base(dbContext)
        {
            _index = index;
            _league = league;
            _threshold = threshold;
            _position = position;
            _stat = stat;
        }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<PlayerGame> stats = _dbContext.PlayerGame.Where(pg => pg.League == _league && pg.Position.Contains(_position) && userGames.Contains(pg.GameID)).ToList();
            bool foundFifty = false;
            List<List<string>> progress = new List<List<string>>();
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            foreach (var playerGame in stats)
            {
                if (!playerGame.Stats.IsNullOrEmpty())
                {
                    if (int.Parse(playerGame.Stats.Split(',')[_index]) >= _threshold)
                    {
                        foundFifty = true;
                        if (getDetails)
                        {
                            List<string> player = _dbContext.Player.Where(p => p.ID == playerGame.PlayerID).Select(p => new List<string> { p.Name, p.ID}).FirstOrDefault();
                            List<string> gameInfo = _dbContext.Game.Where(g => g.ID == playerGame.GameID).Select(g => new List<string> { g.ID, g.Date, g.Home, g.HomeTeamID, g.Away, g.AwayTeamID}).FirstOrDefault();
                            List<string> playerInfo = new List<string> { player[0], gameInfo[1], playerGame.Team, gameInfo[2] != playerGame.Team ? gameInfo[2] : gameInfo[4], playerGame.Stats.Split(',')[_index], gameInfo[2] == playerGame.Team ? gameInfo[3] : gameInfo[5], gameInfo[2] != playerGame.Team ? gameInfo[5] : gameInfo[3] };
                            progress.Add(playerInfo);
                            gameIDs.Add(gameInfo[0]);
                            playerIDs.Add(player[1]);
                            leagues.Add(_league);
                        }
                        else
                        {
                            break;
                        }
                        
                    }
                }             
            }

            List<string> titles = new List<string> { "Player", "Date", "Team", "Opponent", _stat };
            progress = progress.OrderByDescending(item => int.Parse(item[4])).ToList();
            for (int i = 0; i < progress.Count(); i++)
            {
                homeIDs.Add(progress[i][5]);
                awayIDs.Add(progress[i][6]);
                progress[i] = progress[i].SkipLast(2).ToList();

            }
            List<List<string>> list = new List<List<string>>{titles};
            list.AddRange(progress);

            double completionPercent;
            if (foundFifty)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }
            int index = 1;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }

    public class Totals : BaseBadgeChecker
    {
        private readonly int _index;
        private readonly string _league;
        private readonly int _threshold;
        private readonly string _position;
        private readonly string _stat;

        public Totals(ApplicationDBContext dbContext, int index, string league, int threshold, string position, string stat) : base(dbContext)
        {
            _index = index;
            _league = league;
            _threshold = threshold;
            _position = position;
            _stat = stat;
        }

        public override (List<List<string>>, double, List<string>, int, List<string>, List<string>, List<string>, List<string>) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.Attended == "A").Select(ug => ug.GameID).ToList();
            List<PlayerGame> stats = _dbContext.PlayerGame.Where(pg => pg.League == _league && pg.Position.Contains(_position) && userGames.Contains(pg.GameID)).ToList();
            int statsCount = 0;
            List<List<string>> progress = new List<List<string>>();
            List<string> gameIDs = new List<string>();
            List<string> leagues = new List<string>();
            List<string> homeIDs = new List<string>();
            List<string> awayIDs = new List<string>();
            List<string> playerIDs = new List<string>();
            foreach (var playerGame in stats)
            {
                if (!playerGame.Stats.IsNullOrEmpty())
                {
                    if (playerGame.Stats.Split(',')[_index].Contains('-'))
                    {
                        statsCount += int.Parse(playerGame.Stats.Split(',')[_index].Substring(0, playerGame.Stats.Split(',')[_index].IndexOf('-')));
                    }
                    else
                    {
                        statsCount += int.Parse(playerGame.Stats.Split(',')[_index]);
                    }
                    
                }
            }


            List<string> titles = new List<string> { "League", _stat };
            progress.Add(new List<string> { _league, statsCount.ToString() });

            List<List<string>> list = new List<List<string>> { titles };
            list.AddRange(progress);

            double completionPercent = (double)statsCount / (double)_threshold;
            if(completionPercent > 1)
            {
                completionPercent = 1;
            }
            int index = 0;

            return (list, completionPercent, gameIDs, index, leagues, homeIDs, awayIDs, playerIDs);
        }
    }


    public class BadgeCheckerFactory
    {
        private readonly ApplicationDBContext _dbContext;

        public BadgeCheckerFactory(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IBadgeChecker GetBadgeChecker(string badgeID)
        {
            switch (badgeID)
            {
                case "1":
                    return new AllTeams(_dbContext, "MLB");
                case "2":
                    return new AllTeams(_dbContext, "NFL");
                case "3":
                    return new AllTeams(_dbContext, "NBA");
                case "4":
                    return new GameType(_dbContext, "", "Game 7");
                case "5":
                    return new Stat(_dbContext, 13, "NBA", 50, "", "PTS");
                case "6":
                    return new GameType(_dbContext, "NCAAF", "Bowl");
                case "7":
                    return new Stat(_dbContext, 5, "MLB", 10, "pitching", "K");
                case "8":
                    return new GameType(_dbContext, "NCAAB", "Men's Basketball Championship");
                case "9":
                    return new SameDay(_dbContext);
                case "10":
                    return new Favs(_dbContext);
                case "11":
                    return new Totals(_dbContext, 13, "NBA", 40474, "", "PTS");
                case "12":
                    return new HOF(_dbContext, "NBA");
                case "13":
                    return new HOF(_dbContext, "MLB");
                case "14":
                    return new MVP(_dbContext);
                default:
                    throw new ArgumentException("Invalid badge ID.");
            }
        }
    }
}
