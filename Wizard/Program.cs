using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Patcher;

namespace Wizard
{
	static class Program
	{
		static void TryCopy(string src, string dest)
		{
			if (!File.Exists(dest)) {
				File.Copy(src, dest);
			}
		}

		static IEnumerable<string> RelativeEnumerateFiles(string path)
		{
			return Directory.EnumerateFiles(path).Select(file => file.Substring(path.Length + 1));
		}

		[STAThread]
		static void Main()
		{
			bool gui = Environment.GetCommandLineArgs().Count() == 1;
			try {
				Environment.CurrentDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

				string destPath;
				if (gui) {
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);

					destPath = Environment.GetEnvironmentVariable("ProgramFiles(x86)") + @"\Steam\SteamApps\common\TowerFall";
					while (!Directory.Exists(destPath))
						using (var dialog = new FolderBrowserDialog { Description = @"Please select the SteamApps\common\TowerFall directory." }) {
							if (dialog.ShowDialog() != DialogResult.OK)
								return;
							destPath = dialog.SelectedPath;
						}
				} else {
					destPath = Environment.GetCommandLineArgs()[1];
				}

				Directory.CreateDirectory(Path.Combine("Original", "Content", "Atlas"));
				foreach (string file in RelativeEnumerateFiles(Path.Combine(destPath))) {
					TryCopy(Path.Combine(destPath, file), Path.Combine(file));
				}
				TryCopy(Path.Combine(destPath, "TowerFall.exe"), Path.Combine("Original", "TowerFall.exe"));
				File.Copy(Path.Combine("Original", "TowerFall.exe"), "TowerFall.exe", overwrite: true);
				Patcher.Patcher.Patch("Mod.dll");

				foreach (string file in RelativeEnumerateFiles(Path.Combine(destPath, "Content", "Atlas"))) {
					TryCopy(Path.Combine(destPath, "Content", "Atlas", file), Path.Combine("Original", "Content", "Atlas", file));
				}
				Patcher.Patcher.PatchResources();

				File.Copy(Path.Combine("TowerFall.exe"), Path.Combine(destPath, "TowerFall.exe"), overwrite: true);
				File.Copy(Path.Combine("Mod.dll"), Path.Combine(destPath, "Mod.dll"), overwrite: true);
				foreach (string file in RelativeEnumerateFiles(Path.Combine(destPath, "Content", "Atlas"))) {
					if (File.Exists(Path.Combine("Content", "Atlas", file))) {
						File.Copy(Path.Combine("Content", "Atlas", file), Path.Combine(destPath, "Content", "Atlas", file), overwrite: true);
					}
				}

				if (gui) {
					MessageBox.Show("Success!");
				}
			} catch (Exception e) {
				if (gui) {
					MessageBox.Show("Error:" + Environment.NewLine + e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				} else {
					throw;
				}
			}
		}
	}
}
