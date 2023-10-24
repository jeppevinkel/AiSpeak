using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ElevenLabs;
using ElevenLabs.Models;
using ElevenLabs.Voices;

namespace AiSpeak;

public class Synthesizer
{
    private static ElevenLabsClient? _client;

    private static ElevenLabsClient Client
    {
        get
        {
            return _client ??= new ElevenLabsClient(new ElevenLabsAuthentication(UserSettings.Instance.ElevenLabsApiKey));
        }
    }
    
    private readonly UserSettings _userSettings = UserSettings.Instance;

    private Voice? _voice;
    private Model? _model;
    private VoiceSettings? _voiceSettings;
    private readonly string? _voiceId;
    private IReadOnlyList<Voice>? _voicesCache;

    public Synthesizer(string? voiceId, Model voiceModel)
    {
        _voiceId = voiceId;
        _model = voiceModel;

        if (_userSettings.CachedVoices.Count > 0)
        {
            _voicesCache = _userSettings.CachedVoices;
        }
    }

    public void SetVoice(Voice voice)
    {
        _voice = voice;
    }

    public void SetModel(Model model)
    {
        _model = model;
    }

    public async Task<string> SynthesizeText(string text)
    {
        if (_voice is null)
        {
            if (_voiceId is null)
            {
                _voice = (await Client.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            }
            else
            {
                _voice = await Client.VoicesEndpoint.GetVoiceAsync(_voiceId);
            }
        }

        _voiceSettings ??= await Client.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        
        var clipPath = await Client.TextToSpeechEndpoint.TextToSpeechAsync(text, _voice, _voiceSettings, _model);
        return clipPath;
    }

    public async Task<IReadOnlyList<Model>> GetModels()
    {
        var models = await Client.ModelsEndpoint.GetModelsAsync();
        
        foreach (var model in models)
        {
            Debug.WriteLine(model.Id);
        }
        
        return models;
    }

    public async Task<IReadOnlyList<Voice>> GetVoices()
    {
        if (_voicesCache is not null)
        {
            return _voicesCache;
        }
        
        var gottenVoices = await Client.VoicesEndpoint.GetAllVoicesAsync();
        _userSettings.CachedVoices = gottenVoices.ToList();
        _voicesCache = _userSettings.CachedVoices;
        
        return _voicesCache;
    }
}