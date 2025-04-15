using System;
using System.IO;

namespace Backup
{
    public class WatchedDirectory
    {
        public string SourceFolder { get; set; }
        public string DestinationFolder { get; set; }
        public string Mode { get; set; }
        public int IntervalSeconds { get; set; } = 10;
    }
}
