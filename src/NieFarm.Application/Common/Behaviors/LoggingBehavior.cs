using System.Diagnostics;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace NieFarm.Application.Common.Behaviors;

/// <summary>
/// Structured Serilog logging around every MediatR request.
///
/// Commands are logged at Information with their outcome and elapsed time so admin writes
/// leave an audit trail in logs/app-*.log; queries stay at Debug to keep list-page traffic
/// out of the default sink. Failed and unhandled requests are always logged at Warning /
/// Error regardless of request kind.
/// </summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        var isCommand = name.EndsWith("Command", StringComparison.Ordinal);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (response is IResult { Status: not ResultStatus.Ok } failed)
            {
                logger.LogWarning(
                    "{RequestName} did not succeed ({ResultStatus}) in {ElapsedMs} ms",
                    name, failed.Status, stopwatch.ElapsedMilliseconds);
            }
            else if (isCommand)
            {
                logger.LogInformation(
                    "{RequestName} handled in {ElapsedMs} ms", name, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                logger.LogDebug(
                    "{RequestName} handled in {ElapsedMs} ms", name, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "{RequestName} threw after {ElapsedMs} ms", name, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
