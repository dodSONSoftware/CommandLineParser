using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MirrorAndArchiveTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private Fields
        private static readonly DateTimeOffset _StartTime = DateTimeOffset.Now;
        private static readonly string _DEFAULT_APPID_ = "Application";
        #endregion
        #region Public Properties
        public static TimeSpan RunTime => DateTimeOffset.Now - _StartTime;
        #endregion
        #region Application Events
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // #### search for configuration
            var createdNewConfig = false;
            if (System.IO.File.Exists(Helper.GetApplicationConfigurationFilename))
            {
                // configuration file found
                try
                {
                    // read configuration and populate appropriate values
                    var text = new System.Text.StringBuilder(System.IO.File.ReadAllText(Helper.GetApplicationConfigurationFilename));
                    var config = (new dodSON.Core.Configuration.XmlConfigurationSerializer()).Deserialize(text);
                    // LogDebugMode
                    LogDebugMode = GetConfigItemValue(config, "LogDebugMode", false);
                    // FlushMaximumLogs
                    Helper.LogFlushMaximumLogs = GetConfigItemValue(config, "LogFlushMaximumLogs", 10);
                    // FlushTimeLimit
                    Helper.LogFlushTimeLimit = GetConfigItemValue(config, "LogFlushTimeLimit", TimeSpan.FromSeconds(10));
                    // AppId
                    AppId = GetConfigItemValue(config, "AppId", "");
                    // Log
                    Helper.GetLogger = InstantiateLogFromConfig(config);
                }
                catch (Exception ex)
                {
                    // error loading configuration file
                    MessageBox.Show($"Error reading configuration file." +
                                    $"{Environment.NewLine}{Environment.NewLine}" +
                                    $"{Helper.GetApplicationConfigurationFilename}" +
                                    $"{Environment.NewLine}{Environment.NewLine}" +
                                    $"{ex.Message}",
                                    Helper.FormatTitle("ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }
            }
            else
            {
                // configuration file not found
                CreateAndSaveDefaultConfiguration();
                createdNewConfig = true;
            }
            //
            WriteDebugLog(nameof(App), "--------------------------------");
            WriteDebugLog(nameof(App), $"Application Started, {Helper.GetApplicationFormalTitle}");
            //
            var debugModeInfo = LogDebugMode ? $", Log={Helper.GetDebugLogFilename}" : "";
            WriteLog(nameof(App), LogCategory.App, "--------------------------------");
            WriteLog(nameof(App), LogCategory.App, $"Application Started, {Helper.GetApplicationFormalTitle}");
            WriteLog(nameof(App), LogCategory.App, $"LogDebugMode={LogDebugMode}{debugModeInfo}");
            if (createdNewConfig) { WriteLog(nameof(App), LogCategory.App, $"Created new, default configuration file. {Helper.GetApplicationConfigurationFilename}"); }
            //

            var invalidValidCommandLineArgs = "";
            var runAll = false;
            var minimized = false;
            var shutdown = false;
            if (e.Args.Length > 0)
            {
                // process command line arguments
                var finalMessage = new System.Text.StringBuilder(100);
                foreach (var item in e.Args)
                {
                    var processedArg = false;
                    var dude = item.Trim();
                    // strip post characters
                    while ((dude.StartsWith("\\") || dude.StartsWith("/") || dude.StartsWith("-")))
                    {
                        dude = dude.Substring(1);
                    }
                    // check for known commands
                    if ((dude.StartsWith("h", StringComparison.InvariantCultureIgnoreCase)) ||
                        (dude.StartsWith("?")))
                    {
                        // log it
                        App.WriteLog(nameof(App), LogCategory.App, $"Command Line Argument received: -{dude[0]} (Help)");
                        App.WriteLog(nameof(App), LogCategory.App, $"Showing the Help Window");
                        string msg = $"{Helper.GetApplicationTitle}" + Environment.NewLine +
                                     $"{Helper.GetApplicationVersion}" + Environment.NewLine +
                                     $"{Helper.GetApplicationCopyright}" + Environment.NewLine +
                                     Environment.NewLine +
                                     $"Command Line Arguments:" + Environment.NewLine +
                                     Environment.NewLine +
                                     $"[R]un          \tRuns all enabled jobs" + Environment.NewLine +
                                     Environment.NewLine +
                                     $"[S]hutdown     \tTerminates the application after" + Environment.NewLine +
                                     $"             \t\tthe [R]un command is executed" + Environment.NewLine +
                                     Environment.NewLine +
                                     $"[M]inimized    \tStarts the application minimized" + Environment.NewLine +
                                     $"             \t\t   only works with the [R]un command" + Environment.NewLine +
                                     $"             \t\t   and the [S]hutdown command" + Environment.NewLine +
                                     Environment.NewLine +
                                     $"[H]elp or [ ? ]\tDisplays this help screen" + Environment.NewLine;
                        MessageBox.Show(msg, Helper.FormatTitle("Help"), MessageBoxButton.OK, MessageBoxImage.Information);
                        Shutdown();
                        // will leave this function immediately and terminate the application
                        return;
                    }
                    if (dude.StartsWith("r", StringComparison.InvariantCultureIgnoreCase))
                    {
                        processedArg = true;
                        if (finalMessage.Length > 0) { finalMessage.Append(", "); }
                        finalMessage.Append($"Run All");
                        runAll = true;
                    }
                    if (dude.StartsWith("m", StringComparison.InvariantCultureIgnoreCase))
                    {
                        processedArg = true;
                        if (finalMessage.Length > 0) { finalMessage.Append(", "); }
                        finalMessage.Append($"Minimized");
                        minimized = true;
                    }
                    if (dude.StartsWith("s", StringComparison.InvariantCultureIgnoreCase))
                    {
                        processedArg = true;
                        if (finalMessage.Length > 0) { finalMessage.Append(", "); }
                        finalMessage.Append($"Shutdown");
                        shutdown = true;
                    }
                    //
                    if (!processedArg) { invalidValidCommandLineArgs += $" -{dude}"; }
                }
                // TODO: think about this, end-users should be able to start the application minimized whether it is going to run or not
                //          shutdown, by design, will only work with the run command
                //          minimized should by available regardless of the state of the run command
                // fix minimized
                minimized = (runAll && shutdown && minimized);
                // log it                
                var finalMessageString = finalMessage.ToString();
                if (!string.IsNullOrWhiteSpace(finalMessageString)) { App.WriteLog(nameof(App), LogCategory.App, $"Command Line Arguments received: {finalMessageString}"); }
                if (!string.IsNullOrWhiteSpace(invalidValidCommandLineArgs)) { App.WriteErrorLog(nameof(App), $"Invalid Command Line Arguments received: [{invalidValidCommandLineArgs} ] Execute this application with the command line argument [ -? ] to see the available command line arguments and their uses"); }
            }

            // continues here only if the command line argument to show the COMMAND LINE ARGUMENT HELP WINDOW is not invoked
            var mainWindow = new MainWindow(minimized, runAll, shutdown);
            if (minimized) { mainWindow.WindowState = WindowState.Minimized; }
            mainWindow.Show();


            // ######## INTERNAL FUNCTIONS
            void CreateAndSaveDefaultConfiguration()
            {
                // get/create default values
                LogDebugMode = false;
                // create configuration
                var groupName = System.IO.Path.GetFileNameWithoutExtension(Helper.GetApplicationLogFilename);
                if (groupName.Contains(dodSON.Core.Configuration.ConfigurationHelper.GroupNameSeparator)) { groupName = groupName.Split(dodSON.Core.Configuration.ConfigurationHelper.GroupNameSeparator)[0]; }
                var config = new dodSON.Core.Configuration.ConfigurationGroup(groupName);
                // LogDebugMode
                config.Items.Add("LogDebugMode", LogDebugMode, LogDebugMode.GetType());
                // FlushMaximumLogs
                config.Items.Add("LogFlushMaximumLogs", Helper.LogFlushMaximumLogs, Helper.LogFlushMaximumLogs.GetType());
                // FlushTimeLimit
                config.Items.Add("LogFlushTimeLimit", Helper.LogFlushTimeLimit, Helper.LogFlushTimeLimit.GetType());
                // AppId
                AppId = _DEFAULT_APPID_;
                config.Items.Add("AppId", AppId, AppId.GetType());
                // Log
                config.Add((Helper.GetLogger as dodSON.Core.Logging.ICachedLog).InnerLog.Configuration);
                // save configuration
                System.IO.File.WriteAllText(Helper.GetApplicationConfigurationFilename, (new dodSON.Core.Configuration.XmlConfigurationSerializer()).Serialize(config).ToString());
            }

            T GetConfigItemValue<T>(dodSON.Core.Configuration.IConfigurationGroup config_, string name_, T defaultValue)
            {
                if (dodSON.Core.Configuration.ConfigurationHelper.TryFindConfigurationItem(config_, name_, typeof(T),
                                                                                           out dodSON.Core.Configuration.IConfigurationItem configItem, out Exception outItemException))
                {
                    return (T)configItem.Value;
                }
                //
                return defaultValue;
            }

            dodSON.Core.Logging.ILog InstantiateLogFromConfig(dodSON.Core.Configuration.IConfigurationGroup config_)
            {
                if (dodSON.Core.Configuration.ConfigurationHelper.TryFindConfigurationGroup(config_, "Log",
                                                                                            out dodSON.Core.Configuration.IConfigurationGroup configGroup, out Exception outGroupException))
                {
                    return (dodSON.Core.Logging.ILog)dodSON.Core.Configuration.ConfigurationHelper.InstantiateTypeFromConfigurationGroup(configGroup);
                }
                //
                return null;
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Helper.CleanUpLogger();   // see: the MainWindow.Window_Closing(...) method
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            WriteErrorLog(nameof(App), $"Application Exception, {e.Exception.Message}");
            Helper.GetLogger?.Close();
            MessageBox.Show($"Application Error" +
                            $"{Environment.NewLine}{Environment.NewLine}" +
                            $"{e.Exception.Message}",
                            Helper.FormatTitle("ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion
        #region Internal Static Fields
        internal static int ThreadWorkerCounter = 0;
        internal static bool LogDebugMode = false;
        #endregion
        #region Internal Static Methods
        internal static string AppId { get; private set; }
        internal static void WriteLog(string sourceId,
                                      LogCategory category,
                                      string message)
        {
            Helper.GetLogger.Write(dodSON.Core.Logging.LogEntryType.Information, sourceId, message, 0, (ushort)category);
        }
        internal static void WriteLog(string sourceId,
                                      LogCategory category,
                                      string message,
                                      DateTime timeStamp)
        {
            Helper.GetLogger.Write(new dodSON.Core.Logging.LogEntry(dodSON.Core.Logging.LogEntryType.Information,
                                                                    sourceId,
                                                                    message,
                                                                    0,
                                                                    (ushort)category,
                                                                    timeStamp));
        }
        internal static void WriteDebugLog(string sourceId,
                                           string message)
        {
            if (LogDebugMode)
            {
                Helper.GetLogger.Write(dodSON.Core.Logging.LogEntryType.Debug, sourceId, message, 0, (ushort)LogCategory.Debug);
                Helper.FlushLog();
            }
        }
        internal static void WriteErrorLog(string sourceId, string message)
        {
            Helper.GetLogger.Write(dodSON.Core.Logging.LogEntryType.Error, sourceId, message, 0, (ushort)LogCategory.Error);
            Helper.FlushLog();
        }
        #endregion
        #region Internal Types
        internal enum LogCategory
        {
            Mirror = 0,
            Archive = 1,
            // 
            Debug = 7,
            Error = 8,
            App = 9
        }
        #endregion
    }
}
