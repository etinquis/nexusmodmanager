﻿using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Nexus.Client.Games;
using System.IO;
using Nexus.Client.Util;
using Nexus.Client.BackgroundTasks;

namespace Nexus.Client
{
	public partial class GameModeListViewItem : UserControl
	{
		[DllImport("gdi32.dll")]
		private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
		
		private PrivateFontCollection pfcFonts = new PrivateFontCollection();
		private IGameModeDescriptor m_gmdGameMode = null;
		private GameDiscoverer m_gdtDetector = null;

		public GameModeListViewItem(IGameModeDescriptor p_gmdGameMode, GameDiscoverer p_gdtDetector)
		{
			m_gmdGameMode = p_gmdGameMode;
			m_gdtDetector = p_gdtDetector;
			InitializeComponent();
			SetVisiblePanel(pnlSearching);
			p_gdtDetector.PropertyChanged += new PropertyChangedEventHandler(Detector_PropertyChanged);
			p_gdtDetector.PathFound += new EventHandler<GameModeDiscoveredEventArgs>(Detector_PathFound);
			
			IntPtr pbyt = IntPtr.Zero;
			try
			{
				byte[][] rgbyt = new byte[2][];
				rgbyt[0] = Properties.Resources.LinBiolinum_RI;
				rgbyt[1] = Properties.Resources.LinBiolinum_RB;
				for (Int32 i = 0; i <= rgbyt.GetUpperBound(0); i++)
				{
					pbyt = Marshal.AllocCoTaskMem(rgbyt[i].Length);
					Marshal.Copy(rgbyt[i], 0, pbyt, rgbyt[i].Length);
					pfcFonts.AddMemoryFont(pbyt, rgbyt[i].Length);
					uint dummy = 0;
					AddFontMemResourceEx(pbyt, (uint)rgbyt[i].Length, IntPtr.Zero, ref dummy);
				}
			}
			finally
			{
				Marshal.FreeCoTaskMem(pbyt);
			}
			//these lines are an alternate method to load the font, but require
			// the fonts to be files, instead of embedded resources
			//pfcFonts.AddFontFile(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"data\fonts\LinBiolinum_RI.ttf"));
			//pfcFonts.AddFontFile(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"data\fonts\LinBiolinum_RB.ttf"));

			
			lblNotFoundTitle.Font = new Font(pfcFonts.Families[0], lblNotFoundTitle.Font.Size, lblNotFoundTitle.Font.Style, lblNotFoundTitle.Font.Unit);
			lblFoundTitle.Font = new Font(pfcFonts.Families[0], lblFoundTitle.Font.Size, lblFoundTitle.Font.Style, lblFoundTitle.Font.Unit);

			lblGameModeName.Text = p_gmdGameMode.Name;
			lblGameModeName.ForeColor = p_gmdGameMode.ModeTheme.PrimaryColour;

			pbxGameLogo.Image = new Icon(p_gmdGameMode.ModeTheme.Icon, 96, 96).ToBitmap();
		}

		void Detector_PathFound(object sender, GameModeDiscoveredEventArgs e)
		{
			if (!e.GameMode.ModeId.Equals(m_gmdGameMode.ModeId))
			{
				lblPath.Text = e.InstallationPath;
				SetVisiblePanel(pnlCandidate);
			}
		}

		private void SetVisiblePanel(Panel p_pnlPanel)
		{
			pnlCandidate.Visible = (p_pnlPanel == pnlCandidate);
			pnlNotFound.Visible = (p_pnlPanel == pnlNotFound);
			pnlSearching.Visible = (p_pnlPanel == pnlSearching);
		}

		private void Detector_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (lblProgressMessage.InvokeRequired)
				lblProgressMessage.Invoke((Action<object, PropertyChangedEventArgs>)Detector_PropertyChanged, sender, e);
			else if (e.PropertyName.Equals(ObjectHelper.GetPropertyName<IBackgroundTask>(x => x.OverallMessage)))
				lblProgressMessage.Text = m_gdtDetector.OverallMessage;
		}

		private void butSelectPath_Click(object sender, EventArgs e)
		{
			fbdSelectPath.SelectedPath = tbxInstallPath.Text;
			if (fbdSelectPath.ShowDialog(this) == DialogResult.OK)
				tbxInstallPath.Text = fbdSelectPath.SelectedPath;
		}

		private void butAccept_Click(object sender, EventArgs e)
		{
			m_gdtDetector.Accept(m_gmdGameMode.ModeId);
		}

		private void butReject_Click(object sender, EventArgs e)
		{
			m_gdtDetector.Reject(m_gmdGameMode.ModeId);
		}
	}
}
