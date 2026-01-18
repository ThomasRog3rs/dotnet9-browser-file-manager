using FFMpegCore;
using FFMpegCore.Enums;
using Microsoft.Extensions.Options;

namespace BrowserFileManger.Services;

public class CompressionOptions
{
    public bool Enabled { get; set; } = true;
    public int TargetBitrate { get; set; } = 192;
    public int RecompressThreshold { get; set; } = 192;
    public string TargetFormat { get; set; } = "mp3";
    public bool DeleteOriginals { get; set; } = true;
}

public class CompressionResult
{
    public bool WasCompressed { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string CompressedFileName { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public long BytesSaved => OriginalSize - CompressedSize;
    public double CompressionRatio => OriginalSize > 0 ? (double)CompressedSize / OriginalSize : 1;
    public string? ErrorMessage { get; set; }
    public bool Success => string.IsNullOrEmpty(ErrorMessage);
}

public class AudioCompressionService
{
    private readonly CompressionOptions _options;
    private readonly ILogger<AudioCompressionService> _logger;
    private readonly string _uploadsPath;

    private static readonly string[] LosslessFormats = { ".wav", ".flac", ".aiff", ".aif" };

    public AudioCompressionService(
        IOptions<CompressionOptions> options,
        ILogger<AudioCompressionService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
    }

    /// <summary>
    /// Checks if a file needs compression and compresses it if necessary.
    /// Returns the final filename (may be different if converted to MP3).
    /// </summary>
    public async Task<CompressionResult> CompressIfNeededAsync(string fileName)
    {
        var result = new CompressionResult
        {
            OriginalFileName = fileName,
            CompressedFileName = fileName
        };

        if (!_options.Enabled)
        {
            _logger.LogDebug("Compression is disabled, skipping {FileName}", fileName);
            return result;
        }

        var filePath = Path.Combine(_uploadsPath, fileName);
        if (!File.Exists(filePath))
        {
            result.ErrorMessage = $"File not found: {fileName}";
            _logger.LogWarning("File not found for compression: {FilePath}", filePath);
            return result;
        }

        result.OriginalSize = new FileInfo(filePath).Length;
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            // Check if it's a lossless format that needs transcoding
            if (IsLosslessFormat(extension))
            {
                return await TranscodeToMp3Async(filePath, result);
            }

            // Check if it's an MP3 that might need recompression
            if (extension == ".mp3")
            {
                return await RecompressIfNeededAsync(filePath, result);
            }

            // Other formats - no compression needed
            _logger.LogDebug("File {FileName} does not need compression", fileName);
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error compressing file {FileName}", fileName);
            return result;
        }
    }

    /// <summary>
    /// Transcodes a lossless audio file to MP3 format.
    /// </summary>
    private async Task<CompressionResult> TranscodeToMp3Async(string filePath, CompressionResult result)
    {
        var fileName = Path.GetFileName(filePath);
        var newFileName = Path.ChangeExtension(fileName, ".mp3");
        var newFilePath = Path.Combine(_uploadsPath, newFileName);

        // Handle filename collision
        newFilePath = GetUniqueFilePath(newFilePath);
        newFileName = Path.GetFileName(newFilePath);

        _logger.LogInformation("Transcoding {OriginalFile} to {NewFile} at {Bitrate}kbps",
            fileName, newFileName, _options.TargetBitrate);

        await FFMpegArguments
            .FromFileInput(filePath)
            .OutputToFile(newFilePath, overwrite: true, options => options
                .WithAudioCodec(AudioCodec.LibMp3Lame)
                .WithAudioBitrate(_options.TargetBitrate))
            .ProcessAsynchronously();

        result.WasCompressed = true;
        result.CompressedFileName = newFileName;
        result.CompressedSize = new FileInfo(newFilePath).Length;

        // Delete original file if configured
        if (_options.DeleteOriginals && File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Deleted original file: {FileName}", fileName);
        }

        _logger.LogInformation("Transcoded {OriginalFile}: {OriginalSize:N0} bytes -> {NewSize:N0} bytes (saved {Saved:N0} bytes)",
            fileName, result.OriginalSize, result.CompressedSize, result.BytesSaved);

        return result;
    }

    /// <summary>
    /// Recompresses an MP3 file if its bitrate exceeds the threshold.
    /// </summary>
    private async Task<CompressionResult> RecompressIfNeededAsync(string filePath, CompressionResult result)
    {
        var mediaInfo = await FFProbe.AnalyseAsync(filePath);
        var audioBitrate = mediaInfo.PrimaryAudioStream?.BitRate ?? 0;
        var bitrateKbps = audioBitrate / 1000;

        _logger.LogDebug("File {FileName} has bitrate {Bitrate}kbps, threshold is {Threshold}kbps",
            result.OriginalFileName, bitrateKbps, _options.RecompressThreshold);

        if (bitrateKbps <= _options.RecompressThreshold)
        {
            _logger.LogDebug("File {FileName} bitrate is acceptable, skipping recompression", result.OriginalFileName);
            result.CompressedSize = result.OriginalSize;
            return result;
        }

        // Create temporary file for recompressed output
        var tempFilePath = Path.Combine(_uploadsPath, $"temp_{Guid.NewGuid()}.mp3");

        _logger.LogInformation("Recompressing {FileName} from {CurrentBitrate}kbps to {TargetBitrate}kbps",
            result.OriginalFileName, bitrateKbps, _options.TargetBitrate);

        await FFMpegArguments
            .FromFileInput(filePath)
            .OutputToFile(tempFilePath, overwrite: true, options => options
                .WithAudioCodec(AudioCodec.LibMp3Lame)
                .WithAudioBitrate(_options.TargetBitrate))
            .ProcessAsynchronously();

        // Replace original with compressed version
        File.Delete(filePath);
        File.Move(tempFilePath, filePath);

        result.WasCompressed = true;
        result.CompressedSize = new FileInfo(filePath).Length;

        _logger.LogInformation("Recompressed {FileName}: {OriginalSize:N0} bytes -> {NewSize:N0} bytes (saved {Saved:N0} bytes)",
            result.OriginalFileName, result.OriginalSize, result.CompressedSize, result.BytesSaved);

        return result;
    }

    /// <summary>
    /// Checks if the file extension indicates a lossless audio format.
    /// </summary>
    public bool IsLosslessFormat(string extension)
    {
        return LosslessFormats.Contains(extension.ToLowerInvariant());
    }

    /// <summary>
    /// Gets the current bitrate of an audio file in kbps.
    /// </summary>
    public async Task<int> GetBitrateAsync(string fileName)
    {
        var filePath = Path.Combine(_uploadsPath, fileName);
        if (!File.Exists(filePath))
            return 0;

        try
        {
            var mediaInfo = await FFProbe.AnalyseAsync(filePath);
            return (int)(mediaInfo.PrimaryAudioStream?.BitRate ?? 0) / 1000;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Generates a unique file path to avoid overwriting existing files.
    /// </summary>
    private string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath)!;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var counter = 1;

        while (File.Exists(filePath))
        {
            filePath = Path.Combine(directory, $"{fileNameWithoutExt}_{counter}{extension}");
            counter++;
        }

        return filePath;
    }

    /// <summary>
    /// Gets the compression options for display in the UI.
    /// </summary>
    public CompressionOptions GetOptions() => _options;

    /// <summary>
    /// Formats a byte size as a human-readable string.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
