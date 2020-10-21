using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IddaaSimuService
{
    public class UnclearedMatches
    {
        public List<List<object>> e { get; set; }
        public int eId { get; set; }
        public List<List<object>> m { get; set; }
        public string t { get; set; }
    }

    public class MarketType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }

    public class Outcome
    {
        public int EventId { get; set; }
        public int MarketId { get; set; }
        public int MarketTypeId { get; set; }
        public int OutcomeNo { get; set; }
        public string OutcomeName { get; set; }
        public double Odd { get; set; }
    }

    public class Market
    {
        public int MarketId { get; set; }
        public int MarketNo { get; set; }
        public int EventId { get; set; }
        public MarketType MarketType { get; set; }
        public int MBS { get; set; }
        public double SOV { get; set; }
        public int MarketStatus { get; set; }
        public List<Outcome> Outcomes { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
    }

    public class Event
    {
        public int EventId { get; set; }
        public int SportId { get; set; }
        public DateTime StartDate { get; set; }
        public int LeagueCode { get; set; }
        public bool HasLive { get; set; }
        public bool IsLive { get; set; }
        public List<Market> Markets { get; set; }
    }

    public class MatchData
    {
        public string Match { get; set; }
        public Event Event { get; set; }
    }


}