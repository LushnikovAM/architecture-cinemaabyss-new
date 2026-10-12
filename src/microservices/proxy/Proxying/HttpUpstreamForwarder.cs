using CinemaAbyss.Proxy.Routing;

namespace CinemaAbyss.Proxy.Proxying;

internal sealed class HttpUpstreamForwarder : IUpstreamForwarder
{
    public const string HttpClientName = "upstream";

    private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "Proxy-Connection",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade",
        "Host"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpUpstreamForwarder> _logger;

    public HttpUpstreamForwarder(IHttpClientFactory httpClientFactory, ILogger<HttpUpstreamForwarder> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ForwardAsync(HttpContext context, Upstream upstream)
    {
        var target = upstream.BaseUrl + context.Request.Path + context.Request.QueryString;
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);

        if (RequestHasBody(context.Request))
        {
            request.Content = new StreamContent(context.Request.Body);
        }

        CopyRequestHeaders(context.Request, request);

        if (context.Connection.RemoteIpAddress is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-For", context.Connection.RemoteIpAddress.ToString());
        }

        request.Headers.TryAddWithoutValidation("X-Forwarded-Host", context.Request.Host.Value);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", context.Request.Scheme);

        var client = _httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted);

            context.Response.StatusCode = (int)response.StatusCode;
            CopyResponseHeaders(response, context.Response);
            context.Response.Headers["X-Proxy-Target"] = upstream.Name;
            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (context.RequestAborted.IsCancellationRequested)
            {
                return;
            }

            _logger.LogError(ex, "Upstream {Upstream} is unavailable for {Target}", upstream.Name, target);
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                context.Response.Headers["X-Proxy-Target"] = upstream.Name;
                await context.Response.WriteAsJsonAsync(new { error = "Bad gateway", upstream = upstream.Name });
            }
        }
    }

    private static bool RequestHasBody(HttpRequest request)
    {
        if (request.ContentLength is > 0)
        {
            return true;
        }

        if (request.Headers.ContainsKey("Transfer-Encoding"))
        {
            return true;
        }

        return HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method);
    }

    private static void CopyRequestHeaders(HttpRequest incoming, HttpRequestMessage outgoing)
    {
        foreach (var header in incoming.Headers)
        {
            if (HopByHopHeaders.Contains(header.Key)
                || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
            {
                outgoing.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                continue;
            }

            outgoing.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    private static void CopyResponseHeaders(HttpResponseMessage upstream, HttpResponse outgoing)
    {
        foreach (var header in upstream.Headers)
        {
            if (!HopByHopHeaders.Contains(header.Key))
            {
                outgoing.Headers[header.Key] = header.Value.ToArray();
            }
        }

        foreach (var header in upstream.Content.Headers)
        {
            if (!HopByHopHeaders.Contains(header.Key))
            {
                outgoing.Headers[header.Key] = header.Value.ToArray();
            }
        }
    }
}
