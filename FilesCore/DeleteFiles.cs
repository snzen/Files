using System;
using System.IO;

namespace Utils.Files
{
	public class DeleteFiles : IUtil
	{
		public string Name => "fdelete";
		public string Info => "Deletes files listed in a text file. Each file path must be on a separate line." + Environment.NewLine +
			"Args: not interactive (-ni), map file (-tf), quiet (do not report missing) (-q)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var tf = string.Empty;
			var allText = string.Empty;
			var quiet = false;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				allText = File.ReadAllText(tf);
				var q = string.Empty;
				Utils.ReadWord("Quiet mode (y/*): ", q);
				if (q == "y") quiet = true;
			}
			else
			{
				if (ra.InArgs.ContainsKey("-tf"))
					allText = File.ReadAllText(ra.InArgs.GetFirstValue("-tf"));
				else throw new ArgumentNullException("-tf");

				if (ra.InArgs.ContainsKey("-q")) quiet = true;
			}

			var victims = allText.Split(Environment.NewLine);
			var counter = 0;

			foreach (var line in victims)
				if (line.Length > 0)
					if (File.Exists(line))
					{
						File.Delete(line);
						counter++;
					}
					else if (!quiet) Console.WriteLine($"File not found: {line}");

			Console.WriteLine($"Done, {counter} files were deleted.");

			return 0;
		}
	}
}
