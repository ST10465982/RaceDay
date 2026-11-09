using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Services;

public sealed record ApiReply<T>(bool Success, T? Data, string? Error, HttpStatusCode Status);

public class RaceDayApiService
{
    private const string CookieKey = "RaceDay.ApiCookie";
    private const string ApiCookieName = ".AspNetCore.Session";
    private readonly IHttpClientFactory _clients;
    private readonly IHttpContextAccessor _context;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public RaceDayApiService(IHttpClientFactory clients, IHttpContextAccessor context)
    {
        _clients = clients;
        _context = context;
    }

    public Task<ApiReply<T>> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);
    public Task<ApiReply<T>> PostAsync<T>(string path, object data) => SendAsync<T>(HttpMethod.Post, path, JsonContent.Create(data));
    public Task<ApiReply<T>> PutAsync<T>(string path, object data) => SendAsync<T>(HttpMethod.Put, path, JsonContent.Create(data));
    public Task<ApiReply<T>> DeleteAsync<T>(string path) => SendAsync<T>(HttpMethod.Delete, path);

    public async Task<ApiReply<T>> UploadAsync<T>(string path, IFormFile file)
    {
        using var multipart = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var content = new StreamContent(stream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        multipart.Add(content, "file", file.FileName);
        return await SendAsync<T>(HttpMethod.Post, path, multipart);
    }

    public void ClearApiSession() => _context.HttpContext?.Session.Remove(CookieKey);

    private async Task<ApiReply<T>> SendAsync<T>(HttpMethod method, string path, HttpContent? content = null)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !path.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(path, UriKind.Relative, out _) ||
            path.StartsWith("//", StringComparison.Ordinal))
            throw new ArgumentException("Only relative api/ endpoints are permitted.", nameof(path));

        var context = _context.HttpContext
            ?? throw new InvalidOperationException("HTTP context unavailable.");
        using var request = new HttpRequestMessage(method, path) { Content = content };
        var cookie = context.Session.GetString(CookieKey);
        if (!string.IsNullOrWhiteSpace(cookie))
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        HttpResponseMessage response;
        try
        {
            response = await _clients.CreateClient("RaceDayApi").SendAsync(request);
        }
        catch (HttpRequestException)
        {
            return new ApiReply<T>(false, default,
                "The RaceDay API is not reachable. Check that it is running.",
                HttpStatusCode.ServiceUnavailable);
        }
        catch (TaskCanceledException)
        {
            return new ApiReply<T>(false, default,
                "The RaceDay API took too long to respond.",
                HttpStatusCode.GatewayTimeout);
        }
        using (response)
        {
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var header in cookies)
            {
                var pair = header.Split(';', 2)[0].Trim();
                if (!pair.StartsWith(ApiCookieName + "=", StringComparison.Ordinal)) continue;
                if (pair.Length <= ApiCookieName.Length + 1) ClearApiSession();
                else context.Session.SetString(CookieKey, pair);
            }
        }
        if (!response.IsSuccessStatusCode)
        {
            string error = "The request could not be completed. Please try again.";
            try
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("message", out var prop) && prop.ValueKind == JsonValueKind.String)
                    error = prop.GetString() ?? error;
                if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    error = title.GetString() ?? error;
            }
            catch (JsonException) { }
            return new ApiReply<T>(false, default, error, response.StatusCode);
        }
        if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
            return new ApiReply<T>(true, default, null, response.StatusCode);
        var text = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text)) return new ApiReply<T>(true, default, null, response.StatusCode);
        try
        {
            return new ApiReply<T>(true, JsonSerializer.Deserialize<T>(text, JsonOptions), null, response.StatusCode);
        }
        catch (JsonException)
        {
            return new ApiReply<T>(false, default, "The API returned an unexpected response.", response.StatusCode);
        }
        }
    }
}
