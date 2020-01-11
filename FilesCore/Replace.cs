using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Utils.Files
{
	public class Replace : IUtil
	{
		public string Name => "replace";
		public string Info => "Replaces the matching text with a given string in all/some files in a folder. " + Environment.NewLine +
			"Args: interactive (-ni), source dir [default is current] (-src), search pattern [*.*] (-sp), " +
			"target regex (-reg), text to replace (-txt), ignore path regex [optional, Ex: (?:path\\A|path\\B)] (-ireg), " +
			"recursive [y/*] (-r)" + Environment.NewLine +
			@"Example converting CRLF line endings to LF: files -p replace -ni -src <dir> -reg (?:\r\n) -text \n" + Environment.NewLine +
			"Example normal text replacement: files -p replace -ni -src <dir> -reg \"text with spaces\" -text \"new text\"";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var text = string.Empty;
			var tf = string.Empty;
			var recursive = SearchOption.TopDirectoryOnly;
			var ignoreRegx = string.Empty;
			var targetRegx = string.Empty;

			if (interactive)
			{
				var src = string.Empty;
				Utils.ReadString("Source folder: ", ref src);
				if (!string.IsNullOrEmpty(src)) ra.RootDir = new DirectoryInfo(src);
				Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("Target regex: ", ref targetRegx);
				Utils.ReadString("Ignore regex: ", ref ignoreRegx);
				Utils.ReadString("Text: ", ref text);
				if (Utils.ReadWord("Recursive? (y/*): ", "y")) recursive = SearchOption.AllDirectories;
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) ra.RootDir = new DirectoryInfo(ra.InArgs.GetFirstValue("-src"));
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-reg")) targetRegx = ra.InArgs.GetFirstValue("-reg");
				if (ra.InArgs.ContainsKey("-ireg")) ignoreRegx = ra.InArgs.GetFirstValue("-ireg");
				if (ra.InArgs.ContainsKey("-text")) text = ra.InArgs.GetFirstValue("-text");
				if (ra.InArgs.ContainsKey("-r") && ra.InArgs.GetFirstValue("-r") == "y")
					recursive = SearchOption.AllDirectories;
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
					$"{Environment.NewLine}Files:".PrintLine();
					foreach (var f in F) $"{f.FullName}".PrintLine(ConsoleColor.DarkYellow);
					$"{Environment.NewLine}Ignored files:".PrintLine();
					foreach (var f in I) $"{f.FullName}".PrintLine(ConsoleColor.DarkRed);
				}

				"".PrintLine();

				if (interactive && !Utils.ReadWord("Edit? (y/*): ", "y")) return -1;

				"".PrintLine();

				var c = 0;
				text = Regex.Unescape(text);

				foreach (var f in F)
					try
					{
						var allBytes = File.ReadAllBytes(f.FullName);
						var allText = Encoding.UTF8.GetString(allBytes);

						if (allText != null && allText.Length > 0)
						{
							var r = Regex.Replace(allText, targetRegx, text);
							if (r != null)
							{
								allBytes = Encoding.UTF8.GetBytes(r);
								File.WriteAllBytes(f.FullName, allBytes);
							}
						}

						var pl = string.Format("{0, 6}: {1}", ++c, f.FullName);
						pl.PrintLine(ConsoleColor.Green);
					}
					catch (Exception ex)
					{
						$"{f.FullName} ex: {ex.Message}".PrintSysError();
					}
			}

			$"{Environment.NewLine}Done. {Environment.NewLine}".PrintLine(ConsoleColor.Yellow);

			return 0;
		}
	}
}
