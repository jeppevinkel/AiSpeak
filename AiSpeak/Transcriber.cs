using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AiSpeak;

public static class Transcriber
{
    private static HttpClient? _sharedClient;

    private static HttpClient SharedClient
    {
        get
        {
            if (_sharedClient is null)
            {
                _sharedClient = new()
                {
                    BaseAddress = new Uri("http://127.0.0.1:5000"),
                };
                _sharedClient.DefaultRequestHeaders.Add("Token", "flowglobe");
            }

            return _sharedClient;
        }
    }

    public static async Task<string> Transcribe(string filePath)
    {
        await Task.Delay(500);
        // using StringContent jsonContent = new(
        //     JsonSerializer.Serialize(new
        //     {
        //         filePath
        //     }),
        //     Encoding.UTF8,
        //     "application/json");

        // await using var stream = System.IO.File.OpenRead(filePath);
        // using Stream fileStreamContent = new FileStream(filePath, FileMode.Open);
        // MemoryStream ms = new MemoryStream();
        // await fileStreamContent.CopyToAsync(ms);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/");
        using var content = new MultipartFormDataContent();
        // {
        //     {new StreamContent(stream), "file", "Test.txt"}
        // };
        // content.Add(fileStreamContent, name: "file", fileName: "recording.wav");
        Debug.WriteLine("Opening file stream...");
        byte[] file = await System.IO.File.ReadAllBytesAsync(filePath);
        Debug.WriteLine("Opened file stream!");
        var byteArrayContent = new ByteArrayContent(file);
        content.Add(byteArrayContent, "file", "recording.wav");


        request.Content = content;

        Debug.WriteLine("Sending transcribe request...");
        using var responseMessage = await SharedClient.PostAsync("/", content);
        // using var responseMessage = await SharedClient.SendAsync(request);;
        // using var responseMessage = await SharedClient.PostAsync("", jsonContent);
        // stream.Close();
        Debug.WriteLine("Transcribe request sent!");

        responseMessage.EnsureSuccessStatusCode();

        var transcription = await responseMessage.Content.ReadFromJsonAsync<TranscriptionResponse>();

        return transcription is {Results.Length: > 0} ? transcription.Results[0].Transcript : "";
    }
}