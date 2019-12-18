using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RoboArquivosNewcon
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {

                watcher.Path = _configuration.GetSection("DirectoryReceive").Value;

                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                watcher.Filter = "*." + _configuration.GetSection("FileExtension").Value;

                watcher.Created += OnChanged;

                watcher.EnableRaisingEvents = true;

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(60000, stoppingToken);
                }
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Console.WriteLine("File: {0} {1}!", e.FullPath, e.ChangeType);

                Task.Delay(5000).Wait();

                var processa = new ProcessaCNAB(e.FullPath, _configuration);
                processa.ProcessCNAB400();
            }
        }

    }
}
