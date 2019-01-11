using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class InsertText : IUtil
	{
		public string Name => "insert";
		public string Info => "Inserts/overrides the beginning or the end of all matching files with custom content. " + Environment.NewLine +
			"Args: interactive (-ni), source dir [default is current] (-src), search pattern [*.*] (-sp), " +
			"ignore regex [optional, Ex: (?:path\\A|path\\B)] (-ireg), " +
			"recursive [y/*] (-r), text file [if not -txt] (-tf), text [if not -tf] (-txt)," +
			"append [by default prepends] (-append), override [by default inserts] (-ovr)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var end = false;
			var ovr = false;
			var text = string.Empty;
			var tf = string.Empty;
			var recursive = SearchOption.TopDirectoryOnly;
			var ignoreRegx = string.Empty;

			if (interactive)
			{
				var src = string.Empty;
				Utils.ReadString("Source folder: ", ref src);
				if (!string.IsNullOrEmpty(src)) ra.RootDir = new DirectoryInfo(src);
				Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("Ignore regex: ", ref ignoreRegx);
				Utils.ReadString("Text: ", ref text);
				if (string.IsNullOrWhiteSpace(text))
				{
					Utils.ReadString("Text file: ", ref tf, true);
					text = File.ReadAllText(tf);
				}
				if (Utils.ReadWord("Recursive? (y/*): ", "y")) recursive = SearchOption.AllDirectories;
				ovr = (Utils.ReadWord("Override? (y/*): ", "y"));
				end = (Utils.ReadWord("At the end of the file? (y/*): ", "y"));
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) ra.RootDir = new DirectoryInfo(ra.InArgs.GetFirstValue("-src"));
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-ireg")) ignoreRegx = ra.InArgs.GetFirstValue("-ireg");
				if (ra.InArgs.ContainsKey("-text")) text = ra.InArgs.GetFirstValue("-text");
				else
				{
					if (ra.InArgs.ContainsKey("-tf"))
						text = File.ReadAllText(ra.InArgs.GetFirstValue("-tf"));
					else throw new ArgumentNullException("-tf");
				}
				if (ra.InArgs.ContainsKey("-r") && ra.InArgs.GetFirstValue("-r") == "y")
					recursive = SearchOption.AllDirectories;
				end = ra.InArgs.ContainsKey("-append");
				ovr = ra.InArgs.ContainsKey("-ovr");
			}

			var F = new List<FileInfo>();
			var I = new List<FileInfo>();
			foreach (var file in ra.RootDir.EnumerateFiles(ra.State.SearchPattern, recursive))
			{
				if (!string.IsNullOrEmpty(ignoreRegx) && Regex.IsMatch(file.FullName, ignoreRegx)) I.Add(file);
				else F.Add(file);
			}

			$"There are {F.Count + I.Count} files, {I.Count} ignored by a regex match".PrintLine(ConsoleColor.Yellow);

			if (F.Count > 0)
			{
				if (interactive && Utils.ReadWord("Trace first? (y/*): ", "y"))
				{
					Console.WriteLine();
					"Files:".PrintLine();
					foreach (var f in F) $"{f.FullName}".PrintLine(ConsoleColor.DarkYellow);
					Console.WriteLine();
					"Ignored files:".PrintLine();
					foreach (var f in I) $"{f.FullName}".PrintLine(ConsoleColor.DarkRed);
				}

				Console.WriteLine();

				if (interactive && !Utils.ReadWord("Edit? (y/*): ", "y")) return -1;
				Console.WriteLine();

				var c = 0;

				foreach (var f in F)
					try
					{
						Utils.WriteToFile(f.FullName, text, ovr, end);
						var pl = string.Format("{0, 6}: {1}", ++c, f.FullName);
						pl.PrintLine(ConsoleColor.Green);
					}
					catch (Exception ex)
					{
						$"{f.FullName} ex: {ex.Message}".PrintSysError();
					}
			}

			Console.WriteLine();
			"Done".PrintLine(ConsoleColor.Yellow);
			Console.WriteLine();

			return 0;
		}
	}
}
