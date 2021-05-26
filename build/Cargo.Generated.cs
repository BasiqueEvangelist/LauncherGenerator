// Generated from https://raw.githubusercontent.com/BasiqueEvangelist/LauncherGenerator/master/build/Cargo.json

using JetBrains.Annotations;
using Newtonsoft.Json;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Tooling;
using Nuke.Common.Tools;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace LauncherGenerator.Build
{
    /// <summary>
    ///   <p>For more details, visit the <a href="https://www.rust-lang.org/">official website</a>.</p>
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class CargoTasks
    {
        /// <summary>
        ///   Path to the Cargo executable.
        /// </summary>
        public static string CargoPath =>
            ToolPathResolver.TryGetEnvironmentExecutable("CARGO_EXE") ??
            ToolPathResolver.GetPathExecutable("cargo");
        public static Action<OutputType, string> CargoLogger { get; set; } = CustomLogger;
        /// <summary>
        ///   <p>For more details, visit the <a href="https://www.rust-lang.org/">official website</a>.</p>
        /// </summary>
        public static IReadOnlyCollection<Output> Cargo(string arguments, string workingDirectory = null, IReadOnlyDictionary<string, string> environmentVariables = null, int? timeout = null, bool? logOutput = null, bool? logInvocation = null, bool? logTimestamp = null, string logFile = null, Func<string, string> outputFilter = null)
        {
            using var process = ProcessTasks.StartProcess(CargoPath, arguments, workingDirectory, environmentVariables, timeout, logOutput, logInvocation, logTimestamp, logFile, CargoLogger, outputFilter);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>For more details, visit the <a href="https://www.rust-lang.org/">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>--release</c> via <see cref="CargoBuildSettings.Release"/></li>
        ///     <li><c>--target-dir</c> via <see cref="CargoBuildSettings.TargetDir"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> CargoBuild(CargoBuildSettings toolSettings = null)
        {
            toolSettings = toolSettings ?? new CargoBuildSettings();
            using var process = ProcessTasks.StartProcess(toolSettings);
            process.AssertZeroExitCode();
            return process.Output;
        }
        /// <summary>
        ///   <p>For more details, visit the <a href="https://www.rust-lang.org/">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>--release</c> via <see cref="CargoBuildSettings.Release"/></li>
        ///     <li><c>--target-dir</c> via <see cref="CargoBuildSettings.TargetDir"/></li>
        ///   </ul>
        /// </remarks>
        public static IReadOnlyCollection<Output> CargoBuild(Configure<CargoBuildSettings> configurator)
        {
            return CargoBuild(configurator(new CargoBuildSettings()));
        }
        /// <summary>
        ///   <p>For more details, visit the <a href="https://www.rust-lang.org/">official website</a>.</p>
        /// </summary>
        /// <remarks>
        ///   <p>This is a <a href="http://www.nuke.build/docs/authoring-builds/cli-tools.html#fluent-apis">CLI wrapper with fluent API</a> that allows to modify the following arguments:</p>
        ///   <ul>
        ///     <li><c>--release</c> via <see cref="CargoBuildSettings.Release"/></li>
        ///     <li><c>--target-dir</c> via <see cref="CargoBuildSettings.TargetDir"/></li>
        ///   </ul>
        /// </remarks>
        public static IEnumerable<(CargoBuildSettings Settings, IReadOnlyCollection<Output> Output)> CargoBuild(CombinatorialConfigure<CargoBuildSettings> configurator, int degreeOfParallelism = 1, bool completeOnFailure = false)
        {
            return configurator.Invoke(CargoBuild, CargoLogger, degreeOfParallelism, completeOnFailure);
        }
    }
    #region CargoBuildSettings
    /// <summary>
    ///   Used within <see cref="CargoTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    [Serializable]
    public partial class CargoBuildSettings : ToolSettings
    {
        /// <summary>
        ///   Path to the Cargo executable.
        /// </summary>
        public override string ProcessToolPath => base.ProcessToolPath ?? CargoTasks.CargoPath;
        public override Action<OutputType, string> ProcessCustomLogger => CargoTasks.CargoLogger;
        public virtual bool? Release { get; internal set; }
        public virtual string TargetDir { get; internal set; }
        protected override Arguments ConfigureProcessArguments(Arguments arguments)
        {
            arguments
              .Add("build")
              .Add("--release", Release)
              .Add("--target-dir {value}", TargetDir);
            return base.ConfigureProcessArguments(arguments);
        }
    }
    #endregion
    #region CargoBuildSettingsExtensions
    /// <summary>
    ///   Used within <see cref="CargoTasks"/>.
    /// </summary>
    [PublicAPI]
    [ExcludeFromCodeCoverage]
    public static partial class CargoBuildSettingsExtensions
    {
        #region Release
        /// <summary>
        ///   <p><em>Sets <see cref="CargoBuildSettings.Release"/></em></p>
        /// </summary>
        [Pure]
        public static T SetRelease<T>(this T toolSettings, bool? release) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Release = release;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="CargoBuildSettings.Release"/></em></p>
        /// </summary>
        [Pure]
        public static T ResetRelease<T>(this T toolSettings) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Release = null;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Enables <see cref="CargoBuildSettings.Release"/></em></p>
        /// </summary>
        [Pure]
        public static T EnableRelease<T>(this T toolSettings) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Release = true;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Disables <see cref="CargoBuildSettings.Release"/></em></p>
        /// </summary>
        [Pure]
        public static T DisableRelease<T>(this T toolSettings) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Release = false;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Toggles <see cref="CargoBuildSettings.Release"/></em></p>
        /// </summary>
        [Pure]
        public static T ToggleRelease<T>(this T toolSettings) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.Release = !toolSettings.Release;
            return toolSettings;
        }
        #endregion
        #region TargetDir
        /// <summary>
        ///   <p><em>Sets <see cref="CargoBuildSettings.TargetDir"/></em></p>
        /// </summary>
        [Pure]
        public static T SetTargetDir<T>(this T toolSettings, string targetDir) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.TargetDir = targetDir;
            return toolSettings;
        }
        /// <summary>
        ///   <p><em>Resets <see cref="CargoBuildSettings.TargetDir"/></em></p>
        /// </summary>
        [Pure]
        public static T ResetTargetDir<T>(this T toolSettings) where T : CargoBuildSettings
        {
            toolSettings = toolSettings.NewInstance();
            toolSettings.TargetDir = null;
            return toolSettings;
        }
        #endregion
    }
    #endregion
}
