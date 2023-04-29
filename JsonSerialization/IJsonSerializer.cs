using Newtonsoft.Json;

namespace JsonSerialization;

public interface IJsonSerializer
{
    void Serialize<T>(T objectToSerialize, string outputPath);
    T Deserialize<T>(string inputPath);
    void AddConverters(JsonConverter jsonConverter);
}