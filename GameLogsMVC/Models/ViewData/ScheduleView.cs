namespace GameLogsMVC.Models.ViewData
{
    public class FullScheduleView
    {
        public ScheduleView regularSchedule { get; set; }
        public ScheduleView? postSchedule { get; set; }
        public ScheduleView? playInSchedule { get; set; }
        public string League { get; set; }
        public string Team { get; set; }
        public string Logo { get; set; }
        public string ID { get; set; }
        public string Dates { get; set; }
    }

    public class ScheduleView
    {
        public List<Result> Results { get; set; }

        public ScheduleView()
        {
            Results = new List<Result>();
        }
    }

    public class Result
    {
        public string id { get; set; }
        public string date { get; set; }
        public string opponent { get; set; }
        public string score { get; set; }
        public bool? attended { get; set; }
    }
}
