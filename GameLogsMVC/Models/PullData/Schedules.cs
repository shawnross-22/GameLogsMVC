namespace GameLogsMVC.Models.PullData
{
        public class BoxScoreItem
        {
            public List<Event> Events { get; set; }
        }

        public class Season
        {
            public string Slug { get; set; }
        }

        public class Event
        {
            public string ID { get; set; }
            public string Date { get; set; }
            public string Name { get; set; }
            public Season Season { get; set; }
            public List<Competition> Competitions { get; set; }
        }

        public class Competition
        {
            public string Name { get; set; }
            public bool neutralSite { get; set; }
            public List<Competitor> Competitors { get; set; }
        }

        public class Competitor
        {
            public Score? Score { get; set; }
            public CompetitorTeam Team { get; set; }  // Contains information about the team
        }

        public class Score
        {
            public string DisplayValue { get; set; }
        }

        public class CompetitorTeam
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }// The property representing the team name
                                              // Add other properties related to the team if needed
        }

}
