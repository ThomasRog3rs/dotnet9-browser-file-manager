namespace Phono.Models;

/// <summary>
/// Response model for MagnetAPI torrent search results
/// </summary>
public class TorrentSearchResult
{
    public string Name { get; set; } = string.Empty;
    public string Magnet { get; set; } = string.Empty;
    public string Seeders { get; set; } = string.Empty;
    public string Leechers { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public object? Images { get; set; } // Can be string "Na" or array of strings
    public TorrentDetails? OtherDetails { get; set; }
}

public class TorrentDetails
{
    public string? Category { get; set; }
    public string? Type { get; set; }
    public string? Language { get; set; }
    public string? Uploader { get; set; }
    public string? Downloads { get; set; }
    public string? DateUploaded { get; set; }
}

/// <summary>
/// Error response from MagnetAPI
/// </summary>
public class MagnetApiError
{
    public string Message { get; set; } = string.Empty;
}

public enum MagnetApiErrorType
{
    None,
    EmptyRequest,
    NoDataFound,
    RemoteError,
    HttpError,
    Timeout,
    NetworkError,
    DeserializationError,
    Unknown
}

public class MagnetApiSearchResult
{
    public bool IsSuccess => ErrorType == MagnetApiErrorType.None;
    public MagnetApiErrorType ErrorType { get; init; } = MagnetApiErrorType.None;
    public string? ErrorMessage { get; init; }
    public List<TorrentSearchResult> Results { get; init; } = new();
}
