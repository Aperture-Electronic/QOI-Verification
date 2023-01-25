namespace BatchCompressionTest;

public class EncodingStatistic
{
    private readonly Dictionary<QOIEncoding, int> counter;
    private readonly string[] names;

    public int this[QOIEncoding index] => counter[index];

    public void SetCounterByName(string name, int value)
    {
        if (!names.Contains(name)) return;
        QOIEncoding encoding = (QOIEncoding)Enum.Parse(typeof(QOIEncoding), name);
        counter[encoding] = value;
    }

    private void SetCounterByEncoding(QOIEncoding encoding, int value) => counter[encoding] = value;

    public static EncodingStatistic operator +(EncodingStatistic a, EncodingStatistic b)
    {
        QOIEncoding[] encodingTypes = Enum.GetValues<QOIEncoding>();
        EncodingStatistic newStat = new EncodingStatistic();
        foreach (QOIEncoding type in encodingTypes)
        {
            newStat.SetCounterByEncoding(type, a[type] + b[type]);
        }

        return newStat;
    }

    public int Sum => counter.Values.Sum();

    public EncodingStatistic()
    {
        counter = new Dictionary<QOIEncoding, int>();
        QOIEncoding[] encodingTypes = Enum.GetValues<QOIEncoding>();
        names = Enum.GetNames<QOIEncoding>();
        foreach (QOIEncoding type in encodingTypes)
        {
            counter.Add(type, 0);
        }
    }
}