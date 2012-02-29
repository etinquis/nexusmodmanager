﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace Nexus.Client.Games.Gamebryo.PluginManagement.Boss
{
	public class BossSorter : IDisposable
	{
		#region Native Methods

		[DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

		#region Native BAPI Methods

		#region Delegates

		#region Error Handling Functions

		/// <summary>
		/// Gets the details of the last error.
		/// </summary>
		/// <remarks>
		/// Outputs a string giving the details of the last time an error or 
		/// warning return code was returned by a function.
		/// </remarks>
		/// <param name="details">The details of the last error.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 GetLastErrorDetailsDelegate(ref string details);

		#endregion

		#region Lifecycle Management Functions

		/// <summary>
		/// The delegate for methods that Create the BOSS DB.
		/// </summary>
		/// <remarks>
		/// Explicitly manage database lifetime. Allows clients to free memory when
		/// they want/need to. This function also checks that
		/// plugins.txt and loadorder.txt (if they both exist) are in sync.
		/// </remarks>
		/// <param name="db">The created BOSS DB.</param>
		/// <param name="clientGame">The value identifying the game for which to create the DB.</param>
		/// <param name="dataPath">The path to that game's Data folder, and is case-sensitive if the
		/// underlying filesystem is case-sensitive</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 CreateBossDbDelegate(ref IntPtr db, UInt32 clientGame, string dataPath);

		/// <summary>
		/// the delegate for methods that Destroy the given BOSS DB.
		/// </summary>
		/// <param name="db">The db to destroy.</param>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DestroyBossDbDelegate(IntPtr db);

		#endregion

		#region Database Loading Functions

		/// <summary>
		/// Loads the specified masterlist.
		/// </summary>
		/// <remarks>
		/// Loads the masterlist and userlist from the paths specified.
		/// Can be called multiple times. On error, the database is unchanged.
		/// Paths are case-sensitive if the underlying filesystem is case-sensitive.
		/// masterlistPath and userlistPath are files.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="masterlistPath">The path to the masterlist to load.</param>
		/// <param name="userlistPath">The path to the userlist to load.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 LoadDelegate(IntPtr db, string masterlistPath, string userlistPath);

		/// <summary>
		/// Evaluates the loaded masterlist.
		/// </summary>
		/// <remarks>
		/// Evaluates all conditional lines and regex mods the loaded masterlist. 
		/// This exists so that Load() doesn't need to be called whenever the mods 
		/// installed are changed. Evaluation does not take place unless this function 
		/// is called. Repeated calls re-evaluate the masterlist from scratch each time, 
		/// ignoring the results of any previous evaluations. Paths are case-sensitive 
		/// if the underlying filesystem is case-sensitive.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 EvalConditionalsDelegate(IntPtr db);

		#endregion

		#region Masterlist Updating

		/// <summary>
		/// Updates the masterlist at the given path.
		/// </summary>
		/// <remarks>
		/// Checks if there is a masterlist at masterlistPath. If not,
		/// it downloads the latest masterlist for the DB's game to masterlistPath.
		/// If there is, it first compares online and local versions to see if an
		/// update is necessary.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="masterlistPath">The path to the masterlist to update.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 UpdateMasterlistDelegate(IntPtr db, string masterlistPath);

		#endregion

		#region Plugin Sorting Functions

		/// <summary>
		/// Sorts the user's mods
		/// </summary>
		/// <remarks>
		/// Sorts the mods in the data path, using the masterlist at the masterlist path,
		/// specified when the db was loaded using Load. Outputs a list of plugins, pointed to
		/// by sortedPlugins, of length pointed to by listLength. lastRecPos points to the
		/// position in the sortedPlugins list of the last plugin recognised by BOSS.
		/// If the trialOnly parameter is true, no plugins are actually redated.
		/// If trialOnly is false, then sortedPlugins, listLength and lastRecPos can be null
		/// pointers, in case you do not require the information.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="trialOnly">Whether the sort should actually be performed, or just previewed.</param>
		/// <param name="sortedPlugins">The list of plugins, in order as sorted.</param>
		/// <param name="listLength">The length of the list of sorted plugins.</param>
		/// <param name="lastRecPos">The position in the list of sorted plugins of the last plugin recognised by BOSS</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 SortModsDelegate(IntPtr db, [MarshalAsAttribute(UnmanagedType.I1)] bool trialOnly, out string[] sortedPlugins, out UInt32 listLength, out UInt32 lastRecPos);

		/// <summary>
		/// Gets the list of plugin, sorted by load order.
		/// </summary>
		/// <remarks>
		/// Gets a list of plugins in load order, with the number of plugins given by numPlugins.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugins">The returned list of plugins.</param>
		/// <param name="numPlugins">The length of the returned list.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 GetLoadOrderDelegate(IntPtr db, out string[] plugins, out UInt32 numPlugins);

		/// <summary>
		/// Sets the load order of the plugins.
		/// </summary>
		/// <remarks>
		/// Sets the load order to the given plugins list of length numPlugins.
		/// Then scans the Data directory and appends any other plugins not included in the
		/// array passed to the function.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugins">The list of plugins in the desired order. Plugins not in the list will be appeneded to the end.</param>
		/// <param name="numPlugins">The number of plugins in the given array.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 SetLoadOrderDelegate(IntPtr db, ref string[] plugins, [MarshalAsAttribute(UnmanagedType.SysUInt)] UInt32 numPlugins);

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <remarks>
		/// Returns the contents of plugins.txt.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugins">The returned list of active plugins.</param>
		/// <param name="numPlugins">the number of active plugins.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 GetActivePluginsDelegate(IntPtr db, out string[] plugins, out UInt32 numPlugins);

		/// <summary>
		/// Sets the list of active plugins.
		/// </summary>
		/// <remarks>
		/// Edits plugins.txt so that it lists the given plugins, in load order.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugins">The list of plugins to set as active.</param>
		/// <param name="numPlugins">The number of plugins to set as active.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 SetActivePluginsDelegate(IntPtr db, string[] plugins, [MarshalAsAttribute(UnmanagedType.SysUInt)] UInt32 numPlugins);

		/// <summary>
		/// Gets the load index of the specified plugin.
		/// </summary>
		/// <remarks>
		/// Gets the load order of the specified plugin, giving it as index. The first position 
		/// in the load order is 0.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugin">The plugin whose load order is to be retrieved.</param>
		/// <param name="index">The returned load index of the specified plugin.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 GetPluginLoadOrderDelegate(IntPtr db, string plugin, out UInt32 index);

		/// <summary>
		/// Sets the load order of the specified plugin.
		/// </summary>
		/// <remarks>
		/// Sets the load order of the specified plugin, removing it from its current position 
		/// if it has one. The first position in the load order is 0. If the index specified is
		/// greater than the number of plugins in the load order, the plugin will be inserted at
		/// the end of the load order.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugin">The plugin whose load order is to be set.</param>
		/// <param name="index">The load index at which to place the specified plugin.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 SetPluginLoadOrderDelegate(IntPtr db, string plugin, [MarshalAsAttribute(UnmanagedType.SysUInt)] UInt32 index);

		/// <summary>
		/// Gets the plugin at the specified load index.
		/// </summary>
		/// <remarks>
		/// Gets what plugin is at the specified load order position. The first position in the
		/// load order is 0.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="index">The load index of the plugin to retrieve.</param>
		/// <param name="plugin">The returned plugin at the specified load index.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 GetIndexedPluginDelegate(IntPtr db, [MarshalAsAttribute(UnmanagedType.SysUInt)] UInt32 index, out string plugin);

		/// <summary>
		/// Sets the active status of the specified plugin.
		/// </summary>
		/// <remarks>
		/// If (active), adds the plugin to plugins.txt in its load order if it is not already present.
		/// If (!active), removes the plugin from plugins.txt if it is present.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugin">The plugin whose active status is to be set.</param>
		/// <param name="active">Whether the specified plugin should be made active or inactive.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 SetPluginActiveDelegate(IntPtr db, string plugin, [MarshalAsAttribute(UnmanagedType.I1)] bool active);

		/// <summary>
		/// Determines if the specified plugin is active.
		/// </summary>
		/// <remarks>
		/// Checks to see if the given plugin is listed in plugins.txt.
		/// </remarks>
		/// <param name="db">The db for which we are performing the action.</param>
		/// <param name="plugin">The plugins whose active state is to be determined.</param>
		/// <param name="isActive">Returns whether the specified plugin is active.</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 IsPluginActiveDelegate(IntPtr db, string plugin, out bool isActive);

		#endregion

		#endregion

		private GetLastErrorDetailsDelegate m_dlgGetLastErrorDetails = null;
		private CreateBossDbDelegate m_dlgCreateBossDb = null;
		private DestroyBossDbDelegate m_dlgDestroyBossDb = null;
		private LoadDelegate m_dlgLoad = null;
		private EvalConditionalsDelegate m_dlgEvalConditionals = null;
		private UpdateMasterlistDelegate m_dlgUpdateMasterlist = null;
		private SortModsDelegate m_dlgSortMods = null;
		private GetLoadOrderDelegate m_dlgGetLoadOrder = null;
		private SetLoadOrderDelegate m_dlgSetLoadOrder = null;
		private GetActivePluginsDelegate m_dlgGetActivePlugins = null;
		private SetActivePluginsDelegate m_dlgSetActivePlugins = null;
		private GetPluginLoadOrderDelegate m_dlgGetPluginLoadOrder = null;
		private SetPluginLoadOrderDelegate m_dlgSetPluginLoadOrder = null;
		private GetIndexedPluginDelegate m_dlgGetIndexedPlugin = null;
		private SetPluginActiveDelegate m_dlgSetPluginActive = null;
		private IsPluginActiveDelegate m_dlgIsPluginActive = null;

		#endregion

		#endregion

		private IntPtr m_ptrBossApi = IntPtr.Zero;
		private IntPtr m_ptrBossDb = IntPtr.Zero;

		#region Properties

		/// <summary>
		/// Gets the application's envrionment info.
		/// </summary>
		/// <value>The application's envrionment info.</value>
		protected IEnvironmentInfo EnvironmentInfo { get; private set; }

		/// <summary>
		/// Gets the current game mode.
		/// </summary>
		/// <value>The current game mode.</value>
		protected GamebryoGameModeBase GameMode { get; private set; }

		/// <summary>
		/// Gets the path to the masterlist.
		/// </summary>
		/// <value>The path to the masterlist.</value>
		protected string MasterlistPath { get; private set; }

		/// <summary>
		/// Gets the path to the userlist.
		/// </summary>
		/// <value>The path to the userlist.</value>
		protected string UserlistPath { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given dependencies.
		/// </summary>
		/// <param name="p_eifEnvironmentInfo">The application's envrionment info.</param>
		/// <param name="p_gmdGameMode">The game mode for which plugins are being managed.</param>
		public BossSorter(IEnvironmentInfo p_eifEnvironmentInfo, GamebryoGameModeBase p_gmdGameMode)
		{
			EnvironmentInfo = p_eifEnvironmentInfo;
			GameMode = p_gmdGameMode;

			string strBAPIPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"data"), p_eifEnvironmentInfo.Is64BitProcess ? "boss64.dll" : "boss32.dll");

			m_ptrBossApi = LoadLibrary(strBAPIPath);
			if (m_ptrBossApi == IntPtr.Zero)
				throw new BossException(String.Format("Could not load BAPI library: {0}", strBAPIPath));

			m_ptrBossDb = CreateBossDb();
			///TODO add masterlist path
			Load(MasterlistPath, UserlistPath);
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		/// <remarks>
		/// Disposes unmanaged resources used by BOSS.
		/// </remarks>
		~BossSorter()
		{
			Dispose(false);
		}

		#endregion

		private void LoadMethods()
		{
			m_dlgGetLastErrorDetails = (GetLastErrorDetailsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "GetLastErrorDetails"), typeof(GetLastErrorDetailsDelegate));
			m_dlgCreateBossDb = (CreateBossDbDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "CreateBossDb"), typeof(CreateBossDbDelegate));
			m_dlgDestroyBossDb = (DestroyBossDbDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "DestroyBossDb"), typeof(DestroyBossDbDelegate));
			m_dlgLoad = (LoadDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "Load"), typeof(LoadDelegate));
			m_dlgEvalConditionals = (EvalConditionalsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "EvalConditionals"), typeof(EvalConditionalsDelegate));
			m_dlgUpdateMasterlist = (UpdateMasterlistDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "UpdateMasterlist"), typeof(UpdateMasterlistDelegate));
			m_dlgSortMods = (SortModsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "SortMods"), typeof(SortModsDelegate));
			m_dlgGetLoadOrder = (GetLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "GetLoadOrder"), typeof(GetLoadOrderDelegate));
			m_dlgSetLoadOrder = (SetLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "SetLoadOrder"), typeof(SetLoadOrderDelegate));
			m_dlgGetActivePlugins = (GetActivePluginsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "GetActivePlugins"), typeof(GetActivePluginsDelegate));
			m_dlgSetActivePlugins = (SetActivePluginsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "SetActivePlugins"), typeof(SetActivePluginsDelegate));
			m_dlgGetPluginLoadOrder = (GetPluginLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "GetPluginLoadOrder"), typeof(GetPluginLoadOrderDelegate));
			m_dlgSetPluginLoadOrder = (SetPluginLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "SetPluginLoadOrder"), typeof(SetPluginLoadOrderDelegate));
			m_dlgGetIndexedPlugin = (GetIndexedPluginDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "GetIndexedPlugin"), typeof(GetIndexedPluginDelegate));
			m_dlgSetPluginActive = (SetPluginActiveDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "SetPluginActive"), typeof(SetPluginActiveDelegate));
			m_dlgIsPluginActive = (IsPluginActiveDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrBossApi, "IsPluginActive"), typeof(IsPluginActiveDelegate));
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		private void Dispose(bool p_booDisposing)
		{
			if (m_ptrBossDb != IntPtr.Zero)
				DestroyBossDb();
			if (m_ptrBossApi != IntPtr.Zero)
				FreeLibrary(m_ptrBossApi);
		}

		private void HandleStatusCode(UInt32 p_uintStatusCode)
		{
			const UInt32 BOSS_ERROR_MAX = 30;
			const UInt32 BOSS_API_OK_NO_UPDATE_NECESSARY = BOSS_ERROR_MAX + 1;
			const UInt32 BOSS_API_WARN_LO_MISMATCH = BOSS_ERROR_MAX + 3;
			const UInt32 BOSS_API_ERROR_NO_MEM = BOSS_ERROR_MAX + 4;
			const UInt32 BOSS_API_ERROR_INVALID_ARGS = BOSS_ERROR_MAX + 5;
			const UInt32 BOSS_API_ERROR_NETWORK_FAIL = BOSS_ERROR_MAX + 6;
			const UInt32 BOSS_API_ERROR_NO_INTERNET_CONNECTION = BOSS_ERROR_MAX + 7;
			const UInt32 BOSS_API_ERROR_NO_TAG_MAP = BOSS_ERROR_MAX + 8;

			if (p_uintStatusCode == 0)
				return;

			string strDetails = GetLastErrorDetails();
			switch (p_uintStatusCode)
			{
				case BOSS_API_OK_NO_UPDATE_NECESSARY:
					//BOSS_API_OK_NO_UPDATE_NECESSARY;
					break;
				case 10:
					//BOSS_API_WARN_BAD_FILENAME;
					break;
				case BOSS_API_WARN_LO_MISMATCH:
					//BOSS_API_WARN_LO_MISMATCH;
					break;
				case 3:
					//BOSS_API_ERROR_FILE_WRITE_FAIL;
					throw new BossException("BOSS_API_ERROR_FILE_WRITE_FAIL: " + strDetails);
				case 18:
					//BOSS_API_ERROR_FILE_DELETE_FAIL;
					throw new BossException("BOSS_API_ERROR_FILE_DELETE_FAIL: " + strDetails);
				case 4:
					//BOSS_API_ERROR_FILE_NOT_UTF8;
					throw new BossException("BOSS_API_ERROR_FILE_NOT_UTF8: " + strDetails);
				case 5:
					//BOSS_API_ERROR_FILE_NOT_FOUND;
					throw new BossException("BOSS_API_ERROR_FILE_NOT_FOUND: " + strDetails);
				case 15:
					//BOSS_API_ERROR_MASTER_TIME_READ_FAIL;
					//BOSS_API_ERROR_MOD_TIME_READ_FAIL;
					throw new BossException("BOSS_API_ERROR_MASTER_TIME_READ_FAIL/BOSS_API_ERROR_MOD_TIME_READ_FAIL: " + strDetails);
				case 16:
					//BOSS_API_ERROR_MOD_TIME_WRITE_FAIL;
					throw new BossException("BOSS_API_ERROR_MOD_TIME_WRITE_FAIL: " + strDetails);
				case 6:
					//BOSS_API_ERROR_PARSE_FAIL;
					throw new BossException("BOSS_API_ERROR_PARSE_FAIL: " + strDetails);
				case 7:
					//BOSS_API_ERROR_CONDITION_EVAL_FAIL;
					throw new BossException("BOSS_API_ERROR_CONDITION_EVAL_FAIL: " + strDetails);
				case BOSS_API_ERROR_NO_MEM:
					//BOSS_API_ERROR_NO_MEM;
					throw new BossException("BOSS_API_ERROR_NO_MEM: " + strDetails);
				case BOSS_API_ERROR_INVALID_ARGS:
					//BOSS_API_ERROR_INVALID_ARGS;
					throw new BossException("BOSS_API_ERROR_INVALID_ARGS: " + strDetails);
				case BOSS_API_ERROR_NETWORK_FAIL:
					//BOSS_API_ERROR_NETWORK_FAIL;
					throw new BossException("BOSS_API_ERROR_NETWORK_FAIL: " + strDetails);
				case BOSS_API_ERROR_NO_INTERNET_CONNECTION:
					//BOSS_API_ERROR_NO_INTERNET_CONNECTION;
					throw new BossException("BOSS_API_ERROR_NO_INTERNET_CONNECTION: " + strDetails);
				case BOSS_API_ERROR_NO_TAG_MAP:
					//BOSS_API_ERROR_NO_TAG_MAP;
					throw new BossException("BOSS_API_ERROR_NO_TAG_MAP: " + strDetails);
				case 8:
					//BOSS_API_ERROR_REGEX_EVAL_FAIL;
					//BOSS_API_RETURN_MAX;
					throw new BossException("BOSS_API_ERROR_REGEX_EVAL_FAIL: " + strDetails);
			}
		}

		#region Error Handling Functions

		/// <summary>
		/// Gets the details of the last error.
		/// </summary>
		/// <returns>The details of the last error.</returns>
		private string GetLastErrorDetails()
		{
			string strDetails = null;
			UInt32 uintStatus = m_dlgGetLastErrorDetails(ref strDetails);
			HandleStatusCode(uintStatus);
			return strDetails;
		}

		#endregion

		#region Lifecycle Management Functions

		/// <summary>
		/// Creates the BOSS DB.
		/// </summary>
		/// <remarks>
		/// Explicitly manage database lifetime. Allows clients to free memory when
		/// they want/need to. This function also checks that
		/// plugins.txt and loadorder.txt (if they both exist) are in sync.
		/// </remarks>
		/// <returns>The created BOSS DB.</returns>
		/// <exception cref="BossException">Thrown if the BOSS DB could not be created.</exception>
		private IntPtr CreateBossDb()
		{
			IntPtr ptrBossDb = IntPtr.Zero;
			UInt32 uintClientGameId = 0;
			switch (GameMode.ModeId)
			{
				case "Oblivion":
					uintClientGameId = 1;
					break;
				case "Fallout3":
					uintClientGameId = 2;
					break;
				case "FalloutNV":
					uintClientGameId = 3;
					break;
				case "Nehrim":
					uintClientGameId = 4;
					break;
				case "Skyrim":
					uintClientGameId = 5;
					break;
			}
			UInt32 uintStatus = m_dlgCreateBossDb(ref ptrBossDb, uintClientGameId, GameMode.PluginDirectory);
			HandleStatusCode(uintStatus);
			if (ptrBossDb == IntPtr.Zero)
				throw new BossException("Could not create BOSS DB.");
			return ptrBossDb;
		}

		/// <summary>
		/// Destroys the BOSS DB.
		/// </summary>
		private void DestroyBossDb()
		{
			m_dlgDestroyBossDb(m_ptrBossDb);
		}

		#endregion

		#region Database Loading Functions

		/// <summary>
		/// Loads the specified masterlist.
		/// </summary>
		/// <remarks>
		/// Loads the masterlist and userlist from the paths specified.
		/// Can be called multiple times. On error, the database is unchanged.
		/// </remarks>
		/// <param name="p_strMasterlistPath">The path to the masterlist to load.</param>
		/// <param name="p_strUserlistPath">The path to the userlist to load.</param>
		private void Load(string p_strMasterlistPath, string p_strUserlistPath)
		{
			UInt32 uintStatus = m_dlgLoad(m_ptrBossDb, p_strMasterlistPath, p_strUserlistPath);
			HandleStatusCode(uintStatus);
		}

		/// <summary>
		/// Evaluates the loaded masterlist.
		/// </summary>
		/// <remarks>
		/// Evaluates all conditional lines and regex mods the loaded masterlist. 
		/// This exists so that Load() doesn't need to be called whenever the mods 
		/// installed are changed. Evaluation does not take place unless this function 
		/// is called. Repeated calls re-evaluate the masterlist from scratch each time, 
		/// ignoring the results of any previous evaluations. Paths are case-sensitive 
		/// if the underlying filesystem is case-sensitive.
		/// </remarks>
		private void EvalConditionals()
		{
			UInt32 uintStatus = m_dlgEvalConditionals(m_ptrBossDb);
			HandleStatusCode(uintStatus);
		}

		#endregion

		#region Masterlist Updating

		/// <summary>
		/// Updates the masterlist at the given path.
		/// </summary>
		public void UpdateMasterlist()
		{
			UInt32 uintStatus = m_dlgUpdateMasterlist(m_ptrBossDb, MasterlistPath);
			HandleStatusCode(uintStatus);
			Load(MasterlistPath, UserlistPath);
		}

		#endregion

		#region Plugin Sorting Functions

		/// <summary>
		/// Sorts the user's mods
		/// </summary>
		/// <param name="p_booTrialOnly">Whether the sort should actually be performed, or just previewed.</param>
		/// <returns>The list of plugins, sorted by load order.</returns>
		public string[] SortMods(bool p_booTrialOnly)
		{
			string[] strSortedPlugins = null;
			UInt32 uintListLength = 0;
			UInt32 uintLastRecognizedPosition = 0;
			UInt32 uintStatus = m_dlgSortMods(m_ptrBossDb, p_booTrialOnly, out strSortedPlugins, out uintListLength, out uintLastRecognizedPosition);
			HandleStatusCode(uintStatus);
			return strSortedPlugins;
		}


		/// <summary>
		/// Gets the list of plugin, sorted by load order.
		/// </summary>
		/// <returns>The list of plugins, sorted by load order.</returns>
		public string[] GetLoadOrder()
		{
			string[] strSortedPlugins = null;
			UInt32 uintListLength = 0;
			UInt32 uintStatus = m_dlgGetLoadOrder(m_ptrBossDb, out strSortedPlugins, out uintListLength);
			HandleStatusCode(uintStatus);
			return strSortedPlugins;
		}


		/// <summary>
		/// Sets the load order of the plugins.
		/// </summary>
		/// <remarks>
		/// The returned list of sorted plugins will include plugins that were not
		/// included in the specified order list, if plugins exist that weren't included.
		/// The extra plugins will be apeended to the end of the given order.
		/// </remarks>
		/// <param name="p_strPlugins">The list of plugins in the desired order.</param>
		/// <returns>The list of plugins that were sorted, in order. Plugins that were not specfied
		/// in <paramref name="p_strPlugins"/> will be included at the end of the list.</returns>
		public string[] SetLoadOrder(string[] p_strPlugins)
		{
			string[] strSortedPlugins = p_strPlugins;
			UInt32 uintStatus = m_dlgSetLoadOrder(m_ptrBossDb, ref strSortedPlugins, Convert.ToUInt32(strSortedPlugins.Length));
			HandleStatusCode(uintStatus);
			return strSortedPlugins;
		}

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <returns>The list of active plugins.</returns>
		public string[] GetActivePlugins()
		{
			string[] strPlugins = null;
			UInt32 uintListLength = 0;
			UInt32 uintStatus = m_dlgGetActivePlugins(m_ptrBossDb, out strPlugins, out uintListLength);
			HandleStatusCode(uintStatus);
			return strPlugins;
		}
		
		/// <summary>
		/// Sets the list of active plugins.
		/// </summary>
		/// <param name="p_strActivePlugins">The list of plugins to set as active.</param>
		public void SetActivePlugins(string[] p_strActivePlugins)
		{
			UInt32 uintStatus = m_dlgSetActivePlugins(m_ptrBossDb, p_strActivePlugins, Convert.ToUInt32(p_strActivePlugins.Length));
			HandleStatusCode(uintStatus);
		}

		
		/// <summary>
		/// Gets the load index of the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose load order is to be retrieved.</param>
		/// <returns>The load index of the specified plugin.</returns>
		public Int32 GetPluginLoadOrder(string p_strPlugin)
		{
			UInt32 uintIndex = 0;
			UInt32 uintStatus = m_dlgGetPluginLoadOrder(m_ptrBossDb, p_strPlugin, out uintIndex);
			HandleStatusCode(uintStatus);
			return Convert.ToInt32(uintIndex);
		}

		/// <summary>
		/// Sets the load order of the specified plugin.
		/// </summary>
		/// <remarks>
		/// Sets the load order of the specified plugin, removing it from its current position 
		/// if it has one. The first position in the load order is 0. If the index specified is
		/// greater than the number of plugins in the load order, the plugin will be inserted at
		/// the end of the load order.
		/// </remarks>
		/// <param name="p_strPlugin">The plugin whose load order is to be set.</param>
		/// <param name="p_intIndex">The load index at which to place the specified plugin.</param>
		public void SetPluginLoadOrder(string p_strPlugin, Int32 p_intIndex)
		{
			UInt32 uintStatus = m_dlgSetPluginLoadOrder(m_ptrBossDb, p_strPlugin, Convert.ToUInt32(p_intIndex));
			HandleStatusCode(uintStatus);
		}

		/// <summary>
		/// Gets the plugin at the specified load index.
		/// </summary>
		/// <param name="p_intIndex">The load index of the plugin to retrieve.</param>
		/// <returns>The name of the plugin at the specified index.</returns>
		public string GetIndexedPlugin(Int32 p_intIndex)
		{
			string strPlugin = null;
			UInt32 uintStatus = m_dlgGetIndexedPlugin(m_ptrBossDb, Convert.ToUInt32(p_intIndex), out strPlugin);
			HandleStatusCode(uintStatus);
			return strPlugin;
		}

		/// <summary>
		/// Sets the active status of the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose active status is to be set.</param>
		/// <param name="p_booActive">Whether the specified plugin should be made active or inactive.</param>
		public void SetPluginActive(string p_strPlugin, bool p_booActive)
		{
			UInt32 uintStatus = m_dlgSetPluginActive(m_ptrBossDb, p_strPlugin, p_booActive);
			HandleStatusCode(uintStatus);
		}
		
		/// <summary>
		/// Determines if the specified plugin is active.
		/// </summary>
		/// <param name="p_strPlugin">The plugins whose active state is to be determined.</param>
		/// <returns><c>true</c> if the specfified plugin is active;
		/// <c>false</c> otherwise.</returns>
		public bool IsPluginActive(string p_strPlugin)
		{
			bool booIsActive = false;
			UInt32 uintStatus = m_dlgIsPluginActive(m_ptrBossDb, p_strPlugin, out booIsActive);
			HandleStatusCode(uintStatus);
			return booIsActive;
		}
		
		#endregion
	}
}
