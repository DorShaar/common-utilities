namespace BytesHandler
{
    public static class BytesWriter
    {
        /// <summary>
        /// Writting to buffer several byte[] with suffix of their size in order to read them according to their size.
        /// </summary>
        public static async Task<byte[]> WriteFilesToBuffer(Dictionary<string, int> fileToSizeMap,
            CancellationToken cancellationToken)
        {
            byte[] buffer = prepareBuffer(fileToSizeMap);
            int bufferOffset = 0;

            foreach ((string fileToUpload, int _) in fileToSizeMap)
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(fileToUpload, cancellationToken).ConfigureAwait(false);
                byte[] sizeInBytesRepresentation = BitConverter.GetBytes(fileBytes.Length);

                Buffer.BlockCopy(sizeInBytesRepresentation, 0, buffer, bufferOffset, sizeInBytesRepresentation.Length);
                bufferOffset += sizeInBytesRepresentation.Length;

                Buffer.BlockCopy(fileBytes, 0, buffer, bufferOffset, fileBytes.Length);
                bufferOffset += fileBytes.Length;
            }

            return buffer;
        }

        private static byte[] prepareBuffer(Dictionary<string, int> fileToSizeMap)
        {
            long totalBufferSize = 0;
            foreach ((string _, int size) in fileToSizeMap)
            {
                totalBufferSize += size + 4; // 4 for the size represented in 4 bytes.
            }

            return new byte[totalBufferSize];
        }
    }
}
