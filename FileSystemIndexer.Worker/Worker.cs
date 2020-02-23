using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileSystemReadTest;
using LiteDB;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServiceTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                foreach (var drive in Drives)
                {
                    ReadDrive(drive);
                }
                await Task.Delay(120000, stoppingToken);
            }
        }

        private static readonly string[] Drives = {@"C:\", @"D:\"};

		private const string ConnectionString = @"D:\Temp\MyData.db";


       private static void ReadDrive(string location) { 
			var di = new DirectoryInfo(location);
            var startTime = DateTime.Now;
			Console.WriteLine("Starting scan at " + startTime);
			using (var db = new LiteDatabase(ConnectionString))
			{
				var col = db.GetCollection<StorageInfo>("StorageInfo");
				var infos = new List<StorageInfo>();
				ReadDirectory(di,  infos);
				Console.WriteLine("Finished processing at " + DateTime.Now);
				Console.WriteLine("Elapsed time : " + (DateTime.Now - startTime).TotalSeconds);
				Console.WriteLine($"Inserting at {DateTime.Now}");
				col.InsertBulk(infos);
				Console.WriteLine($"Insertion Completed at {DateTime.Now}");
			}
			Console.Read();	
		}

		private static void ReadDirectory(DirectoryInfo di,  List<StorageInfo> info)
		{
			try
			{
				var directory = new StorageInfo { Name=di.Name, Location=di.FullName, IsFile=false };
				info.Add(directory);
				var files = di.GetFiles();
				info.AddRange(files.Select(file => new StorageInfo {Name = file.Name, Location = file.FullName, IsFile = true}));
				var subDirectories = di.GetDirectories();
				foreach (var subDirectory in subDirectories)  {
					ReadDirectory(subDirectory,  info);
				}
			}

            catch (Exception)
            {
                // ignored
            }
        }
    }
}
