using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Markup;

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
                _tokensRemaining = value;
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

        #region Request Endpoints

        private async Task<TResult?> Send_Request<TResult>(string request) where TResult : Result
        {
            // Send a connection validation request

            if (httpClient == null)
                throw new NullReferenceException("ERROR: HTTP Client failed to initialize");

            try
            {

                HttpResponseMessage httpResponse = await httpClient.GetAsync(request);

                if (httpResponse.IsSuccessStatusCode)
                {
                    // Read the JSON response
                    string replyString = await httpResponse.Content.ReadAsStringAsync();

                    // Deserialize to a base Response object to determine specific type
                    Response? volResponse = JsonSerializer.Deserialize<Response>(replyString);

                    if (volResponse == null)
                        throw new Exception("System ERROR: Could not deserialize JSON response to base Response object");

                    // Deserialize to the appropriate specific Response type

                    switch (volResponse.request_type)
                    {
                        case "trend_request":
                            break;
                        case "paradign_request":
                            break;
                        case "zerodte_request":
                            break;
                        case "exposure_request":
                            {
                                Exposure_Response? exposure_response = JsonSerializer.Deserialize<Exposure_Response>(replyString);
                                return (await Process_Exposure_Response(exposure_response)) as TResult;
                            }
                        default:
                            throw new Exception($"API ERROR: Unknown response type: {volResponse.request_type}");
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

        private async Task<Exposure_Result> Process_Exposure_Response(Exposure_Response? response)
        {
            if (response == null) throw new Exception("System ERROR: Null response object");


        }

        #endregion

    }
}
