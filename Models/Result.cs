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

        public DateTime LastModified { get; set; }

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
                LastModified = DateTime.ParseExact(trendData.lastModified, "yyyy-MM-dd HH:mm:ss", null);
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
        public Paradigm Paradigm { get; set; }
        public double? Target { get; set; }
        public double? LIS { get; set; }
        public DateTime LastUpdated { get; set; }

        public Paradigm_Result(string ticker, string paradigm, double? target, double? LIS, string lastUpdated)
            : base(Request_Type.paradigm_request, ticker)
        {
            Paradigm = Enum.Parse<Paradigm>(paradigm);
            Target = target;
            this.LIS = LIS;
            LastUpdated = DateTime.ParseExact(lastUpdated, "yyyy-MM-dd HH:mm:ss", null);
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

        public DateTime LastUpdated { get; set; }

        public List<DateTime> Expirations { get; set; } = new List<DateTime>();

        public List<Exposure_Point> Exposures { get; set; }

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
                LastUpdated = DateTime.ParseExact(exposures.lastModified, "yyyy-MM-dd HH:mm:ss", null);
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
    }
}
