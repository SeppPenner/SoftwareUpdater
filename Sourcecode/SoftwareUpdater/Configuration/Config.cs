using System.Collections.Generic;

namespace SoftwareUpdater.Configuration
{
    public class Config
    {
        public string PathToLatestVersion { get; set; }
        public List<FileModel> Files { get; set; }
        public FileModel MainExecutable { get; set; }
    }
}