// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileUpdater.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to update the files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Implementation;

/// <inheritdoc cref="IFileUpdater"/>
/// <summary>
/// A class to update the files.
/// </summary>
/// <seealso cref="IFileUpdater"/>
public class FileUpdater : IFileUpdater
{
    /// <inheritdoc cref="IFileUpdater"/>
    /// <summary>
    /// Checks whether a newer version of a file exists.
    /// </summary>
    /// <param name="baseDirectory">The directory of the installed file.</param>
    /// <param name="pathToLatestVersion">The directory of the latest version of the file.</param>
    /// <param name="file">The file.</param>
    /// <returns><c>true</c> if the file needs to be updated, <c>false</c> else.</returns>
    /// <seealso cref="IFileUpdater"/>
    public bool NewFileVersionExists(string baseDirectory, string pathToLatestVersion, FileModel file)
    {
        var currentFile = Path.Combine(baseDirectory, file.FileName);
        var latestFile = Path.Combine(pathToLatestVersion, file.FileName);

        if (!File.Exists(latestFile))
        {
            return false;
        }

        if (!File.Exists(currentFile))
        {
            return true;
        }

        var currentVersion = FileVersionInfo.GetVersionInfo(currentFile).FileVersion;
        var latestVersion = FileVersionInfo.GetVersionInfo(latestFile).FileVersion;

        if (currentVersion is not null && latestVersion is not null && !currentVersion.Equals(latestVersion))
        {
            return true;
        }

        // Files without version information, such as a changelog or a configuration file, are only
        // comparable by their write time.
        return !File.GetLastWriteTime(currentFile).Equals(File.GetLastWriteTime(latestFile));
    }

    /// <inheritdoc cref="IFileUpdater"/>
    /// <summary>
    /// Copies a file if a newer version of it exists.
    /// </summary>
    /// <param name="baseDirectory">The directory of the installed file.</param>
    /// <param name="pathToLatestVersion">The directory of the latest version of the file.</param>
    /// <param name="file">The file.</param>
    /// <seealso cref="IFileUpdater"/>
    public void CopyFileIfNecessary(string baseDirectory, string pathToLatestVersion, FileModel file)
    {
        if (!this.NewFileVersionExists(baseDirectory, pathToLatestVersion, file))
        {
            return;
        }

        var currentFile = Path.Combine(baseDirectory, file.FileName);
        var latestFile = Path.Combine(pathToLatestVersion, file.FileName);
        File.Delete(currentFile);
        File.Copy(latestFile, currentFile);
    }
}
