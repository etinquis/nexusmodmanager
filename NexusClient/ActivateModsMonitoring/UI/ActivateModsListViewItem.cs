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
		
		#region Properties

		/// <summary>
		/// Gets the <see cref="BasicInstallTask"/> whose status is being displayed by this list view item.
		/// </summary>
		/// <value>The <see cref="BasicInstallTask"/> whose status is being displayed by this list view item.</value>
		public BasicInstallTask Task { get; private set; }

		#endregion

		#region Constructors

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_tskTask">The task whose status is to be displayed by this list
		/// view item.</param>
		public ActivateModsListViewItem(BasicInstallTask p_tskTask)
		{
			Task = p_tskTask;
			
			ListViewSubItem lsiSubItem = SubItems[0];
			lsiSubItem.Name = ObjectHelper.GetPropertyName(() => p_tskTask.OverallMessage);
			lsiSubItem.Text = p_tskTask.OverallMessage;

			lsiSubItem = SubItems.Add(new ListViewSubItem());
			lsiSubItem.Name = ObjectHelper.GetPropertyName(() => p_tskTask.OverallProgress);
			if (p_tskTask.ShowOverallProgressAsMarquee)
				lsiSubItem.Text = "Working...";
			else
			{
				lsiSubItem.Text = "";
			}

			lsiSubItem = SubItems.Add(new ListViewSubItem());
			lsiSubItem.Name = ObjectHelper.GetPropertyName(() => p_tskTask.Status);
			if (p_tskTask.Status == TaskStatus.Running)
				lsiSubItem.Text = "Downloading";
			else
				lsiSubItem.Text = p_tskTask.Status.ToString();

			lsiSubItem = SubItems.Add(new ListViewSubItem());
			lsiSubItem.Name = ObjectHelper.GetPropertyName(() => p_tskTask.InnerTaskStatus);

			p_tskTask.PropertyChanged += new PropertyChangedEventHandler(Task_PropertyChanged);
		}

		/// <summary>
		/// A simple constructor that initializes the object with the given values.
		/// </summary>
		/// <param name="p_btsTask">The task whose status is to be displayed by this list
		/// view item.</param>
		public ActivateModsListViewItem(IBackgroundTaskSet p_btsTask, ActivateModsMonitorControl p_amcControl)
		{
			m_amcControl = p_amcControl;

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

            if (SubItems["Operation"].Text == "")
            {
                if (((IBackgroundTaskSet)sender).GetType() == typeof(ModInstaller))
                    ((ModInstaller)sender).Install();
                else if (((IBackgroundTaskSet)sender).GetType() == typeof(ModUninstaller))
                    ((ModUninstaller)sender).Install();
            }
                

			SubItems["Status"].Text = "Complete";

			m_amcControl.IsInstalling = false;
			
			//m_amcControl.lvwActiveTasks.DrawSubItem += new DrawListViewSubItemEventHandler(m_amcControl.lvwActiveTasks_DrawSubItem);
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
			if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.OverallMessage)))
			{
				SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.OverallMessage)].Text = p_tskTask.OverallMessage;
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.TaskSpeed)))
			{
				SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemMessage)].Text = String.Format("{0:} kb/s", p_tskTask.TaskSpeed.ToString());
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.OverallProgress)))
			{
				SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.OverallProgress)].Text = p_tskTask.Status.ToString();
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ActiveThreads)))
			{
				if (Task.Status == TaskStatus.Running)
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = p_tskTask.ActiveThreads.ToString();
				else
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = String.Empty;
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress))
					|| p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgressMaximum))
					|| p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgressMinimum))
					|| p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ShowItemProgress))
					|| p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ShowItemProgressAsMarquee)))
			{
				if (p_tskTask.ShowItemProgress)
				{
					if (p_tskTask.ShowItemProgressAsMarquee)
						SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = "Working...";
				}
				else
				{
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemMessage)].Text = null;
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = null;
				}
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.Status)))
			{
				if (p_tskTask.Status == TaskStatus.Running)
				{
					SubItems[p_strPropertyName].Text = "Downloading";
				}
				else
				{
					SubItems[p_strPropertyName].Text = p_tskTask.Status.ToString();
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemMessage)].Text = "";
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = "";
					if (!(p_tskTask.Status.ToString() == "Paused"))
						SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.OverallProgress)].Text = p_tskTask.Status.ToString();
					SubItems["ETA"].Text = "";
				}
			}
			else if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.InnerTaskStatus)))
			{
				if ((p_tskTask.InnerTaskStatus.ToString() == "Retrying") && ((p_tskTask.Status != TaskStatus.Paused) && (p_tskTask.Status != TaskStatus.Queued)))
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.Status)].Text = p_tskTask.InnerTaskStatus.ToString();
				else if (p_tskTask.InnerTaskStatus.ToString() == "Running")
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.Status)].Text = "Downloading";
				else
				{
					SubItems["ETA"].Text = "";
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemMessage)].Text = "";
					SubItems[ObjectHelper.GetPropertyName<BasicInstallTask>(x => x.ItemProgress)].Text = "";
				}
			}
		}

		#endregion
	}
}
