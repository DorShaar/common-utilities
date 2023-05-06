using JsonSerialization;
using Xunit;

namespace Tests;

public class JsonSerializerTests
{
    [Fact]
    public async Task SerializeAndDeserialize_Success()
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
            await jsonSerializer.SerializeAsync(hashToFilePathDict, tempFile, CancellationToken.None).ConfigureAwait(false);
            Dictionary<string, List<string>> testedHashToFilePathDict =
                await jsonSerializer.DeserializeAsync<Dictionary<string, List<string>>>(tempFile, CancellationToken.None).ConfigureAwait(false);

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