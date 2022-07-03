using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.CommandLine.DragonFruit;
using System.Diagnostics;

namespace WtTools.Unpacker
{
    internal class Program
    {
        static void Main(string inputPath = null, string outputPath = null, bool generateExta = false, params string[] args)
        {
            Console.Title = "WtTools.Unpacker";
            if (inputPath == null && args.Length > 0)
            {
                inputPath = args[0];
            }

            if (outputPath == null && args.Length > 1)
            {
                outputPath = args[1];
            }
            if (inputPath == null)
            {
                Console.WriteLine("Provide input file/folder");
                return;
            }
            Console.Clear();
            Console.CursorVisible = false;
            Console.WriteLine($"Initializing...");
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = Path.GetDirectoryName(inputPath);
            }
            using var worker = new ProcessingWorker(inputPath, outputPath);

            Console.Clear();
            var consoleTask = Task.Run(async () =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                {
                    await Task.Delay(100);
                }
                Console.SetCursorPosition(0, 8);
                Console.WriteLine($"Stopping");
                worker.Stop();
            });
            worker.Start(TargetFormat.JSON);
            UpdateLine(8, $"Logs:");
            while (worker.IsRunning)
            {
                PrintStatus(worker);
                System.Threading.Thread.Sleep(50);
            }
            PrintStatus(worker);
            if (worker.LastException != null)
            {
                PrintException(worker.LastException);
            }
        }
        const int LOG_LINE_OFFSET = 9;
        static int LogLine = LOG_LINE_OFFSET;
        static int LastLogCount = 0;

        static void PrintStatus(ProcessingWorker worker)
        {
            while (LastLogCount < worker.LogMessages.Count)
            {
                var message = worker.LogMessages[LastLogCount];
                UpdateLine(LogLine, message);
                ++LastLogCount;
                LogLine += CountLines(message);
            }
            UpdateLine(1, $"Status: {(worker.IsRunning ? "Running" : (worker.LastException == null ? "Completed" : "Errored"))}");
            UpdateLine(2, $"Files: {worker.FilesProcessed}/{worker.FilesPassed}");
            UpdateLine(3, $"Current file: {worker.CurrentFile}");
            UpdateLine(4, $"Currently processing: {worker.CurrentSubLabel?.Split('/')?.LastOrDefault()}");
            UpdateLine(5, $"Process: {worker.FileCurrent}/{worker.FileTotal}; Errors: {worker.FilesErrored}");
            UpdateLine(6, $"Time elapsed: {worker.TimeElapsed.Elapsed.ToString()}");
        }

        static void UpdateLine(int index, string content)
        {
            Console.SetCursorPosition(0, index);
            Console.Write(content.PadRight(Math.Min(Console.WindowWidth, Console.BufferWidth)));
        }

        static void PrintException(Exception ex)
        {
            Console.SetCursorPosition(0, 10);
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
        }

        private static int CountLines(string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)))
                count++;

            return count + 1;
        }



    }
}
