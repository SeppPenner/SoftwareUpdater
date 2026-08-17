// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUpdaterTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="FileUpdater" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Tests;

/// <summary>
/// A class to test the <see cref="FileUpdater"/> class.
/// </summary>
[TestClass]
public class FileUpdaterTests
{
    /// <summary>
    /// The file updater under test.
    /// </summary>
    private readonly IFileUpdater fileUpdater = new FileUpdater();

    /// <summary>
    /// The directory that stands for the installation directory of the running test.
    /// </summary>
    private string currentDirectory = string.Empty;

    /// <summary>
    /// The directory that stands for the share with the latest version of the running test.
    /// </summary>
    private string latestDirectory = string.Empty;

    /// <summary>
    /// The directory that holds both directories of the running test.
    /// </summary>
    private string testDirectory = string.Empty;

    /// <summary>
    /// Creates two empty directories outside of the repository for the files of the running test.
    /// </summary>
    [TestInitialize]
    public void CreateTestDirectories()
    {
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"SoftwareUpdater_{Guid.NewGuid():N}");
        this.currentDirectory = Path.Combine(this.testDirectory, "Current");
        this.latestDirectory = Path.Combine(this.testDirectory, "Latest");
        Directory.CreateDirectory(this.currentDirectory);
        Directory.CreateDirectory(this.latestDirectory);
    }

    /// <summary>
    /// Removes the directories of the finished test.
    /// </summary>
    [TestCleanup]
    public void DeleteTestDirectories()
    {
        if (Directory.Exists(this.testDirectory))
        {
            Directory.Delete(this.testDirectory, true);
        }
    }

    /// <summary>
    /// Checks whether two binaries with a different file version are reported as an update.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsForBinariesWithDifferentVersions()
    {
        var file = this.PlaceBinaries();

        Assert.IsTrue(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether the same binary on both sides is not reported as an update. The copy keeps the write time
    /// of its source, so neither the version nor the write time differs.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsIsFalseForTheSameBinary()
    {
        var file = new FileModel { FileName = "Payload.exe" };
        File.Copy(TestDataProvider.BinaryWithFixedVersion, Path.Combine(this.currentDirectory, file.FileName));
        File.Copy(TestDataProvider.BinaryWithFixedVersion, Path.Combine(this.latestDirectory, file.FileName));

        Assert.IsFalse(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether a file without version information is reported as an update when its write time differs.
    /// Up to version 1.0.7.0 such a file was never updated, although the shipped example configuration lists one.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsForAFileWithoutVersionInformationAndANewerWriteTime()
    {
        var file = this.PlaceTextFiles("Old content", "New content");

        Assert.IsTrue(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether a file without version information and the same write time is not reported as an update.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsIsFalseForAFileWithoutVersionInformationAndTheSameWriteTime()
    {
        var file = this.PlaceTextFiles("Same content", "Same content");
        var writeTime = new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Local);
        File.SetLastWriteTime(Path.Combine(this.currentDirectory, file.FileName), writeTime);
        File.SetLastWriteTime(Path.Combine(this.latestDirectory, file.FileName), writeTime);

        Assert.IsFalse(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether a file that is missing on the share is not reported as an update, so that an unreachable
    /// or incomplete share does not stop the remaining files from being updated.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsIsFalseForAFileMissingOnTheShare()
    {
        var file = new FileModel { FileName = "Changelog.txt" };
        File.WriteAllText(Path.Combine(this.currentDirectory, file.FileName), "Old content");

        Assert.IsFalse(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether a file that does not exist in the installation directory yet is reported as an update.
    /// </summary>
    [TestMethod]
    public void NewFileVersionExistsForAFileMissingInTheInstallation()
    {
        var file = new FileModel { FileName = "Changelog.txt" };
        File.WriteAllText(Path.Combine(this.latestDirectory, file.FileName), "New content");

        Assert.IsTrue(this.fileUpdater.NewFileVersionExists(this.currentDirectory, this.latestDirectory, file));
    }

    /// <summary>
    /// Checks whether an outdated file is replaced by the one from the share.
    /// </summary>
    [TestMethod]
    public void CopyFileIfNecessaryReplacesAnOutdatedFile()
    {
        var file = this.PlaceTextFiles("Old content", "New content");

        this.fileUpdater.CopyFileIfNecessary(this.currentDirectory, this.latestDirectory, file);

        Assert.AreEqual("New content", File.ReadAllText(Path.Combine(this.currentDirectory, file.FileName)));
    }

    /// <summary>
    /// Checks whether a file that is missing on the share is left alone instead of being deleted.
    /// </summary>
    [TestMethod]
    public void CopyFileIfNecessaryKeepsAFileMissingOnTheShare()
    {
        var file = new FileModel { FileName = "Changelog.txt" };
        var currentFile = Path.Combine(this.currentDirectory, file.FileName);
        File.WriteAllText(currentFile, "Old content");

        this.fileUpdater.CopyFileIfNecessary(this.currentDirectory, this.latestDirectory, file);

        Assert.IsTrue(File.Exists(currentFile));
        Assert.AreEqual("Old content", File.ReadAllText(currentFile));
    }

    /// <summary>
    /// Places two binaries with a different file version under the same name in both directories.
    /// </summary>
    /// <returns>The <see cref="FileModel"/> of the placed file.</returns>
    private FileModel PlaceBinaries()
    {
        var fixedVersion = FileVersionInfo.GetVersionInfo(TestDataProvider.BinaryWithFixedVersion).FileVersion;
        var buildVersion = FileVersionInfo.GetVersionInfo(TestDataProvider.BinaryWithBuildVersion).FileVersion;
        Assert.AreNotEqual(fixedVersion, buildVersion, "The two binaries of the version comparison need different file versions.");

        var file = new FileModel { FileName = "Payload.exe" };
        File.Copy(TestDataProvider.BinaryWithFixedVersion, Path.Combine(this.currentDirectory, file.FileName));
        File.Copy(TestDataProvider.BinaryWithBuildVersion, Path.Combine(this.latestDirectory, file.FileName));
        return file;
    }

    /// <summary>
    /// Places a text file, which carries no version information, in both directories.
    /// </summary>
    /// <param name="currentContent">The content of the file in the installation directory.</param>
    /// <param name="latestContent">The content of the file on the share.</param>
    /// <returns>The <see cref="FileModel"/> of the placed file.</returns>
    private FileModel PlaceTextFiles(string currentContent, string latestContent)
    {
        var file = new FileModel { FileName = "Changelog.txt" };
        File.WriteAllText(Path.Combine(this.currentDirectory, file.FileName), currentContent);
        File.WriteAllText(Path.Combine(this.latestDirectory, file.FileName), latestContent);
        File.SetLastWriteTime(Path.Combine(this.currentDirectory, file.FileName), new DateTime(2026, 8, 16, 12, 0, 0, DateTimeKind.Local));
        File.SetLastWriteTime(Path.Combine(this.latestDirectory, file.FileName), new DateTime(2026, 8, 17, 12, 0, 0, DateTimeKind.Local));
        return file;
    }
}
