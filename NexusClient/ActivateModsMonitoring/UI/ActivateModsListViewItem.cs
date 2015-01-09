using System;
using System.ComponentModel;
using System.Windows.Forms;
using Nexus.Client.BackgroundTasks;
using Nexus.Client.ModManagement;
using Nexus.Client.Util;

namespace Nexus.Client.ActivateModsMonitoring.UI
{
	/// <summary>
	/// A list view item that displays the status of a <see cref="BasicInstallTask"/>
	/// </summary>
	public class ActivateModsListViewItem : ListViewItem
	{
		ActivateModsMonitorControl m_amcControl = null;
		private bool m_booRemovable = false;
		
		#region Properties

		/// <summary>
		/// Gets the <see cref="IBackgroundTaskSet"/> whose status is being displayed by this list view item.
		/// </summary>
		/// <value>The <see cref="IBackgroundTaskSet"/> whose status is being displayed by this list view item.</value>
		public IBackgroundTaskSet Task { get; private set; }

		public bool IsRemovable
		{
			get 
			{
				return m_booRemovable;
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_btsTask">The task whose status is to be displayed by this list
		/// view item.</param>
		public ActivateModsListViewItem(IBackgroundTaskSet p_btsTask, ActivateModsMonitorControl p_amcControl)
		{
			m_amcControl = p_amcControl;
			Task = p_btsTask;

			ListViewSubItem lsiSubItem = SubItems[0];
			lsiSubItem.Name = "ModName";

            if (p_btsTask.GetType() == typeof(ModUninstaller))
			    lsiSubItem.Text = ((ModUninstaller)p_btsTask).ModName;
            else if (p_btsTask.GetType() == typeof(ModInstaller))
                lsiSubItem.Text = ((ModInstaller)p_btsTask).ModName;
		
			lsiSubItem = SubItems.Add(new ListViewSubItem());
			lsiSubItem.Name = "Status";
			lsiSubItem.Text = "Queued";
			p_btsTask.IsQueued = true;

            lsiSubItem = SubItems.Add(new ListViewSubItem());
            lsiSubItem.Name = "Operation";
            lsiSubItem.Text = "";		

			lsiSubItem = SubItems.Add(new ListViewSubItem());
			lsiSubItem.Name = "Progress";

			m_booRemovable = true;
			p_btsTask.TaskStarted += new EventHandler<EventArgs<IBackgroundTask>>(TaskSet_TaskSetStarted);

			p_btsTask.TaskSetCompleted += new EventHandler<TaskSetCompletedEventArgs>(TaskSet_TaskSetCompleted);
		}

		private void TaskSet_TaskSetStarted(object sender, EventArgs<IBackgroundTask> e)
		{
            if ((ListView != null) && ListView.InvokeRequired)
			{
				ListView.Invoke((Action<IBackgroundTaskSet,  EventArgs<IBackgroundTask>>)TaskSet_TaskSetStarted, sender, e);
				return;
			}

			m_booRemovable = false;
            SubItems["Status"].Text = "Running";
            if (((IBackgroundTaskSet)sender).GetType() == typeof(ModInstaller))
                SubItems["Operation"].Text = "Install";
            else if (((IBackgroundTaskSet)sender).GetType() == typeof(ModUninstaller))
                SubItems["Operation"].Text = "Uninstall";

			((IBackgroundTaskSet)sender).IsQueued = false;
		}

		private void TaskSet_TaskSetCompleted(object sender, TaskSetCompletedEventArgs e)
		{

            if ((ListView != null) && ListView.InvokeRequired)
			{
				ListView.Invoke((Action<IBackgroundTaskSet, TaskSetCompletedEventArgs>)TaskSet_TaskSetCompleted, sender, e);
				return;
			}

			bool booComplete = false;
			bool booSuccess = false;

			IBackgroundTaskSet btsExecutor = (IBackgroundTaskSet)sender;
			booSuccess = e.Success;
			if (btsExecutor.GetType() == typeof(ModInstaller))
				booComplete = ((ModInstaller)btsExecutor).IsCompleted;
			else if (btsExecutor.GetType() == typeof(ModUninstaller))
				booComplete = ((ModUninstaller)btsExecutor).IsCompleted;

			if (booComplete)
			{
				if (!booSuccess)
					SubItems["Status"].Text = e.Message;
				else
					SubItems["Status"].Text = "Complete";
			}
			else
				SubItems["Status"].Text = e.Message;

			m_booRemovable = true;
		}

		#endregion

		#region Task Property Change Handling

		/// <summary>
		/// Handles the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of the task.
		/// </summary>
		/// <remarks>
		/// This updates the progress message and other text in the list view item.
		/// </remarks>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">A <see cref="PropertyChangedEventArgs"/> that describes the event arguments.</param>
		private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			try
			{
				string strPropertyName = e.PropertyName;
				if ((ListView != null) && ListView.InvokeRequired)
				{
					ListView.Invoke((Action<BasicInstallTask, string>)HandleChangedTaskProperty, sender, e.PropertyName);
					return;
				}
				HandleChangedTaskProperty((BasicInstallTask)sender, e.PropertyName);
			}
			catch { }
		}

		/// <summary>
		/// Updates the list view item to display the changed property.
		/// </summary>
		/// <param name="p_tskTask">The task whose property has changed.</param>
		/// <param name="p_strPropertyName">The name of the propety that has changed.</param>
		private void HandleChangedTaskProperty(BasicInstallTask p_tskTask, string p_strPropertyName)
		{
		}

		#endregion
	}
}
