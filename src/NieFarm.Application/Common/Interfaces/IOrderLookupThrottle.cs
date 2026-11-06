namespace NieFarm.Application.Common.Interfaces;

/// <summary>
/// Minimal, process-local limiter for the code + phone order lookup. Counts failures per
/// normalised phone and per normalised code. Extended or replaced by R-21.
/// </summary>
public interface IOrderLookupThrottle
{
    bool IsLockedOut(string code, string phone);
    void RecordFailure(string code, string phone);
}
