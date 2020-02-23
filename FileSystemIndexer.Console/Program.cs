using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSystemReadTest
{
    internal class Program
    {
        private static readonly string[] Drives = {@"C:\", @"D:\"};

		private const string ConnectionString = @"D:\Temp\MyData.db";

		private static void Main()
		{
            foreach (var drive in Drives)
            {
                ReadDrive(drive);
            }
        }

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
