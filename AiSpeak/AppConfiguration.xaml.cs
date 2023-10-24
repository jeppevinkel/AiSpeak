using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;

namespace AiSpeak;

public partial class AppConfiguration : Window
{
    private readonly UserSettings _userSettings = UserSettings.Instance;

    private string _googleCloudKey;
    private string _elevenLabsApiKey;
    
    private string GoogleCloudKey
    {
        get
        {
            return _googleCloudKey;
        }
        set
        {
            _googleCloudKey = value;
            TxtGoogleCloudKey.Text = value;
        }
    }
    
    private string ElevenLabsApiKey
    {
        get
        {
            return _elevenLabsApiKey;
        }
        set
        {
            _elevenLabsApiKey = value;
        }
    }
    
    public AppConfiguration()
    {
        InitializeComponent();

        GoogleCloudKey = _userSettings.GoogleCloudKey;
        ElevenLabsApiKey = _userSettings.ElevenLabsApiKey;

        TxtBoxElevenLabsApiKey.Text = ElevenLabsApiKey;

        TxtBoxElevenLabsApiKey.TextChanged += (sender, args) =>
        {
            _elevenLabsApiKey = TxtBoxElevenLabsApiKey.Text;
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
            UserSettings.Save();
            DialogResult = true;
        };
    }
}