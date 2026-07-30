using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace LBA2Launcher {
	public partial class Form1 : Form {
		private const string SERVER = "https://cabfiel.tmxc.ru/server/";
		private string whichOne = "lba2.exe";
		private Version ver;
		private Ini config;
		private string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		public Form1() {
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {
			if(!(File.Exists("lba2.exe") || File.Exists("nw.exe")) || !File.Exists("package.nw")) {
				MessageBox.Show("Couldn't find the game!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			if(Directory.Exists(userProfile + "\\LittleBigAwoglet 2")) {
				button3.Visible = true;
				button3.Image = ExtractIcon(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\system32\\imageres.dll", 3);
			}
			if(File.Exists("nw.exe") && !File.Exists("lba2.exe")) whichOne = "nw.exe";
			if(File.Exists("AutoUpdate.exe")) button2.Visible = true;
			config = new Ini("Launcher.ini");
			if(config.GetValue("VSync", "Settings") == "false") checkBox1.Checked = false;
			if(config.GetValue("ForceCanvas2D", "Settings") == "true") checkBox2.Checked = true;
			if(config.GetValue("NeverSuspend", "Settings") == "true") checkBox3.Checked = true;
			if(config.GetValue("BackupManager", "Settings") == "true") checkBox4.Checked = true;
			var client = new WebClient();
			client.DownloadStringAsync(new Uri(SERVER + "misc/motd"));
			client.DownloadStringCompleted += Client_DownloadStringCompleted;
		}

		private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			label1.Text = e.Result;
		}

		private void button1_Click(object sender, EventArgs e) {
			var builder = new StringBuilder();
			if(!checkBox1.Checked) builder.Append("--disable-gpu-vsync --disable-frame-rate-limit ");
			if(checkBox2.Checked) builder.Append("--force-canvas2d ");
			if(checkBox3.Checked) builder.Append("--no-suspend ");
			if(checkBox4.Checked) builder.Append("--backup-manager ");
			config.WriteValue("VSync", "Settings", checkBox1.Checked ? "true" : "false");
			config.WriteValue("ForceCanvas2D", "Settings", checkBox2.Checked ? "true" : "false");
			config.WriteValue("NeverSuspend", "Settings", checkBox3.Checked ? "true" : "false");
			config.WriteValue("BackupManager", "Settings", checkBox4.Checked ? "true" : "false");
			config.Save();
			Process.Start(whichOne, builder.ToString());
			Close();
		}
		private static Version ReadVersion(string filePath) {
			using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				if(fs.Length < 3) throw new Exception("File too small");
				long bytesToRead = Math.Min(3, fs.Length);
				fs.Seek(fs.Length - bytesToRead, SeekOrigin.Begin);
				byte[] buffer = new byte[bytesToRead];
				fs.Read(buffer, 0, (int)bytesToRead);
				return new Version(buffer);
			}
		}

		private void button2_Click(object sender, EventArgs e) {
			Process.Start("AutoUpdate.exe");
		}

		private void Form1_Activated(object sender, EventArgs e) {
			bool success = true;
			try {
				ver = ReadVersion("package.nw");
			}
			catch { success = false; }
			checkBox2.Enabled = false;
			checkBox3.Enabled = false;
			if(success) {
				label2.Text = "v" + ver;
				if(ver.Major > 10 || (ver.Major == 10 && ver.Minor >= 7)) checkBox2.Enabled = true;
				if(ver.Major > 10 || (ver.Major == 10 && ver.Minor >= 8)) checkBox3.Enabled = true;
			}
			else label2.Text = "unknown version (PTB era?)";
		}

		private void button3_Click(object sender, EventArgs e) {
			Process.Start(userProfile + "\\LittleBigAwoglet 2");
		}

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern uint ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool DestroyIcon(IntPtr hIcon);
		private Bitmap ExtractIcon(string path, int index) {
			IntPtr[] largeIcons = new IntPtr[1];
			IntPtr[] smallIcons = new IntPtr[1];
			uint count = ExtractIconEx(path, index, largeIcons, smallIcons, 1);
			if(count > 0 && largeIcons[0] != IntPtr.Zero) {
				using(Icon icon = Icon.FromHandle(largeIcons[0])) {
					Bitmap bmp = icon.ToBitmap();
					DestroyIcon(largeIcons[0]);
					if(smallIcons[0] != IntPtr.Zero) DestroyIcon(smallIcons[0]);
					return bmp;
				}
			}
			if(smallIcons[0] != IntPtr.Zero)
				DestroyIcon(smallIcons[0]);
			return null;
		}

		private void Form1_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			MessageBox.Show("LBA2Launcher v1.0\nLauncher for the game LittleBigAwoglet 2", "About LBA2Launcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
