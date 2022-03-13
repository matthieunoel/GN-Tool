using NsfwSpyNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.Json;

// TODO : Limit perf when user is active (https://www.codeproject.com/articles/9104/how-to-check-for-user-inactivity-with-and-without).
// TODO : Make a standalone exe.
// TODO : Remove NsfwSpyNS and train a better AI.
// TODO : Optimize perfs (memory and exec time) => Multi-thread tasks to gain perfs ?
// TODO : Add options and arguments (Verbose, outputDir, ...)
// TODO : Add logs

namespace GN__GetNudes_
{
    [SupportedOSPlatform("windows")]
    class Program
    {
        static void Main(string[] args)
        {

            var nsfwSpy = new NsfwSpy();

            Console.Write("\n");
            Console.Write("  -\n");
            Console.Write("\n");

            //string[] folders = new string[] { @"C:\Users\matth\Desktop", "Desktop", @"C:\Users\matth\Documents", @"C:\Users\matth\Pictures", @"C:\Users\matth\Music", @"C:\Users\matth\Downloads", @"C:\Users\matth\Videos" };
            string[] folders = new string[] { /*Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Desktop"),*/ Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Documents"), Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Pictures"), Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Music"), Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Downloads"), Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Videos") };

            List<string> files = new List<string>();

            foreach (var folder in folders)
            {
                files.AddRange(GetFilesFrom(folder, new string[] { "jpg", "jpeg", "png", "tiff", "bmp", "svg", "gif", "mp4" }, true));
            }

            files = files.OrderBy(o => o.ToLower().EndsWith(".gif")).OrderBy(o => o.ToLower().EndsWith(".mp4")).ToList();

            //var filesImg = GetFilesFrom(Environment.GetEnvironmentVariable("USERPROFILE"), new string[] { "jpg", "jpeg", "png", "tiff", "bmp", "svg" }, true);
            //var filesGif = GetFilesFrom(Environment.GetEnvironmentVariable("USERPROFILE"), new string[] { "gif" }, true);
            //var filesVideo = GetFilesFrom(Environment.GetEnvironmentVariable("USERPROFILE"), new string[] { "mp4" }, true);
            //var files = GetFilesFrom(@"./", new string[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp", "svg" }, true);

            //Console.WriteLine($"Files : {JsonSerializer.Serialize(files)}");

            //var files = Directory.GetFiles(@"./Data/");

            Console.Write("\n");
            Console.Write("  -\n");
            Console.Write("\n");

            Console.Write("NSFW Results : ");
            Directory.CreateDirectory(Path.Join("./", "GN-Export/"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Pornography"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Sexy"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Neutral"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Hentai"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Gif"));
            Directory.CreateDirectory(Path.Join("./", "GN-Export/", "Video"));

            //nsfwSpy.ClassifyImages(filesImg, (filePath, result) =>
            //{
            //    if (result.IsNsfw && result.PredictedLabel.ToLower() != "hentai")
            //    {
            //        Console.WriteLine($"{filePath} - {result.PredictedLabel}");
            //        File.Copy(filePath, Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Desktop/GN-Export/", Path.GetFileName(filePath)), true);
            //    }
            //});
            //foreach (string gif in filesGif)
            //{
            //    var result = nsfwSpy.ClassifyGif(gif);
            //    if (result.IsNsfw)
            //    {
            //        Console.WriteLine($"{gif}");
            //        File.Copy(gif, Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Desktop/GN-Export/", Path.GetFileName(gif)), true);
            //    }
            //}
            //foreach (string video in filesVideo)
            //{
            //    var result = nsfwSpy.ClassifyVideo(video);
            //    if (result.IsNsfw)
            //    {
            //        Console.WriteLine($"{video}");
            //        File.Copy(video, Path.Join(Environment.GetEnvironmentVariable("USERPROFILE"), "Desktop/GN-Export/", Path.GetFileName(video)), true);
            //    }
            //}

            foreach (string file in files)
            {
                try
                {
                    if (file.ToLower().EndsWith(".gif"))
                    {
                        NsfwSpyFramesResult result = nsfwSpy.ClassifyGif(file);
                        if (result.IsNsfw)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{file}");
                            Console.ResetColor();
                            File.Copy(file, Path.Join("./GN-Export/Gif", Path.GetFileName(file)), true);
                        }
                    }
                    else if (file.ToLower().EndsWith(".mp4"))
                    {
                        NsfwSpyFramesResult result = nsfwSpy.ClassifyVideo(file);
                        if (result.IsNsfw)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{file}");
                            Console.ResetColor();
                            File.Copy(file, Path.Join("./GN-Export/Video", Path.GetFileName(file)), true);
                        }
                    }
                    else
                    {
                        using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (Image image = Image.FromStream(fileStream, false, false))
                            {
                                int height = image.Height;
                                int width = image.Width;
                                if(height < 256 || width < 256)
                                {
                                    continue;
                                }
                            }
                        }
                        NsfwSpyResult result = nsfwSpy.ClassifyImage(file);
                        if (result.IsNsfw)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{file} - {result.PredictedLabel}");
                            Console.ResetColor();
                            File.Copy(file, Path.Join("./GN-Export/", System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(result.PredictedLabel.ToLower()), Path.GetFileName(file)), true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{file} - ERROR");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"|  {e.Message}");
                    if (e.InnerException is not null) Console.Write($", {e.InnerException}");
                    Console.Write("\n");
                    Console.ResetColor();
                }
            }

            //Console.ReadKey();

        }

        /// <summary>
        /// From : https://stackoverflow.com/questions/2953254/cgetting-all-image-files-in-folder#answer-18321162
        /// </summary>
        /// <param name="searchFolder"></param>
        /// <param name="filters"></param>
        /// <param name="isRecursive"></param>
        /// <returns></returns>
        public static string[] GetFilesFrom(string searchFolder, string[] filters, bool isRecursive)
        {

            Console.WriteLine($"Scanning {Path.GetFullPath(searchFolder)} ...");
            List<string> filesFound = new List<string>();

            try
            {
                foreach (string filter in filters)
                {
                    filesFound.AddRange(Directory.GetFiles(searchFolder, string.Format("*.{0}", filter), SearchOption.TopDirectoryOnly));

                }

                if (isRecursive)
                {
                    foreach (string subfolder in Directory.GetDirectories(searchFolder))
                    {
                        filesFound.AddRange(GetFilesFrom(Path.GetFullPath(subfolder), filters, isRecursive));
                    }
                }

            }
            catch (Exception e)
            {
                Console.Write($"|  {e.Message}");
                if (e.InnerException is not null) Console.Write($", {e.InnerException}");
                Console.Write("\n");
            }

            return filesFound.ToArray();
        }

    }
}
