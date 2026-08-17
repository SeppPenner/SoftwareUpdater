// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFileUpdater.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface to update the files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Interface;

/// <summary>
/// An interface to update the files.
/// </summary>
public interface IFileUpdater
{
    /// <summary>
    /// Checks whether a newer version of a file exists.
    /// </summary>
    /// <param name="baseDirectory">The directory of the installed file.</param>
    /// <param name="pathToLatestVersion">The directory of the latest version of the file.</param>
    /// <param name="file">The file.</param>
    /// <returns><c>true</c> if the file needs to be updated, <c>false</c> else.</returns>
    bool NewFileVersionExists(string baseDirectory, string pathToLatestVersion, FileModel file);

    /// <summary>
    /// Copies a file if a newer version of it exists.
    /// </summary>
    /// <param name="baseDirectory">The directory of the installed file.</param>
    /// <param name="pathToLatestVersion">The directory of the latest version of the file.</param>
    /// <param name="file">The file.</param>
    void CopyFileIfNecessary(string baseDirectory, string pathToLatestVersion, FileModel file);
}
