using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using WtTools.Formats;

namespace WtTools.Unpacker
{
    public enum TargetFormat { Strict, JSON }

    public class ProcessingWorker : IDisposable
    {
        public static readonly IReadOnlyCollection<string> SupportedExtensions = Array.AsReadOnly(new string[] { "vromfs.bin", "blk" });

        private bool _disposedValue;
        private Thread _thread;
        private List<Task> _writeTasks = new List<Task>();

        private string _inputPath;
        private string _outputPath;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public int FilesPassed { get; private set; }
        public int FilesProcessed { get; private set; }
        public int FileTotal { get; private set; }
        public int FileCurrent { get; private set; }
        public string CurrentFile { get; private set; }
        public string CurrentSubLabel { get; private set; }
        public System.Diagnostics.Stopwatch TimeElapsed { get; private set; }

        public Exception LastException { get; private set; }

        public bool IsRunning { get; private set; }

        private TargetFormat _targetFormat;



        public ProcessingWorker(string inputPath, string outputPath)
        {
            _inputPath = inputPath;
            _outputPath = outputPath;
        }

        public void Start(TargetFormat format)
        {
            IsRunning = true;
            _targetFormat = format;
            _thread = new Thread((o) => WorkerMethod(_cancellationTokenSource.Token))
            {
                IsBackground = true
            };
            _thread.Start();
            TimeElapsed = System.Diagnostics.Stopwatch.StartNew();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _thread.Join(2000);
        }

        private void WorkerMethod(CancellationToken cancellationToken)
        {
#if !DEBUG
            try
            {
#endif
                if (File.Exists(_inputPath))
                {
                    var info = new FileInfo(_inputPath);
                    ProcessFiles(new FileInfo[] { info }, _outputPath, cancellationToken);
                }
                else if (Directory.Exists(_inputPath))
                {
                    var dirInfo = new DirectoryInfo(_inputPath);
                    var files = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(x => SupportedExtensions.Any(y => x.Name.EndsWith($".{y}"))).ToArray();
                    ProcessFiles(files, _outputPath, cancellationToken);
                }
                else
                {
                    throw new ArgumentException($"Path: '{_inputPath}' doesn't exist");
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                LastException = ex;
            }
            finally
            {
#endif
                TimeElapsed.Stop();
                IsRunning = false;
                _thread.Join(5000);
#if !DEBUG
            }
#endif
        }

        private void ProcessFiles(FileInfo[] files, string outputPath, CancellationToken cancellationToken)
        {
            FilesPassed = files.Length;
            for (int i = 0; i < files.Length && !cancellationToken.IsCancellationRequested; i++)
            {
                FilesProcessed = i;
                CurrentFile = files[i].Name;
                if (files[i].Name.EndsWith(".vromfs.bin"))
                {
                    ProcessVromfs(files[i], outputPath, cancellationToken);
                }
                else if (files[i].Name.EndsWith(".blk"))
                {
                    ProcessBlk(files[i], outputPath, cancellationToken);
                }
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                FilesProcessed += 1;
            }
        }

        private void ProcessBlk(FileInfo file, string outputPath, CancellationToken cancellationToken, VromfsInfo parent = null)
        {
            var data = File.ReadAllBytes(file.FullName);
            ProcessBlk(file.Name, data, outputPath, cancellationToken, parent);
        }

        private void ProcessBlk(string fileName, byte[] data, string outputPath, CancellationToken cancellationToken, VromfsInfo parent = null)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = _outputPath;
            }
            var blk = new BlkInfo(fileName, data, parent);
            var targetPath = Path.Combine(outputPath, fileName + "x");
            var targetFile = new FileInfo(targetPath);
            var targetDir = targetFile.Directory;
            if (!targetDir.Exists)
            {
                targetDir.Create();
            }
            string content = string.Empty;
            if (_targetFormat == TargetFormat.JSON)
            {
                content = blk.ToJSON();
            }
            else if (_targetFormat == TargetFormat.Strict)
            {
                content = blk.ToStrict();
            }
            _writeTasks.Add(File.WriteAllTextAsync(targetFile.FullName, content, cancellationToken));
        }

        private void ProcessVromfs(FileInfo file, string outputPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = _outputPath;
            }
            var data = File.ReadAllBytes(file.FullName);
            var vromfs = new VromfsInfo(data);
            var vromfsOutPath = Path.Combine(outputPath, $"{file.Name}_u");
            var dir = new DirectoryInfo(vromfsOutPath);
            if (!dir.Exists)
            {
                dir.Create();
            }
            CurrentSubLabel = String.Empty;
            FileTotal = vromfs.Files.Length;
            FileCurrent = 0;
            for (int j = 0; j < vromfs.Files.Length && !cancellationToken.IsCancellationRequested; ++j)
            {
                CurrentSubLabel = vromfs.Files[j].Name;
                FileCurrent = j;
                var fullPath = Path.Combine(vromfsOutPath, vromfs.Files[j].Name);
                var targetFile = new FileInfo(fullPath);
                var targetDir = targetFile.Directory;
                if (!targetFile.Exists)
                {
                    targetDir.Create();
                }
                if (vromfs.Files[j].Name.EndsWith(".blk") && vromfs.Files[j].Size > 0)
                {
                    ProcessBlk(vromfs.Files[j].Name, vromfs.Files[j].Data, vromfsOutPath, cancellationToken, vromfs);
                }
                else
                {
                    if (vromfs.Files[j].Size == 0)
                    {
                        _writeTasks.Add(File.WriteAllTextAsync(targetFile.FullName, String.Empty, cancellationToken));
                    }
                    else
                    {
                        _writeTasks.Add(File.WriteAllBytesAsync(targetFile.FullName, vromfs.Files[j].Data, cancellationToken));
                    }
                }
            }
            Task.WaitAll(_writeTasks.ToArray(), cancellationToken);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }
                    _thread?.Join(TimeSpan.FromSeconds(5));
                    _cancellationTokenSource.Dispose();

                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ProcessingWorker()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
