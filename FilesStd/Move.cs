using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class Move : IUtil
	{
		public string Name => "move";
		public string Info => "Moves the matching files from the current dir as DestinationDir/Prefix + Counter. Can be used as rename.";

		public void Run(RunArgs ra)
		{
			Utils.ReadString("destination dir: ", ref ra.State.DestinationDir, true);
			Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
			Utils.ReadString("prefix: ", ref ra.State.Prefix);
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
					"There are {0} files from a previous subprogram run. {1}Frst of the old files: {2} ",
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

			if (ra.Trace)
			{
				var tcounter = ra.State.NameCounter;
				foreach (var f in ra.State.Files)
				{
					if (f.FullName == ra.Me) continue;
					tcounter += ra.State.NameCounterStep;
					var padded = PadNumbers.PadZeroes(tcounter.ToString(), ra.State.PadZeroes);
					var newname = string.Format("{0}{1}{2}", ra.State.Prefix, padded, f.Extension);
					var newpath = Path.Combine(ra.State.DestinationDir, newname);
					string.Format(f.Name + " --> " + newpath).PrintLine(ConsoleColor.Yellow);
				}
			}

			if (Utils.ReadWord("Rename all? (y/*): ", "y"))
			{
				var FI = new List<FileInfo>();
				foreach (var f in ra.State.Files)
				{
					if (f.FullName == ra.Me) continue;
					ra.State.IncrementCounter();
					var padded = PadNumbers.PadZeroes(ra.State.NameCounter.ToString(), ra.State.PadZeroes);
					var newname = string.Format("{0}{1}{2}", ra.State.Prefix, padded, f.Extension);
					var newpath = Path.Combine(ra.State.DestinationDir, newname);
					File.Move(f.FullName, newpath);
					FI.Add(new FileInfo(newpath));
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
