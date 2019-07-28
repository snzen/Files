using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class Move : IUtil
	{
		public string Name => "move";
		public string Info =>
			"Moves the matching files to DestinationDir/Prefix + Counter. Can be used as rename." + Environment.NewLine +
			"Args: not interactive (-ni), source (-src), destination dir (-dest), prefix (-prf), indexing counter (-ic), ic step (-step)" +
			"Number length with zero padding (-zpad), Sort options: (-sort) no-0, asc name-1, desc name-2, randomize-3,  asc createdate-4, desc createdate-5.";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			int sort = 4;
			string src = null;
			ra.Trace = false;

			if (interactive)
			{
				Utils.ReadString("Source dir (current): ", ref src);
				Utils.ReadString("Destination dir: ", ref ra.State.DestinationDir, true);
				Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("Prefix: ", ref ra.State.Prefix);
				Utils.ReadInt("Counter (0): ", ref ra.State.NameCounter);
				Utils.ReadInt("Counter step (1): ", ref ra.State.NameCounterStep);
				Utils.ReadInt("Number length with zero padding (6): ", ref ra.State.PadZeroes);
				Console.WriteLine("Sort options: no-0, asc name-1, desc name-2, randomize-3,  asc createdate-4, desc createdate-5. ");
				Utils.ReadIntIn("Option (4): ", ref sort, new int[] { 0, 1, 2, 3, 4, 5 });
			}
			else
			{
				ra.ChangeRoot(ra.InArgs.GetFirstValue("-src"));
				ra.State.DestinationDir = ra.InArgs.GetFirstValue("-dest");
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-prf")) ra.State.Prefix = ra.InArgs.GetFirstValue("-prf");
				if (ra.InArgs.ContainsKey("-ic")) ra.State.NameCounter = int.Parse(ra.InArgs.GetFirstValue("-ic"));
				if (ra.InArgs.ContainsKey("-step")) ra.State.NameCounterStep = int.Parse(ra.InArgs.GetFirstValue("-step"));
				if (ra.InArgs.ContainsKey("-zpad")) ra.State.PadZeroes = int.Parse(ra.InArgs.GetFirstValue("-zpad"));
				if (ra.InArgs.ContainsKey("-sort")) ra.State.PadZeroes = int.Parse(ra.InArgs.GetFirstValue("-sort"));
			}

			ra.State.Sort = (SortType)sort;
			ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly);
			ra.State.SortFiles(ra.State.Sort);

			string.Format("There are {0} matches.", ra.State.Files.Length).PrintLine();

			if (interactive) ra.Trace = Utils.ReadWord("Trace first? (y/*): ", "y");

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

			if (!interactive || Utils.ReadWord("Rename all? (y/*): ", "y"))
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
			else Console.WriteLine("Aborting move.");

			return 0;
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
