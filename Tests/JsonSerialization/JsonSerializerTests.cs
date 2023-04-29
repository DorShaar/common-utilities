using JsonSerialization;
using Xunit;

namespace Tests;

public class JsonSerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_Success()
    {
        JsonSerializer jsonSerializer = new();

        Dictionary<string, List<string>> hashToFilePathDict = new()
        {
            { "abc", new List<string> { "123", "456" } },
            { "def", new List<string> { "789", "456" } }
        };

        string tempFile = Guid.NewGuid().ToString();

        try
        {
            jsonSerializer.Serialize(hashToFilePathDict, tempFile);
            Dictionary<string, List<string>> testedHashToFilePathDict =
                jsonSerializer.Deserialize<Dictionary<string, List<string>>>(tempFile);

            Assert.Equal(hashToFilePathDict["abc"][0], testedHashToFilePathDict["abc"][0]);
            Assert.Equal(hashToFilePathDict["abc"][1], testedHashToFilePathDict["abc"][1]);
            Assert.Equal(hashToFilePathDict["def"][0], testedHashToFilePathDict["def"][0]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}