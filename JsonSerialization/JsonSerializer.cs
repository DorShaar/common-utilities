using Newtonsoft.Json;

namespace JsonSerialization;

public class JsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerSettings mSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
    };
    
    public void Serialize<T>(T objectToSerialize, string databasePath)
    {
        string jsonText = JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented);
        File.WriteAllText(databasePath, jsonText);
        Console.WriteLine($"Serialized object {typeof(T)} into {databasePath}");
    }

    public T Deserialize<T>(string databasePath)
    {
        T deserializedObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(databasePath), mSettings)
                               ?? throw new NullReferenceException($"Failed to deserialize '{databasePath}'");
        Console.WriteLine($"Deserialized object {typeof(T)} from {databasePath}");
            
        return deserializedObject;
    }

    public void AddConverters(JsonConverter jsonConverter)
    {
        mSettings.Converters.Add(jsonConverter);
    }
}