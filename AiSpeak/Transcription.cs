namespace AiSpeak;

public record TranscriptionResponse(Result[] Results);

public record Result(string Filename, string Transcript);