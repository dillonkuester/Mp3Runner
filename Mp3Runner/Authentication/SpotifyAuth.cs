using System.Net.Http;
using System.Text;
using Mp3Runner.Properties;

public class SpotifyAuth
{

    public static async Task<string> GetAccessToken()
    {
        string clientId = Settings.Default.ClientId;
        string clientSecret = Settings.Default.ClientSecret;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Client ID or Client Secret is not set.");
        }

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {auth}");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
        {
            Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded")
        };

        var response = await httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        var tokenResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResponse>(responseString);

        return tokenResponse.AccessToken;
    }
}
