// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataProvider.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class to provide the test data used in the tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SoftwareUpdater.Tests;

/// <summary>
/// A class to provide the test data used in the tests.
/// </summary>
public static class TestDataProvider
{
    /// <summary>
    /// The stub binary of the main project, linked into the output directory. Its file version is 1.0.0.1 and it
    /// never changes, which makes it the fixed side of every version comparison.
    /// </summary>
    public static readonly string BinaryWithFixedVersion = Path.Combine(AppContext.BaseDirectory, "TestData", "MainExecutable.exe");

    /// <summary>
    /// The test assembly itself. Its file version comes from GitVersion, so it differs from the one of the stub
    /// binary, which makes it the other side of every version comparison.
    /// </summary>
    public static readonly string BinaryWithBuildVersion = typeof(TestDataProvider).Assembly.Location;

    /// <summary>
    /// Gets the example configuration that is shipped next to the executable.
    /// </summary>
    /// <returns>The configuration as a <see cref="string"/>.</returns>
    public static string GetExampleConfiguration()
    {
        return """
            <?xml version="1.0"?>
            <Config>
            	<PreferredLanguage>Deutsch</PreferredLanguage>
            	<PathToLatestVersion>C:\Users\asdf\Desktop\Test</PathToLatestVersion>
            	<MainExecutable>
            		<FileName>MainExecutable.exe</FileName>
            		<StartAgain>true</StartAgain>
            	</MainExecutable>
            	<Files>
            		<FileModel>
            			<FileName>SecondExe.exe</FileName>
            			<StartAgain>false</StartAgain>
            		</FileModel>
            		<FileModel>
            			<FileName>Changelog.txt</FileName>
            			<StartAgain>true</StartAgain>
            		</FileModel>
            	</Files>
            </Config>
            """;
    }
}
