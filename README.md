# Readme

This client is 3rd-party and not provided by or supported by Volland in any capacity.  A subscription is required to use the API service.

For details regarding the API service visit [vol.land](https://www.vol.land/API/)

## Structure

Provides a REST client interface for querying Volland API endpoints.

`VClient` class is instantiated with your API key and current API Token count.

*Note: Each call consumes a token.  The API does not provide a token count method, the user must instatiate the client with their current token count each use.
Tokens in a session are deducted in a static field across all instances of the client*

`VClient` provides 4 methods corresponding to the 4 Volland endpoints.  Each method is asynchronous, task-based, and returns a `x_Result` object containing the requested data.

`public async Task<Exposure_Result?> RequestExposureAsync(...)`

`public async Task<Trend_Result?> RequestTrendAsync(...)`

`public async Task<ZeroDTE_Result?> RequestZeroDTEAsync(...)`

`public async Task<Paradigm_Result?> RequestParadigmAsync(...)`

## Usage Examples

Instatiate a new VClient with user API key and current count:

`var client = new VClient("API_Key_Here", 2000);`

Make a request (null value for `expirations` field queries all expirations):

`Exposure_Result result = await client.RequestExposureAsync("SPX", Kind.both, Greek.delta, null);`

User your data:

`result.Ticker`
`result.LastUpdated`
`result.SpotPrice`
...

The actual exposures aper strike for this call are contained as a list of Strike-Exposure pair objects.

`result.Exposures.Strike`
`result.Exposures.Exposure`

Note: Requesting multiple expirations does not return independent results - each call will return the combined total for all indicated expirations.  To retrieve individual expiration values, make a single call for each.
