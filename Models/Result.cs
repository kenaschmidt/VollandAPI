using System.Data;
using VollandAPI.Helpers;

namespace VollandAPI
{
    //
    // RESULTS - User friendly reply objects
    //

    public abstract class Result
    {
        public Request_Type RequestType { get; set; }
        public string Ticker { get; set; }

        protected Result(Request_Type requestType, string ticker)
        {
            RequestType = requestType;
            Ticker = ticker;
        }

    }

    public class Trend_Result : Result
    {

        public Greek Greek { get; set; }
        public List<Trend_Point> TrendPoints { get; set; }

        public DateTime LastModifiedUTC { get; set; }

        public Trend_Result(string ticker, string greek, Trend_Data? trendData)
            : base(Request_Type.trend_request, ticker)
        {
            Greek = Enum.Parse<Greek>(greek);

            TrendPoints = new List<Trend_Point>();

            if (trendData == null || trendData.trend == null)
                throw new ArgumentException("TrendData is null)");

            foreach (var item in trendData.trend)
                TrendPoints.Add(item.ToTrendPoint());

            if (trendData.lastModified != null)
                LastModifiedUTC = DateTime.ParseExact(trendData.lastModified, "yyyy-MM-dd HH:mm:ss", null);
        }
    }

    public class Trend_Point
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }

        public Trend_Point(DateTime date, double value)
        {
            Date = date;
            Value = value;
        }
    }

    public class Paradigm_Result : Result
    {
        //public Paradigm Paradigm { get; set; }
        public Paradigm Paradigm { get; set; }
        public double? Target { get; set; }
        public double[]? LIS { get; set; }
        public DateTime LastUpdatedUTC { get; set; }

        public Paradigm_Result(string ticker, string paradigm, double? target, double[]? lis, string lastUpdated)
            : base(Request_Type.paradigm_request, ticker)
        {
            //Paradigm = Enum.Parse<Paradigm>(paradigm);
            Paradigm = paradigm.ToEnumByDescription<Paradigm>();

            Target = target;
            this.LIS = lis;
            LastUpdatedUTC = DateTime.ParseExact(lastUpdated, "yyyy-MM-dd HH:mm:ss", null);
        }
    }

    public class ZeroDTE_Result : Result
    {
        public double? DealerPremium { get; set; }
        public long? OptionVolume { get; set; }
        public long? AggregateCharm { get; set; }

        public ZeroDTE_Result(string ticker, double? dealerPremium, long? optionVolume, long? aggregateCharm)
            : base(Request_Type.zerodte_request, ticker)
        {
            DealerPremium = dealerPremium;
            OptionVolume = optionVolume;
            AggregateCharm = aggregateCharm;
        }
    }

    public class Exposure_Result : Result
    {
        public Greek Greek { get; set; }
        public Kind Kind { get; set; }
        public double SpotPrice { get; set; }

        public DateTime LastUpdatedUTC { get; set; }

        public List<DateTime> Expirations { get; set; } = new List<DateTime>();

        public List<Exposure_Point> Exposures { get; set; } = new List<Exposure_Point>();

        public Exposure_Result(string ticker, string greek, string kind, string[] expirations, Exposure_Data exposures, double currentPrice) : base(Request_Type.exposure_request, ticker)
        {
            Greek = Enum.Parse<Greek>(greek);
            Kind = Enum.Parse<Kind>(kind);

            foreach (var item in expirations)
            {
                if (item == "*")
                    break;

                Expirations.Add(DateTime.ParseExact(item, "yyyy-MM-dd", null));
            }

            Exposures = exposures.ToExposurePoints();
            SpotPrice = currentPrice;

            if (exposures.lastModified != null)
                LastUpdatedUTC = DateTime.ParseExact(exposures.lastModified, "yyyy-MM-dd HH:mm:ss", null);
        }

        public Exposure_Result(string ticker) : base(Request_Type.exposure_request, ticker)
        {
        }

        public static Exposure_Result TestData(double minStrike, double maxStrike, double step, double maxExposure)
        {

            var ret = new Exposure_Result("TEST");
            ret.Greek = Greek.delta;
            ret.Kind = Kind.both;
            ret.SpotPrice = 456.50;
            ret.LastUpdatedUTC = DateTime.UtcNow;
            ret.Expirations = new List<DateTime>(new DateTime[] {
            DateTime.Today,
            DateTime.Today.AddDays(1)
            });


            Random rnd = new Random();
            for (double s = minStrike; s <= maxStrike; s += step)
            {
                int sign = rnd.Next(0, 2) == 0 ? 1 : -1;
                ret.Exposures.Add(new Exposure_Point(s, rnd.NextDouble() * rnd.NextDouble() * maxExposure * sign));
            }

            return ret;
        }

        /// <summary>
        /// Returns a new copy of the object
        /// </summary>
        /// <returns></returns>
        public Exposure_Result Copy()
        {
            var ret = new Exposure_Result(this.Ticker);
            ret.Greek = this.Greek;
            ret.Kind = this.Kind;
            ret.SpotPrice = this.SpotPrice;
            ret.LastUpdatedUTC = this.LastUpdatedUTC;
            ret.Expirations = new List<DateTime>(this.Expirations);
            foreach (var e in this.Exposures)
                ret.Exposures.Add(new Exposure_Point(e.Strike, e.Exposure));

            return ret;
        }

        public override string ToString()
        {
            string exp = Expirations.Count == 0 ? "All Expiries" : (Expirations.DateRangeString() ?? "ERR");

            return $"{Ticker} {Kind.ToString()} {Greek.ToString()} Exp {exp}";
        }
    }

    public class Exposure_Point
    {
        public double Strike { get; set; }
        public double Exposure { get; set; }

        public Exposure_Point(double strike, double exposure)
        {
            Strike = strike;
            Exposure = exposure;
        }

        public static Exposure_Point Add(Exposure_Point first, Exposure_Point second)
        {
            if (first.Strike != second.Strike)
            {
                throw new Exception();
            }

            return new Exposure_Point(first.Strike, (first.Exposure + second.Exposure));
        }
    }
}
