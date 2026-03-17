using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerEngagement.Infrastructure.ExternalServices.GeoIp;

public record GeoIpResult(
    string? Country,
    string? CountryCode,
    string? City,
    string? Region,
    double? Latitude,
    double? Longitude,
    string? PostalCode,
    string? TimeZone);

public class MaxMindOfflineGeoIpService : IDisposable
{
    private readonly DatabaseReader? _reader;
    private readonly ILogger<MaxMindOfflineGeoIpService> _logger;
    private bool _disposed;

    public MaxMindOfflineGeoIpService(IConfiguration configuration, ILogger<MaxMindOfflineGeoIpService> logger)
    {
        _logger = logger;

        var databasePath = configuration["GeoIp:DatabasePath"] ?? "data/GeoLite2-City.mmdb";

        try
        {
            if (File.Exists(databasePath))
            {
                _reader = new DatabaseReader(databasePath);
                _logger.LogInformation("MaxMind GeoIP2 database loaded from {Path}", databasePath);
            }
            else
            {
                _logger.LogWarning("MaxMind GeoIP2 database not found at {Path}. GeoIP lookups will return empty results.", databasePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load MaxMind GeoIP2 database from {Path}", databasePath);
        }
    }

    public GeoIpResult? Lookup(string ipAddress)
    {
        if (_reader is null)
        {
            _logger.LogDebug("GeoIP lookup skipped: database not loaded");
            return null;
        }

        if (string.IsNullOrWhiteSpace(ipAddress) || IsPrivateIp(ipAddress))
        {
            return null;
        }

        try
        {
            if (_reader.TryCity(ipAddress, out CityResponse? response) && response is not null)
            {
                return new GeoIpResult(
                    Country: response.Country?.Name,
                    CountryCode: response.Country?.IsoCode,
                    City: response.City?.Name,
                    Region: response.MostSpecificSubdivision?.Name,
                    Latitude: response.Location?.Latitude,
                    Longitude: response.Location?.Longitude,
                    PostalCode: response.Postal?.Code,
                    TimeZone: response.Location?.TimeZone);
            }

            _logger.LogDebug("No GeoIP data found for {IpAddress}", ipAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GeoIP lookup failed for {IpAddress}", ipAddress);
            return null;
        }
    }

    private static bool IsPrivateIp(string ipAddress)
    {
        if (!System.Net.IPAddress.TryParse(ipAddress, out var ip))
            return true;

        var bytes = ip.GetAddressBytes();
        return bytes.Length == 4 && (
            bytes[0] == 10 ||
            (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
            (bytes[0] == 192 && bytes[1] == 168) ||
            (bytes[0] == 127));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _reader?.Dispose();
            }
            _disposed = true;
        }
    }
}
