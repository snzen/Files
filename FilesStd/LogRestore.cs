using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.Files
{
	public class LogRestore : IUtil
	{
		public string Name => "logrestore";
		public string Info => "Creates a file with paths (log), which can be used to move files (restore).";

		public void Run(RunArgs ra)
		{
			"The Log can:".PrintLine();
			" (log) save the file names in the current dir".PrintLine();
			" (restore) move the files from a log file to a destination dir".PrintLine();

			var mode = "";
			while (mode != "log" && mode != "restore")
				Utils.ReadString("mode (log) or (restore): ", ref mode, true);
			Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);

			if (mode == "log")
			{
				var F = new List<string>();
				foreach (var f in ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
					F.Add(f.Name);

				string.Format("The local dir has {0} matching files.", F.Count).PrintLine();
				if (F.Count > 0)
				{
					if (Utils.ReadWord("See them? (y/*)", "y"))
						foreach (var f in F)
							f.PrintLine(ConsoleColor.Yellow);

					"Save as (file path): ".Print();
					var p = Path.GetFullPath(Console.ReadLine());
					File.WriteAllLines(p, F.ToArray());
					string.Format("Name file saved as {0}", p).PrintLine();
				}
			}
			else
			{
				Utils.ReadString("the restore dir: ", ref ra.State.DestinationDir, true);
				"Name file path: ".Print();
				var nameFilePath = Path.GetFullPath(Console.ReadLine());
				var names = File.ReadAllLines(nameFilePath);

				if (names == null || names.Length < 1)
				{
					"No names found.".PrintLine();
					return;
				}
				else
				{
					var localNames = new HashSet<string>();

					foreach (var f in ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
						localNames.Add(f.Name);

					string.Format(
						"{0} names in the log file, {1} in the local dir.",
							names.Length,
							localNames.Count)
						.PrintLine();

					var matchings = new List<string>(localNames.Intersect(names));

					if (matchings.Count > 0)
					{
						string.Format("The local dir has {0} matching files.", matchings.Count).PrintLine();
						if (Utils.ReadWord("See the  them? (y/*)", "y"))
							foreach (var f in matchings)
								f.PrintLine(ConsoleColor.Yellow);

						var q = string.Format(
							"Restore the local dir matching files ({0}) to {1}? (y/*)",
							matchings.Count,
							ra.State.DestinationDir);

						if (Utils.ReadWord(q, "y"))
						{
							var existing = new List<string>();
							foreach (var f in matchings)
							{
								var lf = Path.Combine(ra.RootDir.FullName, f);
								var rf = Path.Combine(ra.State.DestinationDir, f);
								if (!File.Exists(rf)) File.Move(lf, rf);
								else existing.Add(f);
							}
							string.Format(
								"{0} files were moved, {1} were skipped as existing.",
									matchings.Count - existing.Count,
									existing.Count)
								.PrintLine();

							if (existing.Count > 0 && Utils.ReadWord("See the skipped names? (y/*)", "y"))
								foreach (var f in existing)
									f.PrintLine(ConsoleColor.Yellow);
						}
						else "Aborting.".PrintLine();
					}
					else string.Format(
							"There are no local files matching the names in {0}.", nameFilePath)
							.PrintLine();
				}
			}
		}
	}
}
