using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using VollandAPI.Helpers;

namespace VollandAPI
{
    public class VClient
    {

        private HttpClient? httpClient { get; set; }

        private readonly string API_Key;

        private static int? _tokensRemaining { get; set; } = null;
        public static int TokensRemaining
        {
            get
            {
                if (_tokensRemaining.HasValue) return _tokensRemaining.Value;
                else return 0;
            }
            set
            {
                _tokensRemaining = Math.Max(value, 0);
            }
        }


        #region Construction and Initialization

        /// <summary>
        /// Initializes as new instance of the Volland API Client.
        /// </summary>
        /// <param name="apiKey">Volland API Key</param>
        /// <param name="tokenRemaining">Number of tokens remaining.  Does not update value if another client has already been initialized.</param>
        public VClient(string apiKey, int tokenRemaining)
        {
            this.API_Key = apiKey;

            if (_tokensRemaining == null)
            {
                TokensRemaining = tokenRemaining;
            }

            _initHHPClient();
        }

        /// <summary>
        /// Initializes the HTTP client with the base URL and API Key header
        /// </summary>
        private void _initHHPClient()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(@"https://prod-api.vol.land/api/v1/volland");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"X-API-KEY: {API_Key}");
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
        }

        /// <summary>
        /// Updates the number of API tokens the user has.  This is a static value used by all instances of the VClient.
        /// </summary>
        /// <param name="tokensRemaining"></param>
        public static void UpdateTokensRemaining(int tokensRemaining)
        {
            TokensRemaining = tokensRemaining;
        }

        #endregion

        #region Request Handling

        private async Task<TResult?> SendRequestAsync<TResult>(string request) where TResult : class
        {
            if (httpClient == null)
                throw new NullReferenceException("ERROR: HTTP Client failed to initialize");

            try
            {
                TokensRemaining -= 1;

                HttpResponseMessage httpResponse = await httpClient.GetAsync(request);

                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read the response and process the JSON
                    string replyString = await httpResponse.Content.ReadAsStringAsync();

                    Request_Type requestType = (JsonSerializer.Deserialize<Result>(replyString))?.RequestType ?? Request_Type.None;

                    switch (requestType)
                    {
                        case Request_Type.exposure_request:
                            return ProcessResponse<Exposure_Response, Exposure_Result>(replyString) as TResult;
                        case Request_Type.trend_request:
                            return ProcessResponse<Trend_Response, Trend_Result>(replyString) as TResult;
                        case Request_Type.paradigm_request:
                            return ProcessResponse<Paradigm_Response, Paradigm_Result>(replyString) as TResult;
                        case Request_Type.zerodte_request:
                            return ProcessResponse<ZeroDTE_Response, ZeroDTE_Result>(replyString) as TResult;
                        default:
                            throw new Exception("System ERROR: Invalid request type returned");
                    }

                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new Exception($"API ERROR 401: Missing or invalid API key, no Live subscription");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new Exception($"API ERROR 400: Request JSON is not in correct format.");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    UpdateTokensRemaining(0);
                    throw new Exception($"API ERROR 429: You don't have any API credits left.");
                }
                else
                {
                    throw new Exception($"System ERROR: HTTP Client request returned as unsuccessful: {httpResponse.ReasonPhrase}: {request}");
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private TResult ProcessResponse<TResponse, TResult>(string? httpReplyString) where TResponse : Response where TResult : Result
        {
            if (httpReplyString == null) throw new Exception("System ERROR: Null response object");

            try
            {
                TResponse? response = JsonSerializer.Deserialize<TResponse>(httpReplyString);

                if (response == null)
                    throw new NullReferenceException("System ERROR: Response conversion resulted in null value");

                return response.ToResult<TResult>();

            }
            catch (JsonException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Public Request Methods

        public async Task<Exposure_Result?> RequestExposureAsync(string ticker, Kind kind, Greek greek, List<DateTime> expirations)
        {
            var request = new Exposure_Request(ticker, kind, greek, expirations);
            return await SendRequestAsync<Exposure_Result>(request.ToJsonString());
        }

        public async Task<Trend_Result?> RequestTrendAsync(string ticker, Greek greek)
        {
            var request = new Trend_Request(ticker, greek);
            return await SendRequestAsync<Trend_Result>(request.ToJsonString());
        }

        public async Task<ZeroDTE_Result?> RequestZeroDTEAsync(string ticker)
        {
            var request = new ZeroDTE_Request(ticker);
            return await SendRequestAsync<ZeroDTE_Result>(request.ToJsonString());
        }

        public async Task<Paradigm_Result?> RequestParadigmAsync(string ticker)
        {
            var request = new Paradigm_Request(ticker);
            return await SendRequestAsync<Paradigm_Result>(request.ToJsonString());
        }

        #endregion

    }
}
