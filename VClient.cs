using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Windows.Markup;
using VollandAPI.Helpers;

namespace VollandAPI
{
    public class VClientSettings
    {
        public int HttpClientTimeoutSeconds { get; set; } = 60;
        internal TimeSpan _httpClientTimeout => new TimeSpan(0, 0, HttpClientTimeoutSeconds);

        public int ApiRequestsPerSecond { get; set; } = 2;
        internal double _apiRequestsPerSecond => Convert.ToDouble(ApiRequestsPerSecond);

        public int ApiRequestsPerTransmission { get; set; } = 20;
    }

    public class VClient
    {
        /// <summary>
        /// Finalizer
        /// </summary>
        ~VClient()
        {
            httpClient?.Dispose();
        }

        /// <summary>
        /// Static settings for the Client which can be modified by the user
        /// </summary>
        public static VClientSettings Settings { get; }

        /// <summary>
        /// The HTTP client
        /// </summary>
        private HttpClient? httpClient { get; set; }

        /// <summary>
        /// The Volland API base address
        /// </summary>
        public static Uri BaseAddress { get; } = new Uri(@"https://prod-api.vol.land/api/v1/volland");

        private readonly string API_Key;

        /// <summary>
        /// Token counter
        /// </summary>
        private static int? _tokensRemaining { get; set; } = null;
        public static int TokensRemaining
        {
            get
            {
                if (_tokensRemaining.HasValue) return _tokensRemaining.Value;
                else return 0;
            }
            private set
            {
                _tokensRemaining = Math.Max(value, 0);
            }
        }

        /// <summary>
        /// Charges tokens for API request.  Returns true if sufficient tokens available, otherwise false.
        /// </summary>
        /// <param name="tokensUsed"></param>
        /// <returns></returns>
        private static bool UseTokens(int tokensUsed)
        {
            if (tokensUsed > TokensRemaining)
                return false;
            else
            {
                TokensRemaining -= tokensUsed;
                return true;
            }
        }

        /// <summary>
        /// Updates the number of API tokens the user has.  This is a static value used by all instances of the VClient.
        /// </summary>
        /// <param name="tokensRemaining"></param>
        public static void UpdateTokensRemaining(int tokensRemaining)
        {
            TokensRemaining = tokensRemaining;
        }

        #region Construction and Initialization

        /// <summary>
        /// Static Constructor
        /// </summary>
        static VClient()
        {
            Settings = new VClientSettings();
        }

        /// <summary>
        /// Initializes a new instance of the Volland API Client.
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

            _initHTTPClient();
            _initQueueTimer();
        }

        /// <summary>
        /// Initializes the HTTP client with the base URL and API Key header
        /// </summary>
        private void _initHTTPClient()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-API-KEY", $"{API_Key}");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.Timeout = Settings._httpClientTimeout;
        }

        #endregion

        #region Parallel Request Queuing

        private List<Request> PendingRequests { get; } = new List<Request>();

        private static System.Timers.Timer? queueTimer { get; set; }

        private void _initQueueTimer()
        {
            if (queueTimer != null)
                return;

            queueTimer = new System.Timers.Timer(1000 / Settings._apiRequestsPerSecond);
            queueTimer.Elapsed += QueueTimer_Elapsed;
            queueTimer.Start();
        }

        /// <summary>
        /// Callback for the queue timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueueTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (PendingRequests.Count > 0)
            {
                //
                // Process all pending requests as groups of like types
                //

                Task.Run(async () => await ProcessQueuedRequests<Exposure_Request>());

                Task.Run(async () => await ProcessQueuedRequests<Trend_Request>());

                Task.Run(async () => await ProcessQueuedRequests<ZeroDTE_Request>());

                Task.Run(async () => await ProcessQueuedRequests<Paradigm_Request>());
            }
        }

        /// <summary>
        /// Processes all requests in the queue matching the provided type
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <returns></returns>
        private async Task ProcessQueuedRequests<TRequest>() where TRequest : Request
        {
            // If no matching request types are queued, return
            if (!PendingRequests.Where(req => req is TRequest).Any())
                return;

            // Create a request package for this type of request
            var requestPackage = new Request_Package<TRequest>();

            // Lock for concurrency
            lock (PendingRequests)
            {
                // Pull a batch of matching requests
                var requests = PendingRequests.Where(r => r is TRequest).Cast<TRequest>().Take(Settings.ApiRequestsPerTransmission).ToList();

                // Add matching requests to the package and remove from pending queue
                requests.ForEach(request =>
                {
                    requestPackage.AddRequest(request);
                    PendingRequests.Remove(request);
                });

            }

            // Send the request package
            await SendRequestsAsync(requestPackage);
        }

        /// <summary>
        /// Adds a new request to the queue for processing
        /// </summary>
        /// <param name="request"></param>
        private void EnqueueRequest(Request request)
        {
            // Lock the queue for concurrency
            lock (PendingRequests)
            {
                // Add the new request to the queue
                PendingRequests.Add(request);
            }
        }

        #endregion

        #region Request Handling

        /// <summary>
        /// Transmits a request package to the API and processes the returned data
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="requestPackage"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        private async Task SendRequestsAsync<TRequest>(Request_Package<TRequest> requestPackage) where TRequest : Request
        {
            if (httpClient == null)
                throw new NullReferenceException("ERROR: HTTP Client failed to initialize");

            if (requestPackage == null)
                throw new ArgumentException(nameof(requestPackage));

            try
            {
                // Serialize the request package
                string requestString = JsonSerializer.Serialize(requestPackage);

                // Create a JSON Content package for HTTP transmission
                var content = new StringContent(requestString, System.Text.Encoding.UTF8, "application/json");

                //
                //  TRANSMIT REQUEST -- TOKENS CHARGED HERE
                //

                if (!UseTokens(requestPackage.RequestCount))
                {
                    //
                    // If there are not enough tokens remaining to processes the entirety of this request, return
                    //

                    Debug.WriteLine("Not enough API tokens remain");
                    throw new Exception("Not enough API tokens remain");
                }

                Debug.WriteLine($"SEND API REQUESTS ({TokensRemaining} tokens remaining)");

                //
                // HTTP REQUEST TRANSMITTED HERE
                //
                HttpResponseMessage httpResponse = await httpClient.PostAsync(BaseAddress, content);

                //
                // HTTP Response Processing
                //
                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read the replay as a string
                    string replyString = await httpResponse.Content.ReadAsStringAsync();

                    // Convert the reply string to an array of JSON objects
                    JsonArray? responseGroup = JsonSerializer.Deserialize<JsonArray>(replyString);

                    // Check for null errors due to deserializing
                    if (responseGroup == null)
                        throw new NullReferenceException("Response group was null after deserializing");

                    // Check for data structure consistency
                    if (responseGroup.Count != requestPackage.RequestCount)
                    {
                        // Client assumes that returned data object matches request data object 1:1 in terms of order and type of requests.  
                        Debug.WriteLine("API Response ERROR: Responses do not align with requests");
                        throw new Exception("API Response ERROR: Responses do not align with requests");
                    }
                    else
                    {
                        // Go through the results array
                        for (int i = 0; i < responseGroup?.Count; i++)
                        {
                            try
                            {

                                // Deserialize each node into a base Response object
                                var responseNode = responseGroup[i];
                                Response? response = JsonSerializer.Deserialize<Response>(responseNode);

                                if (response != null && response.request_type != null)
                                {
                                    // Get the specific request type from the Response object
                                    Request_Type requestType = Enum.Parse<Request_Type>(response.request_type);

                                    // Process the response based on the specific request type
                                    switch (requestType)
                                    {

                                        //
                                        // Each Response object (node) is deserialized into its appropriate Result object, and then assigned to the original request assuming a 1:1 ordering
                                        //

                                        case Request_Type.exposure_request:
                                            {
                                                var result = ProcessResponse<Exposure_Response, Exposure_Result>(responseNode.Deserialize<Exposure_Response>());
                                                (requestPackage._requests[i] as Exposure_Request)?.SetResult(result);
                                            }
                                            break;
                                        case Request_Type.trend_request:
                                            {
                                                var result = ProcessResponse<Trend_Response, Trend_Result>(responseNode.Deserialize<Trend_Response>());
                                                (requestPackage._requests[i] as Trend_Request)?.SetResult(result);
                                            }
                                            break;
                                        case Request_Type.paradigm_request:
                                            {
                                                var result = ProcessResponse<Paradigm_Response, Paradigm_Result>(responseNode.Deserialize<Paradigm_Response>());
                                                (requestPackage._requests[i] as Paradigm_Request)?.SetResult(result);
                                            }
                                            break;
                                        case Request_Type.zerodte_request:
                                            {
                                                var result = ProcessResponse<ZeroDTE_Response, ZeroDTE_Result>(responseNode.Deserialize<ZeroDTE_Response>());
                                                (requestPackage._requests[i] as ZeroDTE_Request)?.SetResult(result);
                                            }
                                            break;
                                        default:
                                            {
                                                // Don't throw an exception here since it could cause us to lose any good data that we received.
                                                Debug.WriteLine($"API Return Error: Invalid Request type indicated: {requestType.ToString()}");
                                            }
                                            break;
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"*** API Processing Error: {ex.Message}");
                                Debug.WriteLine($"JSON: {responseGroup[i]?.ToJsonString()}");
                                continue;
                            }
                        }
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
                    Debug.WriteLine("API TIMEOUT");
                    throw new HttpRequestException("API Request Timeout");
                }
                else if (httpResponse.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Debug.WriteLine("API INTERNAL SERVER ERROR");
                    throw new HttpRequestException("API Internal Server Error");
                }
                else
                {
                    Debug.WriteLine($"System ERROR: HTTP Client request returned as unsuccessful: {httpResponse.ReasonPhrase} - Request Body: {requestString}");
                    throw new HttpRequestException($"System ERROR: HTTP Client request returned as unsuccessful: {httpResponse.ReasonPhrase} - Request Body: {requestString}");
                }

            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Converts a single Response to its matching Result
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
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

        //
        // Exposure Requests
        //

        public async Task<Exposure_Result?> RequestExposureAsync(string ticker, Kind kind, Greek greek, DateTime expiration)
        {
            var request = new Exposure_Request(ticker, kind, greek, new List<DateTime>(new[] { expiration }));
            return await RequestExposureAsync(request);
        }

        public async Task<Exposure_Result?> RequestExposureAsync(string ticker, Kind kind, Greek greek, List<DateTime>? expirations = null)
        {
            var request = new Exposure_Request(ticker, kind, greek, expirations);
            return await RequestExposureAsync(request);
        }

        public async Task<Exposure_Result?> RequestExposureAsync(Exposure_Request request)
        {
            EnqueueRequest(request);
            return await request.Wait();
        }

        //
        // Trend Requests
        //

        public async Task<Trend_Result?> RequestTrendAsync(string ticker, Greek greek)
        {
            var request = new Trend_Request(ticker, greek);
            return await RequestTrendAsync(request);
        }

        public async Task<Trend_Result?> RequestTrendAsync(Trend_Request request)
        {
            EnqueueRequest(request);
            return await request.Wait();
        }

        //
        // Zero DTE Statistics Requests
        //

        public async Task<ZeroDTE_Result?> RequestZeroDTEAsync(string ticker)
        {
            var request = new ZeroDTE_Request(ticker);
            return await RequestZeroDTEAsync(request);
        }

        public async Task<ZeroDTE_Result?> RequestZeroDTEAsync(ZeroDTE_Request request)
        {
            EnqueueRequest(request);
            return await request.Wait();
        }

        //
        // Zero DTE Paradigm Requests
        //

        public async Task<Paradigm_Result?> RequestParadigmAsync(string ticker)
        {
            var request = new Paradigm_Request(ticker);
            return await RequestParadigmAsync(request);
        }

        public async Task<Paradigm_Result?> RequestParadigmAsync(Paradigm_Request request)
        {
            EnqueueRequest(request);
            return await request.Wait();
        }

        #endregion

    }

}
