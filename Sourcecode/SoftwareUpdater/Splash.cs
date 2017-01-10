using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Timers;
using System.Windows.Forms;
using SoftwareUpdater.Configuration;
using SoftwareUpdater.Implementation;
using SoftwareUpdater.Interface;
using Timer = System.Timers.Timer;

namespace SoftwareUpdater
{
    public partial class Splash : Form
    {
        private readonly IGetConfig _getConfig = new GetConfig();
        private readonly string _splashImage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Splash.jpg");
        private readonly Timer _timer = new Timer();
        private readonly string _updateXml = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UpdateConfig.xml");
        private Config _config = new Config();

        public Splash()
        {
            InitializeComponent();
            LoadSplash();
        }

        private void LoadSplash()
        {
            try
            {
                _config = _getConfig.ImportConfiguration(_updateXml);
                pictureBox_Splash.Image = Image.FromFile(_splashImage);
                StartTimer();
            }
            catch
            {
                Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.MainExecutable.FileName));
                Exit();
            }
        }

        private void StartTimer()
        {
            _timer.Interval = 2000;
            _timer.Elapsed += GetLatestVersion;
            _timer.Start();
        }

        private void GetLatestVersion(object sender, ElapsedEventArgs e)
        {
            try
            {
                CheckFiles();
            }
            catch
            {
                var mainExecutable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.MainExecutable.FileName);
                Process.Start(mainExecutable);
                Exit();
            }
        }

        private void CheckFiles()
        {
            foreach (var file in _config.Files)
                CheckSingleFile(file);
        }

        private void CheckSingleFile(FileModel file)
        {
            if (!NewFileVersionExists(file)) return;
            CopyFiles();
            RestartFiles();
        }

        private bool NewFileVersionExists(FileModel file)
        {
            var oldFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.FileName);
            var newFile = Path.Combine(_config.PathToLatestVersion, file.FileName);
            var oldFileInfo = FileVersionInfo.GetVersionInfo(oldFile).FileVersion;
            var newFileInfo = FileVersionInfo.GetVersionInfo(newFile).FileVersion;
            return !oldFileInfo.Equals(newFileInfo) &&
                   !File.GetLastWriteTime(oldFile).Equals(File.GetLastWriteTime(newFile));
        }

        private void CopyFiles()
        {
            foreach (var file in _config.Files)
            {
                var currentFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.FileName);
                var newFile = Path.Combine(_config.PathToLatestVersion, file.FileName);
                File.Delete(currentFile);
                File.Copy(newFile, currentFile);
            }
        }

        private void RestartFiles()
        {
            foreach (var file in _config.Files)
                if (file.StartAgain)
                    Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file.FileName));
        }

        private static void Exit()
        {
            Environment.Exit(0);
        }
    }
}