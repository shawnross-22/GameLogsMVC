using GameLogsMVC.Models.DBData;
using Microsoft.EntityFrameworkCore;

namespace GameLogsMVC.Models.ViewData
{
    public class BadgesView
    {
        public string UserID { get; set; }
        public double Completion { get; set; }
        public Badge Badge { get; set; }
        public List<List<string>> Progress { get; set; }

    }
}
