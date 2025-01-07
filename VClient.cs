using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using VollandAPI.Helpers;

namespace VollandAPI
{
    public class VClient
    {

        private HttpClient? httpClient { get; set; }

        private Uri BaseAddress { get; } = new Uri(@"https://prod-api.vol.land/api/v1/volland");

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
        /// <param name="tokensRemaining">Number of tokens remaining.  Does not update value if another client has already been initialized.</param>
        public VClient(string apiKey, int tokensRemaining)
        {
            this.API_Key = apiKey;

            if (_tokensRemaining == null)
            {
                TokensRemaining = tokensRemaining;
            }

            _initHHPClient();
        }

        /// <summary>
        /// Initializes the HTTP client with the base URL and API Key header
        /// </summary>
        private void _initHHPClient()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", $"{API_Key}");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.Timeout = new TimeSpan(0, 0, 5);
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

        private async Task<TResult?> SendRequestAsync<TRequest, TResult>(Request_Package<TRequest> requestPackage) where TResult : Result where TRequest : Request
        {
            if (httpClient == null)
                throw new NullReferenceException("ERROR: HTTP Client failed to initialize");

            if (requestPackage == null)
                throw new ArgumentException(nameof(requestPackage));

            try
            {
                // Serialize the request from the package

                string request = JsonSerializer.Serialize(requestPackage);
                var content = new StringContent(request, System.Text.Encoding.UTF8, "application/json");

                //
                //  TRANSMIT REQUEST -- TOKEN CHARGED HERE
                //

                if (TokensRemaining <= 0)
                    throw new Exception("System ERROR: No API tokens remain");
                else
                    TokensRemaining -= 1;

                HttpResponseMessage httpResponse = await httpClient.PostAsync(BaseAddress, content);

                //
                // ---------------------------------------
                //

                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read the replay as a string
                    string replyString = await httpResponse.Content.ReadAsStringAsync();

                    // Convert the reply string to an array of JSON objects
                    JsonArray? responseGroup = JsonSerializer.Deserialize<JsonArray>(replyString);

                    if (responseGroup == null)
                        throw new NullReferenceException("Response group was null after deserializing");

                    // Convert the single response to a local Response object (since we only have one request at a time)
                    Response? response = JsonSerializer.Deserialize<Response>(responseGroup?.SingleOrDefault());

                    if (response == null || response.request_type == null)
                        throw new NullReferenceException("Response was null after deserializing");

                    // Get the request type from the Response object
                    Request_Type requestType = Enum.Parse<Request_Type>(response.request_type);

                    // Process the response based on the request type
                    switch (requestType)
                    {
                        case Request_Type.exposure_request:
                            return ProcessResponse<Exposure_Response, Exposure_Result>(responseGroup?.SingleOrDefault().Deserialize<Exposure_Response>()) as TResult;
                        case Request_Type.trend_request:
                            return ProcessResponse<Trend_Response, Trend_Result>(responseGroup?.SingleOrDefault().Deserialize<Trend_Response>()) as TResult;
                        case Request_Type.paradigm_request:
                            return ProcessResponse<Paradigm_Response, Paradigm_Result>(responseGroup?.SingleOrDefault().Deserialize<Paradigm_Response>()) as TResult;
                        case Request_Type.zerodte_request:
                            return ProcessResponse<ZeroDTE_Response, ZeroDTE_Result>(responseGroup?.SingleOrDefault().Deserialize<ZeroDTE_Response>()) as TResult;
                        default:
                            throw new Exception("System ERROR: Invalid request type returned");
                    }

                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException($"API ERROR 401: Missing or invalid API key, no Live subscription");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"API ERROR 400: Request JSON is not in correct format.");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    UpdateTokensRemaining(0);
                    throw new HttpRequestException($"API ERROR 429: You don't have any API credits left.");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    throw new TimeoutException("ERROR Request timeout.");
                }
                else
                {
                    throw new HttpRequestException($"System ERROR: HTTP Client request returned as unsuccessful: {httpResponse.ReasonPhrase}: {request}");
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        private TResult ProcessResponse<TResponse, TResult>(TResponse? response) where TResponse : Response where TResult : Result
        {
            try
            {
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

        /// <summary>
        /// This request retrieves dealer exposure data for the given ticker and greek.
        /// </summary>
        /// <param name="ticker">Ticker</param>
        /// <param name="kind">Put, Call, or Both</param>
        /// <param name="greek">Delta, Gamma, etc.</param>
        /// <param name="expirations">List of valid expirations or null to retrieve all.</param>
        /// <returns>A single result object containing the data requested.  Note that multiple expiration requests return the combined data, not individual expirations.</returns>
        public async Task<Exposure_Result?> RequestExposureAsync(string ticker, Kind kind, Greek greek, List<DateTime>? expirations = null)
        {
            var request = new Exposure_Request(ticker, kind, greek, expirations);

            var requestPackage = request.Package();

            return await SendRequestAsync<Exposure_Request, Exposure_Result>(requestPackage);
        }

        public async Task<Trend_Result?> RequestTrendAsync(string ticker, Greek greek)
        {
            var request = new Trend_Request(ticker, greek);

            var requestPackage = request.Package();

            return await SendRequestAsync<Trend_Request, Trend_Result>(requestPackage);
        }

        public async Task<ZeroDTE_Result?> RequestZeroDTEAsync(string ticker)
        {
            var request = new ZeroDTE_Request(ticker);

            var requestPackage = request.Package();

            return await SendRequestAsync<ZeroDTE_Request, ZeroDTE_Result>(requestPackage);
        }

        public async Task<Paradigm_Result?> RequestParadigmAsync(string ticker)
        {
            var request = new Paradigm_Request(ticker);

            var requestPackage = request.Package();

            return await SendRequestAsync<Paradigm_Request, Paradigm_Result>(requestPackage);
        }

        #endregion

    }
}
