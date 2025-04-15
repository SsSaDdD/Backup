using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Backup;

namespace Program
{
    class Program
    {
        static List<WatchedDirectory> watchedDirs = new List<WatchedDirectory>();

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.WriteLine("\n=== SmartBackup Меню ===");
                Console.WriteLine("1. Додати директорію");
                Console.WriteLine("2. Видалити директорію");
                Console.WriteLine("3. Переглянути директорії");
                Console.WriteLine("4. Запустити моніторинг");
                Console.WriteLine("5. Вийти");

                Console.Write("Обери дію: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddDirectory();
                        break;
                    case "2":
                        RemoveDirectory();
                        break;
                    case "3":
                        ShowDirectories();
                        break;
                    case "4":
                        StartMonitoring();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }

        static void AddDirectory()
        {
            Console.Write("Введи шлях до директорії, яку відстежувати: ");
            string source = Console.ReadLine();

            Console.Write("Введи шлях для збереження резервних копій: ");
            string destination = Console.ReadLine();

            Console.Write("Режим (regular / onchange): ");
            string mode = Console.ReadLine().ToLower();

            int interval = 10;
            if (mode == "regular")
            {
                Console.Write("інтервал у секундах: ");
                int.TryParse(Console.ReadLine(), out interval);
            }

            watchedDirs.Add(new WatchedDirectory
            {
                SourceFolder = source,
                DestinationFolder = destination,
                Mode = mode,
                IntervalSeconds = interval
            });

            Console.WriteLine("Директорію додано.");
        }

        static void RemoveDirectory()
        {
            ShowDirectories();

            Console.Write("Введи номер директорії для видалення: ");
            if (int.TryParse(Console.ReadLine(), out int index) &&
                index >= 1 && index <= watchedDirs.Count)
            {
                watchedDirs.RemoveAt(index - 1);
                Console.WriteLine("Директорію видалено.");
            }
            else
            {
                Console.WriteLine("Невірний номер.");
            }
        }

        static void StartMonitoring()
        {
            if (watchedDirs.Count == 0)
            {
                Console.WriteLine("Немає директорій для моніторингу.");
                return;
            }

            Console.WriteLine("Запуск моніторингу...");

            foreach (var dir in watchedDirs)
            {
                if (dir.Mode == "regular")
                {
                    Task.Run(() => RunRegularBackup(dir));
                }
                else if (dir.Mode == "onchange")
                {
                    Task.Run(() => RunOnChangeBackup(dir));
                }
                else
                {
                    Console.WriteLine($"Невідомий режим для {dir.SourceFolder}");
                }
            }

            Console.WriteLine("Моніторинг активний. Натисни Enter для зупинки...");
            Console.ReadLine();
        }

        static void RunRegularBackup(WatchedDirectory dir)
        {
            while (true)
            {
                try
                {
                    bool success = BackupService.Backup(dir);
                    if (success)
                    {
                        Console.WriteLine($"Файл скопійовано: {dir.SourceFolder}");
                    }
                    else
                    {
                        Console.WriteLine($"Помилка при копіюванні: {dir.SourceFolder}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Виняток: {ex.Message}");
                }

                Thread.Sleep(dir.IntervalSeconds * 1000);
            }
        }


        static void RunOnChangeBackup(WatchedDirectory dir)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(dir.SourceFolder));
            watcher.Filter = Path.GetFileName(dir.SourceFolder);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.EnableRaisingEvents = true;

            watcher.Changed += (s, e) =>
            {
                try
                {
                    bool success = BackupService.Backup(dir);
                    if (success)
                        Console.WriteLine($"Файл змінено та скопійовано: {e.Name}");
                    else
                        Console.WriteLine($"Не вдалося скопіювати: {e.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Виняток: {ex.Message}");
                }
            };

            Console.WriteLine($"Моніторимо файл: {dir.SourceFolder}");

            while (true)
            {
                Thread.Sleep(1000);
            }
        }


        static void CopyAllFiles(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            var files = Directory.GetFiles(sourceDir);

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }
        }


        static void ShowDirectories()
        {
            if (watchedDirs.Count == 0)
            {
                Console.WriteLine("📭 Немає директорій.");
                return;
            }

            for (int i = 0; i < watchedDirs.Count; i++)
            {
                var d = watchedDirs[i];
                Console.WriteLine($"{i + 1}. {d.SourceFolder} → {d.DestinationFolder} ({d.Mode})");
            }
        }

    }
}