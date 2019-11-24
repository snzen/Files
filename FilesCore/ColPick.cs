using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class CSVPick : IUtil
	{
		public string Name => "colpick";
		public string Info => "Picks columns by index from a file and copies them into a new file." + Environment.NewLine +
			"Can also be used to rearrange the columns in a csv file." + Environment.NewLine +
			"Args: not interactive (-ni), file (-in), output file (-out), separator (-sep), columns (-cols) [last arg] ex: -cols 1, 4 ";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var inf = string.Empty;
			var outf = string.Empty;
			var colsText = string.Empty;
			var allText = string.Empty;
			var sep = COMMA;
			var cols = new List<int>();
			string[] C = null;

			if (interactive)
			{
				Utils.ReadString("File: ", ref inf, true);
				Utils.ReadString("Out file: ", ref outf, true);
				Utils.ReadString("Column indices: ", ref colsText, true);
				Utils.ReadString($"Separator ({COMMA}): ", ref sep);

				allText = File.ReadAllText(inf);
				C = colsText.Split(COMMA);
			}
			else
			{
				if (ra.InArgs.ContainsKey("-in"))
					allText = File.ReadAllText(ra.InArgs.GetFirstValue("-in"));
				else throw new ArgumentNullException("-in");

				if (ra.InArgs.ContainsKey("-out"))
					outf = ra.InArgs.GetFirstValue("-out");
				else throw new ArgumentNullException("-out");

				if (ra.InArgs.ContainsKey("-cols"))
				{
					var x = ra.InArgs["-cols"];
					C = x.Count < 2 ? x[0].Split(COMMA) : x.ToArray();
				}
				else throw new ArgumentNullException("-cols");

				if (ra.InArgs.ContainsKey("-sep")) sep = ra.InArgs.GetFirstValue("-sep");
			}

			foreach (var c in C)
				if (!string.IsNullOrWhiteSpace(c))
				{
					var cc = c.TrimEnd(',');
					cols.Add(int.Parse(cc));
				}

			var L = allText.Split(Environment.NewLine);
			var outsb = new StringBuilder();
			var counter = 0;

			foreach (var line in L)
				if (line.Length > 0)
				{
					var S = line.Split(sep);
					outsb.AppendLine();

					for (int i = 0; i < cols.Count; i++)
						if (cols[i] < S.Length)
						{
							outsb.Append(S[cols[i]]);
							if (i < cols.Count - 1) outsb.Append(sep);
						}

					counter++;
				}

			File.WriteAllText(outf, outsb.ToString());
			$"Done, {counter}/{L.Length} lines were copied.".PrintLine();

			return 0;
		}

		public const string COMMA = ",";
	}
}
