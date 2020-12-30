// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Splash.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The splash form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
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

    /// <summary>
    /// The splash form.
    /// </summary>
    public partial class Splash : Form
    {
        /// <summary>
        /// The configuration loader.
        /// </summary>
        private readonly IGetConfig configLoader = new GetConfig();

        /// <summary>
        /// The language manager.
        /// </summary>
        private readonly ILanguageManager languageManager = new LanguageManager();

        /// <summary>
        /// The timer.
        /// </summary>
        private readonly Timer timer = new Timer();

        /// <summary>
        /// The base directory.
        /// </summary>
        private string baseDirectory;

        /// <summary>
        /// The configuration.
        /// </summary>
        private Config config = new Config();

        /// <summary>
        /// The language.
        /// </summary>
        private ILanguage language;

        /// <summary>
        /// The splash image.
        /// </summary>
        private string splashImage;

        /// <summary>
        /// The update XML.
        /// </summary>
        private string updateXml;

        /// <summary>
        /// Initializes a new instance of the <see cref="Splash"/> class.
        /// </summary>
        public Splash()
        {
            this.InitializeComponent();
            this.LoadPaths();
            this.LoadSplash();
        }

        /// <summary>
        /// Checks whether the user is an administrator or not.
        /// </summary>
        /// <returns><c>true</c> if the user is an administrator, <c>false</c> else.</returns>
        private static bool IsElevated()
        {
            var id = WindowsIdentity.GetCurrent();
            return id.Owner != id.User;
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        private static void Exit()
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Loads the paths.
        /// </summary>
        private void LoadPaths()
        {
            try
            {
                var location = Assembly.GetExecutingAssembly().Location;
                this.baseDirectory = Directory.GetParent(location)?.FullName ?? string.Empty;
                this.splashImage = Path.Combine(this.baseDirectory, "Splash.jpg");
                this.updateXml = Path.Combine(this.baseDirectory, "UpdateConfig.xml");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Loads the splash image.
        /// </summary>
        private void LoadSplash()
        {
            try
            {
                this.config = this.configLoader.ImportConfiguration(this.updateXml);
                this.pictureBox_Splash.Image = Image.FromFile(this.splashImage);
                this.InitializeLanguageManager();
                this.CheckAdminPrivileges();
                this.StartTimer();
            }
            catch
            {
                var mainExecutable = Path.Combine(this.baseDirectory, this.config.MainExecutable.FileName);
                Process.Start(mainExecutable);
                Exit();
            }
        }

        /// <summary>
        /// Initializes the language manager.
        /// </summary>
        private void InitializeLanguageManager()
        {
            this.languageManager.SetCurrentLanguageFromName(this.config.PreferredLanguage);
            this.languageManager.OnLanguageChanged += this.OnLanguageChanged;
            this.language = this.languageManager.GetCurrentLanguage();
        }

        /// <summary>
        /// Handles the language changed event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnLanguageChanged(object sender, EventArgs e)
        {
            this.language = this.languageManager.GetCurrentLanguage();
        }

        /// <summary>
        /// Checks the administrator privileges.
        /// </summary>
        private void CheckAdminPrivileges()
        {
            if (IsElevated())
            {
                return;
            }

            var message = this.language.GetWord("RunInAdminModeMessage");
            var title = this.language.GetWord("RunInAdminModeTitle");
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Exit();
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private void StartTimer()
        {
            this.timer.Interval = 2000;
            this.timer.Elapsed += this.GetLatestVersion;
            this.timer.Start();
        }

        /// <summary>
        /// Gets the latest version.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void GetLatestVersion(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.CopyFilesIfNecessary();
                this.RestartFiles();
            }
            catch
            {
                var mainExecutable = Path.Combine(this.baseDirectory, this.config.MainExecutable.FileName);
                Process.Start(mainExecutable);
                Exit();
            }
        }

        /// <summary>
        /// Copies the files if necessary.
        /// </summary>
        private void CopyFilesIfNecessary()
        {
            this.CopySingleFileIfNecessary(this.config.MainExecutable);

            foreach (var file in this.config.Files)
            {
                this.CopySingleFileIfNecessary(file);
            }
        }

        /// <summary>
        /// Copies the single file if necessary.
        /// </summary>
        /// <param name="file">The file.</param>
        private void CopySingleFileIfNecessary(FileModel file)
        {
            if (!this.NewFileVersionExists(file))
            {
                return;
            }

            this.CopyFile(file);
        }

        /// <summary>
        /// Checks whether a new file exists.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if a new file exists, <c>false</c> else.</returns>
        private bool NewFileVersionExists(FileModel file)
        {
            var oldFile = Path.Combine(this.baseDirectory, file.FileName);
            var newFile = Path.Combine(this.config.PathToLatestVersion, file.FileName);
            var oldFileInfo = FileVersionInfo.GetVersionInfo(oldFile).FileVersion;
            var newFileInfo = FileVersionInfo.GetVersionInfo(newFile).FileVersion;

            if (oldFileInfo == null || newFileInfo == null)
            {
                return false;
            }

            return !oldFileInfo.Equals(newFileInfo) &&
                   !File.GetLastWriteTime(oldFile).Equals(File.GetLastWriteTime(newFile));
        }

        /// <summary>
        /// Copies a file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void CopyFile(FileModel file)
        {
            var currentFile = Path.Combine(this.baseDirectory, file.FileName);
            var newFile = Path.Combine(this.config.PathToLatestVersion, file.FileName);
            File.Delete(currentFile);
            File.Copy(newFile, currentFile);
        }

        /// <summary>
        /// Restarts the files.
        /// </summary>
        private void RestartFiles()
        {
            this.RestartOtherFiles();
            this.RestartMainExecutable();
            Exit();
        }

        /// <summary>
        /// Restarts the main executable.
        /// </summary>
        private void RestartMainExecutable()
        {
            if (this.config.MainExecutable.StartAgain)
            {
                Process.Start(Path.Combine(this.baseDirectory, this.config.MainExecutable.FileName));
            }
        }

        /// <summary>
        /// Restarts the other files.
        /// </summary>
        private void RestartOtherFiles()
        {
            foreach (var file in this.config.Files.Where(file => file.StartAgain))
            {
                Process.Start(Path.Combine(this.baseDirectory, file.FileName));
            }
        }
    }
}