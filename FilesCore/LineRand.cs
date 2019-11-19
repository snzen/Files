using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils.Files
{
	public class LineRand : IUtil
	{
		public string Name => "lrand";
		public string Info => "Randomizes the lines in a text file. Note that lrand will override the original file!" + Environment.NewLine +
			"Args: not interactive (-ni), text file (-tf) ";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var allText = string.Empty;
			var tf = string.Empty;

			if (interactive)
				Utils.ReadString("File: ", ref tf, true);
			else
			{
				if (ra.InArgs.ContainsKey("-tf")) tf = ra.InArgs.GetFirstValue("-tf");
				else throw new ArgumentNullException("-tf");
			}

			allText = File.ReadAllText(tf);
			var allLines = allText.Split(Environment.NewLine);
			var rdm = new Random();
			var len = allLines.Length;

			for (int i = 0; i < len; i++)
			{
				var idx = rdm.Next(0, len);
				var tmp = allLines[i];

				allLines[i] = allLines[idx];
				allLines[idx] = tmp;
			}

			File.Delete(tf);
			File.WriteAllLines(tf, allLines);

			Console.WriteLine("Done.");

			return 0;
		}
	}
}
