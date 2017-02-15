using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using log4net;
using log4net.Config;

namespace KpdApps.Common.Logging
{
	/// <summary>
	/// Wrapper class for log4net.
	/// Implementation of cache and easy access to instances of log4net.пути
	/// </summary>
	public sealed class Logger
	{
		private static string _overridedConfigPath;

		private readonly object _syncObject = new object();

		private const string ConfigFileName = "log4net.config";

		private const string DefaultPath = "c:\\logs\\log4net.config";

		private static readonly Dictionary<string, ILog> LoggersCacheDictionary = new Dictionary<string, ILog>();

		/// <summary>
		/// Access to cached logger.
		/// </summary>
		public static Logger Cache { get; } = new Logger();

		/// <summary>
		/// Return default logger.
		/// </summary>
		public ILog Default => this["Default"];

		/// <summary>
		/// Set path to configuration file.
		///</summary>
		/// <param name="path">Path to configaration.</param>
		public static void SetConfigPath(string path)
		{
			if (_overridedConfigPath == null)
				_overridedConfigPath = path;
		}

		/// <summary>
		/// Implement access to named instances of <see cref="ILog"/>.
		/// If logger does not exist it will be created.
		/// </summary>
		/// <param name="name">Name of logger.</param>
		/// <returns><see cref="ILog"/>.</returns>
		public ILog this[string name]
		{
			get
			{
				lock (_syncObject)
				{
					EnsureLoggerExists(name);
					return LoggersCacheDictionary[name];
				}
			}
		}

		/// <summary>
		/// Return path to configuration file for setup log4net.
		/// </summary>
		/// <returns>Path to configuration.</returns>
		private static string GetConfigPath()
		{
			if (_overridedConfigPath != null)
				return _overridedConfigPath;

			string configPath = string.Empty;

			if (HttpContext.Current != null && HttpContext.Current.Request.PhysicalApplicationPath != null)
				configPath = Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, ConfigFileName);

			if (string.IsNullOrEmpty(configPath))
			{
				Assembly start = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
				string configDirectory = Path.GetDirectoryName(start.Location);
				if (configDirectory != null)
					configPath = Path.Combine(configDirectory, ConfigFileName);
			}

			if (!File.Exists(configPath))
			{
				Assembly current = Assembly.GetCallingAssembly();
				string configDirectory = Path.GetDirectoryName(current.Location);
				if (configDirectory != null)
					configPath = Path.Combine(configDirectory, ConfigFileName);
			}

			return string.IsNullOrWhiteSpace(configPath) ? DefaultPath : configPath;
		}

		private void EnsureLoggerExists(string name)
		{
			lock (_syncObject)
			{
				if (!LoggersCacheDictionary.ContainsKey(name))
				{
					string fileName = GetConfigPath();
					if (!string.IsNullOrWhiteSpace(fileName))
					{
						FileInfo fi = new FileInfo(fileName);
						if (fi.Exists)
							XmlConfigurator.Configure(fi);
					}
				}

				ILog logger = LogManager.GetLogger(name);

				LoggersCacheDictionary[name] = logger;
			}
		}
	}
}
