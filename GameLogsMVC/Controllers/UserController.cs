using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.ViewData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLogsMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDBContext _dbContext;

        public UserController(ILogger<UserController> logger, ApplicationDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task<IActionResult> Index(string userName)
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
            .Where(joined => joined.UserGame.UserID == userView.ID && joined.Game.Date.Substring(0, 4) == DateTime.Now.Year.ToString())
            .CountAsync();
            return View(userView);
        }

        public IActionResult Diary()
        {
            DiaryView diaryView = new DiaryView();
            diaryView.MLBGames = new List<Game>();
            diaryView.NBAGames = new List<Game>();
            diaryView.NCAAFGames = new List<Game>();
            diaryView.NFLGames = new List<Game>();
            string userId = Request.Cookies["userID"];
            List<string> userGames = _dbContext.UserGame
                .Where(UserGame => UserGame.UserID == userId)
                .Select(UserGame => UserGame.GameID)
                .ToList();

            for (int i = 0; i < userGames.Count(); i++)
            {
                Game userGame = _dbContext.Game.Where(Game => Game.ID == userGames[i]).FirstOrDefault();
                switch (userGame.League)
                {
                    case "MLB":
                        diaryView.MLBGames.Add(userGame);
                        break;
                    case "NBA":
                        diaryView.NBAGames.Add(userGame);
                        break;
                    case "NFL":
                        diaryView.NFLGames.Add(userGame);
                        break;
                    case "NCAAF":
                        diaryView.NCAAFGames.Add(userGame);
                        break;
                }
            }
            diaryView.MLBGames = diaryView.MLBGames.OrderByDescending(g => g.Date).ToList();
            diaryView.NBAGames = diaryView.NBAGames.OrderByDescending(g => g.Date).ToList();
            diaryView.NFLGames = diaryView.NFLGames.OrderByDescending(g => g.Date).ToList();
            diaryView.NCAAFGames = diaryView.NCAAFGames.OrderByDescending(g => g.Date).ToList();
            return View(diaryView);
        }
    }
}
