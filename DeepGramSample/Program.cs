using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    private static string? DeepgramApiKey;
    private const string ApiUrl = "https://api.deepgram.com/v1/listen";

    // Directories for input and output
    private static string InputDirectory;
    private static string OutputDirectory;

    static async Task Main(string[] args)
    {
        if (args.Length < 4)
        {
            ShowUsage();
            return;
        }

        var config = LoadConfiguration();
        DeepgramApiKey = config["Deepgram:ApiKey"];
        if (string.IsNullOrEmpty(DeepgramApiKey))
        {
            Console.WriteLine("Deepgram API key not found in configuration.");
            return;
        }
        
        string inputDirectory = null;
        string outputDirectory = null;
        string specificFile = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-d":
                    inputDirectory = args[i + 1];
                    i++;
                    break;
                case "-o":
                    outputDirectory = args[i + 1];
                    i++;
                    break;
                case "-f":
                    specificFile = args[i + 1];
                    i++;
                    break;
                default:
                    Console.WriteLine($"Unknown argument: {args[i]}");
                    ShowUsage();
                    return;
            }
        }

        if (string.IsNullOrEmpty(inputDirectory) || string.IsNullOrEmpty(outputDirectory))
        {
            Console.WriteLine("Error: Both input and output directories are required.");
            ShowUsage();
            return;
        }

        if (!Directory.Exists(inputDirectory))
        {
            Console.WriteLine($"Error: Input directory '{inputDirectory}' does not exist.");
            return;
        }

        Directory.CreateDirectory(outputDirectory);

        if (!string.IsNullOrEmpty(specificFile))
        {
            string filePath = Path.Combine(inputDirectory, specificFile);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{specificFile}' does not exist in the input directory.");
                return;
            }

            Console.WriteLine($"Processing specific file: {specificFile}");
            await ProcessFile(filePath, outputDirectory);
        }
        else
        {
            Console.WriteLine($"Processing all files in directory: {inputDirectory}");
            var files = Directory.GetFiles(inputDirectory, "*.*")
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".wav"))
                .ToArray();

            if (files.Length == 0)
            {
                Console.WriteLine("No audio files found in the input directory.");
                return;
            }

            foreach (var file in files)
            {
                Console.WriteLine($"Processing file: {Path.GetFileName(file)}");
                await ProcessFile(file, outputDirectory);
            }
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  -d <input-directory> -o <output-directory> [-f <specific-file>]");
        Console.WriteLine("Options:");
        Console.WriteLine("  -d  Specifies the input directory containing audio files.");
        Console.WriteLine("  -o  Specifies the output directory to save transcriptions.");
        Console.WriteLine("  -f  (Optional) Specifies a specific file to transcribe.");
    }

    private static string GetCustomVocabulary()
    {
        return Uri.EscapeDataString("Apoquel:1,Bordetella:1,Leptospirosis:1,Distemper Parvo Combo:1,Heartworm:1,Bravecto:1,AppleQuil:-1");
    }
    static async Task ProcessFile(string inputFilePath, string outputDirectory)
    {
        try
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);
            string outputFilePath = Path.Combine(outputDirectory, $"{fileNameWithoutExtension}.txt");

            // Call the transcription method (to be implemented)
            string transcription =
                await TranscribeAudioAsync(inputFilePath); // Placeholder for actual transcription logic

            // Save transcription to the output file
            File.WriteAllText(outputFilePath, FormatTranscription(transcription));

            Console.WriteLine($"Transcribed '{inputFilePath}' and saved to '{outputFilePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file '{inputFilePath}': {ex.Message}");
        }
    }

    static string FormatTranscription(string transcription)
    {
        if (string.IsNullOrEmpty(transcription))
        {
            return transcription;
        }

        // Split by sentence-ending punctuation
        char[] sentenceDelimiters = { '.', '!', '?' };
        var sentences = transcription.Split(sentenceDelimiters);

        // Reconstruct with newlines
        return string.Join(Environment.NewLine, sentences.Select(s => s.Trim() + "."));
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
        string requestUrl = $"{ApiUrl}?language=en&punctuate=true&keywords={GetCustomVocabulary()}";

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