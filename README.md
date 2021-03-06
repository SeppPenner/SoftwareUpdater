SoftwareUpdater
====================================

SoftwareUpdater is an executable to update other software.
The assembly was written and tested in .Net 5.0.

[![Build status](https://ci.appveyor.com/api/projects/status/vgx29eqgt9ply7b7?svg=true)](https://ci.appveyor.com/project/SeppPenner/softwareupdater)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/SoftwareUpdater.svg)](https://github.com/SeppPenner/SoftwareUpdater/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/SoftwareUpdater.svg)](https://github.com/SeppPenner/SoftwareUpdater/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/SoftwareUpdater.svg)](https://github.com/SeppPenner/SoftwareUpdater/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/SoftwareUpdater/master/License.txt)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/SoftwareUpdater/badge.svg)](https://snyk.io/test/github/SeppPenner/SoftwareUpdater)

## Screenshot from the executable
![Screenshot from the executable](https://github.com/SeppPenner/SoftwareUpdater/blob/master/Screenshot.png "Screenshot from the executable")

## Basic usage
The PreferredLanguage property needs to be set to the language that is wanted.
See the languages subfolder in the execution location (to add new languages)
and the language manager: https://github.com/SeppPenner/CSharpLanguageManager
```xml
<PreferredLanguage>Deutsch</PreferredLanguage>
```

The PathToLatestVersion property needs to be set to the path to where
the newest executable is located, e.g. a server share, etc.
```xml
<PathToLatestVersion>C:\Users\asdf\Desktop\Test</PathToLatestVersion>
```

The MainExecutable property needs to be set to the application's main
executable. StartAgain means that after the update the executable is
restarted.
```xml
<MainExecutable>
	<FileName>MainExecutable.exe</FileName>
	<StartAgain>true</StartAgain>
</MainExecutable>
```

The files property contains all other files that should be updated and
if they should be restarted after the update.
```xml
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
```

## How the full configuration needs to look like (Version 1.0.0.2)
The config file needs to be named "UpdateConfig.xml".
```xml
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
```
## How the full configuration needs to look like (Version 1.0.0.1 and prior)
The config file needs to be named "UpdateConfig.xml".
```xml
<?xml version="1.0"?>
<Config>
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
```

An example project can be found [here](https://github.com/SeppPenner/SoftwareUpdater/tree/master/Sourcecode).
A test setup can be found [here](https://github.com/SeppPenner/SoftwareUpdater/tree/master/Testsetup).

## Special advice for version 1.0.0.0
If the executable is located under **C:\Program Files** or **C:\Program Files (x86)**, **ADMIN RIGHTS**
will be needed to use the updater properly. Otherwise it will not perform an update.

Change history
--------------

See the [Changelog](https://github.com/SeppPenner/SoftwareUpdater/blob/master/Changelog.md).