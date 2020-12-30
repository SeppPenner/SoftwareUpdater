// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileModel.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The file model class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Configuration
{
    /// <summary>
    /// The file model class.
    /// </summary>
    public class FileModel
    {
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the process should be started again or not.
        /// </summary>
        public bool StartAgain { get; set; }
    }
}