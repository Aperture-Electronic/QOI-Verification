using System.Collections.Concurrent;
using System.Diagnostics;
using System.DrawingCore;

namespace BatchCompressionTest
{
    public static class Program
    {
        private const string compressCmdLine = "-e";
        private const string decompressCmdLine = "-d";
        private const string compareCmdLine = "-c";
        private const string tempFilePath = "temp";
        private const string tempFileName = "temp";
        private const string qoiStatisticConsoleOut = "-- QOI Encoding Statistic --";
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Batch Compression Performance Test");
            Console.WriteLine("dotnet batch_test.dll <test program> <test set path>");

            if (args.Length < 2)
            {
                return;
            }
            
            string testProgram = args[0];
            string testSetPath = args[1];

            string[] files = Directory.GetFiles(testSetPath, "*.bmp");

            int count = files.Length, i = 0, encodeErrorCount = 0;
            ConcurrentBag<TestResult> testResults = new ConcurrentBag<TestResult>();

            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, tempFilePath)))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, tempFilePath));
            }
            
            Parallel.ForEach(files, delegate(string file)
            {
                // 0. Reconstruction the temp path
                int threadId = Thread.CurrentThread.ManagedThreadId;
                string qoiFileName = $"{tempFileName}_{threadId}.qoi";
                string bmpFileName = $"{tempFileName}_{threadId}.bmp";
                string tempEncodeOutputFilePath = Path.Combine(Environment.CurrentDirectory, tempFilePath, qoiFileName);
                string tempDecodeOutputFilePath = Path.Combine(Environment.CurrentDirectory, tempFilePath, bmpFileName);
                
                // 1. Compression
                ProgramResult compressResult = RunProgram(testProgram, 
                    compressCmdLine, 
                    file, 
                    tempEncodeOutputFilePath);

                // 2. Decompression
                ProgramResult decompressResult = RunProgram(testProgram, 
                    decompressCmdLine, 
                    tempEncodeOutputFilePath, 
                    tempDecodeOutputFilePath);

                // 3. Compare
                ProgramResult compareResult = RunProgram(testProgram, 
                    compareCmdLine, 
                    file, 
                    tempDecodeOutputFilePath);

                // 4. Check result
                if (compressResult.ExitCode != 0)
                {
                    // Error when compress
                    Console.WriteLine($"Compression test error on {file}.");
                    return;
                }

                if (decompressResult.ExitCode != 0)
                {
                    // Error when decompress
                    Console.WriteLine($"Decompression test error on {file}.");
                    return;
                }

                if (compareResult.ExitCode != 0)
                {
                    // Error when compare
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Unmatched image between source and decompressed! ({file})");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Interlocked.Increment(ref encodeErrorCount);
                    Interlocked.Increment(ref i);
                    return;
                }

                // 4. Calculation
                CompressionInfo compressionInfo = CalculateCompressRatio(file, tempEncodeOutputFilePath);
                EncodingStatistic encodingStatistic = GetEncodingStatistic(compressResult);

                // 5. Collect test result
                testResults.Add(new TestResult(file, encodingStatistic, compressionInfo));

                Console.WriteLine($"{i} of {count}");
                Interlocked.Increment(ref i);
            });
            
            // Statistic and final report
            int resultsCount = testResults.Count;
            QOIEncoding[] encodingTypes = Enum.GetValues<QOIEncoding>();

            float averageCompressionRatio = 0;
            EncodingStatistic totalStatistic = new();
            
            foreach (TestResult result in testResults)
            {
                averageCompressionRatio += result.CompressInfo.CompressionRatio;
                totalStatistic += result.Statistic;
            }

            averageCompressionRatio /= resultsCount;

            int encodingCount = totalStatistic.Sum;

            Console.WriteLine("-- QOI Batch Compression Test Result Report --");
            Console.WriteLine("Pixel format of all images in test set are 24-bit RGB, no Alpha channel.");
            Console.WriteLine($"{i} of {count} file(s) has been tested. {count - i} file(s) can not be opened and tested.");
            Console.WriteLine($"{encodeErrorCount} file(s) has difference between source and decoded.");
            Console.WriteLine($"Average compression ratio: {averageCompressionRatio * 100:F2}%");
            Console.WriteLine($"Statistic of each encoding, total {encodingCount}");
            foreach (QOIEncoding encoding in encodingTypes)
            {
                Console.WriteLine($"\t{Enum.GetName(typeof(QOIEncoding), encoding)}\t: " +
                                  $"{totalStatistic[encoding]}\t({(float)totalStatistic[encoding] / encodingCount * 100:F2}%)");
            }

            Console.WriteLine("-- Statistics Grouped by Image Class --");
            // Grouping the class of images
            IEnumerable<IGrouping<string, TestResult>> imageClassGroup = from result in testResults 
                group result by result.FileClass into g select g;

            foreach (IGrouping<string, TestResult> imageClass in imageClassGroup)
            {
                float averageCompressionRatioClass = 0;
                EncodingStatistic totalStatisticClass = new();
                foreach (TestResult result in imageClass)
                {
                    averageCompressionRatioClass += result.CompressInfo.CompressionRatio;
                    totalStatisticClass += result.Statistic;
                }
                
                averageCompressionRatioClass /= imageClass.Count();
                int encodingCountClass = totalStatisticClass.Sum;
                string encodingStatClassString = encodingTypes.Aggregate(string.Empty, (current, encoding) => 
                    current + ($"\n\t{Enum.GetName(typeof(QOIEncoding), encoding)}\t= " + 
                               $"{totalStatisticClass[encoding]}\t({(float)totalStatisticClass[encoding] / encodingCountClass * 100:F2}%)"));

                Console.WriteLine($"[{imageClass.Key}]\n" +
                                  $"Ratio = {averageCompressionRatioClass * 100:F2}%" +
                                  $"{encodingStatClassString}");
            }
        }

        private static ProgramResult RunProgram(string program, string command, string source, string target)
        {
            string commandLine = $"{command} \"{source}\" \"{target}\"";
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = program,
                Arguments = commandLine,
                RedirectStandardOutput = true,
            };
                
            Process testProcess = new Process()
            {
                StartInfo = processStartInfo
            };
                
            testProcess.Start();

            string consoleOutput = testProcess.StandardOutput.ReadToEnd();
            testProcess.WaitForExit();
            int returnCode = testProcess.ExitCode;

            return new ProgramResult(consoleOutput, returnCode);
        }

        private static long GetQOIBlockLength(string path)
        {
            FileStream fs = File.OpenRead(path);
            fs.Seek(2 * sizeof(uint), SeekOrigin.Begin);
            byte[] buffer = new byte[4];
            int rdLength = 0;
            const int totalLength = 4;
            
            do
            {
                rdLength += fs.Read(buffer, rdLength, totalLength - rdLength);
            } while (rdLength != totalLength);

            fs.Close();
            
            return BitConverter.IsLittleEndian ? BitConverter.ToInt32(buffer) : BitConverter.ToInt32(buffer.Reverse().ToArray());
        }

        private static long GetImageBlockLength(string path)
        {
            Bitmap bitmap = new Bitmap(path);
            int width = bitmap.Width;
            int height = bitmap.Height;
            return width * height * 3;
        }
        
        private static CompressionInfo CalculateCompressRatio(string file, string outputFile)
        {
            string tempEncodeOutputFilePath = Path.Combine(Environment.CurrentDirectory, outputFile);
            long sourceFileLength = GetImageBlockLength(file);
            long encodedFileLength = GetQOIBlockLength(tempEncodeOutputFilePath);

            return new CompressionInfo(sourceFileLength, encodedFileLength);
        }

        private static EncodingStatistic GetEncodingStatistic(ProgramResult pgResult)
        {
            EncodingStatistic stat = new EncodingStatistic();
            string consoleOutput = pgResult.StandardOut;
            int startIndex = consoleOutput.IndexOf(qoiStatisticConsoleOut, StringComparison.Ordinal);
            string statReport = consoleOutput[startIndex..];
            string[] reportLines = statReport.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int length = reportLines.Length;
            for (int i = 1; i < length; i++)
            {
                string line = reportLines[i];
                string[] equalSides = line.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (equalSides.Length != 2)
                {
                    break;
                }

                string name = equalSides[0];
                string valueString = equalSides[1];
                int.TryParse(valueString, out int value);
                
                stat.SetCounterByName(name, value);
            }
            
            return stat;
        }
    }
}