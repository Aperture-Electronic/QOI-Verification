namespace BatchCompressionTest;

public class CompressionInfo
{
    public long SourceSize { get; private set; }
    public long CompressedSize { get; private set; }

    public float CompressionRatio => (float)CompressedSize / SourceSize;

    public CompressionInfo(long sourceSize, long compressedSize)
    {
        SourceSize = sourceSize;
        CompressedSize = compressedSize;

        // if (compressedSize > sourceSize)
        // {
        //     throw new Exception("Compressed size over the source size.");
        // }
    }
}