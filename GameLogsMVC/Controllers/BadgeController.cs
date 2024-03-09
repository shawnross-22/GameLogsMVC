using GameLogsMVC.Models.DBData;
using GameLogsMVC.Models.ViewData;
using Microsoft.AspNetCore.Mvc;

namespace GameLogsMVC.Controllers
{
    public class BadgeController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDBContext _dbContext;
        private readonly BadgeCheckerFactory _badgeCheckerFactory;

        public BadgeController(ILogger<UserController> logger, ApplicationDBContext dbContext, BadgeCheckerFactory badgeCheckerFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _badgeCheckerFactory = badgeCheckerFactory;
        }
        public IActionResult Index(string userName, string badge)
        {
            BadgesView badgeView = new BadgesView();
            badgeView.UserID = userName;
            IBadgeChecker badgeChecker = _badgeCheckerFactory.GetBadgeChecker(badge);
            badgeView.Badge = _dbContext.Badge.Where(b => b.ID == badge).FirstOrDefault();
            if (badgeChecker != null)
            {
                var (progress, completion) = badgeChecker.CheckCompletion(userName, true);
                badgeView.Completion = completion;
                badgeView.Progress = progress;
            }
            return View(badgeView);
        }
    }
}
