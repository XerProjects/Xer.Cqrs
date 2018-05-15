///////////////////////////////////////////////////////////////////////////////
// ADDINS/TOOLS
///////////////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine"
#addin nuget:?package=Cake.Git

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument<string>("target", "Default");
var configuration = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutions = GetFiles("./**/*.sln");
var projects = GetFiles("./**/*.csproj").Select(x => x.GetDirectory());
BuildParameters buildParameters;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    buildParameters = new BuildParameters(Context);
    
    // Executed BEFORE the first task.
    Information("Xer.Cqrs");
    Information("===========================================================================================");
    Information("Git Version");
    Information("Semver: {0}", buildParameters.GitVersion.LegacySemVerPadded);
    Information("Major minor patch: {0}", buildParameters.GitVersion.MajorMinorPatch);
    Information("Assembly: {0}", buildParameters.GitVersion.AssemblySemVer);
    Information("Informational: {0}", buildParameters.GitVersion.InformationalVersion);
    if (DirectoryExists(buildParameters.BuildArtifactsDirectory))
    {
        // Cleanup build artifacts.
        Information($"Cleaning up {buildParameters.BuildArtifactsDirectory} directory.");
        DeleteDirectory(buildParameters.BuildArtifactsDirectory, new DeleteDirectorySettings { Recursive = true });
    }    
    Information("===========================================================================================");
});

Teardown(context =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    if (projects.Count() == 0)
    {
        Information("No projects found.");
        return;
    }

    // Clean solution directories.
    foreach (var project in projects)
    {
        Information("Cleaning {0}", project);
        DotNetCoreClean(project.FullPath);
    }
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{    
    if (solutions.Count() == 0)
    {
        Information("No solutions found.");
        return;
    }

    var settings = new DotNetCoreRestoreSettings
    {
        ArgumentCustomization = args => buildParameters.AppendVersionArguments(args)
    };

    // Restore all NuGet packages.
    foreach (var solution in solutions)
    {
        Information("Restoring {0}...", solution);
      
        DotNetCoreRestore(solution.FullPath, settings);
    }
});

Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    if (solutions.Count() == 0)
    {
        Information("No solutions found.");
        return;
    }
    
    var settings = new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        ArgumentCustomization = args => buildParameters.AppendVersionArguments(args)
    };

    // Build all solutions.
    foreach (var solution in solutions)
    {
        Information("Building {0}", solution);
        
        DotNetCoreBuild(solution.FullPath, settings);
    }
});

Task("Test")
    .Description("Execute all unit test projects.")
    .IsDependentOn("Build")
    .Does(() =>
{
    var projects = GetFiles("./Tests/**/*.Tests.csproj");
    
    if (projects.Count == 0)
    {
        Information("No test projects found.");
        return;
    }

    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    };

    foreach (var project in projects)
    {
        DotNetCoreTest(project.FullPath, settings);
    }
});

Task("Pack")
    .Description("Create NuGet packages.")
    .IsDependentOn("Test")
    .Does(() =>
{
    var projects = GetFiles("./Src/**/*.csproj");
    
    if (projects.Count() == 0)
    {
        Information("No projects found.");
        return;
    }

    var settings = new DotNetCorePackSettings 
    {
        OutputDirectory = buildParameters.BuildArtifactsDirectory,
        Configuration = configuration,
        NoBuild = true,
        ArgumentCustomization = args => buildParameters.AppendVersionArguments(args)
    };

    foreach (var project in projects)
    {
        DotNetCorePack(project.ToString(), settings);
    }
});

///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("Pack")
    .IsDependentOn("Test")
    .IsDependentOn("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

public class BuildParameters
{    
    private ICakeContext _context;
    private GitVersion _gitVersion;

    public BuildParameters(ICakeContext context)
    {
        _context = context;
        _gitVersion = context.GitVersion();
    }

    public GitVersion GitVersion => _gitVersion;

    public string BuildArtifactsDirectory => "./BuildArtifacts";

    public ProcessArgumentBuilder AppendVersionArguments(ProcessArgumentBuilder args) => args
        .Append("/p:Version={0}", GitVersion.LegacySemVerPadded)
        .Append("/p:AssemblyVersion={0}", GitVersion.MajorMinorPatch)
        .Append("/p:FileVersion={0}", GitVersion.MajorMinorPatch)
        .Append("/p:AssemblyInformationalVersion={0}", GitVersion.InformationalVersion);
}