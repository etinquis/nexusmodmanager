﻿using Nexus.Client.BackgroundTasks;
using Nexus.Client.Commands.Generic;
using Nexus.Client.ModManagement;
using Nexus.Client.ModRepositories;
using Nexus.Client.Settings;
using Nexus.Client.Util.Collections;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Nexus.Client.ActivateModsMonitoring.UI
{
	/// <summary>
	/// This class encapsulates the data and the operations presented by UI
	/// elements that display Download monitoring.
	/// </summary>
	public class ActivateModsMonitorVM : INotifyPropertyChanged
	{
		ModManager m_mmgModManager = null;

		#region Properties

		/// <summary>
		/// Raised whenever a property of the class changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region Commands

		/// <summary>
		/// Gets the command to cancel a task.
		/// </summary>
		/// <remarks>
		/// The commands takes an argument describing the task to be cancel.
		/// </remarks>
		/// <value>The command to cancel a task.</value>
		public Command<BasicInstallTask> CancelTaskCommand { get; private set; }
		
		
		/// <summary>
		/// Gets the number of maximum allowed concurrent downloads.
		/// </summary>
		/// <value>The number of maximum allowed concurrent downloads.</value>
		public int MaxConcurrentActivation
		{
			get
			{
				return 1;
			}
		}

		#endregion

		/// <summary>
		/// Gets the Download manager to use to manage the monitored activities.
		/// </summary>
		/// <value>The Download manager to use to manage the monitored activities.</value>
		protected ActivateModsMonitor ActivateModsMonitor { get; private set; }

		/// <summary>
		/// Gets the mod repository from which to get mods and mod metadata.
		/// </summary>
		/// <value>The mod repository from which to get mods and mod metadata.</value>
		public IModRepository ModRepository { get; private set; }

		/// <summary>
		/// Gets the list of tasks being monitored.
		/// </summary>
		/// <value>The list of tasks being monitored.</value>
		public ReadOnlyObservableList<IBackgroundTaskSet> Tasks
		{
			get
			{
				return ActivateModsMonitor.Tasks;
			}
		}

		/// <summary>
		/// Gets the list of tasks being executed.
		/// </summary>
		/// <value>The list of tasks being executed.</value>
		public ReadOnlyObservableList<IBackgroundTaskSet> ActiveTasks
		{
			get
			{
				return ActivateModsMonitor.ActiveTasks;
			}
		}
		
		/// <summary>
		/// Gets the total task speed.
		/// </summary>
		/// <value>The total task speed.</value>
		public string Status
		{
			get
			{
				return ActivateModsMonitor.Status;
			}
		}
				

		/// <summary>
		/// Gets the application and user settings.
		/// </summary>
		/// <value>The application and user settings.</value>
		public ISettings Settings { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with its dependencies.
		/// </summary>
		/// <param name="p_amnActivateModsMonitor">The Activate Mods  manager to use to manage the monitored activities.</param>
		/// <param name="p_setSettings">The application and user settings.</param>
		public ActivateModsMonitorVM(ActivateModsMonitor p_amnActivateModsMonitor, ISettings p_setSettings, ModManager p_mmgModManager)
		{
			ActivateModsMonitor = p_amnActivateModsMonitor;
			Settings = p_setSettings;
			m_mmgModManager = p_mmgModManager;
			ActivateModsMonitor.PropertyChanged += new PropertyChangedEventHandler(ActiveTasks_PropertyChanged);

			CancelTaskCommand = new Command<BasicInstallTask>("Cancel", "Cancels the selected Download.", CancelTask);
		}

		#endregion

		private void ActiveTasks_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Status")
			{
				OnPropertyChanged(e.PropertyName);
			}
		}

		/// <summary>
		/// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of the project.
		/// </summary>
		/// <param name="name">The property name.</param>
		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
		
		#region Remove Command

		/// <summary>
		/// Removes the given task.
		/// </summary>
		/// <param name="p_tskTask">BasicInstallTask task to remove.</param>
		public void RemoveTask(ModInstaller p_tskTask)
		{
			if (ActivateModsMonitor.CanRemove(p_tskTask))
				ActivateModsMonitor.RemoveDownload(p_tskTask);
		}

		/// <summary>
		/// Removes the given task.
		/// </summary>
		/// <param name="p_tskTask">BasicInstallTask task to remove.</param>
		public void RemoveQueuedTask(ModInstaller p_tskTask)
		{
			if (ActivateModsMonitor.CanRemoveQueued(p_tskTask))
				ActivateModsMonitor.RemoveQueuedDownload(p_tskTask);
		}

		/// <summary>
		/// Removes the given task.
		/// </summary>
		/// <param name="p_tskTask">BasicInstallTask task to remove.</param>
		public void RemoveSelectedTask(ModInstaller p_tskTask)
		{
			if (ActivateModsMonitor.CanRemoveSelected(p_tskTask))
			{
				if (p_tskTask.IsCompleted)
					ActivateModsMonitor.RemoveDownload(p_tskTask);
				else if (p_tskTask.IsQueued)
					ActivateModsMonitor.RemoveQueuedDownload(p_tskTask);
			}
		}

        /// <summary>
        /// Removes the given task (the task is already in queue or running).
        /// </summary>
        /// <param name="p_tskTask">BasicInstallTask task to remove.</param>
		public void RemoveUselessTask(ModInstaller p_tskTask)
        {
            ActivateModsMonitor.RemoveUselessTask(p_tskTask);
        }

		/// <summary>
		/// Cancels the given task.
		/// </summary>
		/// <param name="p_tskTask">The task to cancel.</param>
		public void CancelTask(BasicInstallTask p_tskTask)
		{
			//p_tskTask.Cancel();
		}
		

		/// <summary>
		/// Removes all the completed/failed tasks.
		/// </summary>
		public void RemoveAllTasks()
		{
			List<IBackgroundTaskSet> lstTasks = new List<IBackgroundTaskSet>();
			lock (Tasks)
			{
				foreach (IBackgroundTaskSet btTask in Tasks)
					lstTasks.Add(btTask);
			}
			if (lstTasks.Count > 0)
				foreach (ModInstaller btRemovable in lstTasks)
					RemoveTask((ModInstaller)btRemovable);
		}

		/// <summary>
		/// Removes the selected task.
		/// </summary>
		public void RemoveSelectedTask(string p_strTask)
		{
			List<IBackgroundTaskSet> lstTasks = new List<IBackgroundTaskSet>();
			lock (Tasks)
			{
				foreach (IBackgroundTaskSet btTask in Tasks)
					lstTasks.Add(btTask);
			}

			if (lstTasks.Count > 0)
			{
				foreach (ModInstaller btRemovable in lstTasks)
				{
					if (btRemovable.ModName == p_strTask)
						RemoveSelectedTask((ModInstaller)btRemovable);
				}
			}
		}

		/// <summary>
		/// Check the task status.
		/// </summary>
		public bool CheckTaskStatus(string p_strTask)
		{
			bool booStatus = false;
			List<IBackgroundTaskSet> lstTasks = new List<IBackgroundTaskSet>();
			lock (Tasks)
			{
				foreach (IBackgroundTaskSet btTask in Tasks)
					lstTasks.Add(btTask);
			}

			if (lstTasks.Count > 0)
			{
				foreach (ModInstaller btRemovable in lstTasks)
				{
					if (btRemovable.ModName == p_strTask)
						if ((btRemovable.IsQueued) || (btRemovable.IsCompleted))
							booStatus = true;
				}
			}
			return booStatus;
		}
		

		/// <summary>
		/// Removes all the queued tasks.
		/// </summary>
		public void RemoveQueuedTasks()
		{
			List<IBackgroundTaskSet> lstTasks = new List<IBackgroundTaskSet>();
			lock (Tasks)
			{
				foreach (IBackgroundTaskSet btTask in Tasks)
					lstTasks.Add(btTask);
			}

			if (lstTasks.Count > 0)
				foreach (ModInstaller btRemovable in lstTasks)
					RemoveQueuedTask((ModInstaller)btRemovable);
		
		}

		

		/// <summary>
		/// Determines if the given <see cref="BasicInstallTask"/> can be removed.
		/// </summary>
		/// <param name="p_tskTask">The task for which it is to be determined
		/// if it can be removed.</param>
		/// <returns><c>true</c> if the task can be removed;
		/// <c>false</c> otherwise.</returns>
		public bool CanRemoveDownload(BasicInstallTask p_tskTask)
		{
			return true; // ActivateModsMonitor.CanRemove(p_tskTask);
		}

		/// <summary>
		/// Determines if the given <see cref="IBackgroundTask"/> can be cancelled.
		/// </summary>
		/// <param name="p_tskTask">The task for which it is to be determined
		/// if it can be cancelled.</param>
		/// <returns><c>true</c> if the task can be cancelled;
		/// <c>false</c> otherwise.</returns>
		public bool CanCancelTask(IBackgroundTask p_tskTask)
		{
			return (p_tskTask.Status == TaskStatus.Paused) || (p_tskTask.Status == TaskStatus.Incomplete) || (p_tskTask.InnerTaskStatus == TaskStatus.Retrying) || (p_tskTask.Status == TaskStatus.Queued);
		}

		#endregion
		
		
	}
}