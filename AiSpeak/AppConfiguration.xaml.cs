using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

namespace AiSpeak;

public partial class AppConfiguration : Window
{
    private readonly UserSettings _userSettings = UserSettings.Instance;

    private string _googleCloudKey = "";
    private string _elevenLabsApiKey = "";
    private string _transcriberApiUrl = "";
    private string _transcriberApiToken = "";
    
    private string GoogleCloudKey
    {
        get => _googleCloudKey;
        set
        {
            _googleCloudKey = value;
            TxtGoogleCloudKey.Text = value;
        }
    }
    
    private string ElevenLabsApiKey
    {
        get => _elevenLabsApiKey;
        init => _elevenLabsApiKey = value;
    }
    
    private string TranscriberApiUrl
    {
        get => _transcriberApiUrl;
        init => _transcriberApiUrl = value;
    }
    
    private string TranscriberApiToken
    {
        get => _transcriberApiToken;
        init => _transcriberApiToken = value;
    }
    
    public AppConfiguration()
    {
        InitializeComponent();

        GoogleCloudKey = _userSettings.GoogleCloudKey;
        ElevenLabsApiKey = _userSettings.ElevenLabsApiKey;
        TranscriberApiUrl = _userSettings.Transcriber.ApiUrl;
        TranscriberApiToken = _userSettings.Transcriber.ApiToken;

        TxtBoxElevenLabsApiKey.Text = ElevenLabsApiKey;
        TxtBoxTranscriberApiUrl.Text = TranscriberApiUrl;
        TxtBoxTranscriberApiToken.Text = TranscriberApiToken;

        TxtBoxElevenLabsApiKey.TextChanged += (sender, args) =>
        {
            _elevenLabsApiKey = TxtBoxElevenLabsApiKey.Text;
        };

        TxtBoxTranscriberApiUrl.TextChanged += (sender, args) =>
        {
            _transcriberApiUrl = TxtBoxTranscriberApiUrl.Text;
        };

        TxtBoxTranscriberApiToken.TextChanged += (sender, args) =>
        {
            _transcriberApiToken = TxtBoxTranscriberApiToken.Text;
        };

        BtnOpenFile.Click += (_, _) =>
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.ShowReadOnly = false;
            fileDialog.Filter = "JSON Files |*.json";
            fileDialog.FileName = GoogleCloudKey;
            if (fileDialog.ShowDialog() == true)
            {
                GoogleCloudKey = fileDialog.FileName;
            }
        };

        BtnCancel.Click += (_, _) =>
        {
            DialogResult = false;
        };

        BtnOk.Click += (_, _) =>
        {
            _userSettings.GoogleCloudKey = GoogleCloudKey;
            _userSettings.ElevenLabsApiKey = ElevenLabsApiKey;
            _userSettings.Transcriber.ApiUrl = TranscriberApiUrl;
            _userSettings.Transcriber.ApiToken = TranscriberApiToken;
            UserSettings.Save();
            DialogResult = true;
        };
    }
}