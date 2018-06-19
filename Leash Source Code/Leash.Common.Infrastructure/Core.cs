using Leash.Common.Infrastructure.Services;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Author - Prince Owen
 * Project - Leash.Common.Infrastructure
 * Description - Base Library for Roping Management Software
 */

namespace Leash.Common.Infrastructure
{
    public static class Core
    {
        #region Properties

        public static Logger Log { get; } = LogManager.GetCurrentClassLogger();

        #region Solids
        public const string PRODUCT_NAME = "Leash";
        public const string AUTHOR = "Prince Owen";

        #region Prism Constants

        #region Regions
        public const string MAIN_REGION = "Main Region";
        #endregion

        #region Views
        public const string START_VIEW = "Start View";
        public const string HOME_VIEW = "Home View";
        public const string REGISTER_VIEW = "Register View";
        public const string CONTEST_VIEW = "Contest View";
        public const string ABOUT_VIEW = "About View";
        public const string CONTESTANT_VIEW = "Contestant View";
        public const string EXPORT_CONTESTANTS_VIEW = "Export Contestants View";
        #endregion

        #endregion

        #region Names
        public readonly static string[] SUPPORTED_EXPORTS = new string[]
        {
            "Excel Spreadsheet,.xlsx", "Comma Seperated Values,.csv"
        };
        public const string CONTESTANTS_DOC_NAME = "Contestants";
        public const string ERROR_LOG_NAME = "Errors";
        public const string CONSOLE_LOG_NAME = "console-debugger";
        #endregion

        #region Directories
        public readonly static string SYSTEM_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public readonly static string USER_DOCUMENT_DIR = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public readonly static string WORK_BASE = Path.Combine(SYSTEM_DATA_DIR, PRODUCT_NAME);
        public readonly static string LOG_DIR = Path.Combine(WORK_BASE, "Logs");
        public readonly static string DATA_DIR = Path.Combine(WORK_BASE, "Data");
        public readonly static string DOCUMENT_BASE = Path.Combine(USER_DOCUMENT_DIR, "Leash");
        #endregion

        #region Paths
        public readonly static string COMPANY_PAGE = @"https://www.fiverr.com/s2/eb22b7af9e";
        public readonly static string BASE_PATH = System.Reflection.Assembly.GetEntryAssembly().Location;
        public const string STARTUP_REGISTRY_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public readonly static string CONFIGURATION_FILE_PATH = Path.Combine(DATA_DIR, "Configuration.config");
        public const string LOG_LAYOUT = "${longdate}|${uppercase:${level}}| ${message}";
        public static readonly string ERROR_LOG_PATH = Path.Combine(LOG_DIR, ERROR_LOG_NAME + ".log");
        public static readonly string DATABASE_PATH = Path.Combine(DATA_DIR, "Data.db");
        #endregion

        #endregion

        #endregion

        #region Methods
        public static void Initialize()
        {
            ConfigureLogger();
#if DEBUG
            // Register and Initialize the Console Debugger
            Trace.Listeners.Add(new ConsoleTraceListener(true));
            Debug.Listeners.Add(new ConsoleTraceListener(true));
            ConsoleManager.Show();

            //Log.Info("Welcome to the {0} Debugger", PRODUCT_NAME);
#endif
            CreateDirectories(WORK_BASE, LOG_DIR, DATA_DIR, DOCUMENT_BASE);
            //ClearDirectory(TEMP_DIR);
        }

        /// <summary>
        /// Easy and safe way to create multiple directories. 
        /// </summary>
        /// <param name="directories">The set of directories to create</param>
        public static void CreateDirectories(params string[] directories)
        {
            if (directories == null || directories.Length <= 0) return;

            foreach (var directory in directories)
                try
                {
                    if (Directory.Exists(directory)) continue;

                    Directory.CreateDirectory(directory);
                    Log.Info("A new directory has been created ({0})", directory);
                }
                catch (Exception e)
                {
                    Log.Error("Error while creating directory {0} - {1}", directory, e);
                }
        }

        public static void ClearDirectory(string directory)
        {
            if (!Directory.Exists(directory)) return;

            foreach (var file in Directory.EnumerateFiles(directory, "*"))
                try { File.Delete(file); }
                catch (Exception e) { Log.Error("Error Deleting file {0}\n{1}", file, e); }
        }

        public static void ConfigureStartup(bool runAtStartup)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(STARTUP_REGISTRY_PATH, true);

                if (runAtStartup)
                    key.SetValue(PRODUCT_NAME, BASE_PATH);
                else key.DeleteValue(PRODUCT_NAME);
            }
            catch (Exception e)
            {
                Log.Error(e, "An error occured while accessing the registry for startup information");
            }
        }

        /// <summary>
        /// Setups up settings for NLog.
        /// </summary>
        static void ConfigureLogger()
        {
            var config = new LoggingConfiguration();

#if DEBUG
            var debugConsoleTarget = new ConsoleTarget()
            {
                Name = Core.CONSOLE_LOG_NAME,
                Layout = Core.LOG_LAYOUT,
                Header = string.Format("{0} Debugger", PRODUCT_NAME)
            };

            var debugRule = new LoggingRule("*", LogLevel.Debug, debugConsoleTarget);
            config.LoggingRules.Add(debugRule);
#endif

            var errorFileTarget = new FileTarget()
            {
                Name = Core.ERROR_LOG_NAME,
                FileName = Core.ERROR_LOG_PATH,
                Layout = Core.LOG_LAYOUT
            };

            config.AddTarget(errorFileTarget);

            var errorRule = new LoggingRule("*", LogLevel.Error, errorFileTarget);
            config.LoggingRules.Add(errorRule);

            LogManager.Configuration = config;

            LogManager.ReconfigExistingLoggers();
        }
        #endregion
    }
}
