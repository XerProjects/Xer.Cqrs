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

GitVersion gitVersion;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    gitVersion = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true
    });

    BuildParameters.Initialize(Context);
    
    // Executed BEFORE the first task.
    Information("Xer.Cqrs");
    Information("Parameters");
    Information("///////////////////////////////////////////////////////////////////////////////");
    Information("Branch: {0}", BuildParameters.Instance.BranchName);
    Information("Version semver: {0}", gitVersion.LegacySemVerPadded);
    Information("Version assembly: {0}", gitVersion.MajorMinorPatch);
    Information("Version informational: {0}", gitVersion.InformationalVersion);
    Information("Master branch: {0}", BuildParameters.Instance.IsMasterBranch);
    Information("Dev branch: {0}", BuildParameters.Instance.IsDevBranch);
    Information("Hotfix branch: {0}", BuildParameters.Instance.IsHotFixBranch);
    Information("Publish to myget: {0}", BuildParameters.Instance.ShouldPublishMyGet);
    Information("Publish to nuget: {0}", BuildParameters.Instance.ShouldPublishNuGet);
    Information("///////////////////////////////////////////////////////////////////////////////");
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
        ArgumentCustomization = args => args
            .Append("/p:Version={0}", gitVersion.LegacySemVerPadded)
            .Append("/p:AssemblyVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:FileVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:AssemblyInformationalVersion={0}", gitVersion.InformationalVersion)
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
        ArgumentCustomization = args => args
            .Append("/p:Version={0}", gitVersion.LegacySemVerPadded)
            .Append("/p:AssemblyVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:FileVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:AssemblyInformationalVersion={0}", gitVersion.InformationalVersion)
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
    .IsDependentOn("Test")
    .Does(() =>
{
    var projects = GetFiles("./src/**/*.csproj");
    
    if (projects.Count() == 0)
    {
        Information("No projects found.");
        return;
    }

    var settings = new DotNetCorePackSettings 
    {
        NoBuild = true,
        Configuration = configuration,
        ArgumentCustomization = (args) => args
            .Append("/p:Version={0}", gitVersion.LegacySemVerPadded)
            .Append("/p:AssemblyVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:FileVersion={0}", gitVersion.MajorMinorPatch)
            .Append("/p:AssemblyInformationalVersion={0}", gitVersion.InformationalVersion)
    };

    foreach (var project in projects)
    {
        DotNetCorePack(project.ToString(), settings);
    }
});

Task("PublishMyGet")
    .WithCriteria(() => BuildParameters.Instance.ShouldPublishMyGet)
    .IsDependentOn("Pack")
    .Does(() =>
{
    var nupkgs = GetFiles("./**/*.nupkg");
    
    if (nupkgs.Count() == 0)
    {
        Information("No nupkgs found.");
        return;
    }

    foreach (var nupkgFile in nupkgs)
    {
        Information("Pulishing to myget {0}", nupkgFile);

        NuGetPush(nupkgFile, new NuGetPushSettings 
        {
            Source = BuildParameters.Instance.MyGetFeed,
            ApiKey = BuildParameters.Instance.MyGetApiKey
        });
    }
});

Task("PublishNuGet")
    .WithCriteria(() => BuildParameters.Instance.ShouldPublishNuGet)
    .IsDependentOn("Pack")
    .Does(() =>
{
    var nupkgs = GetFiles("./**/*.nupkg");

    if (nupkgs.Count() == 0)
    {
        Information("No nupkgs found.");
        return;
    }

    foreach (var nupkgFile in nupkgs)
    {
        Information("Pulishing to nuget {0}", nupkgFile);
        NuGetPush(nupkgFile, new NuGetPushSettings 
        {
            Source = BuildParameters.Instance.NuGetFeed,
            ApiKey = BuildParameters.Instance.NuGetApiKey
        });
    }
});


///////////////////////////////////////////////////////////////////////////////
// TARGETS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("PublishNuGet")
    .IsDependentOn("PublishMyGet");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);

public class BuildParameters
{
    private static BuildParameters _buildParameters;

    public static BuildParameters Instance => _buildParameters;
    
    private ICakeContext _context;

    private BuildParameters(ICakeContext context)
    {
        _context = context;
    }

    public static void Initialize(ICakeContext context)
    {
        if(_buildParameters != null)
        {
            return;
        }

        _buildParameters = new BuildParameters(context);
    }

    public bool IsAppVeyorBuild => _context.BuildSystem().AppVeyor.IsRunningOnAppVeyor;

    public bool IsLocalBuild => _context.BuildSystem().IsLocalBuild;

    public string BranchName
    {
        get
        {
            return IsLocalBuild 
                ? _context.GitBranchCurrent(".").FriendlyName
                : _context.BuildSystem().AppVeyor.Environment.Repository.Branch;
        }
    }

    public string MyGetFeed => _context.EnvironmentVariable("MYGET_SOURCE");

    public string MyGetApiKey => _context.EnvironmentVariable("MYGET_API_KEY");

    public string NuGetFeed => _context.EnvironmentVariable("NUGET_SOURCE");

    public string NuGetApiKey => _context.EnvironmentVariable("NUGET_API_KEY");

    public bool IsMasterBranch => StringComparer.OrdinalIgnoreCase.Equals("master", BranchName);

    public bool IsDevBranch => StringComparer.OrdinalIgnoreCase.Equals("dev", BranchName);

    public bool IsReleaseBranch => BranchName.StartsWith("release", StringComparison.OrdinalIgnoreCase);

    public bool IsHotFixBranch => BranchName.StartsWith("hotfix", StringComparison.OrdinalIgnoreCase);

    public bool ShouldPublishMyGet => !string.IsNullOrWhiteSpace(MyGetApiKey) && !string.IsNullOrWhiteSpace(MyGetFeed);

    public bool ShouldPublishNuGet => !string.IsNullOrWhiteSpace(NuGetApiKey) 
        && !string.IsNullOrWhiteSpace(NuGetFeed)
        && (IsMasterBranch || IsHotFixBranch);
}