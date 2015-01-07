using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Nexus.Client.BackgroundTasks;
using Nexus.Client.ModManagement;
using Nexus.Client.Util;
using Nexus.Client.Util.Collections;


namespace Nexus.Client.ActivateModsMonitoring
{
	/// <summary>
	/// This monitors the status of activities.
	/// </summary>
	public class ActivateModsMonitor : INotifyPropertyChanged
	{
		private ThreadSafeObservableList<IBackgroundTaskSet> m_oclTasks = new ThreadSafeObservableList<IBackgroundTaskSet>();
		//private IBackgroundTaskSet m_bstRunningTask = null;
		private string m_Status = null;
		
		/// <summary>
		/// Raised whenever a property of the class changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		#region Properties

		/// <summary>
		/// Gets the list of tasks being monitored.
		/// </summary>
		/// <value>The list of tasks being monitored.</value>
		public ReadOnlyObservableList<IBackgroundTaskSet> Tasks { get; private set; }

		/// <summary>
		/// Gets the list of tasks being executed.
		/// </summary>
		/// <value>The list of tasks being executed.</value>
		public ReadOnlyObservableList<IBackgroundTaskSet> ActiveTasks { get; private set; }

		public string Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				bool booChanged = false;
				if (m_Status != value)
				{
					booChanged = true;
					m_Status = value;
				}
				if (booChanged)
					OnPropertyChanged("Status");
			}
		}
		
		
		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public ActivateModsMonitor()
		{
			Tasks = new ReadOnlyObservableList<IBackgroundTaskSet>(m_oclTasks);
			//m_oclTasks.CollectionChanged += new NotifyCollectionChangedEventHandler(oclTasks_CollectionChanged);
		}

		#endregion

		/// <summary>
		/// Adds a task to the monitor.
		/// </summary>
		/// <param name="p_tskTask">The task to monitor.</param>
		public void AddActivity(IBackgroundTaskSet p_bstTask)
		{
			m_oclTasks.Add(p_bstTask);
			//m_setQueuedTasks.Add(p_bstTask);
		}
				

		#region Mods Removal

		/// <summary>
		/// Removes a task from the monitor.
		/// </summary>
		/// <remarks>
		/// Tasks can only be removed if they are not running.
		/// </remarks>
		/// <param name="p_tskTask">The task to remove.</param>
		public void RemoveDownload(ModInstaller p_tskTask)
		{
			if (CanRemove(p_tskTask))
				m_oclTasks.Remove(p_tskTask);
		}

        /// <summary>
        /// Removes an uninstalling task from the monitor.
        /// </summary>
        /// <remarks>
        /// Tasks can only be removed if they are not running.
        /// </remarks>
        /// <param name="p_tskTask">The task to remove.</param>
        public void RemoveDownloadUn(ModUninstaller p_tskTask)
        {
            if (CanRemoveUn(p_tskTask))
                m_oclTasks.Remove(p_tskTask);
        }

		/// <summary>
		/// Removes a task from the monitor.
		/// </summary>
		/// <remarks>
		/// Tasks can only be removed if they are not running.
		/// </remarks>
		/// <param name="p_tskTask">The task to remove.</param>
		public void RemoveQueuedDownload(ModInstaller p_tskTask)
		{
			if (CanRemoveQueued(p_tskTask))
				m_oclTasks.Remove(p_tskTask);
		}

        /// <summary>
        /// Removes an uninstalling task from the monitor.
        /// </summary>
        /// <remarks>
        /// Tasks can only be removed if they are not running.
        /// </remarks>
        /// <param name="p_tskTask">The task to remove.</param>
        public void RemoveQueuedDownloadUn(ModUninstaller p_tskTask)
        {
            if (CanRemoveQueuedUn(p_tskTask))
                m_oclTasks.Remove(p_tskTask);
        }

        /// <summary>
        /// Removes a useless task (the task is already in queue or running).
        /// </summary>
        public void RemoveUselessTask(ModInstaller p_tskTask)
        {
            m_oclTasks.Remove(p_tskTask);
        }

        /// <summary>
        /// Removes a useless uninstalling task (the task is already in queue or running).
        /// </summary>
        public void RemoveUselessTaskUn(ModUninstaller p_tskTask)
        {
            m_oclTasks.Remove(p_tskTask);
        }
		
		/// <summary>
		/// Determines if the given <see cref="BasicInstallTask"/> can be removed from
		/// the monitor.
		/// </summary>
		/// <param name="p_tskTask">The task for which it is to be determined
		/// if it can be removed from the monitor.</param>
		/// <returns><c>true</c> if the p_tskTask can be removed;
		/// <c>false</c> otherwise.</returns>
		public bool CanRemove(ModInstaller p_tskTask)
		{
			return p_tskTask.IsCompleted;
		}

        /// <summary>
        /// Determines if the given <see cref="BasicInstallTask"/> can be removed from
        /// the monitor.
        /// </summary>
        /// <param name="p_tskTask">The task for which it is to be determined
        /// if it can be removed from the monitor.</param>
        /// <returns><c>true</c> if the p_tskTask can be removed;
        /// <c>false</c> otherwise.</returns>
        public bool CanRemoveUn(ModUninstaller p_tskTask)
        {
            return p_tskTask.IsCompleted;
        }

        /// <summary>
        /// Determines if the given <see cref="BasicInstallTask"/> queued can be removed from
        /// the monitor.
        /// </summary>
        /// <param name="p_tskTask">The task for which it is to be determined
        /// if it can be removed from the monitor.</param>
        /// <returns><c>true</c> if the p_tskTask can be removed;
        /// <c>false</c> otherwise.</returns>
		public bool CanRemoveQueued(ModInstaller p_tskTask)
		{
			return p_tskTask.IsQueued;
		}

        /// <summary>
        /// Determines if the given uninstalling <see cref="BasicInstallTask"/> queued can be removed from
        /// the monitor.
        /// </summary>
        /// <param name="p_tskTask">The task for which it is to be determined
        /// if it can be removed from the monitor.</param>
        /// <returns><c>true</c> if the p_tskTask can be removed;
        /// <c>false</c> otherwise.</returns>
        public bool CanRemoveQueuedUn(ModUninstaller p_tskTask)
        {
            return p_tskTask.IsQueued;
        }

        /// <summary>
        /// Determines if the given <see cref="BasicInstallTask"/> selected can be removed from
        /// the monitor.
        /// </summary>
        /// <param name="p_tskTask">The task for which it is to be determined
        /// if it can be removed from the monitor.</param>
        /// <returns><c>true</c> if the p_tskTask can be removed;
        /// <c>false</c> otherwise.</returns>
		public bool CanRemoveSelected(ModInstaller p_tskTask)
		{
			if (p_tskTask.IsQueued || p_tskTask.IsCompleted)
				return true;
			else
				return false;
		}
        
        /// <summary>
        /// Determines if the given uninstalling <see cref="BasicInstallTask"/> selected can be removed from
        /// the monitor.
        /// </summary>
        /// <param name="p_tskTask">The task for which it is to be determined
        /// if it can be removed from the monitor.</param>
        /// <returns><c>true</c> if the p_tskTask can be removed;
        /// <c>false</c> otherwise.</returns>
        public bool CanRemoveSelectedUn(ModUninstaller p_tskTask)
        {
            if (p_tskTask.IsQueued || p_tskTask.IsCompleted)
                return true;
            else
                return false;
        }
		
		
		#endregion


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

		
	}
}
