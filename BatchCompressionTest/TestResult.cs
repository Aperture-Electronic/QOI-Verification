namespace BatchCompressionTest;

public class TestResult
{
    public string FileName { get; private set; }

    public string FileClass
    {
        get
        {
            string[] fileNameSplit = Path.GetFileNameWithoutExtension(FileName).Split("___");
            return fileNameSplit.Length > 1 ? fileNameSplit.First() : string.Empty;
        }
    }
    
    public EncodingStatistic Statistic { get; private set; }
    public CompressionInfo CompressInfo { get; private set; }

    public TestResult(string fileName, EncodingStatistic stat, CompressionInfo compressInfo)
    {
        FileName = fileName;
        Statistic = stat;
        CompressInfo = compressInfo;
    }
}