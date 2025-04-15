using System.Runtime.InteropServices;

namespace Backup
{
    class BackupService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CopyFile(string lpExistingFileName, string lpNewFileName, bool bFailIfExists);

        public static bool Backup(WatchedDirectory dir)
        {
            string baseName = Path.GetFileNameWithoutExtension(dir.SourceFolder);
            string extension = Path.GetExtension(dir.SourceFolder);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string destinationPath = Path.Combine(dir.DestinationFolder, $"{baseName}_{timestamp}{extension}");

            Console.WriteLine($"копіюємо з: {dir.SourceFolder}");
            Console.WriteLine($"копіюємо до: {destinationPath}");

            return CopyFile(dir.SourceFolder, destinationPath, false);
        }

    }
}

