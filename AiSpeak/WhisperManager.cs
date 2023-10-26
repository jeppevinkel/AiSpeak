using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

namespace AiSpeak;

public class WhisperManager : IAsyncDisposable, IDisposable
{
    private WhisperProcessor? _whisperProcessor;

    public async Task<WhisperTranscription> Transcribe(Stream waveStream, CancellationToken cancellationToken = default)
    {
        if (_whisperProcessor is null)
        {
            await LoadModel(cancellationToken: cancellationToken);
        }
        
        var segments = _whisperProcessor!.ProcessAsync(waveStream, cancellationToken);
        var sb = new StringBuilder();
        var transcription = new WhisperTranscription();

        await foreach (var segment in segments)
        {
            sb.Append(segment.Text);
            transcription.Languages.Add(segment.Language);
        }

        transcription.Text = sb.ToString();

        return transcription;
    }

    public async Task LoadModel(string modelName = "ggml-medium.bin", CancellationToken cancellationToken = default)
    {
        if (_whisperProcessor is not null) throw new ConstraintException("Only one model can be loaded at a time.");
        if (!File.Exists(modelName)) await DownloadModel(modelName, cancellationToken);
        
        using var whisperFactory = WhisperFactory.FromPath(modelName);
        var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        _whisperProcessor = processor;
    }

    private async Task DownloadModel(string modelName, CancellationToken cancellationToken = default)
    {
        
        await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base, QuantizationType.NoQuantization, cancellationToken);
        await using var fileWriter = File.OpenWrite(modelName);
        await modelStream.CopyToAsync(fileWriter, cancellationToken);
    }

    public void Dispose()
    {
        _whisperProcessor?.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_whisperProcessor is null) return;
        await _whisperProcessor.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}