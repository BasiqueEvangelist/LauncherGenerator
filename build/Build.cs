using System.IO;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
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
        [PathExecutable("cargo")] readonly Tool Cargo;

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

        Target Compile => _ => _
            .DependsOn(Restore)
            .DependsOn(CompileMCAH)
            .Executes(() =>
            {
                DotNetBuild(s => s
                    .SetProjectFile(Solution)
                    .SetConfiguration(Configuration)
                    .SetOutputDirectory(OutputDirectory / "LauncherGenerator")
                    .EnableNoRestore());
            });

        Target CompileMCAH => _ => _
            .Executes(() =>
            {
                var releaseFlag = Configuration == Configuration.Release ? "--release" : "";
                var srcDir = SourceDirectory / "MCAuthHelper";
                var targetDir = (RelativePath)Path.GetRelativePath(srcDir, OutputDirectory / "MCAuthHelper");
                Cargo($"build {releaseFlag} --target-dir {targetDir}", workingDirectory: srcDir, customLogger: (_, text) => Logger.Normal(text));

                var outFile = OutputDirectory / "MCAuthHelper" / (Configuration == Configuration.Release ? "release" : "debug") / ("mcauthhelper" + (IsWin ? ".exe" : ""));

                CopyFileToDirectory(outFile, targetDir / "LauncherGeneratorNeeded", FileExistsPolicy.OverwriteIfNewer);
            });
    }
}