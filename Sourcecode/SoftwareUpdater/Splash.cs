using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Timers;
using System.Windows.Forms;
using Languages.Implementation;
using Languages.Interfaces;
using SoftwareUpdater.Configuration;
using SoftwareUpdater.Implementation;
using SoftwareUpdater.Interface;
using Timer = System.Timers.Timer;

namespace SoftwareUpdater
{
    public partial class Splash : Form
    {
        private readonly IGetConfig _getConfig = new GetConfig();
        private readonly ILanguageManager _lm = new LanguageManager();
        private readonly Timer _timer = new Timer();
        private string _baseDirectory;
        private Config _config = new Config();
        private Language _lang;
        private string _splashImage;
        private string _updateXml;

        public Splash()
        {
            InitializeComponent();
            LoadPaths();
            LoadSplash();
        }

        private void LoadPaths()
        {
            try
            {
                var location = Assembly.GetExecutingAssembly().Location;
                if (location == null) throw new ArgumentNullException(nameof(_baseDirectory));
                _baseDirectory = Directory.GetParent(location).FullName;
                _splashImage = Path.Combine(_baseDirectory, "Splash.jpg");
                _updateXml = Path.Combine(_baseDirectory, "UpdateConfig.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSplash()
        {
            try
            {
                _config = _getConfig.ImportConfiguration(_updateXml);
                pictureBox_Splash.Image = Image.FromFile(_splashImage);
                InitializeLanguageManager();
                CheckAdminPrivileges();
                StartTimer();
            }
            catch
            {
                var mainExecutable = Path.Combine(_baseDirectory, _config.MainExecutable.FileName);
                Process.Start(mainExecutable);
                Exit();
            }
        }


        private void InitializeLanguageManager()
        {
            _lm.SetCurrentLanguageFromName(_config.PreferredLanguage);
            _lm.OnLanguageChanged += OnLanguageChanged;
            _lang = _lm.GetCurrentLanguage();
        }

        private void OnLanguageChanged(object sender, EventArgs eventArgs)
        {
            _lang = _lm.GetCurrentLanguage();
        }

        private void CheckAdminPrivileges()
        {
            if (IsElevated()) return;
            var message = _lang.GetWord("RunInAdminModeMessage");
            var title = _lang.GetWord("RunInAdminModeTitle");
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Exit();
        }

        private bool IsElevated()
        {
            var id = WindowsIdentity.GetCurrent();
            return id.Owner != id.User;
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
                CopyFilesIfNecessary();
                RestartFiles();
            }
            catch
            {
                var mainExecutable = Path.Combine(_baseDirectory, _config.MainExecutable.FileName);
                Process.Start(mainExecutable);
                Exit();
            }
        }

        private void CopyFilesIfNecessary()
        {
            CopySingleFileIfNecessary(_config.MainExecutable);
            foreach (var file in _config.Files)
                CopySingleFileIfNecessary(file);
        }

        private void CopySingleFileIfNecessary(FileModel file)
        {
            if (!NewFileVersionExists(file)) return;
            CopyFile(file);
        }

        private bool NewFileVersionExists(FileModel file)
        {
            var oldFile = Path.Combine(_baseDirectory, file.FileName);
            var newFile = Path.Combine(_config.PathToLatestVersion, file.FileName);
            var oldFileInfo = FileVersionInfo.GetVersionInfo(oldFile).FileVersion;
            var newFileInfo = FileVersionInfo.GetVersionInfo(newFile).FileVersion;
            if (oldFileInfo == null || newFileInfo == null) return false;
            return !oldFileInfo.Equals(newFileInfo) &&
                   !File.GetLastWriteTime(oldFile).Equals(File.GetLastWriteTime(newFile));
        }

        private void CopyFile(FileModel file)
        {
            var currentFile = Path.Combine(_baseDirectory, file.FileName);
            var newFile = Path.Combine(_config.PathToLatestVersion, file.FileName);
            File.Delete(currentFile);
            File.Copy(newFile, currentFile);
        }

        private void RestartFiles()
        {
            RestartOtherFiles();
            RestartMainExecutable();
            Exit();
        }

        private void RestartMainExecutable()
        {
            if (_config.MainExecutable.StartAgain)
                Process.Start(Path.Combine(_baseDirectory, _config.MainExecutable.FileName));
        }

        private void RestartOtherFiles()
        {
            foreach (var file in _config.Files)
                if (file.StartAgain)
                    Process.Start(Path.Combine(_baseDirectory, file.FileName));
        }

        private static void Exit()
        {
            Environment.Exit(0);
        }
    }
}