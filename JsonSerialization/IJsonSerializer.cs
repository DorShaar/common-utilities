using Newtonsoft.Json;

namespace JsonSerialization;

public interface IJsonSerializer
{
    Task SerializeAsync<T>(T objectToSerialize, string filePath, CancellationToken cancellationToken);
    Task<T> DeserializeAsync<T>(string filePath, CancellationToken cancellationToken);
    void AddConverters(JsonConverter jsonConverter);
}