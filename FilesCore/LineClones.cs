using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class LineClones : IUtil
	{
		public string Name => "lclones";
		public string Info => "Removes line clones in a text file." +
			"If -paths parses each line as a path and removes duplicate filenames. " + Environment.NewLine +
			"Args: not interactive (-ni), source text file (-src), output text file (-dst), clones file (-clones), lines are paths (-paths)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var allText = string.Empty;
			var src = string.Empty;
			var dst = string.Empty;
			var clones = string.Empty;
			var linesArePaths = false;

			if (interactive)
			{
				Utils.ReadString("Source file: ", ref src, true);
				Utils.ReadString("Output file: ", ref dst, true);
				Utils.ReadString("Clones file: ", ref clones, false);
				linesArePaths = Utils.ReadWord("Lines are paths? [default is no] (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) src = ra.InArgs.GetFirstValue("-src");
				else throw new ArgumentNullException("-src");
				if (ra.InArgs.ContainsKey("-dst")) dst = ra.InArgs.GetFirstValue("-dst");
				else throw new ArgumentNullException("-dst");
				if (ra.InArgs.ContainsKey("-paths")) linesArePaths = true;
				if (ra.InArgs.ContainsKey("-clones")) clones = ra.InArgs.GetFirstValue("-clones");
			}

			allText = File.ReadAllText(src);
			var allLines = allText.Split(Environment.NewLine);
			var linesMap = new Dictionary<string, string>();
			var clonesList = new List<string>();

			for (var i = 0; i < allLines.Length; i++)
			{
				var line = allLines[i];

				if (linesArePaths) line = Path.GetFileName(line);
				if (!linesMap.ContainsKey(line)) linesMap.Add(line, allLines[i]);
				else clonesList.Add(allLines[i]);
			}

			var sb = new StringBuilder();

			foreach (var line in linesMap.Values)
				sb.AppendLine(line);

			File.WriteAllText(dst, sb.ToString());

			if (!string.IsNullOrWhiteSpace(clones))
			{
				clonesList.Sort();
				
				sb.Clear();
				foreach (var line in clonesList)
					sb.AppendLine(line);

				File.WriteAllText(clones, sb.ToString());
			}

			"Done.".PrintLine();
			$"{clonesList.Count} clones found".PrintLine();

			return 0;
		}
	}
}
