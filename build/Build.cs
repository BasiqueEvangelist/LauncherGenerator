using System;
using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace LauncherGenerator.Build
{
    [CheckBuildProjectConfigurations]
    [ShutdownDotNetAfterServerBuild]
    class Build : NukeBuild
    {
        /// Support plugins are available for:
        ///   - JetBrains ReSharper        https://nuke.build/resharper
        ///   - JetBrains Rider            https://nuke.build/rider
        ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
        ///   - Microsoft VSCode           https://nuke.build/vscode

        public static int Main() => Execute<Build>(x => x.Compile);

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [Solution] readonly Solution Solution;
        [GitRepository] readonly GitRepository GitRepository;
        [GitVersion(NoFetch = true, Framework = "net5.0")] readonly GitVersion GitVersion;

        AbsolutePath SourceDirectory => RootDirectory / "sources";
        AbsolutePath OutputDirectory => RootDirectory / "output";

        Target Clean => _ => _
            .Before(Restore)
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(OutputDirectory);
            });

        Target Restore => _ => _
            .Executes(() =>
            {
                DotNetRestore(s => s
                    .SetProjectFile(Solution));
            });

        Target BuildSingle => _ => _
            .DependsOn(CompileMCAH)
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetPublish(s => s
                    .SetProject(Solution.GetProject("LauncherGenerator"))
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .SetOutput(OutputDirectory / "LauncherGenerator")
                    .SetRuntime(GetRID())
                    .SetPublishSingleFile(true));
            });

        Target Compile => _ => _
            .DependsOn(CompileMCAH)
            .DependsOn(Restore)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion)
                    .SetOutputDirectory(OutputDirectory / "LauncherGeneratorCompile")
                    .EnableNoRestore());
            });

        Target CompileMCAH => _ => _
            .Executes(() =>
            {
                var srcDir = SourceDirectory / "MCAuthHelper";
                var targetDir = (RelativePath)Path.GetRelativePath(srcDir, OutputDirectory / "MCAuthHelper");
                CargoTasks.CargoBuild(s => s
                    .SetRelease(Configuration == Configuration.Release)
                    .SetTargetDir(targetDir)
                    .SetProcessWorkingDirectory(srcDir));

                var outFile = OutputDirectory / "MCAuthHelper" / (Configuration == Configuration.Release ? "release" : "debug") / ("mcauthhelper" + (IsWin ? ".exe" : ""));

                CopyFileToDirectory(outFile, OutputDirectory / "MCAuthHelper" / "LauncherGeneratorNeeded", FileExistsPolicy.OverwriteIfNewer);
            });

        static string GetRID()
        {
            return Platform switch
            {
                PlatformFamily.Linux => "linux",
                PlatformFamily.OSX => "osx",
                PlatformFamily.Windows => "win",

                _ => throw new NotImplementedException()
            } + (Is64Bit ? "-x64" : "-x86");
        }
    }
}