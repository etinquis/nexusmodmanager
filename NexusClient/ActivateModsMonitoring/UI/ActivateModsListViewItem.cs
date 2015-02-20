﻿using System;
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
			else if (p_btsTask.GetType() == typeof(ModUpgrader))
				lsiSubItem.Text = ((ModUpgrader)p_btsTask).ModName;
		
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
			e.Argument.PropertyChanged += new PropertyChangedEventHandler(Task_PropertyChanged);
			
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
			else if (((IBackgroundTaskSet)sender).GetType() == typeof(ModUpgrader))
				SubItems["Operation"].Text = "Upgrading";

			((IBackgroundTaskSet)sender).IsQueued = false;

			m_amcControl.CallUpdateBottomBarFeedback(this);
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
			else if (btsExecutor.GetType() == typeof(ModUpgrader))
				booComplete = ((ModUpgrader)btsExecutor).IsCompleted;

			if (booComplete)
			{
				if (!booSuccess)
				{
					SubItems["Status"].Text = e.Message;
					SubItems["Progress"].Text = "";
				}
				else
				{
					SubItems["Status"].Text = "Complete";
					SubItems["Progress"].Text = "100%";
				}
			}
			else
			{
				SubItems["Status"].Text = e.Message;
				SubItems["Progress"].Text = "";
			}

			m_amcControl.CallUpdateBottomBarFeedback(this);

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
					ListView.Invoke((Action<IBackgroundTask, string>)HandleChangedTaskProperty, (IBackgroundTask)sender, e.PropertyName);
					return;
				}
				HandleChangedTaskProperty((IBackgroundTask)sender, e.PropertyName);
			}
			catch { }
		}

		/// <summary>
		/// Updates the list view item to display the changed property.
		/// </summary>
		/// <param name="p_tskTask">The task whose property has changed.</param>
		/// <param name="p_strPropertyName">The name of the propety that has changed.</param>
		private void HandleChangedTaskProperty(IBackgroundTask p_tskTask, string p_strPropertyName)
		{
			try
			{
				if (p_tskTask.GetType() == typeof(BasicUninstallTask))
				{
					if ((p_strPropertyName.Equals(ObjectHelper.GetPropertyName<IBackgroundTask>(x => x.ItemProgress))) && (p_tskTask.ItemProgress > 0))
						SubItems["Progress"].Text = "Uninstalling, please wait...(" + ((p_tskTask.ItemProgress * 100) / p_tskTask.ItemProgressMaximum).ToString() + "%)";
				}
				else
				{
					if ((p_tskTask.GetType() == typeof(PrepareModTask)))
					{
						if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<IBackgroundTask>(x => x.OverallProgress)))
							SubItems["Progress"].Text = "Unpacking, please wait...(" + (((p_tskTask.OverallProgress * 100) / p_tskTask.OverallProgressMaximum)/2).ToString() + "%)";
					}
					else
					{
						if (p_strPropertyName.Equals(ObjectHelper.GetPropertyName<IBackgroundTask>(x => x.OverallProgress)))
							SubItems["Progress"].Text = "Installing, please wait...(" + ((((p_tskTask.OverallProgress * 100) / p_tskTask.OverallProgressMaximum)/2)+50).ToString() + "%)";
					}
				}
			}
			catch (NullReferenceException)
			{
				//this can happen if we try to update the form before its handle has been created
				// we should never get here, but if we do, we don't need to care
			}
			catch (ArgumentOutOfRangeException)
			{
				// we don't care if that happens
			}
		}

		#endregion
	}
}
