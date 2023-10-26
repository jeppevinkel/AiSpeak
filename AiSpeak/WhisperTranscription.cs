using System.Collections.Generic;
using System.Windows.Documents;

namespace AiSpeak;

public class WhisperTranscription
{
    public List<string> Languages { get; } = new ();
    public string Text { get; set; }
}