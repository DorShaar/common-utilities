using Newtonsoft.Json;

namespace JsonSerialization;

public class JsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings mSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
    
    public async Task SerializeAsync<T>(T objectToSerialize, string filePath, CancellationToken cancellationToken)
    {
        string jsonText = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, jsonText, cancellationToken);
        Console.WriteLine($"Serialized object {typeof(T)} into file '{filePath}'");
    }

    public async Task<T> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken)
    {
        string text = await File.ReadAllTextAsync(filePath, cancellationToken);
        T deserializedObject = JsonConvert.DeserializeObject<T>(text, mSettings)
                               ?? throw new NullReferenceException($"Failed to deserialize '{filePath}'");
        Console.WriteLine($"Deserialized object {typeof(T)} from {filePath}");
            
        return deserializedObject;
    }

    public void AddConverters(JsonConverter jsonConverter)
    {
        mSettings.Converters.Add(jsonConverter);
    }
}