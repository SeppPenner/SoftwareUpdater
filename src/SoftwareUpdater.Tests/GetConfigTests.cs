// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetConfigTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to test the <see cref="GetConfig" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Tests;

/// <summary>
/// A class to test the <see cref="GetConfig"/> class.
/// </summary>
[TestClass]
public class GetConfigTests
{
    /// <summary>
    /// The configuration loader under test.
    /// </summary>
    private readonly IGetConfig configLoader = new GetConfig();

    /// <summary>
    /// The directory the configuration files of a single test are written to.
    /// </summary>
    private string testDirectory = string.Empty;

    /// <summary>
    /// Creates an empty directory outside of the repository for the files of the running test.
    /// </summary>
    [TestInitialize]
    public void CreateTestDirectory()
    {
        this.testDirectory = Path.Combine(Path.GetTempPath(), $"SoftwareUpdater_{Guid.NewGuid():N}");
        Directory.CreateDirectory(this.testDirectory);
    }

    /// <summary>
    /// Removes the directory of the finished test.
    /// </summary>
    [TestCleanup]
    public void DeleteTestDirectory()
    {
        if (Directory.Exists(this.testDirectory))
        {
            Directory.Delete(this.testDirectory, true);
        }
    }

    /// <summary>
    /// Checks whether the example configuration that is shipped next to the executable is read completely.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationReadsTheShippedExample()
    {
        var fileName = this.WriteConfiguration(TestDataProvider.GetExampleConfiguration());

        var config = this.configLoader.ImportConfiguration(fileName);

        Assert.AreEqual("Deutsch", config.PreferredLanguage);
        Assert.AreEqual(@"C:\Users\asdf\Desktop\Test", config.PathToLatestVersion);
        Assert.AreEqual("MainExecutable.exe", config.MainExecutable.FileName);
        Assert.IsTrue(config.MainExecutable.StartAgain);
        Assert.AreEqual(2, config.Files.Count);
        Assert.AreEqual("SecondExe.exe", config.Files[0].FileName);
        Assert.IsFalse(config.Files[0].StartAgain);
        Assert.AreEqual("Changelog.txt", config.Files[1].FileName);
        Assert.IsTrue(config.Files[1].StartAgain);
    }

    /// <summary>
    /// Checks whether a configuration without a preferred language returns an empty one. Such a file was shipped
    /// up to version 1.0.7.0 and makes the language manager throw later on.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationWithoutPreferredLanguageReturnsAnEmptyLanguage()
    {
        var fileName = this.WriteConfiguration("<Config><PathToLatestVersion>C:\\Test</PathToLatestVersion></Config>");

        var config = this.configLoader.ImportConfiguration(fileName);

        Assert.AreEqual(string.Empty, config.PreferredLanguage);
    }

    /// <summary>
    /// Checks whether a configuration without any files returns an empty list instead of <c>null</c>.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationWithoutFilesReturnsAnEmptyList()
    {
        var fileName = this.WriteConfiguration("<Config><PreferredLanguage>Deutsch</PreferredLanguage></Config>");

        var config = this.configLoader.ImportConfiguration(fileName);

        Assert.AreEqual(0, config.Files.Count);
        Assert.AreEqual(string.Empty, config.MainExecutable.FileName);
    }

    /// <summary>
    /// Checks whether an element that the configuration class does not know is ignored. That is what allows an
    /// older updater to read a newer configuration file.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationIgnoresAnUnknownElement()
    {
        var fileName = this.WriteConfiguration("<Config><Unknown>Value</Unknown><PreferredLanguage>Deutsch</PreferredLanguage></Config>");

        var config = this.configLoader.ImportConfiguration(fileName);

        Assert.AreEqual("Deutsch", config.PreferredLanguage);
    }

    /// <summary>
    /// Checks whether a missing configuration file throws.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationOfAMissingFileThrowsAFileNotFoundException()
    {
        var fileName = Path.Combine(this.testDirectory, "DoesNotExist.xml");

        Assert.ThrowsExactly<FileNotFoundException>(() => this.configLoader.ImportConfiguration(fileName));
    }

    /// <summary>
    /// Checks whether a broken configuration file throws.
    /// </summary>
    [TestMethod]
    public void ImportConfigurationOfBrokenXmlThrowsAnXmlException()
    {
        var fileName = this.WriteConfiguration("<Config>");

        Assert.ThrowsExactly<XmlException>(() => this.configLoader.ImportConfiguration(fileName));
    }

    /// <summary>
    /// Writes a configuration into the directory of the running test.
    /// </summary>
    /// <param name="content">The content of the configuration file.</param>
    /// <returns>The file name of the written configuration.</returns>
    private string WriteConfiguration(string content)
    {
        var fileName = Path.Combine(this.testDirectory, "UpdateConfig.xml");
        File.WriteAllText(fileName, content, Encoding.UTF8);
        return fileName;
    }
}
