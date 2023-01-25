namespace BatchCompressionTest;

public class ProgramResult
{
    public string StandardOut { get; private set; }
    public int ExitCode { get; private set; }

    public ProgramResult(string standardOut, int exitCode)
    {
        StandardOut = standardOut;
        ExitCode = exitCode;
    }
}