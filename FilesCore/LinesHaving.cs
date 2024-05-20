using System;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class LinesHaving : IUtil
	{
		public string Name => "lhaving";
		public string Info => "Takes matching lines from a text file and saves them to another." + Environment.NewLine +
			"Args: not interactive (-ni), text file (-tf), search text (-text) output file (-out) except [takes all but the matched lines] (-x), append to output (-a)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var text = string.Empty;
			var allText = string.Empty;
			var tf = string.Empty;
			var outfile = string.Empty;
			var except = false;
			var append = 0;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				Utils.ReadString("Search text: ", ref text);
				allText = File.ReadAllText(tf);
				Utils.ReadString("Output file: ", ref outfile, true);

				var x = string.Empty;
				Utils.ReadString("Take all but the matched lines [default is no] (y/*): ", ref x);
				except = x == "y";
				Utils.ReadInt("Append to output: ", ref append, true);
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

				if (ra.InArgs.ContainsKey("-x")) except = true;
				if (ra.InArgs.ContainsKey("-a")) append = 1;
			}

			var lines = allText.Split(Environment.NewLine);
			var outLines = new StringBuilder();
			var counter = 0;

			foreach (var line in lines)
				if (line.Length > 0)
				{
					if (line.Contains(text))
					{
						if (!except)
						{
							outLines.AppendLine(line);
							counter++;
						}
					}
					else if (except)
					{
						outLines.AppendLine(line);
						counter++;
					}
				}

			if (append < 1)
				File.WriteAllText(outfile, outLines.ToString().Trim());
			else 
				File.AppendAllText(outfile, outLines.ToString().Trim());
			$"Done, {counter} matching lines.".PrintLine();

			return 0;
		}
	}
}
