// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGetConfig.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   An interface to load the configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Interface;

/// <summary>
/// An interface to load the configuration.
/// </summary>
public interface IGetConfig
{
    /// <summary>
    /// Loads the configuration.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The <see cref="Config"/>.</returns>
    Config ImportConfiguration(string fileName);
}
