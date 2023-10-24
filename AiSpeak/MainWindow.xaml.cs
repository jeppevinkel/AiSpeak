using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ElevenLabs.Models;
using ElevenLabs.Voices;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using NAudio.Wave;
using WinUtilities.Keyboard;
using WinUtilities.Keyboard.Enums;
using WinUtilities.Keyboard.EventArgs;

namespace AiSpeak
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserSettings _userSettings = UserSettings.Instance;
        private readonly Recorder _recorder = new ();
        private readonly AudioPlayer _audioPlayer;
        private readonly Synthesizer _synthesizer;

        public MainWindow()
        {
            InitializeComponent();

            List<AudioDeviceEntry> audioDeviceEntries = new();
            List<LanguageEntry> languageEntries = new();
            IReadOnlyList<Voice> voiceEntries;
            IReadOnlyList<Model> modelEntries;
            List<KeyBindEntry> keyBindEntries = new();

            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                Debug.WriteLine($"{n}: {caps.ProductName}");
                audioDeviceEntries.Add(new AudioDeviceEntry(caps.ProductName, n));
            }

            foreach (var fieldInfo in GetConstants(typeof(LanguageCodes)))
            {
                languageEntries.Add(new LanguageEntry(fieldInfo.Name,
                    (string?) fieldInfo.GetRawConstantValue() ?? string.Empty));
            }

            if (audioDeviceEntries.All(audioDeviceEntry => audioDeviceEntry.Id != _userSettings.AudioDevice))
            {
                _userSettings.AudioDevice = -1;
            }

            foreach (var keyName in Enum.GetNames<Key>())
            {
                if(!Enum.TryParse(keyName, out Key value)) continue;
                keyBindEntries.Add(new KeyBindEntry(keyName,
                    value));
            }

            AudioEntrySelection.ItemsSource = audioDeviceEntries;
            AudioEntrySelection.DisplayMemberPath = "Name";
            AudioEntrySelection.SelectedValuePath = "Id";
            AudioEntrySelection.SelectedValue = _userSettings.AudioDevice;

            LanguageSelection.ItemsSource = languageEntries;
            LanguageSelection.DisplayMemberPath = "Name";
            LanguageSelection.SelectedValuePath = "Code";
            LanguageSelection.SelectedValue = _userSettings.Language;

            KeyBindSelection.ItemsSource = keyBindEntries;
            KeyBindSelection.DisplayMemberPath = "Name";
            KeyBindSelection.SelectedValuePath = "Id";
            KeyBindSelection.SelectedValue = _userSettings.SelectedKeyBind;

            KeyBindOutSelection.ItemsSource = keyBindEntries;
            KeyBindOutSelection.DisplayMemberPath = "Name";
            KeyBindOutSelection.SelectedValuePath = "Id";
            KeyBindOutSelection.SelectedValue = _userSettings.SelectedKeyBindOut;

            KeyBindOutCheckBox.IsChecked = _userSettings.KeyBindOutEnabled;

            // var synthesizer = new Synthesizer("x54B9HhFq9eENQmTm1o8");
            _synthesizer = new Synthesizer(_userSettings.SelectedVoice?.Id, _userSettings.SelectedModel);
            _audioPlayer = new AudioPlayer(_userSettings.AudioDevice, _userSettings.SelectedKeyBindOut, _userSettings.KeyBindOutEnabled);

            Task.Run(async () =>
            {
                Debug.WriteLine("Getting voices...");
                var voices = await _synthesizer.GetVoices();
                Debug.WriteLine("Gotten voices!");

                voiceEntries = voices;

                Dispatcher.Invoke(() =>
                {
                    VoiceSelection.ItemsSource = voiceEntries;
                    VoiceSelection.DisplayMemberPath = "Name";
                    VoiceSelection.SelectedValuePath = "Id";
                    VoiceSelection.SelectedValue = _userSettings.SelectedVoice;
                });
                
                
                Debug.WriteLine("Getting models...");
                var models = await _synthesizer.GetModels();
                Debug.WriteLine("Gotten models!");
                
                modelEntries = models;

                Dispatcher.Invoke(() =>
                {
                    ModelSelection.ItemsSource = modelEntries;
                    ModelSelection.DisplayMemberPath = "Name";
                    ModelSelection.SelectedValuePath = "Id";
                    ModelSelection.SelectedValue = _userSettings.SelectedModel;
                });
            });

            AudioEntrySelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.AudioDevice = (int) AudioEntrySelection.SelectedValue;
                _audioPlayer.SetOutputDevice((int) AudioEntrySelection.SelectedValue);
                Debug.WriteLine($"Device set to {(int) AudioEntrySelection.SelectedValue}");
                UserSettings.Save();
            };

            LanguageSelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.Language = (string) LanguageSelection.SelectedValue;
                Debug.WriteLine($"Language set to {LanguageSelection.SelectedValue}");
                UserSettings.Save();
            };

            VoiceSelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.SelectedVoice = (Voice) VoiceSelection.SelectedItem;
                _synthesizer.SetVoice(_userSettings.SelectedVoice);
                Debug.WriteLine($"Voice set to {VoiceSelection.SelectedValue}");
                UserSettings.Save();
            };

            ModelSelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.SelectedModel = (Model) ModelSelection.SelectedItem;
                _synthesizer.SetModel(_userSettings.SelectedModel);
                Debug.WriteLine($"Model set to {ModelSelection.SelectedValue}");
                UserSettings.Save();
            };

            KeyBindSelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.SelectedKeyBind = (Key) KeyBindSelection.SelectedValue;
                Debug.WriteLine($"KeyBind set to {KeyBindSelection.SelectedValue}");
                UserSettings.Save();
            };

            KeyBindOutSelection.SelectionChanged += (sender, args) =>
            {
                _userSettings.SelectedKeyBindOut = (Key) KeyBindOutSelection.SelectedValue;
                _audioPlayer.SetKeyBindOut(_userSettings.SelectedKeyBindOut);
                Debug.WriteLine($"KeyBindOut set to {KeyBindOutSelection.SelectedValue}");
                UserSettings.Save();
            };

            KeyBindOutCheckBox.Checked += (sender, args) =>
            {
                _userSettings.KeyBindOutEnabled = true;
                _audioPlayer.SetKeyBindOutEnabled(true);
                Debug.WriteLine($"KeyBindOut enabled");
                UserSettings.Save();
            };
            KeyBindOutCheckBox.Unchecked += (sender, args) =>
            {
                _userSettings.KeyBindOutEnabled = false;
                _audioPlayer.SetKeyBindOutEnabled(false);
                Debug.WriteLine($"KeyBindOut disabled");
                UserSettings.Save();
            };

            _recorder.StoppedRecordingEvent += (sender, args) =>
            {
                Debug.WriteLine("Stopped recording.");
                var outputFile = args.FilePath;
                LatestRecording.Text = outputFile;
                
                Task.Run(async () =>
                {
                    Debug.WriteLine("Start transcribing...");
                    var transcription = await Transcriber.Transcribe(outputFile);
                    Debug.WriteLine("Finished transcribing!");

                    Dispatcher.Invoke(() => { TranscribedText.Text = transcription; });

                    var client =
                        TranslationClient.Create(
                            GoogleCredential.FromFile(_userSettings.GoogleCloudKey));
                    var response = client.TranslateText(transcription, _userSettings.Language);

                    Dispatcher.Invoke(() => { TranslatedText.Text = response.TranslatedText; });

                    var clipPath = await _synthesizer.SynthesizeText(response.TranslatedText);

                    Dispatcher.Invoke(() => { _audioPlayer.PlayFile(clipPath); });
                    Debug.WriteLine("Finished handling recording.");
                });
            };
            
            // Subscribe to the key press event.
            KeyboardManager.Instance.OnKeyPressEvent += OnKeyPressEvent;
            // Subscribe to the key release event.
            KeyboardManager.Instance.OnKeyReleaseEvent += OnKeyReleaseEvent;

            BtnStartRecording.Click += (_, _) => StartRecording();
            BtnStopRecording.Click += (_, _) => StopRecording();

            BtnSettings.Click += (_, _) =>
            {
                var settingsDialog = new AppConfiguration();

                settingsDialog.ShowDialog();
            };
        }
        
        private void OnKeyPressEvent(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyCode == _userSettings.SelectedKeyBind)
            {
                StartRecording();
            }
        }

        private void OnKeyReleaseEvent(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.KeyCode == _userSettings.SelectedKeyBind)
            {
                StopRecording();
            }
        }

        public bool StartRecording()
        {
            return _recorder.StartRecording();
        }

        public bool StopRecording()
        {
            return _recorder.StopRecording();
        }

        private List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                                                    BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
    }

    public class AudioDeviceEntry
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public AudioDeviceEntry(string name, int id)
        {
            Name = name;
            Id = id;
        }

        public override string ToString()
        {
            return $"{Id}: {Name}";
        }
    }

    public class LanguageEntry
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public LanguageEntry(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return $"{Name}: {Code}";
        }
    }
    
    public class KeyBindEntry
    {
        public string Name { get; set; }
        public Key Id { get; set; }

        public KeyBindEntry(string name, Key id)
        {
            Name = name;
            Id = id;
        }

        public override string ToString()
        {
            return $"{(int)Id}: {Name}";
        }
    }
}