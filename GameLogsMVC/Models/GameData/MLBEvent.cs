using GameLogsMVC.Models.PullData;
using Microsoft.AspNetCore.Authentication;

namespace GameLogsMVC.Models.GameData
{
    public class Teams
    {
        public Team Team { get; set; }
        public List<TeamStatistic> Statistics { get; set; }
    }

    public class Team
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public string Abbreviation { get; set; }
        public string Logo { get; set; }
    }

    public class GameInfo
    {
        public Venue Venue { get; set; }
        public int Attendance { get; set; }
        public string GameDuration { get; set; }
    }
    public class Venue
    {
        public string FullName { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
    }

    public class TeamStatistic
    {
        public string DisplayValue { get; set; }
        public string Label { get; set; }
        public List<TeamStat> Stats { get; set; }
    }

    public class TeamStat
    {
        public string ShortDisplayName { get; set; }
        public string Abbreviation { get; set; }
        public string DisplayValue { get; set; }
    }


    public class PlayerStat
    {
        public string? Type { get; set; }
        public string Name { get; set; }
        public List<string> Labels { get; set; }
        public List<PlayerAthlete> Athletes { get; set; }
        public List<string> Totals { get; set; }
    }

    public class PlayerAthlete
    {
        public Athlete Athlete { get; set; }
        public Position Position { get; set; }
        public List<Position> Positions { get; set; }
        public bool Starter { get; set; }
        public int BatOrder { get; set; }
        public List<string> Stats { get; set; }
    }

    public class Athlete
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public string Jersey { get; set; }
    }

    public class Position
    {
        public string Abbreviation { get; set; } 
    }

    public class Period
    {
        public string Type { get; set; }
        public int Number { get; set; }
    }

    public class Clock
    {
        public string DisplayValue { get; set; }
    }

    public class Player
    {
        public Team Team { get; set; }
        public List<PlayerStat> Statistics { get; set; }

    }

    public class BoxScore
    {
        public List<Teams> Teams { get; set; }
        public List<Player> Players { get; set; }
    }

    public class Competitors
    {
        public string Score { get; set; }
        public string HomeAway { get; set; }
        public List<LineScore> LineScores { get; set; }
        public List<Record> Record { get; set; }
        public string Hits { get; set; }
        public string Errors { get; set; }
    }

    public class LineScore
    {
        public string DisplayValue { get; set; }   
    }

    public class Record
    {
        public string DisplayValue { get; set; } 
    }

    public class Competitions
    {
        public List<Competitors> Competitors { get; set;}
        public string Date { get; set; }
        public bool NeutralSite { get; set; }
    }

    public class Type 
    { 
        public string Abbreviation { get; set; }
    }



    public class Header
    {
        public string ID { get; set; }
        public List<Competitions> Competitions { get; set; }
        public League League { get; set; }
        public string GameNote { get; set; }
    }

    public class League
    {
        public string Abbreviation { get; set; }
    }

    public class ScoringPlay
    {
        public string Text { get; set; }
        public Type Type { get; set; }
        public Period Period { get; set; }
        public Clock Clock { get; set; }
        public Team Team { get; set; }
        public string AwayScore { get; set; }
        public string HomeScore { get; set; }
    }

    public class Previous
    {
        public List<Play> Plays { get; set; }
    }

    public class Drive
    {
        public List<Previous> Previous { get; set; }
    }

    public class WinProbability
    {
        public double HomeWinPercentage { get; set; }
        public string PlayID { get; set; }
    }

    public class Play
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public Period Period { get; set; }   
        public Clock Clock { get; set; }
        public int AwayScore { get; set; }
        public int HomeScore { get; set; }
        public int Outs { get; set; }
    }

    public class GameEvent
    {
        public BoxScore Boxscore { get; set; }
        public GameInfo GameInfo { get; set; }
        public Header Header { get; set; }
        public List<ScoringPlay> ScoringPlays { get; set; }
        public Drive Drives { get; set; }
        public List<WinProbability> WinProbability { get; set; }
        public List<Play> Plays { get; set; }
    }
}
