﻿using System;
using System.Collections.Generic;
using System.IO;
using ChinhDo.Transactions;
using System.Linq;
using Nexus.Client.Games;
using Nexus.Client.ModManagement.InstallationLog;
using Nexus.Client.ModManagement.Scripting;
using Nexus.Client.Mods;
using Nexus.Client.PluginManagement;
using Nexus.Client.Util;

namespace Nexus.Client.ModManagement
{
	/// <summary>
	/// This installs mod files.
	/// </summary>
	public class ModFileInstaller : IModFileInstaller
	{
		private List<string> m_lstOverwriteFolders = new List<string>();
		private List<string> m_lstDontOverwriteFolders = new List<string>();
		private List<string> m_lstOverwriteMods = new List<string>();
		private List<string> m_lstDontOverwriteMods = new List<string>();
		private List<string> m_lstErrorMods = new List<string>();
		private bool m_booDontOverwriteAll = false;
		private bool m_booOverwriteAll = false;
		private ConfirmItemOverwriteDelegate m_dlgOverwriteConfirmationDelegate = null;
        private ModManager m_mmModManager = null;

		#region Properties

		/// <summary>
		/// Gets or sets the mod being installed.
		/// </summary>
		/// <value>The mod being installed.</value>
		protected IMod Mod { get; set; }

		/// <summary>
		/// Gets the environment info of the current game mode.
		/// </summary>
		/// <value>The environment info of the current game mode.</value>
		protected IGameModeEnvironmentInfo GameModeInfo { get; private set; }

		/// <summary>
		/// Gets or sets the utility class to use to work with data files.
		/// </summary>
		/// <value>The utility class to use to work with data files.</value>
		protected IDataFileUtil DataFileUtility { get; set; }

		/// <summary>
		/// Gets or sets the transactional file manager to use to interact with the file system.
		/// </summary>
		/// <value>The transactional file manager to use to interact with the file system.</value>
		protected TxFileManager TransactionalFileManager { get; set; }

		/// <summary>
		/// Gets or sets the install log to use to log file installations.
		/// </summary>
		/// <value>The install log to use to log file installations.</value>
		protected IInstallLog InstallLog { get; set; }

		/// <summary>
		/// Gets manager to use to manage plugins.
		/// </summary>
		/// <value>The manager to use to manage plugins.</value>
		protected IPluginManager PluginManager { get; private set; }

		/// <summary>
		/// Gets whether the file is a mod or a plugin.
		/// </summary>
		/// <value>true or false.</value>
		protected bool IsPlugin { get; private set; }

		/// <summary>
		/// Gets a list of install errors.
		/// </summary>
		/// <value>The list of errors.</value>
		public List<string> InstallErrors
		{
			get
			{
				return m_lstErrorMods;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with its dependencies.
		/// </summary>
		/// <param name="p_gmiGameModeInfo">The environment info of the current game mode.</param>
		/// <param name="p_modMod">The mod being installed.</param>
		/// <param name="p_ilgInstallLog">The install log to use to log file installations.</param>
		/// <param name="p_pmgPluginManager">The plugin manager.</param>
		/// <param name="p_dfuDataFileUtility">The utility class to use to work with data files.</param>
		/// <param name="p_tfmFileManager">The transactional file manager to use to interact with the file system.</param>
		/// <param name="p_dlgOverwriteConfirmationDelegate">The method to call in order to confirm an overwrite.</param>
		/// <param name="p_UsesPlugins">Whether the file is a mod or a plugin.</param>
        public ModFileInstaller(IGameModeEnvironmentInfo p_gmiGameModeInfo, IMod p_modMod, IInstallLog p_ilgInstallLog, IPluginManager p_pmgPluginManager, IDataFileUtil p_dfuDataFileUtility, TxFileManager p_tfmFileManager, ConfirmItemOverwriteDelegate p_dlgOverwriteConfirmationDelegate, bool p_UsesPlugins, ModManager p_mmModManager)
		{
			GameModeInfo = p_gmiGameModeInfo;
			Mod = p_modMod;
			InstallLog = p_ilgInstallLog;
			PluginManager = p_pmgPluginManager;
			DataFileUtility = p_dfuDataFileUtility;
			TransactionalFileManager = p_tfmFileManager;
			m_dlgOverwriteConfirmationDelegate = p_dlgOverwriteConfirmationDelegate ?? ((s, b, m) => OverwriteResult.No);
			IsPlugin = p_UsesPlugins;
            m_mmModManager = p_mmModManager;
		}

		#endregion

		/// <summary>
		/// Verifies if the given file can be written.
		/// </summary>
		/// <remarks>
		/// This method checks if the given path is valid. If so, and the file does not
		/// exist, the file can be written. If the file does exist, than the user is
		/// asked to overwrite the file.
		/// </remarks>
		/// <param name="p_strPath">The file path, relative to the Data folder, whose writability is to be verified.</param>
		/// <returns><c>true</c> if the location specified by <paramref name="p_strPath"/>
		/// can be written; <c>false</c> otherwise.</returns>
		protected bool TestDoOverwrite(string p_strPath)
		{
			string strDataPath = Path.Combine(GameModeInfo.InstallationPath, p_strPath);
			bool booFIDataPath = false;
			if (!File.Exists(strDataPath))
				return true;
			else
			{
				booFIDataPath = new FileInfo(strDataPath).IsReadOnly;
				if (booFIDataPath)
					m_booOverwriteAll = false;
			}

            if(m_mmModManager.IsBackupActive)
			{
				ModBackupInfo mbInfo = m_mmModManager.lstMBInfo.Where(x => x.Mod == Mod).FirstOrDefault();
				if (mbInfo == null)
					return false;
				else
				{
					KeyValuePair<string, bool> Dictionary = mbInfo.ModFileDictionary.Where(x => x.Key == p_strPath).FirstOrDefault();
					if (Dictionary.Key == null)
						return false;
					else
						return Dictionary.Value;
				}
			}

			string strLoweredPath = strDataPath.ToLowerInvariant();
			if (m_lstOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
				return true;
			if (m_lstDontOverwriteFolders.Contains(Path.GetDirectoryName(strLoweredPath)))
				return false;
			if (m_booOverwriteAll)
				return true;
			if (m_booDontOverwriteAll)
				return false;

			IMod modOld = InstallLog.GetCurrentFileOwner(p_strPath);
			if (modOld == Mod)
				return true;

			string strModFile = String.Empty;
			string strModFileID = String.Empty;
			string strMessage = null;
			if (modOld != null)
			{
				strModFile = modOld.Filename;
				strModFileID = modOld.Id;
				if (!String.IsNullOrEmpty(strModFileID))
				{
					if (m_lstOverwriteMods.Contains(strModFileID))
						return true;
					if (m_lstDontOverwriteMods.Contains(strModFileID))
						return false;
				}
				else
				{
					if (m_lstOverwriteMods.Contains(strModFile))
						return true;
					if (m_lstDontOverwriteMods.Contains(strModFile))
						return false;
				}
				strMessage = String.Format("Data file '{{0}}' has already been installed by '{0}'", modOld.ModName);
				if (booFIDataPath)
					strMessage += " and is ReadOnly.";
				strMessage += Environment.NewLine + String.Format("Overwrite with {0} mod's file?", Mod.ModName);
			}
			else
			{
				strMessage = "Data file '{0}' already exists";
				if (booFIDataPath)
					strMessage += " and is ReadOnly.";
				else
					strMessage += ".";
				strMessage += Environment.NewLine + String.Format("Overwrite with {0} mod's file?", Mod.ModName);
			}
			if (booFIDataPath)
			{
				switch (m_dlgOverwriteConfirmationDelegate(String.Format(strMessage, p_strPath), true, (modOld != null)))
				{
					case OverwriteResult.Yes:
						return true;
					case OverwriteResult.YesToAll:
						m_booOverwriteAll = true;
						return true;
					case OverwriteResult.NoToAll:
						m_booDontOverwriteAll = true;
						return false;
					case OverwriteResult.No:
						return false;
					default:
						throw new Exception("Sanity check failed: OverwriteDialog returned a value not present in the OverwriteResult enum");
				}
			}
			else
			{
				switch (m_dlgOverwriteConfirmationDelegate(String.Format(strMessage, p_strPath), true, (modOld != null)))
				{
					case OverwriteResult.Yes:
						return true;
					case OverwriteResult.No:
						return false;
					case OverwriteResult.NoToAll:
						m_booDontOverwriteAll = true;
						return false;
					case OverwriteResult.YesToAll:
						m_booOverwriteAll = true;
						return true;
					case OverwriteResult.NoToGroup:
						Queue<string> folders = new Queue<string>();
						folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
						while (folders.Count > 0)
						{
							strLoweredPath = folders.Dequeue();
							if (!m_lstOverwriteFolders.Contains(strLoweredPath))
							{
								m_lstDontOverwriteFolders.Add(strLoweredPath);
								foreach (string s in Directory.GetDirectories(strLoweredPath))
								{
									folders.Enqueue(s.ToLowerInvariant());
								}
							}
						}
						return false;
					case OverwriteResult.YesToGroup:
						folders = new Queue<string>();
						folders.Enqueue(Path.GetDirectoryName(strLoweredPath));
						while (folders.Count > 0)
						{
							strLoweredPath = folders.Dequeue();
							if (!m_lstDontOverwriteFolders.Contains(strLoweredPath))
							{
								m_lstOverwriteFolders.Add(strLoweredPath);
								foreach (string s in Directory.GetDirectories(strLoweredPath))
								{
									folders.Enqueue(s.ToLowerInvariant());
								}
							}
						}
						return true;
					case OverwriteResult.NoToMod:
						strModFile = modOld.Filename;
						strModFileID = modOld.Id;
						if (!String.IsNullOrEmpty(strModFileID))
						{
							if (!m_lstOverwriteMods.Contains(strModFileID))
								m_lstDontOverwriteMods.Add(strModFileID);
						}
						else
						{
							if (!m_lstOverwriteMods.Contains(strModFile))
								m_lstDontOverwriteMods.Add(strModFile);
						}
						return false;
					case OverwriteResult.YesToMod:
						strModFile = modOld.Filename;
						strModFileID = modOld.Id;
						if (!String.IsNullOrEmpty(strModFileID))
						{
							if (!m_lstDontOverwriteMods.Contains(strModFileID))
								m_lstOverwriteMods.Add(strModFileID);
						}
						else
						{
							if (!m_lstDontOverwriteMods.Contains(strModFile))
								m_lstOverwriteMods.Add(strModFile);
						}
						return true;
					default:
						throw new Exception("Sanity check failed: OverwriteDialog returned a value not present in the OverwriteResult enum");
				}
			}
		}

		/// <summary>
		/// Installs the speified file from the Mod to the file system.
		/// </summary>
		/// <param name="p_strModFilePath">The path of the file in the Mod to install.</param>
		/// <param name="p_strInstallPath">The path on the file system where the file is to be installed.</param>
		/// <param name="p_booSecondaryInstallPath">Whether to use the secondary install path.</param>
		/// <returns><c>true</c> if the file was written; <c>false</c> if the user chose
		/// not to overwrite an existing file.</returns>
		public bool InstallFileFromMod(string p_strModFilePath, string p_strInstallPath, bool p_booSecondaryInstallPath)
		{
			try
			{
				byte[] bteModFile = Mod.GetFile(p_strModFilePath);
				return GenerateDataFile(p_strInstallPath, bteModFile, p_booSecondaryInstallPath);
			}
			catch(FileNotFoundException)
			{
				return false;
			}
		}

		/// <summary>
		/// Writes the file represented by the given byte array to the given path.
		/// </summary>
		/// <remarks>
		/// This method writes the given data as a file at the given path. If the file
		/// already exists the user is prompted to overwrite the file.
		/// </remarks>
		/// <param name="p_strPath">The path where the file is to be created.</param>
		/// <param name="p_bteData">The data that is to make up the file.</param>
		/// <param name="p_booSecondaryInstallPath">Whether to use the secondary install path.</param>
		/// <returns><c>true</c> if the file was written; <c>false</c> if the user chose
		/// not to overwrite an existing file.</returns>
		/// <exception cref="IllegalFilePathException">Thrown if <paramref name="p_strPath"/> is
		/// not safe.</exception>
		public virtual bool GenerateDataFile(string p_strPath, byte[] p_bteData, bool p_booSecondaryInstallPath)
		{
			DataFileUtility.AssertFilePathIsSafe(p_strPath);
			string strInstallFilePath = String.Empty;

			if (p_booSecondaryInstallPath && !(String.IsNullOrEmpty(GameModeInfo.SecondaryInstallationPath)))
				strInstallFilePath = Path.Combine(GameModeInfo.SecondaryInstallationPath, p_strPath);
			else
				strInstallFilePath = Path.Combine(GameModeInfo.InstallationPath, p_strPath);

			FileInfo Info = new FileInfo(strInstallFilePath);
			if (!Directory.Exists(Path.GetDirectoryName(strInstallFilePath)))
				TransactionalFileManager.CreateDirectory(Path.GetDirectoryName(strInstallFilePath));
			else
			{
				if (!TestDoOverwrite(p_strPath))
					return false;

				if (File.Exists(strInstallFilePath))
				{
					if (Info.IsReadOnly == true)
						File.SetAttributes(strInstallFilePath, File.GetAttributes(strInstallFilePath) & ~FileAttributes.ReadOnly);
					string strInstallDirectory = Path.GetDirectoryName(p_strPath);
					string strBackupDirectory = Path.Combine(GameModeInfo.OverwriteDirectory, strInstallDirectory);
					string strOldModKey = InstallLog.GetCurrentFileOwnerKey(p_strPath);
					if (strOldModKey == null)
					{
						InstallLog.LogOriginalDataFile(p_strPath);
						strOldModKey = InstallLog.OriginalValuesKey;
					}
					string strInstallingModKey = InstallLog.GetModKey(Mod);
					//if this mod has installed this file already we just replace it and don't
					// need to back it up.
					if (!strOldModKey.Equals(strInstallingModKey))
					{
						//back up the current version of the file if the current mod
						// didn't install it
						if (!Directory.Exists(strBackupDirectory))
							TransactionalFileManager.CreateDirectory(strBackupDirectory);

						//we get the file name this way in order to preserve the file name's case
						string strFile = Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(strInstallFilePath), Path.GetFileName(strInstallFilePath))[0]);
						strFile = strOldModKey + "_" + strFile;

						string strBackupFilePath = Path.Combine(strBackupDirectory, strFile);
						Info = new FileInfo(strBackupFilePath);
						if ((Info.IsReadOnly == true) && (File.Exists(strBackupFilePath)))
							File.SetAttributes(strBackupFilePath, File.GetAttributes(strBackupFilePath) & ~FileAttributes.ReadOnly);
						TransactionalFileManager.Copy(strInstallFilePath, strBackupFilePath, true);
					}
					TransactionalFileManager.Delete(strInstallFilePath);
				}
			}
			TransactionalFileManager.WriteAllBytes(strInstallFilePath, p_bteData);
			// Checks whether the file is a gamebryo plugin
			if (IsPlugin)
				if (PluginManager.IsActivatiblePluginFile(strInstallFilePath))
				{
					if (!PluginManager.CanActivatePlugins())
					{
						string strTooManyPlugins = String.Format("The requested change to the active plugins list would result in over {0} plugins being active.", PluginManager.MaxAllowedActivePluginsCount);
						strTooManyPlugins += Environment.NewLine + String.Format("The current game doesn't support more than {0} active plugins, you need to disable at least one plugin to continue.", PluginManager.MaxAllowedActivePluginsCount);
						strTooManyPlugins += Environment.NewLine + Environment.NewLine + String.Format("NOTE: This is a game engine limitation.") + Environment.NewLine;
						throw new Exception(strTooManyPlugins);
					}
					PluginManager.AddPlugin(strInstallFilePath);
				}
			InstallLog.AddDataFile(Mod, p_strPath);
			return IsPlugin;
		}

		/// <summary>
		/// Uninstalls the specified file.
		/// </summary>
		/// <remarks>
		/// If the mod we are uninstalling doesn't own the file, then its version is removed
		/// from the overwrites directory. If the mod we are uninstalling overwrote a file when it
		/// installed the specified file, then the overwritten file is restored. Otherwise
		/// the file is deleted.
		/// </remarks>
		/// <param name="p_strPath">The path to the file that is to be uninstalled.</param>
		/// <param name="p_booSecondaryInstallPath">Whether to use the secondary install path.</param>
		public void UninstallDataFile(string p_strPath, bool p_booSecondaryInstallPath)
		{
			string strInstallFilePath = String.Empty;
			DataFileUtility.AssertFilePathIsSafe(p_strPath);
			string strUninstallingModKey = InstallLog.GetModKey(Mod);

			if (p_booSecondaryInstallPath && !(String.IsNullOrEmpty(GameModeInfo.SecondaryInstallationPath)))
				strInstallFilePath = Path.Combine(GameModeInfo.SecondaryInstallationPath, p_strPath);
			else
				strInstallFilePath = Path.Combine(GameModeInfo.InstallationPath ?? "", p_strPath);

			string strBackupDirectory = Path.Combine(GameModeInfo.OverwriteDirectory, Path.GetDirectoryName(p_strPath));
			string strFile;
			string strRestoreFromPath = string.Empty;
			bool booRestoreFile = false;

			FileInfo fiInfo = null;
			if (File.Exists(strInstallFilePath))
			{
				string strCurrentOwnerKey = InstallLog.GetCurrentFileOwnerKey(p_strPath);
				//if we didn't install the file, then leave it alone
				if (strUninstallingModKey.Equals(strCurrentOwnerKey))
				{
					//if we did install the file, replace it with the file we overwrote
					// when we installed the file
					// if we didn't overwrite a file, then just delete the current file
					fiInfo = new FileInfo(strInstallFilePath);
					if ((fiInfo.IsReadOnly) || (IsFileLocked(strInstallFilePath)))
						m_lstErrorMods.Add(strInstallFilePath);
					else
						TransactionalFileManager.Delete(strInstallFilePath);

					string strPreviousOwnerKey = InstallLog.GetPreviousFileOwnerKey(p_strPath);
					if (strPreviousOwnerKey != null)
					{
						strFile = strPreviousOwnerKey + "_" + Path.GetFileName(p_strPath);
						strRestoreFromPath = Path.Combine(strBackupDirectory, strFile);
						if (File.Exists(strRestoreFromPath))
							booRestoreFile = true;
					}

					if (IsPlugin)
						if ((PluginManager.IsActivatiblePluginFile(strInstallFilePath)) && !booRestoreFile)
							PluginManager.RemovePlugin(strInstallFilePath);

					if (booRestoreFile)
					{
						//we get the file name this way in order to preserve the file name's case
						string strBackupFileName = Path.GetFileName(Directory.GetFiles(Path.GetDirectoryName(strRestoreFromPath), Path.GetFileName(strRestoreFromPath))[0]);
						strBackupFileName = strBackupFileName.Substring(strBackupFileName.IndexOf('_') + 1);
						string strNewDataPath = Path.Combine(Path.GetDirectoryName(strInstallFilePath), strBackupFileName);

						fiInfo = new FileInfo(strRestoreFromPath);
						try
						{
							TransactionalFileManager.Copy(strRestoreFromPath, strNewDataPath, true);
						}
						catch{}

						if ((fiInfo.IsReadOnly) || (IsFileLocked(strRestoreFromPath)))
							m_lstErrorMods.Add(strInstallFilePath);
						else
							TransactionalFileManager.Delete(strRestoreFromPath);
					}

					//remove any empty directories from the data folder we may have created
					TrimEmptyDirectories(Path.GetDirectoryName(strInstallFilePath), GameModeInfo.InstallationPath);
				}
			}

			//remove our version of the file from the backup directory
			string strOverwritePath = Path.Combine(strBackupDirectory, strUninstallingModKey + "_" + Path.GetFileName(p_strPath));
			if (File.Exists(strOverwritePath))
			{
				fiInfo = new FileInfo(strOverwritePath);
				if (((fiInfo.Attributes | FileAttributes.Hidden) == fiInfo.Attributes) || (fiInfo.IsReadOnly) || IsFileLocked(strOverwritePath))
					m_lstErrorMods.Add(strInstallFilePath);
				else
					TransactionalFileManager.Delete(strOverwritePath);
			}

			//remove any empty directories from the overwrite folder we may have created
			string strStopDirectory = GameModeInfo.OverwriteDirectory;
			string strFileName = Path.GetFileName(strOverwritePath);
			TrimEmptyDirectories(strOverwritePath.Replace(strFileName, ""), strStopDirectory);

			InstallLog.RemoveDataFile(Mod, p_strPath);
		}

		/// <summary>
		/// Check if the file is in use.
		/// </summary>
		public bool IsFileLocked(string filePath)
		{
			try
			{
				using (File.Open(filePath, FileMode.Open)) { }
				return false;
			}
			catch (IOException e)
			{
				return true;
			}
		}

		/// <summary>
		/// Deletes any empty directories found between the start path and the end directory.
		/// </summary>
		/// <param name="p_strStartPath">The path from which to start looking for empty directories.</param>
		/// <param name="p_strStopDirectory">The directory at which to stop looking.</param>
		protected void TrimEmptyDirectories(string p_strStartPath, string p_strStopDirectory)
		{
			string strEmptyDirectory = p_strStartPath;
			while (true)
			{
				if (Directory.Exists(strEmptyDirectory) &&
					(Directory.GetFiles(strEmptyDirectory).Length + Directory.GetDirectories(strEmptyDirectory).Length == 0) &&
					!strEmptyDirectory.Equals(p_strStopDirectory, StringComparison.OrdinalIgnoreCase))
				{
					for (Int32 i = 0; i < 5 && Directory.Exists(strEmptyDirectory); i++)
						FileUtil.ForceDelete(strEmptyDirectory);
				}
				else
					break;
				strEmptyDirectory = Path.GetDirectoryName(strEmptyDirectory);
			}
		}

		/// <summary>
		/// Finalizes the installation of the files.
		/// </summary>
		public virtual void FinalizeInstall()
		{
		}
	}
}
