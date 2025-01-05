using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VollandAPI
{

    //
    // RESPONSES
    //

    public class Response_Group
    {
        public Response[]? responses;
    }

    public class Response
    {
        public string? request_type { get; set; }
        public string? ticker { get; set; }
    }

    public class Trend_Response : Response
    {
        public string? greek { get; set; }
        public Trend_Data? data { get; set; }
    }

    public class Paradigm_Response : Response
    {
        public Paradigm_Data? data { get; set; }
    }

    public class ZeroDTE_Response : Response
    {
        public ZeroDTE_Data? data { get; set; }
    }

    public class Exposure_Response : Response
    {
        public string? greek { get; set; }
        public string? kind { get; set; }
        public string[]? expirations { get; set; }
        public Exposure_Data? data { get; set; }
    }

    //
    // RESPONSE DATA OBJECTS
    //

    public abstract class Data
    {
        public virtual string? lastModified { get; set; }
    }

    public class Paradigm_Data : Data
    {
        public string? paradigm { get; set; }
        public double? target { get; set; }
        public double[]? lis { get; set; }

        [JsonPropertyName("last-modified")]
        public override string? lastModified { get; set; }
    }

    public class ZeroDTE_Data : Data
    {
        public double? dealer_premium { get; set; }
        public long? option_volume { get; set; }
        public long? zerodte_agg_charm { get; set; }

    }

    public class Exposure_Data : Data
    {
        public string[]? strikes { get; set; }
        public double[]? exposures { get; set; }
        public double? currentPrice { get; set; }
    }

    public class Trend_Data : Data
    {
        public Trend_Data_Point[]? trend { get; set; }
    }

    public class Trend_Data_Point
    {
        public string? x { get; set; }
        public double? y { get; set; }
    }

}
