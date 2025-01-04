using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VollandAPI
{
    //
    // REQEUSTS
    //

    public abstract class Request
    {
        public string request_type { get; set; }
        public string ticker { get; set; }

        protected Request(string request_type, string ticker)
        {
            this.request_type = request_type ?? throw new ArgumentNullException(nameof(request_type));
            this.ticker = ticker.ToUpper() ?? throw new ArgumentNullException(nameof(ticker));
        }
    }

    public class Paradigm_Request : Request
    {
        public Paradigm_Request(string ticker) : base("paradigm_request", ticker)
        {
        }
    }

    public class ZeroDTE_Request : Request
    {
        public ZeroDTE_Request(string ticker) : base("zerodte_request", ticker)
        {
        }
    }

    public class Trend_Request : Request
    {
        public string? greek { get; set; }

        public Trend_Request(string ticker, Greek greek) : base("trend_request", ticker)
        {
            this.greek = Enum.GetName(greek) ?? throw new ArgumentNullException(nameof(greek));
        }

    }

    public class Exposure_Request : Request
    {

        public string? greek { get; set; }
        public string? kind { get; set; }
        public string[]? expirations { get; set; }

        public Exposure_Request(string ticker, Kind kind, Greek greek, List<DateTime>? expirations = null) : base("exposure_request", ticker)
        {
            this.greek = Enum.GetName(greek) ?? throw new ArgumentNullException(nameof(greek));
            this.kind = Enum.GetName(kind) ?? throw new ArgumentNullException(nameof(kind));

            if (expirations is null)
                this.expirations = new string[] { "*" }; // All expirations
            else
                this.expirations = expirations.ConvertAll(x => x.ToString("yyyy-MM-dd")).ToArray(); // Convert list of DateTimes to strings
        }

    }

}
