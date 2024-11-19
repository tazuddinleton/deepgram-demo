using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    private static string DeepgramApiKey;
    private static string AudioFilePath;
    private const string ApiUrl = "https://api.deepgram.com/v1/listen";

    static async Task Main(string[] args)
    {
        // Load configuration
        var config = LoadConfiguration();
        DeepgramApiKey = config["Deepgram:ApiKey"];
        AudioFilePath = config["Input:AudioFilePath"];

        if (string.IsNullOrEmpty(DeepgramApiKey))
        {
            Console.WriteLine("Deepgram API key not found in configuration.");
            return;
        }


        if (!File.Exists(AudioFilePath))
        {
            Console.WriteLine("Audio file not found!");
            return;
        }

        try
        {
            string transcription = await TranscribeAudioAsync(AudioFilePath);
            Console.WriteLine("Transcription:");
            Console.WriteLine(transcription);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static async Task<string> TranscribeAudioAsync(string filePath)
    {
        using var client = new HttpClient();

        // Set up request headers
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", DeepgramApiKey);

        // Read the audio file
        byte[] audioData = await File.ReadAllBytesAsync(filePath);
        using var content = new ByteArrayContent(audioData);

        // Set the content type (adjust for your audio format)
        content.Headers.ContentType = new MediaTypeHeaderValue("audio/mp3");

        // Add any additional query parameters (e.g., language, punctuate)
        string requestUrl = $"{ApiUrl}?language=en&punctuate=true";

        // Send POST request to Deepgram API
        HttpResponseMessage response = await client.PostAsync(requestUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API call failed with status code {response.StatusCode}: {response.ReasonPhrase}");
        }

        // Parse and return the transcription
        string responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<DeepgramResponse>(responseContent);

        return result?.Results?.Channels?[0]?.Alternatives?[0]?.Transcript ?? "No transcription available.";
    }

    // Classes to parse JSON response
    public class DeepgramResponse
    {
        public DeepgramResults Results { get; set; }
    }

    public class DeepgramResults
    {
        public DeepgramChannel[] Channels { get; set; }
    }

    public class DeepgramChannel
    {
        public DeepgramAlternative[] Alternatives { get; set; }
    }

    public class DeepgramAlternative
    {
        public string Transcript { get; set; }
    }
}
