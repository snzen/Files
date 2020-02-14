using System;
using System.IO;

namespace Utils.Files
{
	public class CopyFiles : IUtil
	{
		public string Name => "fcopy";
		public string Info => "Copies or moves files listed in a text file. Each file path must be on a separate line." + Environment.NewLine +
			"Args: not interactive (-ni), map file (-f), destination dir (-dst), move [default is copy] (-m), " +
			"quiet (do not report missing) (-q)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var tf = string.Empty;
			var dst = string.Empty;
			var allText = string.Empty;
			var quiet = false;
			var move = false;

			if (interactive)
			{
				Utils.ReadString("File: ", ref tf, true);
				allText = File.ReadAllText(tf);
				Utils.ReadString("Destination dir: ", ref dst, true);
				var q = string.Empty;
				var m = string.Empty;
				move = Utils.ReadWord("Move? The default mode is copy (y/*): ", "y");
				quiet = Utils.ReadWord("Quiet mode (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-f")) allText = File.ReadAllText(ra.InArgs.GetFirstValue("-f"));
				else throw new ArgumentNullException("-f");

				if (ra.InArgs.ContainsKey("-dst")) dst = ra.InArgs.GetFirstValue("-dst");
				else throw new ArgumentNullException("-dst");

				if (ra.InArgs.ContainsKey("-q")) quiet = true;
				if (ra.InArgs.ContainsKey("-m")) move = true;
			}

			if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);

			var victims = allText.Split(Environment.NewLine);
			var counter = 0;

			foreach (var line in victims)
				if (line.Length > 0)
					if (File.Exists(line))
					{
						var fn = Path.GetFileName(line);

						if (move) File.Move(line, Path.Combine(dst, fn));
						else File.Copy(line, Path.Combine(dst, fn));
						counter++;
					}
					else if (!quiet) $"File not found: {line}".PrintLine();

			$"Done, {counter} files were copied/moved.".PrintLine();

			return 0;
		}
	}
}
