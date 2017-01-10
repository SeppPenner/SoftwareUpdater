SoftwareUpdater
====================================

SoftwareUpdater is an executable to update other software.
The assembly was written and tested in .Net 4.6.2.

## Basic usage:
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

## How the full configuration needs to look like:
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

An example project can be found [here]().

Change history
--------------

* **Version 1.0.0.0 (2017-01-10)** : 1.0 release.