// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetConfig.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to load the configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Implementation;

/// <inheritdoc cref="IGetConfig"/>
/// <summary>
/// A class to load the configuration.
/// </summary>
/// <seealso cref="IGetConfig"/>
public class GetConfig : IGetConfig
{
    /// <inheritdoc cref="IGetConfig"/>
    /// <summary>
    /// Loads the configuration.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The <see cref="Config"/>.</returns>
    /// <seealso cref="IGetConfig"/>
    public Config ImportConfiguration(string fileName)
    {
        var xDocument = XDocument.Load(fileName);
        return CreateObjectFromString<Config>(xDocument) ?? new();
    }

    /// <summary>
    /// Creates a object from the <see cref="string"/>.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="xDocument">The X document.</param>
    /// <returns>A new object of type <see cref="T"/>.</returns>
    private static T? CreateObjectFromString<T>(XDocument xDocument)
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        return (T?)xmlSerializer.Deserialize(new StringReader(xDocument.ToString()));
    }
}
