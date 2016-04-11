// Import default tasks
#l src\shared.cake

CustomNuGetFeedName = "365FarmNet Feed";
CustomNuGetFeed = "https://nuget.365farmnet.com/NugetGallery/nuget/";

// Set project dependent variables
NuSpecs.Add("CommonBuildTasks.nuspec");

// Execution
RunTarget(Target);
