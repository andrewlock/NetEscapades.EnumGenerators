using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    AbsolutePath OutputDirectory => RootDirectory / "artifacts";
    AbsolutePath ArtifactsDirectory => OutputDirectory / "packages";
    AbsolutePath TestResultsDirectory => OutputDirectory / "results";

    [Parameter] readonly string NuGetToken;
    [Parameter] readonly AbsolutePath PackagesDirectory = RootDirectory / "packages";

    const string NugetOrgUrl = "https://api.nuget.org/v3/index.json";
    bool IsTag => GitHubActions.Instance?.Ref?.StartsWith("refs/tags/") ?? false;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            OutputDirectory.CreateOrCleanDirectory();
            if (!string.IsNullOrEmpty(PackagesDirectory))
            {
                PackagesDirectory.CreateOrCleanDirectory();
            }
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .When(!string.IsNullOrEmpty(PackagesDirectory), x=>x.SetPackageDirectory(PackagesDirectory))
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .When(IsServerBuild, x => x.SetProperty("ContinuousIntegrationBuild", "true"))
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .When(IsServerBuild, x => x.SetProperty("ContinuousIntegrationBuild", "true"))
                .When(IsServerBuild, x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .EnableNoBuild()
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .After(Test)
        .Produces(ArtifactsDirectory)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetConfiguration(Configuration)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableNoBuild()
                .EnableNoRestore()
                .When(IsServerBuild, x => x.SetProperty("ContinuousIntegrationBuild", "true"))
                .SetProject(Solution));
        });

    Target TestPackage => _ => _
        .DependsOn(Pack)
        .After(Test)
        .Executes(() =>
        {
            var projectFiles = new[]
            {
                Solution.tests.NetEscapades_EnumGenerators_Nuget_IntegrationTests.Path,
                Solution.tests.NetEscapades_EnumGenerators_Nuget_Interceptors_IntegrationTests.Path,
                Solution.tests.NetEscapades_EnumGenerators_Nuget_NetStandard_Interceptors_IntegrationTests.Path,
                Solution.tests.NetEscapades_EnumGenerators_Nuget_NetStandard_SystemMemory_IntegrationTests.Path,
            };

            if (!string.IsNullOrEmpty(PackagesDirectory))
            {
                (PackagesDirectory / "netescapades.enumgenerators").DeleteDirectory();
                (PackagesDirectory / "netescapades.enumgenerators.attributes").DeleteDirectory();
                (PackagesDirectory / "netescapades.enumgenerators.interceptors").DeleteDirectory();
                (PackagesDirectory / "netescapades.enumgenerators.interceptors.attributes").DeleteDirectory();
            }

            DotNetRestore(s => s
                .When(!string.IsNullOrEmpty(PackagesDirectory), x => x.SetPackageDirectory(PackagesDirectory))
                .SetConfigFile(RootDirectory / "NuGet.integration-tests.config")
                .CombineWith(projectFiles, (s, p) => s.SetProjectFile(p)));

            DotNetBuild(s => s
                .When(!string.IsNullOrEmpty(PackagesDirectory), x=>x.SetPackageDirectory(PackagesDirectory))
                .EnableNoRestore()
                .SetConfiguration(Configuration)
                .CombineWith(projectFiles, (s, p) => s.SetProjectFile(p)));

            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .When(IsServerBuild, x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .CombineWith(projectFiles, (s, p) => s.SetProjectFile(p)));

            // Now test the analyzers in the package cause warnings as expected
            var expectedAnalyzerErrors = new[]
            {
                "NEEG004",
                "NEEG005",
                "NEEG006",
                "NEEG007",
                "NEEG008",
                "NEEG009",
                "NEEG010",
                "NEEG011",
            };

            DotNetRestore(s => s
                .When(!string.IsNullOrEmpty(PackagesDirectory), x => x.SetPackageDirectory(PackagesDirectory))
                .SetConfigFile(RootDirectory / "NuGet.integration-tests.config")
                .SetProjectFile(Solution.tests.NetEscapades_EnumGenerators_Nuget_AnalyzerTests));

            // Capture the logs so that we don't pollute the CI etc build output 
            var buildLogs = new ConcurrentQueue<string>();
            DotNetBuild(s => s
                .When(!string.IsNullOrEmpty(PackagesDirectory), x => x.SetPackageDirectory(PackagesDirectory))
                .EnableNoRestore()
                .SetConfiguration(Configuration)
                .SetProcessLogger((_, log) => buildLogs.Enqueue(log))
                .SetNoIncremental(true) // force rebuild so if run twice we still get valid results
                .SetProjectFile(Solution.tests.NetEscapades_EnumGenerators_Nuget_AnalyzerTests));
            
            // verify that we have errors
            foreach (var expectedAnalyzerError in expectedAnalyzerErrors)
            {
                var foundLogs = buildLogs.Any(x => x.Contains(expectedAnalyzerError));
                Serilog.Log.Information("Checking for {AnalyzerOutput}: {FoundLogs}", expectedAnalyzerError, foundLogs);
                if (!foundLogs)
                {
                    // print the output
                    foreach (var buildLog in buildLogs)
                    {
                        Serilog.Log.Debug(buildLog);
                    }

                    throw new Exception("Analyzer output did not contain expected analyzer error: " +
                                        expectedAnalyzerError);
                }
            }
        });

    Target PushToNuGet => _ => _
        .DependsOn(Pack)
        .OnlyWhenStatic(() => IsTag && IsServerBuild && IsWin)
        .Requires(() => NuGetToken)
        .After(TestPackage)
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg");
            DotNetNuGetPush(s => s
                .SetApiKey(NuGetToken)
                .SetSource(NugetOrgUrl)
                .EnableSkipDuplicate()
                .CombineWith(packages, (x, package) => x
                    .SetTargetPath(package)));
        });
}
