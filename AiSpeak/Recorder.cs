using System;
using System.IO;
using NAudio.Wave;

namespace AiSpeak;

public class Recorder
{
    private readonly string _applicationFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AiSpeak");

    private readonly string _outputFolder;
    private readonly IWaveIn _waveIn;
    private WaveFileWriter? _waveFileWriter;
    private MemoryStream? _waveStream;
    private bool _isRecording;
    private Guid _fileId = Guid.Empty;

    public delegate void StoppedRecordingEventHandler(object sender, StoppedRecordingEventArgs eventArgs);

    public event StoppedRecordingEventHandler? StoppedRecordingEvent;

    public Recorder()
    {
        _outputFolder = Path.Combine(_applicationFolder, "cache");
        Directory.CreateDirectory(_outputFolder);
        _waveIn = new WaveInEvent();

        _waveIn.DataAvailable += (_, args) =>
        {
            if (_waveStream is not null)
            {
                _waveStream?.Write(args.Buffer, 0, args.BytesRecorded);
                if (_waveStream?.Position > _waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    _waveIn.StopRecording();
                }
            }
            else
            {
                _waveFileWriter?.Write(args.Buffer, 0, args.BytesRecorded);
                if (_waveFileWriter?.Position > _waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    _waveIn.StopRecording();
                }
            }
        };

        _waveIn.RecordingStopped += (_, _) =>
        {
            _waveFileWriter?.Dispose();
            _waveFileWriter = null;
            _waveStream = null;
        };
    }

    public bool StartRecording()
    {
        if (_isRecording)
            return false;

        _fileId = Guid.NewGuid();
        switch (UserSettings.Instance.StandaloneMode)
        {
            case true:
                _waveStream = new MemoryStream();
                break;
            case false:
                var outputFilePath = Path.Combine(_outputFolder, $"{_fileId}.wav");
                _waveFileWriter = new WaveFileWriter(outputFilePath, _waveIn.WaveFormat);
                break;
        }

        _waveIn.StartRecording();
        _isRecording = true;
        return true;
    }

    public bool StopRecording()
    {
        if (!_isRecording)
            return false;

        _waveIn.StopRecording();
        _isRecording = false;

        var eventArgs = new StoppedRecordingEventArgs(
            UserSettings.Instance.StandaloneMode ? null : Path.Combine(_outputFolder, $"{_fileId}.wav"),
            UserSettings.Instance.StandaloneMode ? _waveStream : null);

        StoppedRecordingEvent?.Invoke(this, eventArgs);

        return true;
    }
}

public class StoppedRecordingEventArgs
{
    public string? FilePath { get; }
    public MemoryStream? WaveStream { get; }

    public StoppedRecordingEventArgs(string? filePath = null, MemoryStream? waveStream = null)
    {
        FilePath = filePath;
        WaveStream = waveStream;
    }
}