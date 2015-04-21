using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Nexus.Client.Util;

namespace Nexus.Client.Games.Gamebryo.PluginManagement.LoadOrder
{
	/// <summary>
	/// The interface for LibLoadOrder functionality.
	/// </summary>
	/// <remarks>
	/// This use LibLoadOrder API to expose its plugin sorting and activation abilities.
	/// </remarks>
	public class LoadOrderManager : ILoadOrderManager, IDisposable
	{
		#region Native Methods

		
		[DllImport("kernel32.dll", BestFitMapping = true, ThrowOnUnmappableChar = true)]
		private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName);

		[DllImport("Kernel32.dll", EntryPoint = "GetProcAddress", ExactSpelling = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		public extern static IntPtr GetProcAddress(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)]string funcname);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeLibrary(IntPtr hModule);

		#region Native LibLoadOrder API Methods


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
		private delegate UInt32 GetLoadOrderDelegate(IntPtr db, out IntPtr plugins, out UInt32 numPlugins);

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
		private delegate UInt32 SetLoadOrderDelegate(IntPtr db, IntPtr plugins, UInt32 numPlugins);

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
		private delegate UInt32 GetActivePluginsDelegate(IntPtr db, out IntPtr plugins, out UInt32 numPlugins);

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
		private delegate UInt32 SetActivePluginsDelegate(IntPtr db, IntPtr plugins, UInt32 numPlugins);

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
		private delegate UInt32 GetErrorMessageDelegate([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] out string details);

		/// <summary>
		/// The delegate for methods that Create the LoadOrder DB.
		/// </summary>
		/// <remarks>
		/// Explicitly manage database lifetime. Allows clients to free memory when
		/// they want/need to. This function also checks that
		/// plugins.txt and loadorder.txt (if they both exist) are in sync.
		/// </remarks>
		/// <param name="db">The created LoadOrder DB.</param>
		/// <param name="clientGame">The value identifying the game for which to create the DB.</param>
		/// <param name="dataPath">The path to that game's Data folder, and is case-sensitive if the
		/// underlying filesystem is case-sensitive</param>
		/// <returns>Status code.</returns>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate UInt32 CreateLoadOrderDbDelegate(ref IntPtr db, UInt32 clientGame, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string dataPath, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string dataLocalPath);

		/// <summary>
		/// the delegate for methods that Destroy the given LoadOrder DB.
		/// </summary>
		/// <param name="db">The db to destroy.</param>
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DestroyLoadOrderDbDelegate(IntPtr db);

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
		private delegate UInt32 GetPluginActiveDelegate(IntPtr db, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string plugin, out bool isActive);

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
		private delegate UInt32 GetIndexedPluginDelegate(IntPtr db, UInt32 index, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] out string plugin);

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
		private delegate UInt32 SetPluginActiveDelegate(IntPtr db, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string plugin, [MarshalAsAttribute(UnmanagedType.I1)] bool active);

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
		private delegate UInt32 GetPluginLoadOrderDelegate(IntPtr db, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string plugin, out UInt32 index);

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
		private delegate UInt32 SetPluginLoadOrderDelegate(IntPtr db, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(StringMarshaler), MarshalCookie = "UTF8")] string plugin, UInt32 index);


		#endregion


		private GetActivePluginsDelegate m_dlgGetActivePlugins = null;
		private SetActivePluginsDelegate m_dlgSetActivePlugins = null;
		private GetLoadOrderDelegate m_dlgGetLoadOrder = null;
		private SetLoadOrderDelegate m_dlgSetLoadOrder = null;
		private GetErrorMessageDelegate m_dlgGetErrorMessage = null;
		private CreateLoadOrderDbDelegate m_dlgCreateLoadOrderDb = null;
		private DestroyLoadOrderDbDelegate m_dlgDestroyLoadOrderDb = null;
		private GetPluginActiveDelegate m_dlgGetPluginActive = null;
		private GetIndexedPluginDelegate m_dlgGetIndexedPlugin = null;
		private SetPluginActiveDelegate m_dlgSetPluginActive = null;
		private SetPluginLoadOrderDelegate m_dlgSetPluginLoadOrder = null;
		private GetPluginLoadOrderDelegate m_dlgGetPluginLoadOrder = null;
		
	

		#endregion

		private const Int32 LOADORDER_API_OK_NO_UPDATE_NECESSARY = 31;
		private IntPtr m_ptrLoadOrderApi = IntPtr.Zero;
		private IntPtr m_ptrLoadOrderDb = IntPtr.Zero;

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
		public string MasterlistPath { get; private set; }

		/// <summary>
		/// Gets the path to the userlist.
		/// </summary>
		/// <value>The path to the userlist.</value>
		public string UserlistPath { get; private set; }

		/// <summary>
		/// Gets the file utility class.
		/// </summary>
		/// <value>The file utility class.</value>
		protected FileUtil FileUtility { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given dependencies.
		/// </summary>
		/// <param name="p_eifEnvironmentInfo">The application's envrionment info.</param>
		/// <param name="p_gmdGameMode">The game mode for which plugins are being managed.</param>
		/// <param name="p_futFileUtility">The file utility class.</param>
		/// <param name="p_strMasterlistPath">The path to the masterlist file to use.</param>
		public LoadOrderManager(IEnvironmentInfo p_eifEnvironmentInfo, GamebryoGameModeBase p_gmdGameMode, FileUtil p_futFileUtility, string p_strMasterlistPath)
		{
			EnvironmentInfo = p_eifEnvironmentInfo;
			GameMode = p_gmdGameMode;
			FileUtility = p_futFileUtility;

			string strSorterAPIPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data"), p_eifEnvironmentInfo.Is64BitProcess ? "loadorder64.dll" : "loadorder32.dll");

			m_ptrLoadOrderApi = LoadLibrary(strSorterAPIPath);
			if (m_ptrLoadOrderApi == IntPtr.Zero)
			{
				int a = Marshal.GetLastWin32Error();
				throw new LoadOrderException(String.Format("Could not load BAPI library: {0}", strSorterAPIPath));
			}

			LoadMethods();

			m_ptrLoadOrderDb = CreateLoadOrderDb();
			
			MasterlistPath = p_strMasterlistPath;
			UserlistPath = null;
		}

		/// <summary>
		/// The finalizer.
		/// </summary>
		/// <remarks>
		/// Disposes unmanaged resources used by LoadOrderManager.
		/// </remarks>
		~LoadOrderManager()
		{
			Dispose(false);
		}

		#endregion

		/// <summary>
		/// Loads the native LibLoadOrder API methods.
		/// </summary>
		private void LoadMethods()
		{
			m_dlgGetActivePlugins = (GetActivePluginsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_active_plugins"), typeof(GetActivePluginsDelegate));
			m_dlgSetActivePlugins = (SetActivePluginsDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_set_active_plugins"), typeof(SetActivePluginsDelegate));
			m_dlgGetLoadOrder = (GetLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_load_order"), typeof(GetLoadOrderDelegate));
			m_dlgSetLoadOrder = (SetLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_set_load_order"), typeof(SetLoadOrderDelegate));
			m_dlgGetErrorMessage = (GetErrorMessageDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_error_message"), typeof(GetErrorMessageDelegate));
			m_dlgCreateLoadOrderDb = (CreateLoadOrderDbDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_create_handle"), typeof(CreateLoadOrderDbDelegate));
			m_dlgDestroyLoadOrderDb = (DestroyLoadOrderDbDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_destroy_handle"), typeof(DestroyLoadOrderDbDelegate));
			m_dlgGetPluginActive = (GetPluginActiveDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_plugin_active"), typeof(GetPluginActiveDelegate));
			m_dlgGetIndexedPlugin = (GetIndexedPluginDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_indexed_plugin"), typeof(GetIndexedPluginDelegate));
			m_dlgSetPluginActive = (SetPluginActiveDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_set_plugin_active"), typeof(SetPluginActiveDelegate));
			m_dlgGetPluginLoadOrder = (GetPluginLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_get_plugin_position"), typeof(GetPluginLoadOrderDelegate));
			m_dlgSetPluginLoadOrder = (SetPluginLoadOrderDelegate)Marshal.GetDelegateForFunctionPointer(GetProcAddress(m_ptrLoadOrderApi, "lo_set_plugin_position"), typeof(SetPluginLoadOrderDelegate));
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes of any resources that the LoadOrderManager allocated.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Disposes of the unamanged resources need for LibLoadOrder API.
		/// </summary>
		/// <param name="p_booDisposing">Whether the method is being called from the <see cref="Dispose()"/> method.</param>
		protected virtual void Dispose(bool p_booDisposing)
		{
			if (m_ptrLoadOrderDb != IntPtr.Zero)
				DestroyLoadOrderDb();
			if (m_ptrLoadOrderApi != IntPtr.Zero)
				FreeLibrary(m_ptrLoadOrderApi);
		}

		/// <summary>
		/// Creates the LoadOrder DB.
		/// </summary>
		/// <remarks>
		/// Explicitly manage database lifetime. Allows clients to free memory when
		/// they want/need to. This function also checks that
		/// plugins.txt and loadorder.txt (if they both exist) are in sync.
		/// </remarks>
		/// <returns>The created LoadOrder DB.</returns>
		/// <exception cref="LoadOrderException">Thrown if the LoadOrder DB could not be created.</exception>
		private IntPtr CreateLoadOrderDb()
		{
			IntPtr ptrLoadOrderDb = IntPtr.Zero;
			UInt32 uintClientGameId = 0;

			switch (GameMode.ModeId)
			{
				case "Oblivion":
					uintClientGameId = 2;
					break;
				case "Fallout3":
					uintClientGameId = 4;
					break;
				case "FalloutNV":
					uintClientGameId = 5;
					break;
				case "Skyrim":
					uintClientGameId = 3;
					break;
				case "Morrowind":
					uintClientGameId = 1;
					break;
				default:
					throw new LoadOrderException(String.Format("Unsupported game: {0} ({1})", GameMode.Name, GameMode.ModeId));
			}

			Backup();

			UInt32 uintStatus = m_dlgCreateLoadOrderDb(ref ptrLoadOrderDb, uintClientGameId, GameMode.InstallationPath, null);

			if ((uintStatus == 13) && (uintClientGameId != 1) && (ptrLoadOrderDb == IntPtr.Zero))
			{
				string strGameModeLocalAppData = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData"), GameMode.ModeId);
				string strLoadOrderFilePath = Path.Combine(strGameModeLocalAppData, "loadorder.txt");

				if (File.Exists(strLoadOrderFilePath))
				{
					string strBakFilePath = Path.Combine(strGameModeLocalAppData, "loadorder.nmmbak");
					if (File.Exists(strBakFilePath))
					{
						FileUtil.Move(strBakFilePath, Path.Combine(strGameModeLocalAppData, Path.GetRandomFileName() + ".loadorder.bak"), true);
					}

					FileUtil.Move(strLoadOrderFilePath, strBakFilePath, true);

					uintStatus = m_dlgCreateLoadOrderDb(ref ptrLoadOrderDb, uintClientGameId, GameMode.InstallationPath, null);
				}
			}

			HandleStatusCode(uintStatus);
			if (ptrLoadOrderDb == IntPtr.Zero)
				throw new LoadOrderException("Could not create LoadOrderManager DB.");
			return ptrLoadOrderDb;
		}

		/// <summary>
		/// Backup the plugins.txt and loadorder.txt files
		/// </summary>
		private void Backup()
		{
			string strLocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string strGameModeLocalAppData = Path.Combine(strLocalAppData, GameMode.ModeId);
			string strLoadOrderFilePath = Path.Combine(strGameModeLocalAppData, "loadorder.txt");
			string strPluginsFilePath = Path.Combine(strGameModeLocalAppData, "plugins.txt");

			if (File.Exists(strLoadOrderFilePath))
			{
				string strBakFilePath = Path.Combine(strGameModeLocalAppData, "loadorder.backup.nmm");
				if (!File.Exists(strBakFilePath))
					File.Copy(strLoadOrderFilePath, strBakFilePath, false);
			}

			if (File.Exists(strPluginsFilePath))
			{
				string strBakFilePath = Path.Combine(strGameModeLocalAppData, "plugins.backup.nmm");
				if (!File.Exists(strBakFilePath))
					File.Copy(strPluginsFilePath, strBakFilePath, false);
			}
		}

		/// <summary>
		/// Destroys the LoadOrder DB.
		/// </summary>
		private void DestroyLoadOrderDb()
		{
			m_dlgDestroyLoadOrderDb(m_ptrLoadOrderDb);
		}

		/// <summary>
		/// Handles the status code returned by the LibLoadOrder methods.
		/// </summary>
		/// <param name="p_uintStatusCode">The status code to handle.</param>
		private void HandleStatusCode(UInt32 p_uintStatusCode)
		{
			if (p_uintStatusCode == 0)
				return;

			string strDetails = GetLastErrorDetails();
			switch (p_uintStatusCode)
			{
				case LOADORDER_API_OK_NO_UPDATE_NECESSARY:
					//LOADORDER_API_OK_NO_UPDATE_NECESSARY;
					break;
				case 1:
					//LIBLO_WARN_BAD_FILENAME
					//throw new LoadOrderException("LIBLO_WARN_BAD_FILENAME: " + strDetails);
					break;
				case 2:
					//LIBLO_WARN_LO_MISMATCH;
					//throw new LoadOrderException("LIBLO_WARN_LO_MISMATCH: " + strDetails);
					break;
				case 3:
					//LIBLO_ERROR_FILE_READ_FAIL;
					throw new LoadOrderException("LIBLO_ERROR_FILE_READ_FAIL: " + strDetails);
				case 4:
					//LIBLO_ERROR_FILE_WRITE_FAIL;
					throw new LoadOrderException("LIBLO_ERROR_FILE_WRITE_FAIL: " + strDetails);
				case 5:
					//LIBLO_ERROR_FILE_NOT_UTF8;
					throw new LoadOrderException("LIBLO_ERROR_FILE_NOT_UTF8: " + strDetails);
				case 6:
					//LIBLO_ERROR_FILE_NOT_FOUND;
					throw new LoadOrderException("LIBLO_ERROR_FILE_NOT_FOUND: " + strDetails);
				case 7:
					//LIBLO_ERROR_FILE_RENAME_FAIL
					throw new LoadOrderException("LIBLO_ERROR_FILE_RENAME_FAIL: " + strDetails);
				case 8:
					//LIBLO_ERROR_TIMESTAMP_READ_FAIL;
					throw new LoadOrderException("LIBLO_ERROR_TIMESTAMP_READ_FAIL: " + strDetails);
				case 9:
					//LIBLO_ERROR_TIMESTAMP_WRITE_FAIL
					throw new LoadOrderException("LIBLO_ERROR_TIMESTAMP_WRITE_FAIL: " + strDetails);
				case 10:
					//LIBLO_ERROR_FILE_PARSE_FAIL
					throw new LoadOrderException("LIBLO_ERROR_FILE_PARSE_FAIL: " + strDetails);
				case 11:
					//LIBLO_ERROR_NO_MEM
					throw new LoadOrderException("LIBLO_ERROR_NO_MEM: " + strDetails);
				case 12:
					//LIBLO_ERROR_INVALID_ARGS;
					//throw new LoadOrderException("LIBLO_ERROR_INVALID_ARGS: " + strDetails);
					break;
				case 13:
					//LIBLO_WARN_INVALID_LIST;
					//throw new LoadOrderException("LIBLO_WARN_INVALID_LIST: " + strDetails);
					break;
							
				
				default:
					throw new LoadOrderException(String.Format("Unreconized error value {1}: {0}", strDetails, p_uintStatusCode));
			}
		}
			
		#region Plugin Helpers

		/// <summary>
		/// Marshal the given pointer to an array of plugins.
		/// </summary>
		/// <remarks>
		/// This adjusts the plugin paths to be in the format expected by the  mod manager.
		/// </remarks>
		/// <param name="p_ptrPluginArray">The pointer to the array of plugin names to marshal.</param>
		/// <param name="p_uintLength">the length of the array to marshal.</param>
		/// <returns>The array of plugins names pointed to by the given pointer.</returns>
		protected string[] MarshalPluginArray(IntPtr p_ptrPluginArray, UInt32 p_uintLength)
		{
			if (p_ptrPluginArray == IntPtr.Zero)
				return null;
			string[] strPlugins = null;
			using (StringArrayManualMarshaler ammMarshaler = new StringArrayManualMarshaler("UTF8"))
				strPlugins = ammMarshaler.MarshalNativeToManaged(p_ptrPluginArray, Convert.ToInt32(p_uintLength));

			for (Int32 i = strPlugins.Length - 1; i >= 0; i--)
				strPlugins[i] = Path.Combine(GameMode.PluginDirectory, strPlugins[i]);
			
			return strPlugins;
		}

		/// <summary>
		/// Removes the plugin directory from the given plugin paths.
		/// </summary>
		/// <remarks>
		/// BAPI expects plugin paths to be relative to the plugins directory. This
		/// adjusts the plugin paths for that purpose.
		/// </remarks>
		/// <param name="p_strPlugins">The array of plugin paths to adjust.</param>
		/// <returns>An array containing the given plugin path, in order, but relative to the plugins directory.</returns>
		protected string[] StripPluginDirectory(string[] p_strPlugins)
		{
			string[] strPlugins = new string[p_strPlugins.Length];
			for (Int32 i = strPlugins.Length - 1; i >= 0; i--)
				strPlugins[i] = StripPluginDirectory(p_strPlugins[i]);
			return strPlugins;
		}

		/// <summary>
		/// Removes the plugin directory from the given plugin path.
		/// </summary>
		/// <remarks>
		/// BAPI expects plugin paths to be relative to the plugins directory. This
		/// adjusts the plugin path for that purpose.
		/// </remarks>
		/// <param name="p_strPlugin">The plugin path to adjust.</param>
		/// <returns>The given plugin path, but relative to the plugins directory.</returns>
		protected string StripPluginDirectory(string p_strPlugin)
		{
			return FileUtil.RelativizePath(GameMode.PluginDirectory, p_strPlugin);
		}

		/// <summary>
		/// Makes the given plugin path absolute.
		/// </summary>
		/// <param name="p_strPlugin">The plugin path to adjust.</param>
		/// <returns>The absolute path to the specified plugin.</returns>
		protected string AddPluginDirectory(string p_strPlugin)
		{
			return Path.Combine(GameMode.PluginDirectory, p_strPlugin);
		}

		#endregion

		#region Error Handling Functions

		/// <summary>
		/// Gets the details of the last error.
		/// </summary>
		/// <returns>The details of the last error.</returns>
		private string GetLastErrorDetails()
		{
			IntPtr ptrDetails = IntPtr.Zero;
			string strDetails = null;
			UInt32 uintStatus = m_dlgGetErrorMessage(out strDetails);
			HandleStatusCode(uintStatus);
			return strDetails;
		}
		
		#endregion

		/// <summary>
		/// Removes non-existent and ghosted plugins from the given list.
		/// </summary>
		/// <param name="p_strPlugins">The list of plugins from which to remove non-existent and ghosted plugins.</param>
		/// <returns>The given list of plugins, with all non-existent and ghosted plugins removed.</returns>
		private string[] RemoveNonExistentPlugins(string[] p_strPlugins)
		{
			List<string> lstRealPlugins = new List<string>();
			if (p_strPlugins != null)
			{
				foreach (string strPlugin in p_strPlugins)
					if (File.Exists(strPlugin))
						lstRealPlugins.Add(strPlugin);
			}
			return lstRealPlugins.ToArray();
		}

		/// <summary>
		/// Gets the list of active plugins.
		/// </summary>
		/// <returns>The list of active plugins.</returns>
		public string[] GetActivePlugins()
		{
			IntPtr ptrStrings = IntPtr.Zero;
			UInt32 uintListLength = 0;
			UInt32 uintStatus = m_dlgGetActivePlugins(m_ptrLoadOrderDb, out ptrStrings, out uintListLength);
			HandleStatusCode(uintStatus);
			return RemoveNonExistentPlugins(MarshalPluginArray(ptrStrings, uintListLength) ?? new string[0]);
		}

		/// <summary>
		/// Sets the list of active plugins.
		/// </summary>
		/// <param name="p_strActivePlugins">The list of plugins to set as active.</param>
		public void SetActivePlugins(string[] p_strActivePlugins)
		{
			try
			{
				UInt32 uintStatus = 0;
				using (StringArrayManualMarshaler ammMarshaler = new StringArrayManualMarshaler("UTF8"))
					uintStatus = m_dlgSetActivePlugins(m_ptrLoadOrderDb, ammMarshaler.MarshalManagedToNative(StripPluginDirectory(p_strActivePlugins)), Convert.ToUInt32(p_strActivePlugins.Length));
				HandleStatusCode(uintStatus);
			}
			catch
			{
				MessageBox.Show("The selected plugin has been manually removed." + Environment.NewLine + "Restart NMM or select again your game on Change Game Mode to refresh the plugin list.");
			}
		}

		/// <summary>
		/// Gets the list of plugin, sorted by load order.
		/// </summary>
		/// <returns>The list of plugins, sorted by load order.</returns>
		public string[] GetLoadOrder()
		{
			IntPtr ptrPlugins = IntPtr.Zero;
			UInt32 uintListLength = 0;
			UInt32 uintStatus = m_dlgGetLoadOrder(m_ptrLoadOrderDb, out ptrPlugins, out uintListLength);
			HandleStatusCode(uintStatus);
			string[] strPlugins = MarshalPluginArray(ptrPlugins, uintListLength);
			List<string> lstNonGhostedPlugins = new List<string>();
			return RemoveNonExistentPlugins(MarshalPluginArray(ptrPlugins, uintListLength));
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
		public void SetLoadOrder(string[] p_strPlugins)
		{
			string[] strSortedPlugins = p_strPlugins;
			UInt32 uintStatus = 0;
			using (StringArrayManualMarshaler ammMarshaler = new StringArrayManualMarshaler("UTF8"))
				uintStatus = m_dlgSetLoadOrder(m_ptrLoadOrderDb, ammMarshaler.MarshalManagedToNative(StripPluginDirectory(strSortedPlugins)), Convert.ToUInt32(strSortedPlugins.Length));
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
			UInt32 uintStatus = m_dlgGetPluginActive(m_ptrLoadOrderDb, StripPluginDirectory(p_strPlugin), out booIsActive);
			HandleStatusCode(uintStatus);
			return booIsActive;
		}

		/// <summary>
		/// Sorts the user's mods
		/// </summary>
		/// <param name="p_booTrialOnly">Whether the sort should actually be performed, or just previewed.</param>
		/// <returns>The list of plugins, sorted by load order.</returns>
		public string[] SortMods(bool p_booTrialOnly)
		{
			// Not implemented here, this will be handled by the LOOT API
			return null;
		}

		/// <summary>
		/// Gets the load index of the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose load order is to be retrieved.</param>
		/// <returns>The load index of the specified plugin.</returns>
		public Int32 GetPluginLoadOrder(string p_strPlugin)
		{
			UInt32 uintIndex = 0;
			UInt32 uintStatus = m_dlgGetPluginLoadOrder(m_ptrLoadOrderDb, StripPluginDirectory(p_strPlugin), out uintIndex);
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
			UInt32 uintStatus = m_dlgSetPluginLoadOrder(m_ptrLoadOrderDb, StripPluginDirectory(p_strPlugin), Convert.ToUInt32(p_intIndex));
			HandleStatusCode(uintStatus);
		}

		#region Masterlist Updating

		/// <summary>
		/// Updates the masterlist at the given path.
		/// </summary>
		public void UpdateMasterlist()
		{
			// Not implemented here, this will be handled by the LOOT API
		}

		/// <summary>
		/// Sets the active status of the specified plugin.
		/// </summary>
		/// <param name="p_strPlugin">The plugin whose active status is to be set.</param>
		/// <param name="p_booActive">Whether the specified plugin should be made active or inactive.</param>
		public void SetPluginActive(string p_strPlugin, bool p_booActive)
		{
			UInt32 uintStatus = m_dlgSetPluginActive(m_ptrLoadOrderDb, StripPluginDirectory(p_strPlugin), p_booActive);
			HandleStatusCode(uintStatus);
		}

		/// <summary>
		/// Updates the masterlist at the given path.
		/// </summary>
		/// <returns><c>true</c> if an update to the masterlist is available;
		/// <c>false</c> otherwise.</returns>
		public bool MasterlistHasUpdate()
		{
			// Not implemented here, this will be handled by the LOOT API
			return false;
		}

		/// <summary>
		/// Gets the plugin at the specified load index.
		/// </summary>
		/// <param name="p_intIndex">The load index of the plugin to retrieve.</param>
		/// <returns>The name of the plugin at the specified index.</returns>
		public string GetIndexedPlugin(Int32 p_intIndex)
		{
			///TODO: this method doesn't work with NMM, as NMM is passing an index that doesn't account for ghosted plugins
			string strPlugin = null;
			UInt32 uintStatus = m_dlgGetIndexedPlugin(m_ptrLoadOrderDb, Convert.ToUInt32(p_intIndex), out strPlugin);
			HandleStatusCode(uintStatus);
			return AddPluginDirectory(strPlugin);
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Gets whether the plugin is a master file.
		/// </summary>
		/// <param name="p_strPlugin">The plugin for which it is to be determined if the file is a plugin.</param>
		/// <returns><c>true</c> if the given plugin is a master file;
		/// <c>false</c> otherwise.</returns>
		public bool IsMaster(string p_strPlugin)
		{
			return false;
			//notimplemented
		}

		#endregion
				
	}
}
