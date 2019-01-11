using System;
using System.Collections.Generic;
using System.IO;

namespace Utils.Files
{
	public class Take : IUtil
	{
		public string Name => "take";
		public string Info =>
			"Copies or moves n random files from the current folder into a destination folder." + Environment.NewLine +
			"Args: not interactive (-ni), source dir [default is current] (-src), destination dir (-dest), " +
			"search pattern [*.*] (-sp), prefix (-prf), take count (-take) move [default is copy mode] (-move)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			var copy = true;

			if (interactive)
			{
				var src = string.Empty;
				Utils.ReadString("source dir: ", ref src);
				if (!string.IsNullOrEmpty(src)) ra.RootDir = new DirectoryInfo(src);
				Utils.ReadString("destination dir: ", ref ra.State.DestinationDir, true);
				Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("prefix: ", ref ra.State.Prefix);
				copy = !Utils.ReadWord("move? (y/*): ", "y");
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) ra.RootDir = new DirectoryInfo(ra.InArgs.GetFirstValue("-src"));
				ra.State.DestinationDir = ra.InArgs.GetFirstValue("-dest");
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-prf")) ra.State.Prefix = ra.InArgs.GetFirstValue("-prf");
				ra.State.Take = int.Parse(ra.InArgs.GetFirstValue("-take"));
				if (ra.InArgs.ContainsKey("-move")) copy = false;
			}

			if (!Directory.Exists(ra.State.DestinationDir))
				Directory.CreateDirectory(ra.State.DestinationDir);

			ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly);

			if (interactive) Utils.ReadInt(string.Format("There are {0} matches. take (0): ", ra.State.Files.Length), ref ra.State.Take);

			ra.Trace = interactive ? Utils.ReadWord("Trace first? (y/*): ", "y") : true;
			List<int> R = Utils.Randomize(ra.State.Files.Length);

			if (ra.State.Files.Length < ra.State.Take)
				ra.State.Take = ra.State.Files.Length;

			if (ra.Trace)
			{
				for (var i = 0; i < ra.State.Take; i++)
				{
					var j = R[i];
					var f = ra.State.Files[j];
					if (f.FullName == ra.Me) continue;
					var newname = string.Format("{0}{1}", ra.State.Prefix, f.Name);
					string.Format(f.Name + " --> " + Path.Combine(ra.State.DestinationDir, newname)).PrintLine(ConsoleColor.Yellow);
				}
			}

			if (!interactive || (interactive && Utils.ReadWord("Take all? (y/*): ", "y")))
			{
				var FI = new List<FileInfo>();
				for (var i = 0; i < ra.State.Take; i++)
				{
					var j = R[i];
					var f = ra.State.Files[j];
					if (f.FullName == ra.Me) continue;
					var newname = string.Format("{0}{1}", ra.State.Prefix, f.Name);
					var newpath = Path.Combine(ra.State.DestinationDir, newname);
					if (copy) File.Copy(f.FullName, newpath);
					else File.Move(f.FullName, newpath);
					FI.Add(new FileInfo(newpath));
				}
				ra.State.Files = FI.ToArray();
				Console.WriteLine("Done - {0} files copied.", ra.State.Take);
			}
			else Console.WriteLine("Aborting take.");

			return 0;
		}
	}
}
