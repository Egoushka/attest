using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.RateLimiting;
using Attest.Playground;
using Microsoft.AspNetCore.RateLimiting;

// The chiseled runtime image has no curl or shell, so the Compose healthcheck is this binary asking itself.
if (args.Contains("--healthcheck"))
{
    try
    {
        using var probe = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        string port = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS")?.Split(';')[0] ?? "8080";
        return (await probe.GetAsync("http://127.0.0.1:" + port + "/healthz")).IsSuccessStatusCode ? 0 : 1;
    }
    catch (HttpRequestException)
    {
        return 1;
    }
    catch (TaskCanceledException)
    {
        return 1;
    }
}

var builder = WebApplication.CreateSlimBuilder(args);

// Hosting logs every request URL, query string included, and a GET carries the submitted value.
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);

// Read by hand: Native AOT, no reflection binder.
IConfiguration cfg = builder.Configuration;
var options = new PlaygroundOptions
{
    PerClientPerMinute = int.TryParse(cfg["Playground:PerClientPerMinute"], out int perClient) && perClient > 0 ? perClient : 60,
    GlobalBurst = int.TryParse(cfg["Playground:GlobalBurst"], out int burst) && burst > 0 ? burst : 200,
    GlobalPerSecond = int.TryParse(cfg["Playground:GlobalPerSecond"], out int perSecond) && perSecond > 0 ? perSecond : 100,
    ClientIpHeader = cfg["Playground:ClientIpHeader"] ?? "",
};
builder.Services.AddSingleton(options);

builder.WebHost.ConfigureKestrel(k =>
{
    k.AddServerHeader = false;
    k.Limits.MaxRequestBodySize = 2048;
    k.Limits.MaxRequestHeadersTotalSize = 8192;
    k.Limits.MaxConcurrentConnections = 512;
    k.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
    k.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetSlidingWindowLimiter(ClientKey.Of(ctx, options), _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = options.PerClientPerMinute,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
            })),
        PartitionedRateLimiter.Create<HttpContext, string>(_ =>
            RateLimitPartition.GetTokenBucketLimiter("all", _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = options.GlobalBurst,
                TokensPerPeriod = options.GlobalPerSecond,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                QueueLimit = 0,
            })));
    o.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers.RetryAfter = "60";
        await Responses.WriteJson(context.HttpContext, StatusCodes.Status429TooManyRequests,
            new ErrorResponse("rate_limited", "Too many requests. Slow down and retry after a minute."), Json.Default.ErrorResponse, token);
    };
});

var app = builder.Build();

app.UseRateLimiter();

app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        IHeaderDictionary h = ctx.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["Referrer-Policy"] = "no-referrer";
        h["Content-Security-Policy"] =
            "default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self' data:; " +
            "base-uri 'none'; form-action 'none'; frame-ancestors 'none'";
        if (ctx.Request.Path.StartsWithSegments("/v1"))
        {
            h["Access-Control-Allow-Origin"] = "*";
            h["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
            h["Access-Control-Allow-Headers"] = "content-type, x-attest-source";
            h["Access-Control-Max-Age"] = "86400";
            h.CacheControl = "no-store";
        }

        return Task.CompletedTask;
    });
    await next();
});

app.MapMethods("/v1/{**rest}", new[] { "OPTIONS" }, () => Results.NoContent());

app.MapGet("/healthz", () => Results.Text("ok"));

app.MapGet("/v1/countries", (RequestDelegate)(async ctx =>
{
    ctx.Response.Headers.CacheControl = "public, max-age=3600";
    await Responses.WriteJson(ctx, StatusCodes.Status200OK, Engine.Countries, Json.Default.CountriesResponse, ctx.RequestAborted);
}));

app.MapGet("/v1/validate", (RequestDelegate)(ctx =>
{
    var q = ctx.Request.Query;
    return Validate(ctx, new ValidateRequest(q["value"], q["country"], q["type"]));
}));

app.MapPost("/v1/validate", (RequestDelegate)(async ctx =>
{
    ValidateRequest? body;
    try
    {
        body = await JsonSerializer.DeserializeAsync(ctx.Request.Body, Json.Default.ValidateRequest, ctx.RequestAborted);
    }
    catch (Exception e) when (e is JsonException or BadHttpRequestException or InvalidOperationException)
    {
        body = null;
    }

    if (body == null)
    {
        await Responses.WriteJson(ctx, StatusCodes.Status400BadRequest,
            new ErrorResponse("bad_body", "Send JSON like {\"value\":\"BE0403019261\",\"country\":\"BE\"}."), Json.Default.ErrorResponse, ctx.RequestAborted);
        return;
    }

    await Validate(ctx, body);
}));

MapAsset(app, "/", "index.html", "text/html; charset=utf-8");
MapAsset(app, "/app.js", "app.js", "text/javascript; charset=utf-8");
MapAsset(app, "/app.css", "app.css", "text/css; charset=utf-8");

app.Run();
return 0;

static void MapAsset(WebApplication app, string path, string file, string contentType)
{
    byte[] bytes = Responses.Embedded(file);
    app.MapGet(path, (HttpContext ctx) =>
    {
        ctx.Response.Headers.CacheControl = "public, max-age=300";
        return Results.Bytes(bytes, contentType);
    });
}

static async Task Validate(HttpContext ctx, ValidateRequest request)
{
    var (response, error) = Engine.Run(request);
    ILogger log = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("validate");
    string source = ctx.Request.Headers["X-Attest-Source"] == "playground" ? "playground" : "api";
    if (response == null)
    {
        log.LogInformation("validate source={Source} outcome=error error={Error}", source, error!.Error);
        await Responses.WriteJson(ctx, StatusCodes.Status400BadRequest, error, Json.Default.ErrorResponse, ctx.RequestAborted);
        return;
    }

    // The submitted value is personal data: it is never written to a log, only what was decided.
    IdentifierItem? top = response.Results.FirstOrDefault();
    log.LogInformation("validate source={Source} mode={Mode} country={Country} type={Type} valid={Valid} reason={Reason}",
        source, response.Mode, top?.Country ?? "-", top?.Type ?? "-", response.Valid, top?.Reason ?? "none");
    await Responses.WriteJson(ctx, StatusCodes.Status200OK, response, Json.Default.ValidateResponse, ctx.RequestAborted);
}

public partial class Program;

internal sealed class PlaygroundOptions
{
    public int PerClientPerMinute { get; set; } = 60;
    public int GlobalBurst { get; set; } = 200;
    public int GlobalPerSecond { get; set; } = 100;

    /// <summary>Header carrying the real client address, trusted only behind the edge that sets it. Empty: use the socket.</summary>
    public string ClientIpHeader { get; set; } = "";
}

internal static class ClientKey
{
    public static string Of(HttpContext ctx, PlaygroundOptions options)
    {
        if (options.ClientIpHeader.Length > 0
            && ctx.Request.Headers[options.ClientIpHeader].ToString() is { Length: > 0 and <= 45 } raw
            && IPAddress.TryParse(raw, out IPAddress? ip))
        {
            return ip.ToString();
        }

        return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}

internal static class Responses
{
    public static async Task WriteJson<T>(HttpContext ctx, int status, T value, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> info, CancellationToken token)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        await JsonSerializer.SerializeAsync(ctx.Response.Body, value, info, token);
    }

    public static byte[] Embedded(string file)
    {
        Assembly asm = typeof(Responses).Assembly;
        string name = asm.GetManifestResourceNames().Single(n => n.EndsWith("." + file, StringComparison.Ordinal));
        using Stream s = asm.GetManifestResourceStream(name)!;
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }
}
