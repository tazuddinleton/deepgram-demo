# Deepgram Audio Transcription Integration

This project demonstrates how to integrate with the [Deepgram API](https://deepgram.com/) to transcribe audio files using C#. The application reads audio files, sends them to the Deepgram transcription service, and displays the transcription results.

## Features
- Transcribe audio files (`.mp3`, `.wav`, etc.) using the Deepgram API.
- Reads API keys securely from a `appsettings.json` configuration file.
- Supports additional transcription options like language selection and punctuation inclusion.

---

## Prerequisites
1. **Deepgram API Key**: Obtain from [Deepgram Console](https://console.deepgram.com/).
2. **.NET SDK**: Install [.NET 6.0 or later](https://dotnet.microsoft.com/download).
3. **NuGet Packages**:
    - `Microsoft.Extensions.Configuration`
    - `Microsoft.Extensions.Configuration.Json`
    - `Newtonsoft.Json`

---

## Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/tazuddinleton/deepgram-demo.git
cd deepgram-demo\DeepGramSample
```
### 2. Provide configuration `appsettings.json`
```
{
    "Deepgram": {
        "ApiKey": "YOUR_DEEPGRAM_API_KEY"
    }
} 
```

### 3. Build & Run
`dotnet restore` <br>

`dotnet run`

### 4. Usage 

```-d <input-directory> -o <output-directory> [-f <specific-file>]
Options:
-d  Specifies the input directory containing audio files.
-o  Specifies the output directory to save transcriptions.
-f  (Optional) Specifies a specific file to transcribe.
```
### 5. Sample command:
`DeepGramSample.exe -d "<input directory>" -o "<output directory>" -f <specific file name>`

