using Mp3Runner.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;

namespace Mp3Runner.Classes
{
    public class SpotifyTrackAnalyzer
    {
        public static async Task<(string bpm, string key)> AnalyzeAndFetchSpotifyInfo(string filePath)
        {
            var originalTrackName = ExtractOriginalTrackName(filePath);

            var trackName = ExtractTrackName(originalTrackName);
            var accessToken = await SpotifyAuth.GetAccessToken();
            var trackId = await SearchTrackAsync(trackName, accessToken);

            if (!string.IsNullOrEmpty(trackId))
            {
                var audioFeatures = await GetTrackAudioFeaturesAsync(trackId, accessToken);

                if (audioFeatures != null)
                {
                    var bpm = audioFeatures.tempo.ToString("0");
                    var keyString = Helpers.ConvertKeyToString((int)audioFeatures.key, (int)audioFeatures.mode);
                    return (bpm, keyString);
                }
            }
            return (null, null);
        }

        private static async Task<string> SearchTrackAsync(string trackName, string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync($"https://api.spotify.com/v1/search?q={trackName}&type=track&limit=1");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(responseString);

                if (searchResponse.Tracks.Items.Length > 0)
                {
                    return searchResponse.Tracks.Items[0].Id;
                }
            }

            return null;
        }

        private static async Task<dynamic> GetTrackAudioFeaturesAsync(string trackId, string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.GetAsync($"https://api.spotify.com/v1/audio-features/{trackId}");
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var audioFeatures = JsonConvert.DeserializeObject<dynamic>(responseString);
                return audioFeatures;
            }

            return null;
        }


        public static string ExtractTrackName(string fileName)
        {
            var lastUnderscoreIndex = fileName.LastIndexOf('_');
            if (lastUnderscoreIndex >= 0)
            {
                return fileName.Substring(lastUnderscoreIndex + 1).Replace(".mp3", "");
            }
            return fileName;
        }

        public static string ExtractOriginalTrackName(string filePath)
        {
            var originalFileName = Path.GetFileNameWithoutExtension(filePath); 
            var cleanFileName = Helpers.RemoveExistingBpmKeyPrefix(originalFileName);
            return cleanFileName;
        }

    }
}
