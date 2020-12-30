// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The configuration class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Configuration
{
    using System.Collections.Generic;

    /// <summary>
    /// The configuration class.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Gets or sets the path to the latest version.
        /// </summary>
        public string PathToLatestVersion { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        public List<FileModel> Files { get; set; }

        /// <summary>
        /// Gets or sets the main executable.
        /// </summary>
        public FileModel MainExecutable { get; set; }

        /// <summary>
        /// Gets or sets the preferred language.
        /// </summary>
        public string PreferredLanguage { get; set; }
    }
}