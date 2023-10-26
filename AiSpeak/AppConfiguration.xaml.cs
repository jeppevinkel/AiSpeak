using Microsoft.Win32;

namespace AiSpeak;

public partial class AppConfiguration
{
    private readonly UserSettings _userSettings = UserSettings.Instance;

    private string _googleCloudKey = "";
    private string _elevenLabsApiKey = "";
    private string _transcriberApiUrl = "";
    private string _transcriberApiToken = "";
    private bool _standaloneMode = true;

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
        set => _elevenLabsApiKey = value;
    }

    private string TranscriberApiUrl
    {
        get => _transcriberApiUrl;
        set => _transcriberApiUrl = value;
    }

    private string TranscriberApiToken
    {
        get => _transcriberApiToken;
        set => _transcriberApiToken = value;
    }

    private bool StandaloneMode
    {
        get => _standaloneMode;
        set
        {
            _standaloneMode = value;

            TxtBoxTranscriberApiUrl.IsEnabled = !value;
            TxtBoxTranscriberApiToken.IsEnabled = !value;
        }
    }

    public AppConfiguration()
    {
        InitializeComponent();

        GoogleCloudKey = _userSettings.GoogleCloudKey;
        ElevenLabsApiKey = _userSettings.ElevenLabsApiKey;
        TranscriberApiUrl = _userSettings.Transcriber.ApiUrl;
        TranscriberApiToken = _userSettings.Transcriber.ApiToken;
        StandaloneMode = _userSettings.StandaloneMode;

        TxtBoxElevenLabsApiKey.Text = ElevenLabsApiKey;
        TxtBoxTranscriberApiUrl.Text = TranscriberApiUrl;
        TxtBoxTranscriberApiToken.Text = TranscriberApiToken;
        CheckBoxStandaloneMode.IsChecked = StandaloneMode;

        TxtBoxElevenLabsApiKey.TextChanged += (_, _) => { ElevenLabsApiKey = TxtBoxElevenLabsApiKey.Text; };

        TxtBoxTranscriberApiUrl.TextChanged += (_, _) => { TranscriberApiUrl = TxtBoxTranscriberApiUrl.Text; };

        TxtBoxTranscriberApiToken.TextChanged += (_, _) =>
        {
            TranscriberApiToken = TxtBoxTranscriberApiToken.Text;
        };

        CheckBoxStandaloneMode.Checked += (_, _) =>
        {
            StandaloneMode = true;
        };
        CheckBoxStandaloneMode.Unchecked += (_, _) =>
        {
            StandaloneMode = false;
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

        BtnCancel.Click += (_, _) => { DialogResult = false; };

        BtnOk.Click += (_, _) =>
        {
            _userSettings.GoogleCloudKey = GoogleCloudKey;
            _userSettings.ElevenLabsApiKey = ElevenLabsApiKey;
            _userSettings.Transcriber.ApiUrl = TranscriberApiUrl;
            _userSettings.Transcriber.ApiToken = TranscriberApiToken;
            _userSettings.StandaloneMode = StandaloneMode;
            UserSettings.Save();
            DialogResult = true;
        };
    }
}