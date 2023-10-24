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
    private WaveFileWriter? _waveFileWriter = null;
    private bool _isRecording = false;
    private Guid _fileId = Guid.Empty;
    
    public delegate void StoppedRecordingEventHandler(object sender, StoppedRecordingEventArgs eventArgs);

    public event StoppedRecordingEventHandler StoppedRecordingEvent;

    public Recorder()
    {
        _outputFolder = Path.Combine(_applicationFolder, "cache");
        Directory.CreateDirectory(_outputFolder);
        _waveIn = new WaveInEvent();

        _waveIn.DataAvailable += (sender, args) =>
        {
            _waveFileWriter?.Write(args.Buffer, 0, args.BytesRecorded);
            if (_waveFileWriter?.Position > _waveIn.WaveFormat.AverageBytesPerSecond * 30)
            {
                _waveIn.StopRecording();
            }
        };

        _waveIn.RecordingStopped += (sender, args) =>
        {
            _waveFileWriter?.Dispose();
            _waveFileWriter = null;
        };
    }

    public bool StartRecording()
    {
        if (_isRecording)
            return false;

        _fileId = Guid.NewGuid();
        var outputFilePath = Path.Combine(_outputFolder, $"{_fileId}.wav");
        _waveFileWriter = new WaveFileWriter(outputFilePath, _waveIn.WaveFormat);
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
        
        StoppedRecordingEvent?.Invoke(this, new StoppedRecordingEventArgs(Path.Combine(_outputFolder, $"{_fileId}.wav")));
        
        return true;
    }
}

public class StoppedRecordingEventArgs
{
    public string FilePath { get; }

    public StoppedRecordingEventArgs(string filePath)
    {
        FilePath = filePath;
    }
}