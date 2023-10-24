using GregsStack.InputSimulatorStandard;
using GregsStack.InputSimulatorStandard.Native;
using NAudio.Wave;
using WinUtilities.Keyboard.Enums;

namespace AiSpeak;

public class AudioPlayer
{
    private int _outputDeviceId;
    private WaveOutEvent? _outputDevice;
    private AudioFileReader? _audioFile;
    private Key _keyBindOut;
    private bool _keyBindOutEnabled;
    private InputSimulator _inputSimulator = new InputSimulator();
    private bool _isKeyDown = false;

    public AudioPlayer(int outputDeviceId = -1, Key keyBindOut = Key.V, bool pressKeyWhenPlaying = false)
    {
        _outputDeviceId = outputDeviceId;
        _keyBindOut = keyBindOut;
        _keyBindOutEnabled = pressKeyWhenPlaying;
    }

    public void SetOutputDevice(int outputDeviceId)
    {
        _outputDeviceId = outputDeviceId;
    }

    public void SetKeyBindOut(Key key)
    {
        if (_isKeyDown)
        {
            _inputSimulator.Keyboard.KeyUp((VirtualKeyCode) _keyBindOut);
            _isKeyDown = false;
        }
        _keyBindOut = key;
    }

    public void SetKeyBindOutEnabled(bool enabled)
    {
        _keyBindOutEnabled = enabled;
    }

    public void PlayFile(string filePath)
    {
        if (_outputDevice == null)
        {
            _outputDevice = new WaveOutEvent() {DeviceNumber = _outputDeviceId};
            _outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        if (_audioFile == null)
        {
            _audioFile = new AudioFileReader(filePath);
            _outputDevice.Init(_audioFile);
        }

        if (_keyBindOutEnabled)
        {
            _isKeyDown = true;
            _inputSimulator.Keyboard.KeyDown((VirtualKeyCode) _keyBindOut);
        }
        _outputDevice.Play();
    }

    public void StopPlayback()
    {
        _outputDevice?.Stop();
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs args)
    {
        if (_isKeyDown)
        {
            _inputSimulator.Keyboard.KeyUp((VirtualKeyCode) _keyBindOut);
            _isKeyDown = false;
        }
        _outputDevice?.Dispose();
        _outputDevice = null;
        _audioFile?.Dispose();
        _audioFile = null;
    }
}