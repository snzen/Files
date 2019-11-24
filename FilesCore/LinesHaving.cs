using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utils.Files;

namespace Utils.Files
{
	public class LinesHaving : IUtil
	{
		public string Name => "lhaving";
		public string Info => "Takes matching lines from a text file and saves them to another." + Environment.NewLine +
			"Args: not interactive (-ni), text file (-tf), search text (-text) output file (-out)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var text = string.Empty;
			var allText = string.Empty;
			var tf = string.Empty;
			var outfile = string.Empty;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				Utils.ReadString("Search text: ", ref text);
				allText = File.ReadAllText(tf);
				Utils.ReadString("Output file: ", ref outfile, true);
			}
			else
			{
				if (ra.InArgs.ContainsKey("-tf"))
					allText = File.ReadAllText(ra.InArgs.GetFirstValue("-tf"));
				else throw new ArgumentNullException("-tf");

				if (ra.InArgs.ContainsKey("-text")) text = ra.InArgs.GetFirstValue("-text");
				else throw new ArgumentNullException("-text");

				if (ra.InArgs.ContainsKey("-out")) outfile = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");
			}

			var lines = allText.Split(Environment.NewLine);
			var outLines = new StringBuilder();
			var counter = 0;

			foreach (var line in lines)
				if (line.Length > 0 && line.Contains(text))
				{
					outLines.AppendLine(line);
					counter++;
				}

			File.WriteAllText(outfile, outLines.ToString());
			$"Done, {counter} matching lines.".PrintLine();

			return 0;
		}
	}
}
