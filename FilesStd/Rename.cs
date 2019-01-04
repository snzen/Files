using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class Rename : IUtil
	{
		public string Name => "rename";
		public string Info => "Renames the matching files from the current folder with the names from a text file.";

		public void Run(RunArgs ra)
		{
			Utils.ReadString("the path of the text file with the new file names: ", ref ra.State.FileNamesFilePath, true);
			Utils.ReadString("destination dir: ", ref ra.State.DestinationDir, true);
			Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
			Utils.ReadString("prefix: ", ref ra.State.Prefix);
			Utils.ReadString("name from file prefix: ", ref ra.State.NameFromFilePrefix);
			Utils.ReadInt("counter (0) : ", ref ra.State.NameCounter);
			Utils.ReadInt("counter step (1) : ", ref ra.State.NameCounterStep);
			Utils.ReadInt("number length with zero padding (6) : ", ref ra.State.PadZeroes);
			Console.WriteLine("sort options: no-0, asc name-1, desc name-2, randomize-3,  asc createdate-4, desc createdate-5. ");
			int sort = 0;
			Utils.ReadIntIn("sort first (no) : ", ref sort, new int[] { 0, 1, 2, 3, 4, 5 });
			ra.State.Sort = (SortType)sort;

			var useexisting = false;
			if (ra.State.Files != null && ra.State.Files.Length > 0)
			{
				string.Format(
					"There are {0} files from a previous subprogram run. {1}The first of the old files: {2} ",
					ra.State.Files.Length, Environment.NewLine, ra.State.Files[0]).PrintLine();
				useexisting = Utils.ReadWord("Use them? (y/*) ", "y");
			}

			if (!useexisting) ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly);
			else if (Utils.ReadWord(string.Format("Change the root dir ({0}) ? (y/*)", ra.RootDir.FullName), "y"))
			{
				var newroot = string.Empty;
				Utils.ReadString("Enter new root dir: ", ref newroot, true);
				ra.ChangeRoot(newroot);
			}

			ra.State.SortFiles(ra.State.Sort);

			string.Format("There are {0} matches.", ra.State.Files.Length).PrintLine();
			ra.Trace = Utils.ReadWord("Trace first? (y/*): ", "y");

			var newNames = File.ReadAllLines(ra.State.FileNamesFilePath);

			if (newNames.Length < ra.State.Files.Length)
			{
				var msg = string.Format(
					"The new names length is {0} and the files to be renamed are {1}. Abort? (y/*)",
					newNames.Length,
					ra.State.Files.Length);
				if (Utils.ReadWord(msg, "y")) return;
			}

			string.Format("Files - {0}, Names - {1}", newNames.Length, ra.State.Files.Length).PrintLine(ConsoleColor.Yellow);

			if (ra.Trace)
			{
				var tcounter = ra.State.NameCounter;
				var i = 0;
				foreach (var f in ra.State.Files)
				{
					if (f.FullName == ra.Me) continue;
					tcounter += ra.State.NameCounterStep;
					var padded = PadNumbers.PadZeroes(tcounter.ToString(), ra.State.PadZeroes);
					var newname = string.Format("{0}{1}{2}{3}{4}", ra.State.Prefix, padded, ra.State.NameFromFilePrefix, newNames[i], f.Extension);
					var newpath = Path.Combine(ra.State.DestinationDir, newname);
					string.Format(f.Name + " --> " + newpath).PrintLine(ConsoleColor.Yellow);
					i++;
				}
			}

			if (Utils.ReadWord("Rename all? (y/*): ", "y"))
			{
				var FI = new List<FileInfo>();
				var i = 0;
				foreach (var f in ra.State.Files)
				{
					if (f.FullName == ra.Me) continue;
					ra.State.IncrementCounter();
					var padded = PadNumbers.PadZeroes(ra.State.NameCounter.ToString(), ra.State.PadZeroes);
					var newname = string.Format("{0}{1}{2}{3}{4}", ra.State.Prefix, padded, ra.State.NameFromFilePrefix, newNames[i], f.Extension);
					var newpath = Path.Combine(ra.State.DestinationDir, newname);
					File.Move(f.FullName, newpath);
					FI.Add(new FileInfo(newpath));
					i++;
				}
				ra.State.Files = FI.ToArray();
				Console.WriteLine("Done - {0} files renamed.", ra.State.Files.Length);
			}
			else Console.WriteLine("Aborting Rename. Press <Enter> to exit.");
		}

		static void PadZeroes(ref string filename, RunArgs ra)
		{
			Match m = Regex.Match(filename, @"\d+");
			if (m.Success)
			{
				var padded = m.Value.PadLeft(ra.State.PadZeroes, '0');
				filename = filename.Replace(m.Value, padded);
			}
		}
	}
}
