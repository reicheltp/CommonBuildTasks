#addin "Cake.Xamarin"
#tool "GitVersion.CommandLine"

#addin "Cake.DocFx"
#tool "docfx.msbuild"

#tool "xunit.runner.console"

/// <summary>
///     Global shared build script with common tasks. 
/// </summary>

// *********************
//      ARGUMENTS
// *********************
var Target = Argument("target", "default");
var Configuration = Argument("configuration", "release");

// *********************
//      VARIABLES
// *********************
FilePath Solution = null;
var Version = GitVersion();
var NuSpecs = new List<FilePath>();
var Tests = new List<FilePath>();

FilePath DocFxConfig = File("./docs/docfx.json"); 
var BuildVerbosity = Verbosity.Minimal;

// *********************
//      SETUP
// *********************
Setup(() => 
{
    Information(string.Format("Building Version: {0} on branch {1}", Version.InformationalVersion, Version.BranchName));
});

// *********************
//      TASKS
// *********************

/// <summary>
///     Task to build the solution. Using MSBuild on Windows and MDToolBuild on OSX / Linux
/// </summary>
Task("build")
    .WithCriteria(() => 
    {
        var canBuild = Solution != null && FileExists(Solution);
        
        if(!canBuild)
            Information("Build skipped. To run a build set Solution variable before.");
        
        return canBuild;
    })
    .Does(() =>
    {
        if(IsRunningOnWindows())
            MSBuild(Solution, cfg => 
            {
                cfg.Configuration = Configuration;
                cfg.Verbosity = BuildVerbosity;
            });
        else
            MDToolBuild(Solution, cfg => 
            {
                cfg.Configuration = Configuration;
                cfg.IncreaseVerbosity = BuildVerbosity == Verbosity.Verbose;
            });
    });

/// <summary>
///     Task to clean all obj and bin directories as well as the ./output folder.
///     Commonly called right before build.
/// </summary>
Task("clean")
    .Does(() =>
    {
        CleanDirectories("./output");
        CleanDirectories(string.Format("./src/**/obj/{0}", Configuration));
        CleanDirectories(string.Format("./src/**/bin/{0}", Configuration));
        CleanDirectories(string.Format("./tests/**/obj/{0}", Configuration));
        CleanDirectories(string.Format("./tests/**/bin/{0}", Configuration));
    });

/// <summary>
///     The default task with a predefined flow.
/// </summary>
Task("default")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .IsDependentOn("update-version")    
    .IsDependentOn("build")
    .IsDependentOn("test")
    .IsDependentOn("pack")
    .IsDependentOn("doc");

/// <summary>
///     Task to generate a static documentation website for the current code. 
///     Set DocFxConfig variable to modify docfx config location.
/// </summary>
Task("doc")
    .WithCriteria(() => 
    {
        var canDoc = DocFxConfig != null && FileExists(DocFxConfig);
        
        if(!canDoc)
            Information("Doc skipped. To generate a documentation set DocFxConfig variable.");
            
        return canDoc;
    })
    .Does(() => DocFx(DocFxConfig));

/// <summary>
///     Task to pack NuGetPackages. Use NuSpecs variable to set the specs you want to pack.
/// </summary>
Task("pack")
    .WithCriteria(() =>
    {
        var canPack = NuSpecs != null && NuSpecs.Any();
        
        if(!canPack)
        {
            Information("Pack skipped. To run a NuGet pack add some specs to NuSpecs variable.");
            return false;
        }
            
        if(string.Compare(Configuration, "release", StringComparison.OrdinalIgnoreCase) != 0)
            throw new Exception(string.Format("{0} is not allowed since a nuget package has to be compiled with release configuration.", Configuration));

        return true;
    })
    .Does(() =>
    {
        CreateDirectory("./output/artifacts");
        
        NuGetPack(NuSpecs, new NuGetPackSettings
        {
            Version             = Version.NuGetVersion,
            OutputDirectory     = "./output/artifacts"
        } );
    });
    
/// <summary>
///     Task to rebuild. Nothing else than a clean followed by build.
/// </summary>
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("update-version")
    .IsDependentOn("build");

/// <summary>
///     Task to restore NuGet packages on solution level for all containing projects.
/// </summary>    
Task("restore")
    .WithCriteria(() => 
    {
        var canRestore = Solution != null && FileExists(Solution);
        
        if(!canRestore)
            Information("Restore skipped. To restore packages set Solution variable.");
        
        return canRestore;
    })
    .Does(() => NuGetRestore(Solution));
    
/// <summary>
///     Task to run unit tests.
/// </summary>
Task("test")
    .WithCriteria(() => 
    {
        var canTest = Tests != null && Tests.Any();
        
        if(!canTest)
            Information("Test skipped. To run tests add some dlls to Tests variable.");
        
        return canTest;
    })
    .Does(() => 
    {
        XUnit2(Tests, new XUnit2Settings
        {
            OutputDirectory = "./output/xunit",
            HtmlReport = true,
            Parallelism = ParallelismOption.All
        });
    });
    
/// <summary>
///     Task to update all assembly version files using GitVersion. 
/// </summary>
Task("update-version")
    .Does(() =>
    {
       GitVersion(new GitVersionSettings 
       {
            UpdateAssemblyInfo = true,
        });
    });
