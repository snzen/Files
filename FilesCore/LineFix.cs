using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utils.Files;

namespace Utils.Files
{
	public class LineFix : IUtil
	{
		public string Name => "linefix";
		public string Info => "Includes text at the beginning or at the end of each line in a text file" + Environment.NewLine +
			"Args: not interactive (-ni), text file (-tf), append [by default prepends] (-append), text (-text) " +
			"output file (-out)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var end = false;
			var text = string.Empty;
			var tf = string.Empty;
			var allText = string.Empty;
			var outfile = string.Empty;
			
			if (interactive)
			{
				Utils.ReadString("Text to include: ", ref text);
				Utils.ReadString("File: ", ref tf, true);

				allText = File.ReadAllText(tf);

				Utils.ReadString("Output file: ", ref outfile, true);
				end = (Utils.ReadWord("Append? (y/*): ", "y"));
			}
			else
			{
				if (ra.InArgs.ContainsKey("-text")) text = ra.InArgs.GetFirstValue("-text");
				else throw new ArgumentNullException("-text");

				if (ra.InArgs.ContainsKey("-tf"))
					allText = File.ReadAllText(ra.InArgs.GetFirstValue("-tf"));
				else throw new ArgumentNullException("-tf");

				if (ra.InArgs.ContainsKey("-out")) outfile = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");

				end = ra.InArgs.ContainsKey("-append");
			}

			var lines = allText.Split(Environment.NewLine);
			var outLines = new StringBuilder();

			foreach (var line in lines)
			{
				if (line.Length < 1) continue;
				if (end) outLines.AppendLine(line + text);
				else outLines.AppendLine(text + line);
			}

			File.WriteAllText(outfile, outLines.ToString());

			Console.WriteLine("Done.");

			return 0;
		}
	}
}
