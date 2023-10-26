using System.Collections.Generic;
using System.IO;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using Google.Cloud.Translation.V2;
using SettingsManager;
using WinUtilities.Keyboard.Enums;

namespace AiSpeak;

public class UserSettings : Setting<UserSettings>
{
    public string Language { get; set; } = LanguageCodes.English;
    public int AudioDevice { get; set; } = -1;
    public Voice SelectedVoice { get; set; } = Voice.Adam;
    public Model SelectedModel { get; set; } = Model.MultiLingualV1;
    public Key SelectedKeyBind { get; set; } = Key.K;
    public Key SelectedKeyBindOut { get; set; } = Key.V;
    public bool KeyBindOutEnabled { get; set; } = false;
    public List<Voice> CachedVoices { get; set; } = new List<Voice>();
    public string ElevenLabsApiKey { get; set; } = "";
    public string GoogleCloudKey { get; set; } = "./google-key.json";

    public TranscriberSettings Transcriber { get; set; } = new();
    public bool StandaloneMode { get; set; } = true;
}

public class TranscriberSettings
{
    public string ApiUrl { get; set; } = "http://127.0.0.1:5000";
    public string ApiToken { get; set; } = "";
}