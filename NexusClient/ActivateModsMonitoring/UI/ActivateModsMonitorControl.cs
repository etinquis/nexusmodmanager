using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Nexus.Client.BackgroundTasks;
using Nexus.Client.Commands;
using Nexus.Client.Commands.Generic;
using Nexus.Client.ModManagement;
using Nexus.Client.UI;
using Nexus.Client.Util;
using System.Drawing;

namespace Nexus.Client.ActivateModsMonitoring.UI
{
	/// <summary>
	/// The view that exposes Download monitoring functionality.
	/// </summary>
	public partial class ActivateModsMonitorControl : ManagedFontDockContent
	{
		private ActivateModsMonitorVM m_vmlViewModel = null;
		private float m_fltColumnRatio = 0.5f;
		private bool m_booResizing = false;
		private Timer m_tmrColumnSizer = new Timer();
		private string m_strTitleAllActive = "Mod Activation Queue ({0})";
		private string m_strTitleSomeActive = "Mod Activation Queue ({0}/{1})";
		private bool m_booControlIsLoaded = false;
		private IBackgroundTaskSet m_btsRunningTask = null;
		private string strStatus = null;
		private List<IBackgroundTaskSet> QueuedTasks = new List<IBackgroundTaskSet>();
        private bool booQueued = false;

		
		#region Events

		public event EventHandler EmptyQueue;
		public event EventHandler SetTextBoxFocus;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the view model that provides the data and operations for this view.
		/// </summary>
		/// <value>The view model that provides the data and operations for this view.</value>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ActivateModsMonitorVM ViewModel
		{
			get
			{
				return m_vmlViewModel;
			}
			set
			{
				m_vmlViewModel = value;
				if (m_vmlViewModel != null)
				{
					if (m_vmlViewModel.Tasks != null)
						foreach (IBackgroundTaskSet tskBasicInstall in m_vmlViewModel.Tasks)
							AddTaskToList(tskBasicInstall);
															
					m_vmlViewModel.Tasks.CollectionChanged += new NotifyCollectionChangedEventHandler(Tasks_CollectionChanged);

													
					new ToolStripItemCommandBinding<BasicInstallTask>(tsbCancel, ViewModel.CancelTaskCommand, null);

					Command cmdRemoveAll = new Command("Remove all", "Purges the completed activations from the list.", ViewModel.RemoveAllTasks);
					new ToolStripItemCommandBinding(tsbRemoveAll, cmdRemoveAll);
					Command cmdRemoveQueued = new Command("Remove queued", "Purges the queued activations from the list.", RemoveQueuedTasks);
					new ToolStripItemCommandBinding(tsbRemoveQueued, cmdRemoveQueued);
					Command cmdRemoveSelected = new Command("Remove selected", "Purges the selected activation from the list.", RemoveSelectedTask);
					new ToolStripItemCommandBinding(tsbCancel, cmdRemoveSelected);


					ViewModel.CancelTaskCommand.CanExecute = false;
					

				}
				LoadMetrics();
				UpdateTitle();
			}
		}

		public bool IsInstalling
		{
			get
			{
				return (m_btsRunningTask != null);
			}
			set { }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// The default constructor.
		/// </summary>
		public ActivateModsMonitorControl()
		{
			InitializeComponent();

			clmOverallMessage.Name = "ModName";
			clmOverallProgress.Name = "Status";
			//clmIcon.Name = "Progress";
						
			m_tmrColumnSizer.Interval = 100;
			m_tmrColumnSizer.Tick += new EventHandler(ColumnSizer_Tick);
			
			UpdateTitle();
		}

		#endregion
		
		#region Control Metrics Serialization

		/// <summary>
		/// Raises the <see cref="UserControl.Load"/> event of the control.
		/// </summary>
		/// <remarks>
		/// This loads any saved control metrics.
		/// </remarks>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignMode)
			{
				m_booControlIsLoaded = true;
				LoadMetrics();
			}
		}

		/// <summary>
		/// Loads the control's saved metrics.
		/// </summary>
		protected void LoadMetrics()
		{
			if (m_booControlIsLoaded && (ViewModel != null))
			{
				ViewModel.Settings.ColumnWidths.LoadColumnWidths("ActivateModsMonitor", lvwActiveTasks);

				FindForm().FormClosing += new FormClosingEventHandler(ActivateModsMonitorControl_FormClosing);
				m_fltColumnRatio = (float)clmOverallMessage.Width / (clmOverallMessage.Width);
				SizeColumnsToFit();
			}
		}

		/// <summary>
		/// Handles the <see cref="Form.Closing"/> event of the parent form.
		/// </summary>
		/// <remarks>
		/// This saves the control's metrics.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="FormClosingEventArgs"/> describing the event arguments.</param>
		private void ActivateModsMonitorControl_FormClosing(object sender, FormClosingEventArgs e)
		{
			ViewModel.Settings.ColumnWidths.SaveColumnWidths("ActivateModsMonitor", lvwActiveTasks);
			ViewModel.Settings.Save();
		}

		#endregion

		

		/// <summary>
		/// Hanldes the <see cref="Control.MouseClick"/> event of the controls.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="MouseEventArgs"/> describing the event arguments.</param>
		private void ActivateModsMonitorControl_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				ContextMenu m = new ContextMenu();
				m.MenuItems.Clear();
				m.MenuItems.Add(new MenuItem("Copy to clipboard", new EventHandler(cmsContextMenu_Copy)));
				m.Show((Control)(sender), e.Location);
			}
		}

        /// <summary>
        /// During the backup ebables or disables the Activate Mods Monitoring icons
        /// </summary>
        /// <param name="p_booCheck">The boolean value.</param>
        public void SetCommandBackupAMCStatus(bool p_booCheck)
		{
			Control.CheckForIllegalCrossThreadCalls = false;
			
			tsbCancel.Enabled = p_booCheck;
			tsbRemoveAll.Enabled = p_booCheck;
			tsbRemoveQueued.Enabled = p_booCheck;
		}

		/// <summary>
		/// Hanldes the <see cref="Control.KeyUp"/> event of the controls.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="MouseEventArgs"/> describing the event arguments.</param>
		private void ActivateModsMonitorControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.C | Keys.Control))
			{
				Clipboard.SetText(lvwActiveTasks.FocusedItem.Text);
			}
			if (e.KeyData == (Keys.Control | Keys.F))
			{
				SetTextBoxFocus(this, e);
			}
		}

		#region Binding

		private void RemoveQueuedTasks()
		{
			ViewModel.RemoveQueuedTasks();
			QueuedTasks.RemoveAll(x => x.IsQueued);
		}
		
		private void RemoveSelectedTask()
		{
			string strTaskName = GetSelectedTask();
			ViewModel.RemoveSelectedTask(strTaskName);
			if (QueuedTasks.Count > 0)
			{
				m_btsRunningTask = QueuedTasks.First();
				QueuedTasks.Remove(m_btsRunningTask);
			}
		}

		/// <summary>
		/// Retruns the <see cref="BasicInstallTask"/> that is currently selected in the view.
		/// </summary>
		/// <returns>The <see cref="BasicInstallTask"/> that is currently selected in the view, or
		/// <c>null</c> if no <see cref="BasicInstallTask"/> is selected.</returns>
		private string GetSelectedTask()
		{
			if (lvwActiveTasks.SelectedItems.Count > 0)
				return lvwActiveTasks.SelectedItems[0].Text;
			else
				return null;
		}
				
		/// <summary>
		/// Sets the executable status of the commands.
		/// </summary>
		protected void SetCommandExecutableStatus(bool boo_booCheckStatus)
		{
			ViewModel.CancelTaskCommand.CanExecute = (lvwActiveTasks.SelectedItems.Count > 0) && boo_booCheckStatus;
		}
			
		#endregion

		#region Task Addition/Removal

		/// <summary>
		/// Adds the given <see cref="BasicInstallTask"/> to the view's list. If the <see cref="BasicInstallTask"/>
		/// already exists in the list, nothing is done.
		/// </summary>
		/// <param name="p_tskTask">The <see cref="BasicInstallTask"/> to add to the view's list.</param>
		protected void AddTaskToList(IBackgroundTaskSet p_tskTask)
		{
            foreach (ActivateModsListViewItem lviExisitingDownload in lvwActiveTasks.Items)
				if (lviExisitingDownload.Task == p_tskTask)
					return;

            if (m_btsRunningTask != null)
            {
                foreach (IBackgroundTaskSet iBk in QueuedTasks)
                {
                    if (((ModInstaller)iBk).ModFileName == ((ModInstaller)p_tskTask).ModFileName)
                        if (((ModInstaller)iBk).IsQueued)
                            booQueued = true;
                }

                if ((((ModInstaller)m_btsRunningTask).ModFileName == ((ModInstaller)p_tskTask).ModFileName) || booQueued)
                {
                    booQueued = false;
                    m_vmlViewModel.RemoveUselessTask(((ModInstaller)p_tskTask));
                    return;
                }
            }

			p_tskTask.TaskSetCompleted += new EventHandler<TaskSetCompletedEventArgs>(TaskSet_TaskSetCompleted);
			ActivateModsListViewItem lviDownload = new ActivateModsListViewItem(p_tskTask, this);
			lvwActiveTasks.Items.Add(lviDownload);

            if ((m_btsRunningTask == null) || (m_btsRunningTask.IsCompleted))
            {
                m_btsRunningTask = p_tskTask;
                ((ModInstaller)m_btsRunningTask).Install();
            }
            else
            {
                QueuedTasks.Add(p_tskTask);
            }
		}

		private void TaskSet_TaskSetCompleted(object sender, TaskSetCompletedEventArgs e)
		{
			m_btsRunningTask = null;
			if (QueuedTasks.Count > 0)
			{
				m_btsRunningTask = QueuedTasks.First();
				QueuedTasks.Remove(m_btsRunningTask);
				((ModInstaller)m_btsRunningTask).Install();
			}
			else
				if (EmptyQueue != null)
					EmptyQueue(this, new EventArgs());
			
		}
		
		private void lvwActiveTasks_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}


		/// <summary>
		/// Handles the <see cref="INotifyCollectionChanged.CollectionChanged"/> event of the view model's
		/// task list.
		/// </summary>
		/// <remarks>
		/// This updates the list of tasks to refelct changes to the monitored Download list.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="NotifyCollectionChangedEventArgs"/> describing the event arguments.</param>
		private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (lvwActiveTasks.InvokeRequired)
			{
				lvwActiveTasks.Invoke((MethodInvoker)(() => Tasks_CollectionChanged(sender, e)));
				return;
			}
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
					foreach (IBackgroundTaskSet tskAdded in e.NewItems)
							AddTaskToList(tskAdded);
					break;
				case NotifyCollectionChangedAction.Move:
					//TODO Download order matters (some tasks depend on others)
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (IBackgroundTaskSet tskRemoved in e.OldItems)
					{
						for (Int32 i = lvwActiveTasks.Items.Count - 1; i >= 0; i--)
						{
							if ((((ActivateModsListViewItem)lvwActiveTasks.Items[i]).Text == ((ModInstaller)tskRemoved).ModName) && ((((ModInstaller)tskRemoved).IsCompleted) || (((ModInstaller)tskRemoved).IsQueued)))
								lvwActiveTasks.Items.RemoveAt(i);
						}
						tskRemoved.TaskSetCompleted -= new EventHandler<TaskSetCompletedEventArgs>(TaskSet_TaskSetCompleted);
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					lvwActiveTasks.Items.Clear();
					break;
				default:
					throw new Exception("Unrecognized value for NotifyCollectionChangedAction.");
			}
			UpdateTitle();
		}

		/// <summary>
		/// Handles the <see cref="INotifyCollectionChanged.CollectionChanged"/> event of the view model's
		/// active task list.
		/// </summary>
		/// <remarks>
		/// This updates the control title.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="NotifyCollectionChangedEventArgs"/> describing the event arguments.</param>
		private void ActiveTasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.IsHandleCreated)
			{
				lock (ViewModel.ModRepository)
					
				if (lvwActiveTasks.InvokeRequired)
				{
					lvwActiveTasks.Invoke((Action)UpdateTitle);
				}
				else
				{
					UpdateTitle();
				}
			}
		}

		/// <summary>
		/// Updates the control's title to reflect the current state of activities.
		/// </summary>
		protected void UpdateTitle()
		{
			Int32 intActiveCount = 0;
			Int32 intTotalCount = 0;
			if ((ViewModel != null) && (ViewModel.Tasks != null))
			{
				intActiveCount = ViewModel.Tasks.Count;
				intTotalCount = ViewModel.Tasks.Count;
			}
			if (intTotalCount == intActiveCount)
				Text = String.Format(m_strTitleAllActive, intTotalCount);
			else
				Text = String.Format(m_strTitleSomeActive, intActiveCount, intTotalCount);
		}

		#endregion


		/// <summary>
		/// Handles the <see cref="ListView.SelectedIndexChanged"/> event of the Download list.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void lvwTasks_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool booCheckStatus = ViewModel.CheckTaskStatus(lvwActiveTasks.FocusedItem.Text);
			SetCommandExecutableStatus(booCheckStatus);
		}

		/// <summary>
		/// Handles the cmsContextMenu.ReadmeScan event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="System.EventArgs"/> describing the event arguments.</param>
		private void cmsContextMenu_Copy(object sender, EventArgs e)
		{
			Clipboard.SetText(lvwActiveTasks.FocusedItem.Text);
		}

		#region Column Resizing

		/// <summary>
		/// Handles the <see cref="Timer.Tick"/> event of the column sizer timer.
		/// </summary>
		/// <remarks>
		/// We use a timer to autosize the columns in the list view. This is because
		/// there is a bug in the control such that if we reszize the columns continuously
		/// while the list view is being resized, the item will sometimes disappear.
		/// 
		/// To work around this, the list view resize event continually resets the timer.
		/// This means the timer will only fire occasionally during the resize, and avoid
		/// the disappearing items issue.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void ColumnSizer_Tick(object sender, EventArgs e)
		{
			((Timer)sender).Stop();
			SizeColumnsToFit();
		}

		/// <summary>
		/// This resizes the columns to fill the list view.
		/// </summary>
		protected void SizeColumnsToFit()
		{
			if (lvwActiveTasks.Columns.Count == 0)
				return;
			m_booResizing = true;
			Int32 intFixedWidth = 0;
			for (Int32 i = 0; i < lvwActiveTasks.Columns.Count; i++)
				if (lvwActiveTasks.Columns[i] != clmOverallMessage)
					intFixedWidth += lvwActiveTasks.Columns[i].Width;

			clmOverallMessage.Width = lvwActiveTasks.ClientSize.Width - intFixedWidth;
			m_booResizing = false;
		}

		/// <summary>
		/// Handles the <see cref="Control.Resize"/> event of the plugin list.
		/// </summary>
		/// <remarks>
		/// This resizes the columns to fill the list view.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">An <see cref="EventArgs"/> describing the event arguments.</param>
		private void lvwTasks_Resize(object sender, EventArgs e)
		{
			if (m_booResizing)
				return;
			m_tmrColumnSizer.Stop();
			m_tmrColumnSizer.Start();
		}

		/// <summary>
		/// Handles the <see cref="ListView.ColumnWidthChanging"/> event of the plugin list.
		/// </summary>
		/// <remarks>
		/// This resizes the column next to the column being resized to resize as well,
		/// so that the columns keep the list view filled.
		/// </remarks>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">A <see cref="ColumnWidthChangingEventArgs"/> describing the event arguments.</param>
		private void lvwTasks_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
		{
			if (m_booResizing)
				return;
			ColumnHeader clmThis = lvwActiveTasks.Columns[e.ColumnIndex];
			ColumnHeader clmOther = null;
			if (e.ColumnIndex == lvwActiveTasks.Columns.Count - 1)
				clmOther = lvwActiveTasks.Columns[e.ColumnIndex - 1];
			else
				clmOther = lvwActiveTasks.Columns[e.ColumnIndex + 1];
			m_booResizing = true;
			clmOther.Width += (clmThis.Width - e.NewWidth);
			m_booResizing = false;
		}

		#endregion
	}
}
