using GameLogsMVC.Models.DBData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        (List<List<string>>, double) CheckCompletion(string userID, bool getDetails);
        ApplicationDBContext DbContext { get; set; }
    }

    public abstract class BaseBadgeChecker : IBadgeChecker
    {
        protected ApplicationDBContext _dbContext;

        public BaseBadgeChecker(ApplicationDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public abstract (List<List<string>>, double) CheckCompletion(string userID, bool getDetails);

        // Implement the DbContext property
        public ApplicationDBContext DbContext
        {
            get { return _dbContext; }
            set { _dbContext = value; }
        }
    }

    public class AllMLB : BaseBadgeChecker
    {
        public AllMLB(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.League == "MLB").Select(ug => ug.GameID).ToList();
            List<string> homeTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Home).Distinct().ToList();
            List<string> awayTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Away).Distinct().ToList();
            List<string> allUniqueTeams = homeTeams.Concat(awayTeams).Distinct().ToList();
            List<string> allTeams = _dbContext.Team.Where(t => t.League == "MLB").Select(t => t.Name).ToList();
            List<List<string>> progress = new List<List<string>> { new List<string> { "Team", "Seen", "First Time Seen"} };
            if (getDetails) {
                foreach (var team in allTeams)
                {
                    string seen = allUniqueTeams.Contains(team).ToString();
                    var firstVisitGame = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.Away == team || g.Home == team)).OrderBy(g => g.Date).FirstOrDefault();
                    string firstVisit = firstVisitGame != null ? firstVisitGame.Date.ToString() : "N/A";
                    List<string> teamInfo = new List<string> { team, seen, firstVisit };
                    progress.Add(teamInfo);
                }
            }           
            double completionPercent = (double)allUniqueTeams.Count() / (double)allTeams.Count();
            return (progress, completionPercent);
        }
    }


    public class AllNFL : BaseBadgeChecker
    {
        public AllNFL(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.League == "NFL").Select(ug => ug.GameID).ToList();
            List<string> homeTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Home).Distinct().ToList();
            List<string> awayTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Away).Distinct().ToList();
            List<string> allUniqueTeams = homeTeams.Concat(awayTeams).Distinct().ToList();

            List<string> allTeams = _dbContext.Team.Where(t => t.League == "NFL").Select(t => t.Name).ToList();
            List<List<string>> progress = new List<List<string>> { new List<string> { "Team", "Seen", "First Time Seen" } };
            if (getDetails)
            {
                foreach (var team in allTeams)
                {
                    string seen = allUniqueTeams.Contains(team).ToString();
                    var firstVisitGame = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.Away == team || g.Home == team)).OrderBy(g => g.Date).FirstOrDefault();
                    string firstVisit = firstVisitGame != null ? firstVisitGame.Date.ToString() : "N/A";
                    List<string> teamInfo = new List<string> { team, seen, firstVisit };
                    progress.Add(teamInfo);
                }
            }
            double completionPercent = (double)allUniqueTeams.Count() / (double)allTeams.Count();
            return (progress, completionPercent);
        }
    }

    public class AllNBA : BaseBadgeChecker
    {
        public AllNBA(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID && ug.League == "NBA").Select(ug => ug.GameID).ToList();
            List<string> homeTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Home).Distinct().ToList();
            List<string> awayTeams = _dbContext.Game.Where(g => userGames.Contains(g.ID)).Select(g => g.Away).Distinct().ToList();
            List<string> allUniqueTeams = homeTeams.Concat(awayTeams).Distinct().ToList();
            List<string> allTeams = _dbContext.Team.Where(t => t.League == "NBA").Select(t => t.Name).ToList();
            List<List<string>> progress = new List<List<string>> { new List<string> { "Team", "Seen", "First Time Seen" } };
            if (getDetails)
            {
                foreach (var team in allTeams)
                {
                    string seen = allUniqueTeams.Contains(team).ToString();
                    var firstVisitGame = _dbContext.Game.Where(g => userGames.Contains(g.ID) && (g.Away == team || g.Home == team)).OrderBy(g => g.Date).FirstOrDefault();
                    string firstVisit = firstVisitGame != null ? firstVisitGame.Date.ToString() : "N/A";
                    List<string> teamInfo = new List<string> { team, seen, firstVisit };
                    progress.Add(teamInfo);
                }
            }
            double completionPercent = (double)allUniqueTeams.Count() / (double)allTeams.Count();
            return (progress, completionPercent);
        }
    }

    public class Game7 : BaseBadgeChecker
    {
        public Game7(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID).Select(ug => ug.GameID).ToList();
            List<List<string>> progress = new List<List<string>> { new List<string> { "Date", "Away", "Home", "Score", "Location", "Note"} };
            List<List<string>> gameID = _dbContext.Game.Where(g => g.GameNote.Contains("Game 7") && userGames.Contains(g.ID)).Select(g => new List<string> { g.Date, g.Away, g.Home, g.Score, g.Location, g.GameNote }).ToList();
            if (getDetails)
            {
                progress.AddRange(gameID);
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
            
            return (progress, completionPercent);
        }
    }

    public class FiftyPoint : BaseBadgeChecker
    {
        public FiftyPoint(ApplicationDBContext dbContext) : base(dbContext) { }

        public override (List<List<string>>, double) CheckCompletion(string userID, bool getDetails)
        {
            List<string> userGames = _dbContext.UserGame.Where(ug => ug.UserID == userID).Select(ug => ug.GameID).ToList();
            List<PlayerGame> stats = _dbContext.PlayerGame.Where(pg => pg.League == "NBA" && userGames.Contains(pg.GameID)).ToList();
            bool foundFifty = false;
            List<List<string>> progress = new List<List<string>>{ new List<string> { "Player", "Date", "Team", "Opponent", "Points" } };
            foreach (var playerGame in stats)
            {
                if (!playerGame.Stats.IsNullOrEmpty())
                {
                    if (int.Parse(playerGame.Stats.Split(',')[13]) >= 50)
                    {
                        foundFifty = true;
                        if (getDetails)
                        {
                            string playerName = _dbContext.Player.Where(p => p.ID == playerGame.PlayerID).Select(p => p.Name).FirstOrDefault();
                            List<string> gameInfo = _dbContext.Game.Where(g => g.ID == playerGame.GameID).Select(g => new List<string> { g.Date, g.Away != playerGame.Team ? g.Away : g.Home}).FirstOrDefault();
                            List<string> playerInfo = new List<string> { playerName, gameInfo[0], playerGame.Team, gameInfo[1], playerGame.Stats.Split(',')[13] };
                            progress.Add(playerInfo);
                        }
                        else
                        {
                            break;
                        }
                        
                    }
                }             
            }
            double completionPercent;
            if (foundFifty)
            {
                completionPercent = 1.0;
            }
            else
            {
                completionPercent = 0.0;
            }

            return (progress, completionPercent);
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
                    return new AllMLB(_dbContext);
                case "2":
                    return new AllNFL(_dbContext);
                case "3":
                    return new AllNBA(_dbContext);
                case "4":
                    return new Game7(_dbContext);
                case "5":
                    return new FiftyPoint(_dbContext);
                default:
                    throw new ArgumentException("Invalid badge ID.");
            }
        }
    }
}
