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

		public int Run(RunArgs ra)
		{
			"The Log can:".PrintLine();
			" (log) save the file names in the current dir".PrintLine();
			" (restore) move the files from a log file to a destination dir".PrintLine();

			var mode = "";
			while (mode != "log" && mode != "restore")
				Utils.ReadString("mode (log) or (restore): ", ref mode, true);

			var newSrc = string.Empty;
			Utils.ReadString("Source dir [default is local]: ", ref newSrc);

			if (!string.IsNullOrEmpty(newSrc)) ra.ChangeRoot(newSrc);

			Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);

			if (mode == "log")
			{
				var currentDir = Utils.ReadWord("Recursive? [default is yes] (n/*): ", "n");
				var justNames = Utils.ReadWord("Log full paths? [default is yes] (n/*): ", "n");

				var F = new List<string>();
				foreach (var f in ra.RootDir.GetFiles(ra.State.SearchPattern,
					currentDir ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories))
					F.Add(justNames ? f.Name : f.FullName);

				string.Format("Found {0} matching files.", F.Count).PrintLine();
				if (F.Count > 0)
				{
					if (Utils.ReadWord("See them? (y/*): ", "y"))
						foreach (var f in F)
							f.PrintLine(ConsoleColor.Yellow);

					"Save as (file path): ".Print();
					var p = Path.GetFullPath(Console.ReadLine());
					File.WriteAllLines(p, F.ToArray());
					string.Format("Name file saved as {0}", p).PrintLine();
				}

				"Done.".PrintLine();
			}
			else
			{
				var logfilePath = string.Empty;
				Utils.ReadString("Log file path: ", ref logfilePath, true);
				var paths = File.ReadAllLines(logfilePath);
				var recursive = Utils.ReadWord("Recursive search? (y/*): ", "y");
				var copy = Utils.ReadWord("Copy? [default is move] (y/*): ", "y");
				var so = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
				var allFiles = Directory.GetFiles(ra.RootDir.FullName, ra.State.SearchPattern, so);

				var origMap = new Dictionary<string, string>();
				var localMap = new Dictionary<string, string>();

				foreach (var path in paths)
				{
					var jn = Path.GetFileName(path);

					if (!origMap.ContainsKey(jn))
						origMap.Add(jn, path);
					else $"Log file name duplicate {jn}".Print(ConsoleColor.Yellow, null, true);
				}

				foreach (var path in allFiles)
				{
					var jn = Path.GetFileName(path);

					if (!localMap.ContainsKey(jn))
						localMap.Add(jn, path);
					else $"Local file name duplicate {jn}".Print(ConsoleColor.Yellow, null, true);
				}

				var list = new List<string>(localMap.Keys.Intersect(origMap.Keys));

				$"{list.Count} files match by name.".PrintLine();

				if (list.Count > 0)
				{
					if (Utils.ReadWord("See them? (y/*): ", "y"))
						foreach (var fn in list)
							$"{localMap[fn]} ->  {origMap[fn]}".PrintLine();

					if (Utils.ReadWord("Restore? (y/*): ", "y"))
						foreach (var fn in list)
						{
							if (copy) File.Copy(localMap[fn], origMap[fn], true);
							else File.Move(localMap[fn], origMap[fn], true);
						}
					else "Aborting".PrintLine();
				}

				"Done.".PrintLine();
			}

			return 0;
		}
	}
}
