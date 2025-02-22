namespace BytesHandler
{
    public class BytesReader
    {
        /// <summary>
        /// When buffer is in the next structre: <size_1><bytes_1><size_2><bytes_2>...<size_n><bytes_n>
        /// </summary>
        public static List<byte[]> ReadBytesFromBuffer(byte[] buffer)
        {
            List<byte[]> subBuffers = [];
            int sourceOffset = 0;
            while (sourceOffset < buffer.Length)
            {
                byte[] sizeInBytes = new byte[4];
                Buffer.BlockCopy(buffer, sourceOffset, sizeInBytes, 0, sizeInBytes.Length);
                sourceOffset += sizeInBytes.Length;

                int bytesLength = BitConverter.ToInt32(sizeInBytes, 0);
                byte[] subBuffer = new byte[bytesLength];
                Buffer.BlockCopy(buffer, sourceOffset, subBuffer, 0, subBuffer.Length);
                sourceOffset += subBuffer.Length;

                subBuffers.Add(subBuffer);
            }

            return subBuffers;
        }
    }
}
