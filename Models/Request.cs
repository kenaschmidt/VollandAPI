using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VollandAPI.Helpers;

namespace VollandAPI
{
    //
    // REQUESTS
    //

    public abstract class Request
    {

        public string request_type { get; set; }
        public string ticker { get; set; }

        protected Request(Request_Type request_type, string ticker)
        {
            this.request_type = request_type.ToString();
            this.ticker = ticker.ToUpper() ?? throw new ArgumentNullException(nameof(ticker));
        }

        public abstract override string ToString();

        internal const int resultTimeoutMs = 5000;
        internal int timeoutCount;
    }

    public class Paradigm_Request : Request
    {
        public Paradigm_Request(string ticker) : base(Request_Type.paradigm_request, ticker)
        {
        }

        public override string ToString()
        {
            return $"{ticker} {request_type}";
        }

        internal Paradigm_Result? Result { get; private set; }

        internal void SetResult(Paradigm_Result result)
        {
            this.Result = result;
        }

        internal async Task<Paradigm_Result?> Wait()
        {
            while (Result == null && timeoutCount <= resultTimeoutMs)
            {
                await Task.Delay(500);
                timeoutCount += 500;
            }
            return Result;
        }

    }

    public class ZeroDTE_Request : Request
    {
        public ZeroDTE_Request(string ticker) : base(Request_Type.zerodte_request, ticker)
        {
        }

        public override string ToString()
        {
            return $"{ticker} {request_type}";
        }

        internal ZeroDTE_Result? Result { get; private set; }

        internal void SetResult(ZeroDTE_Result result)
        {
            this.Result = result;
        }

        internal async Task<ZeroDTE_Result?> Wait()
        {
            while (Result == null && timeoutCount <= resultTimeoutMs)
            {
                await Task.Delay(500);
                timeoutCount += 500;
            }
            return Result;
        }
    }

    public class Trend_Request : Request
    {
        public string? greek { get; set; }

        public Trend_Request(string ticker, Greek greek) : base(Request_Type.trend_request, ticker)
        {
            this.greek = Enum.GetName(greek) ?? throw new ArgumentNullException(nameof(greek));
        }

        public override string ToString()
        {
            return $"{ticker} {request_type} {greek}";
        }

        internal Trend_Result? Result { get; private set; }

        internal void SetResult(Trend_Result result)
        {
            this.Result = result;
        }

        internal async Task<Trend_Result?> Wait()
        {
            while (Result == null && timeoutCount <= resultTimeoutMs)
            {
                await Task.Delay(500);
                timeoutCount += 500;
            }
            return Result;
        }
    }

    public class Exposure_Request : Request
    {

        public string? greek { get; set; }
        public string? kind { get; set; }
        public string[]? expirations { get; set; }

        public Exposure_Request(string ticker, Kind kind, Greek greek, List<DateTime>? expirations = null) : base(Request_Type.exposure_request, ticker)
        {
            this.greek = Enum.GetName(greek) ?? throw new ArgumentNullException(nameof(greek));
            this.kind = Enum.GetName(kind) ?? throw new ArgumentNullException(nameof(kind));

            if (expirations is null)
                this.expirations = new string[] { "*" }; // All expirations
            else
                this.expirations = expirations.ConvertAll(x => x.ToString("yyyy-MM-dd")).ToArray(); // Convert list of DateTimes to strings
        }

        internal Exposure_Result? Result { get; private set; }

        internal void SetResult(Exposure_Result result)
        {
            this.Result = result;
        }

        internal async Task<Exposure_Result?> Wait()
        {
            while (Result == null && timeoutCount <= resultTimeoutMs)
            {
                await Task.Delay(500);
                timeoutCount += 500;
            }
            return Result;
        }

        public override string ToString()
        {
            return $"{ticker} {request_type} {greek} {kind} {(expirations?[0] == "*" ? "ALL" : expirations?.Length)}";
        }

    }

    /// <summary>
    /// Packaging class required to send a request, as the API accepts an array only.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    public class Request_Package<TRequest> where TRequest : Request
    {
        internal List<TRequest> _requests = new List<TRequest>();

        public TRequest[] requests => _requests.ToArray();

        internal int RequestCount => _requests.Count;

        public Request_Package()
        {
        }

        public void AddRequest(TRequest request)
        {
            if (request != null)
                _requests.Add(request);
        }
    }
}
