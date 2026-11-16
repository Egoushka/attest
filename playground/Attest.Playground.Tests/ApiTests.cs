using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Attest.Playground.Tests;

public sealed class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static async Task<JsonElement> Json(HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<JsonElement>());

    [Fact]
    public async Task Health_is_ok()
    {
        var r = await _factory.CreateClient().GetAsync("/healthz");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task Index_has_a_csp_and_no_inline_script()
    {
        var r = await _factory.CreateClient().GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Contains("script-src 'self'", r.Headers.GetValues("Content-Security-Policy").Single());
        string html = await r.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public async Task Belgian_vat_is_valid_for_the_country()
    {
        var j = await Json(await _factory.CreateClient().GetAsync("/v1/validate?value=BE0403019261&country=BE"));
        Assert.True(j.GetProperty("valid").GetBoolean());
        Assert.Equal("country", j.GetProperty("mode").GetString());
    }

    [Fact]
    public async Task A_changed_digit_fails_the_checksum()
    {
        var j = await Json(await _factory.CreateClient().GetAsync("/v1/validate?value=BE0403019262&type=be.vat"));
        Assert.False(j.GetProperty("valid").GetBoolean());
        Assert.Equal("invalid_checksum", j.GetProperty("results")[0].GetProperty("reason").GetString());
    }

    [Fact]
    public async Task Autodetect_finds_the_belgian_vat()
    {
        var j = await Json(await _factory.CreateClient().GetAsync("/v1/validate?value=BE0403019261"));
        Assert.True(j.GetProperty("valid").GetBoolean());
        Assert.Contains(j.GetProperty("results").EnumerateArray(), i => i.GetProperty("type").GetString() == "be.vat");
    }

    [Fact]
    public async Task Post_behaves_like_get()
    {
        var body = new StringContent("{\"value\":\"BE0403019261\",\"country\":\"BE\"}", Encoding.UTF8, "application/json");
        var j = await Json(await _factory.CreateClient().PostAsync("/v1/validate", body));
        Assert.True(j.GetProperty("valid").GetBoolean());
    }

    [Theory]
    [InlineData("/v1/validate?value=123&country=ZZ")]
    [InlineData("/v1/validate?value=123&country=1")]
    [InlineData("/v1/validate?value=123&type=nope")]
    [InlineData("/v1/validate")]
    public async Task Bad_input_is_400_with_an_error_code(string url)
    {
        var r = await _factory.CreateClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.False(string.IsNullOrEmpty((await Json(r)).GetProperty("error").GetString()));
    }

    [Fact]
    public async Task A_value_over_256_chars_is_400()
    {
        var r = await _factory.CreateClient().GetAsync("/v1/validate?value=" + new string('1', 257));
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    }

    [Fact]
    public async Task A_body_over_2_KiB_is_refused()
    {
        var body = new StringContent("{\"value\":\"" + new string('1', 4000) + "\"}", Encoding.UTF8, "application/json");
        var r = await _factory.CreateClient().PostAsync("/v1/validate", body);
        Assert.True(r.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestEntityTooLarge);
    }

    [Fact]
    public async Task The_submitted_value_is_never_logged()
    {
        var sink = new ListLogger();
        using var factory = _factory.WithWebHostBuilder(b => b.ConfigureLogging(l => l.AddProvider(sink)));
        var client = factory.CreateClient();
        await client.GetAsync("/v1/validate?value=SECRETVALUE123&country=BE");
        await client.PostAsync("/v1/validate", new StringContent("{\"value\":\"SECRETVALUE456\"}", Encoding.UTF8, "application/json"));

        Assert.NotEmpty(sink.Lines);
        Assert.DoesNotContain(sink.Lines, l => l.Contains("SECRETVALUE"));
    }

    private sealed class ListLogger : ILoggerProvider, ILogger
    {
        public List<string> Lines { get; } = new();
        public ILogger CreateLogger(string categoryName) => this;
        public void Dispose() { }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            lock (Lines) { Lines.Add(formatter(state, exception)); }
        }
    }

    [Fact]
    public async Task Countries_lists_types()
    {
        var j = await Json(await _factory.CreateClient().GetAsync("/v1/countries"));
        var be = j.GetProperty("countries").EnumerateArray().Single(c => c.GetProperty("code").GetString() == "BE");
        Assert.Contains(be.GetProperty("types").EnumerateArray(), t => t.GetProperty("id").GetString() == "be.vat");
    }

    [Fact]
    public async Task The_limit_returns_429_with_retry_after()
    {
        using var factory = _factory.WithWebHostBuilder(b => b.UseSetting("Playground:PerClientPerMinute", "3"));
        var client = factory.CreateClient();
        HttpResponseMessage last = null!;
        for (int i = 0; i < 5; i++)
        {
            last = await client.GetAsync("/v1/validate?value=BE0403019261&country=BE");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last.StatusCode);
        Assert.True(last.Headers.Contains("Retry-After"));
    }
}
