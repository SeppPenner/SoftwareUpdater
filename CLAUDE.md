# Project rules for Claude

## What this is

SoftwareUpdater is a small Windows Forms executable that updates another application. It is meant
to be shipped next to that application, started instead of it, and to hand control over to it once
the update is done. It shows a splash screen, compares the files listed in `UpdateConfig.xml`
against a copy on a share, replaces the outdated ones, starts the application again and exits.

The repository is an application, it is **not** published as a NuGet package: no
`GeneratePackageOnBuild`, no push script. It ships as an Inno Setup installer that is built from
`Setup/SoftwareUpdater-Setup.iss` and attached to the GitHub release of the version tag. Up to and
including version 1.0.8 the installer was committed into `Setup/` instead, which is why the git
history carries one copy per release.

One solution `src/SoftwareUpdater.sln` with exactly one project:

- `src/SoftwareUpdater/SoftwareUpdater.csproj`, `OutputType` `WinExe`, `UseWindowsForms`, target
  framework `net10.0-windows`, `RuntimeIdentifiers` `win-x64`.

Layout inside `src/SoftwareUpdater`:

- `Program.cs`: `Main`, nothing but `Application.Run(new Splash())`.
- `Splash.cs` plus `Splash.Designer.cs` and `Splash.resx`: the whole application. The form has no
  controls to operate, only a picture box and a label. Everything happens in the constructor and in
  the timer callback.
- `Configuration/Config.cs` and `Configuration/FileModel.cs`: the deserialized `UpdateConfig.xml`.
- `Implementation/GetConfig.cs` plus `Interface/IGetConfig.cs`: loads that file.
- `Implementation/FileUpdater.cs` plus `Interface/IFileUpdater.cs`: decides whether a file is
  outdated and copies it. Split out of `Splash.cs` in version 1.0.8.0 so that the decision is
  testable without a form.
- `GlobalUsings.cs`: all usings of the project, including the alias `Timer`.
- `languages/de-DE.xml` and `languages/en-US.xml`: the language files, three keys each.
- `UpdateConfig.xml`, `MainExecutable.exe`, `SecondExe.exe`, `Changelog.txt`: the example
  configuration and its payload, see "Known quirks".
- `Splash.jpg`, `Update.ico`: the splash image and the application icon.
- `Changelog.md`, `README.md`, `License.txt`: copies of the repository root files that are copied
  next to the executable.

`src/SoftwareUpdater.Tests/SoftwareUpdater.Tests.csproj`, MSTest, added in version 1.0.8.0:

- `GetConfigTests.cs`: the full example configuration, a configuration without
  `PreferredLanguage`, one without any files, an unknown element, a missing file and broken XML.
- `FileUpdaterTests.cs`: equal and differing file versions, files without version information,
  a missing file and the copy itself. Each test writes into its own directory below
  `Path.GetTempPath()` and deletes it afterwards, so a test run leaves the working tree untouched.
- `TestDataProvider.cs`: the paths of the two binaries the version tests compare. The project links
  `MainExecutable.exe` from the main project into its output instead of adding another binary to
  the repository.
- `GlobalUsings.cs`: all usings of the test project.

Repository root: `README.md` (the only user documentation, with the configuration sample),
`Changelog.md`, `License.txt` (MIT), `Screenshot.png`, `.editorconfig` in `src/` and
`.gitattributes`. There is no `Updating.md`, no `HowToUse.md` and no `.github` folder.

## Build

```powershell
dotnet build src/SoftwareUpdater.sln -c Release
```

```powershell
dotnet test src/SoftwareUpdater.sln
```

- Single target framework `net10.0-windows` in both projects, no multi-targeting.
- All build properties live directly in the `.csproj` files. There is **no**
  `Directory.Build.props` in this repository.
- `TreatWarningsAsErrors` is enabled, so every warning breaks the build, NuGet warnings (`NU****`)
  from restore included. A clean build reports zero warnings, keep it that way.
- `NU1803` (HTTP source usage during restore) is the one warning suppressed via `NoWarn`. Fix
  warnings instead of extending that list. `NuGetAudit` and `NuGetAuditMode=all` are on, so a
  vulnerable transitive package fails the build too.
- Versions come from GitVersion.MsBuild out of the git tags, for example `1.0.8-1` for the first
  commit after tag `1.0.8`. Never edit a version property or an assembly version by hand.
- Restore needs nuget.org. If a private feed is configured globally on the machine and cannot be
  reached, restore fails with `NU1900` as an error, because warnings are errors. Then build with an
  explicit source:
  `dotnet build src/SoftwareUpdater.sln -c Release --source https://api.nuget.org/v3/index.json`.
  The same applies to `dotnet test` and to `dotnet list package --outdated`, which additionally
  needs `--no-restore` after a restore with that source.
- Tests are MSTest in `src/SoftwareUpdater.Tests`, with the same package set as the sibling
  repositories: `Microsoft.NET.Test.Sdk`, `MSTest.TestAdapter`, `MSTest.TestFramework`,
  `coverlet.collector` and `GitVersion.MsBuild`. `dotnet test` runs 14 tests, they need no network
  and no fixture outside the repository. Never claim a test run happened without running it.
- Beyond the tests, a behaviour change is verified by publishing and starting the executable. It
  shows the splash screen for two seconds, so a run of a few seconds is enough to see whether it
  starts up and reads its configuration.

## Code conventions

Follow the surrounding code, it is consistent in every file except the generated designer file:

- File header comment block with `<copyright file="..." company="Hämmer Electronics">` and a
  `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included, no exceptions.
  Implementations of an interface member additionally carry `<inheritdoc cref="..."/>` and
  `<seealso cref="..."/>` pointing at that interface.
- `Nullable`, `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the
  existing `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig
  requires usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`),
  which global usings cannot satisfy, that is what the pragma is for. Do not add other pragmas. The
  comment text in that block is German because Visual Studio generated it, leave it alone.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, four spaces, CRLF,
  UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005` as warning. Analyzer
  warnings are fixed, not silenced.
- `Splash.Designer.cs` follows none of this. It is generated, it has no header block, it uses a
  block scoped namespace and its comments are German. Leave it to the designer.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **Every failure starts the application and exits.** Both `LoadSplash` and `GetLatestVersion`
  catch everything, start `config.MainExecutable.FileName` and call `Environment.Exit(0)` without
  any message. That is deliberate: a broken updater must never keep the application from starting.
  It also means a misconfigured updater looks exactly like a working one that had nothing to do.
- **The example payload is not the updater.** `MainExecutable.exe` and `SecondExe.exe` are two
  identical stub binaries of 367616 bytes with file version `1.0.0.1`, and `Changelog.txt` contains
  the single line `Example Changelog`. They exist so that `UpdateConfig.xml` points at something
  that is actually there. They are never started by the repository itself.
- **Committed binaries despite `.gitignore`.** The ignore file excludes `*.exe`, yet
  `MainExecutable.exe` and `SecondExe.exe` are tracked. They were added with `git add -f` and have
  to be updated the same way. They are payload of the example configuration and change about never.
  The installer is **not** in that group any more, see "Releasing". Do not add it back with
  `git add -f`, it is 36 MB per release and stays in the history forever.
- **`PreferredLanguage` is the language name, not the culture.** `SetCurrentLanguageFromName` from
  `HaemmerElectronics.SeppPenner.Language` compares against the `<Name>` element of the language
  files, so the valid values are `Deutsch` and `English (US)`, not `de-DE` and `en-US`. An unknown
  or empty name throws, which lands in the catch described above. Up to version 1.0.7.0 the shipped
  `UpdateConfig.xml` had no `PreferredLanguage` element at all, so the shipped example never got
  past that point.
- **`GetWord` returns `null` for an unknown key.** There is no fallback to another language, so a
  key that is missing in one of the two language files shows an empty message. Add new keys to both
  files.
- **The timer callback does not run on the UI thread.** `Timer` is aliased to `System.Timers.Timer`
  in `GlobalUsings.cs`, so `GetLatestVersion` runs on a thread pool thread. It touches no control,
  which is why there is no `Invoke`. Anything that does touch a control has to marshal.
- **`IsElevated` compares owner and user.** `id.Owner != id.User` is true for an elevated process,
  it is not a check for membership in the administrators group. The check runs before the timer
  starts and exits the process with a message if it fails.
- **The order in the constructor matters.** `InitializeComponent`, then `LoadPaths`, then
  `LoadSplash`. The language manager is only initialized inside `LoadSplash`, so `this.language` is
  still `null` while `LoadPaths` runs. The error dialog there falls back to a hard coded title for
  exactly that reason.
- **Three files exist twice.** `README.md` and `License.txt` in `src/SoftwareUpdater` are byte
  identical copies of the ones in the repository root, `Changelog.md` is a copy as well. They are
  copied next to the executable so that the installed application carries its own documentation. A
  change in the root file has to be repeated in the copy, up to version 1.0.7.0 the copy of the
  changelog had fallen behind by three releases.
- **AppVeyor badge without CI in the repository.** `README.md` links an AppVeyor build that is
  configured outside of this repository. There is no pipeline file here.
- **`src/SoftwareUpdater.sln.DotSettings`** is tracked and holds nothing but a ReSharper user
  dictionary (`H_00E4mmer`). Leave it alone.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. Any binary file that git must not normalize needs its own rule.
- **The installer needs administrator rights.** `DefaultDirName={commonpf}` installs into
  `C:\Program Files`, and the updater replaces files in its own directory, which is why it insists
  on being elevated at runtime.

## Releasing

1. Make the change.
2. Add an entry at the top of `Changelog.md` in the existing format:
   `* **Version 1.0.8.0 (2026-08-17)** : Short description.`
   Repeat it in `src/SoftwareUpdater/Changelog.md`.
3. Set `MyAppVersion` in `Setup/SoftwareUpdater-Setup.iss` to the same four part version. The file
   is UTF-8 **with** BOM and CRLF, keep both.
4. Commit that.
5. Tag the commit with the plain version number, no `v` prefix (`1.0.8`, `1.0.7`, ...). The existing
   tags are lightweight tags, create new ones the same way.
6. Run `Setup/build-setup-files.bat`, then compile `Setup/SoftwareUpdater-Setup.iss` with
   `ISCC.exe`. The tag has to exist before this step, otherwise GitVersion burns a prerelease
   version into the shipped executable.
7. Push the commits and the tag.
8. Attach `Setup/SoftwareUpdater-Setup.exe` to the GitHub release of that tag. **Never commit the
   installer.** It is self contained and weighs 36 MB, and every committed copy stays in the
   history for good. `Setup/` is the `OutputDir` of the Inno Setup script, so the file lands there
   during the build and is ignored by `.gitignore` afterwards.

The version in the `Changelog.md` has four parts (`1.0.8.0`), the tag has three (`1.0.8`).
GitVersion turns the tag into the assembly version, so an untagged commit produces something like
`1.0.8-1+Branch.master.Sha...`.

For step 8 there is no `gh` on this machine. The GitHub API does the job, with the token that
`git push` already uses, so nothing has to be stored anywhere:

```bash
c=$(printf "protocol=https\nhost=github.com\n\n" | git credential fill)
tok=$(printf "%s" "$c" | grep '^password=' | cut -d= -f2-)
id=$(curl -s -X POST -H "Authorization: Bearer $tok" \
  https://api.github.com/repos/SeppPenner/SoftwareUpdater/releases \
  -d '{"tag_name":"1.0.9","name":"1.0.9"}' | grep -m1 '"id"' | tr -dc 0-9)
curl -s -X POST -H "Authorization: Bearer $tok" -H "Content-Type: application/octet-stream" \
  --data-binary @Setup/SoftwareUpdater-Setup.exe \
  "https://uploads.github.com/repos/SeppPenner/SoftwareUpdater/releases/$id/assets?name=SoftwareUpdater-Setup.exe"
```

Never print that token, and never write it into a file.

`Setup/build-setup-files.bat` deletes every `bin` and `obj` below `src`, publishes self contained
for `win-x64` into `src/SoftwareUpdater/bin/publish` and removes the `*.pdb` files. In an
environment with `NoDefaultCurrentDirectoryInExePath` it has to be started as
`call .\build-setup-files.bat` from inside the `Setup` folder, because the `cd ..\src` in it is
relative to the start directory.

## Git

- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  release versions come from tags on exact commits, an amended commit leaves its tag pointing at a
  commit that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
