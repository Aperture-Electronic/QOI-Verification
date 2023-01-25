using System;
using System.DrawingCore;
using System.DrawingCore.Imaging;

namespace TestBitmapConverter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Test Bitmap Converter");

            string rootPath = ".";
            string outPath = "./out";
            if (args.Length > 0)
            {
                rootPath = args[0];
            }

            if (args.Length > 1)
            {
                outPath = args[1];
            }

            // Recreate output directory
            if (!Directory.Exists(outPath))
            {
                Directory.CreateDirectory(outPath);
            }
            else
            {
                Directory.Delete(outPath, true);
                Directory.CreateDirectory(outPath);
            }

            ReadAllFilesInDirectory(rootPath, out List<string> fileNames, "*.png");

            int count = fileNames.Count, i = 0;
            Parallel.ForEach(fileNames, delegate(string file)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string subPath = Path.GetDirectoryName(Path.GetRelativePath(rootPath, file)) ?? "";
                subPath = subPath.Replace('/', '_');
                subPath = subPath.Replace('\\', '_');
                string newFileName = Path.Combine(outPath, $"{subPath}___{fileName}.bmp");

                using Bitmap bitmap = new Bitmap(file); // Open and read the bitmap
                using Bitmap newBitmap = new Bitmap(bitmap);
                Rectangle rect = new Rectangle(0, 0, newBitmap.Width, newBitmap.Height);
                using Bitmap targetBitmap = newBitmap.Clone(rect, PixelFormat.Format24bppRgb);
                
                targetBitmap.Save(newFileName, ImageFormat.Bmp);
                
                Interlocked.Increment(ref i);
                Console.WriteLine($"Proceed {i} of {count} images, save to {newFileName}.");
            });
        }

        private static void ReadAllFilesInDirectory(string path, out List<string> fileNames, string pattern = "")
        {
            fileNames = new List<string>();
            string[] files = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            fileNames.AddRange(files);
        }
    }
}

