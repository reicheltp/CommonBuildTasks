# CommonBuildTasks

A collection of common build tasks for cake. This a allow you to create super simple build files. But therefore it is less configurable.

### Sample

Add the following lines to you build scripts after restoring cake

./build.sh
```bash
mono $NUGET_EXE install CommonBuildTasks -Prerelease -ExcludeVersion -Out $TOOLS_DIR
```

./build.ps1
```
Invoke-Expression "$NUGET_EXE install CommonBuildTasks -Prerelease -ExcludeVersion -Out $TOOLS_DIR"
```

After this you can simply load the script within your cake file.

```cake
#l tools\CommonBuildTasks\content\shared.cake

// Set your solution file
Solution = "Sample.sln"

NuSpecs.Add("Sample.nuspec");

// Execution
RunTarget(Target);
```

Per default the following steps are run: `clean`, `restore`, `update-version`, `build`, `test`, `pack`, `doc`.
If one of the steps is not applyable for the project, the step is skipped.

### Tasks

| task | description | variables | tools |
| ---- | ----------- | --------- | ----- |
| **build** | Task to build the solution. Using MSBuild on Windows and MDToolBuild on OSX / Linux | `FilePath Solution = null` | MSBuild, MDToolBuild (Xamarin) |
| **clean** | Task to clean all obj and bin directories as well as the ./output folder. | - | - |
| **default** | The default task with a predefined flow. (`clean`, `restore`, `update-version`, `build`, `test`, `pack`, `doc`.)| - | - |
| **doc** | Task to generate a static documentation website for the current code.  | `FilePath DocFxConfig = "./docs/docfx.json"` | docfx |
| **pack** | Task to pack NuGetPackages. Use NuSpecs variable to set the specs you want to pack. | `List<FilePath> NuSpecs`  | NuGet |
| **rebuild** |  Task to rebuild. Nothing else than a clean followed by build. Performs `clean`, `update-version` and `build` | - | - |
| **restore** | Task to restore NuGet packages on solution level for all containing projects. | `FilePath Solution = null` | NuGet |
| **test** | Task to run unit tests. | `List<FilePath> Tests` | XUnit2 |
| **update-version** | Task to update all assembly version files using GitVersion. | - | GitVersion |
