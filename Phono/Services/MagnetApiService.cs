using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using Phono.Models;

namespace Phono.Services;

public class MagnetApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}

public class MagnetApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public MagnetApiService(HttpClient httpClient, IOptions<MagnetApiOptions> options)
    {
        _httpClient = httpClient;
        _baseUrl = options.Value.BaseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Search The Pirate Bay for audio torrents
    /// </summary>
    /// <param name="query">Search query</param>
    /// <returns>Search result with data or error info</returns>
    public async Task<MagnetApiSearchResult> SearchPirateBayAudioAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new MagnetApiSearchResult
            {
                ErrorType = MagnetApiErrorType.EmptyRequest,
                ErrorMessage = "Query is empty."
            };
        }

        var encodedQuery = HttpUtility.UrlEncode(query);
        const string audioCategory = "audio";
        var url = $"{_baseUrl}/pirate-bay/{encodedQuery}/{audioCategory}";

        return await SearchAsync(url);
    }

    /// <summary>
    /// Generic search method that handles both sites
    /// </summary>
    private async Task<MagnetApiSearchResult> SearchAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return new MagnetApiSearchResult
                {
                    ErrorType = MagnetApiErrorType.HttpError,
                    ErrorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Try to parse as error first
            try
            {
                var error = System.Text.Json.JsonSerializer.Deserialize<MagnetApiError>(content);
                if (error != null && !string.IsNullOrEmpty(error.Message))
                {
                    // Check for known error messages
                    if (error.Message.Contains("Empty Request", StringComparison.OrdinalIgnoreCase) ||
                        error.Message.Contains("No data found", StringComparison.OrdinalIgnoreCase))
                    {
                        var errorType = error.Message.Contains("Empty Request", StringComparison.OrdinalIgnoreCase)
                            ? MagnetApiErrorType.EmptyRequest
                            : MagnetApiErrorType.NoDataFound;
                        return new MagnetApiSearchResult
                        {
                            ErrorType = errorType,
                            ErrorMessage = error.Message
                        };
                    }
                    
                    // Other errors return null
                    return new MagnetApiSearchResult
                    {
                        ErrorType = MagnetApiErrorType.RemoteError,
                        ErrorMessage = error.Message
                    };
                }
            }
            catch
            {
                // Not an error response, continue to parse as results
            }

            // Parse as success response (array of results)
            try
            {
                var results = JsonSerializer.Deserialize<List<TorrentSearchResult>>(content);
                return new MagnetApiSearchResult
                {
                    Results = results ?? new List<TorrentSearchResult>()
                };
            }
            catch (JsonException ex)
            {
                return new MagnetApiSearchResult
                {
                    ErrorType = MagnetApiErrorType.DeserializationError,
                    ErrorMessage = ex.Message
                };
            }
        }
        catch (HttpRequestException ex)
        {
            // Network or HTTP errors
            return new MagnetApiSearchResult
            {
                ErrorType = MagnetApiErrorType.NetworkError,
                ErrorMessage = ex.Message
            };
        }
        catch (TaskCanceledException)
        {
            // Timeout
            return new MagnetApiSearchResult
            {
                ErrorType = MagnetApiErrorType.Timeout,
                ErrorMessage = "Request timed out."
            };
        }
        catch (Exception ex)
        {
            // Other errors
            return new MagnetApiSearchResult
            {
                ErrorType = MagnetApiErrorType.Unknown,
                ErrorMessage = ex.Message
            };
        }
    }
}
