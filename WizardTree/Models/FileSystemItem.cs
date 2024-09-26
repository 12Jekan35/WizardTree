using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using System.Collections.Concurrent;

namespace WizardTree.Models
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public long Size { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsDrive { get; set; }
        public bool IsComputerNode { get; set; }
        public ObservableCollection<FileSystemItem> Children { get; set; }
        public string FormattedSize => FormatSize(Size);

        public FileSystemItem(string name, string path = null, bool isDrive = false, bool isComputerNode = false)
        {
            Name = name;
            FullPath = path ?? name;
            IsDirectory = isDrive || (path != null && Directory.Exists(path));
            IsDrive = isDrive;
            IsComputerNode = isComputerNode;
            Children = new ObservableCollection<FileSystemItem>();
        }

        public async Task LoadChildrenAsync()
        {
            if (IsComputerNode)
            {
                var tasks = DriveInfo.GetDrives().Select(async drive =>
                {
                    var driveItem = new FileSystemItem(drive.Name, drive.Name, true);
                    await driveItem.CalculateSizeAsync();
                    return driveItem;
                });

                var driveItems = await Task.WhenAll(tasks);

                foreach (var driveItem in driveItems)
                {
                    Children.Add(driveItem);
                    await driveItem.LoadChildrenAsync();
                }
            }
            else if (IsDrive || IsDirectory)
            {
                try
                {
                    var di = new DirectoryInfo(FullPath);
                    var fileSystemEntries = di.EnumerateFileSystemInfos();

                    var tasks = fileSystemEntries.Select(async entry =>
                    {
                        var item = new FileSystemItem(entry.Name, entry.FullName, entry is DriveInfo);
                        await item.CalculateSizeAsync();
                        return item;
                    });

                    var items = await Task.WhenAll(tasks);

                    foreach (var item in items)
                    {
                        Children.Add(item);
                        if (item.IsDirectory)
                        {
                            await item.LoadChildrenAsync();
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Обработка отсутствия доступа
                }
            }
        }

        public async Task CalculateSizeAsync()
        {
            if (IsDirectory)
            {
                Size = await CalculateDirectorySizeAsync(FullPath);
            }
            else if (IsDrive)
            {
                var drive = new DriveInfo(FullPath);
                Size = drive.TotalSize - drive.TotalFreeSpace;
            }
            else
            {
                Size = new FileInfo(FullPath).Length;
            }
        }

        private async Task<long> CalculateDirectorySizeAsync(string path)
        {
            long size = 0;
            var concurrentBag = new ConcurrentBag<long>();

            try
            {
                var dir = new DirectoryInfo(path);
                var files = dir.EnumerateFiles();
                var subDirs = dir.EnumerateDirectories();

                await Task.WhenAll(
                    Task.Run(() =>
                    {
                        Parallel.ForEach(files, file =>
                        {
                            concurrentBag.Add(file.Length);
                        });
                    }),
                    Task.Run(async () =>
                    {
                        await Task.WhenAll(subDirs.Select(async subDir =>
                        {
                            var subDirSize = await CalculateDirectorySizeAsync(subDir.FullName);
                            concurrentBag.Add(subDirSize);
                        }));
                    })
                );

                size = concurrentBag.Sum();
            }
            catch (UnauthorizedAccessException)
            {
                // Обработка отсутствия доступа
            }

            return size;
        }

        private string FormatSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int counter = 0;
            double number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }
}
