namespace NieFarm.Infrastructure.Services.Addresses;

public class AddressLookupOptions
{
    public const string SectionName = "AddressLookup";

    public string BaseUrl { get; set; } = "https://provinces.open-api.vn/api/v2/";
    public int TimeoutSeconds { get; set; } = 5;
    public int CacheHours { get; set; } = 24;
    public int FailureCacheSeconds { get; set; } = 60;
}
