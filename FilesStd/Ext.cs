using System;
using System.IO;


namespace Utils.Files
{
	public class Ext : IUtil
	{
		public string Name => "ext";
		public string Info =>
			"Recursively changes the extensions of all matched files. " + Environment.NewLine +
			"Args: not interactive (-ni), source dir [default is current] (-src), " +
			"search pattern [*.*] (-sp), extension (-ext)";

		public void Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");

			if (interactive)
			{
				var src = string.Empty;
				Utils.ReadString("source dir: ", ref src);
				if (!string.IsNullOrEmpty(src)) ra.RootDir = new DirectoryInfo(src);
				Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);
				Utils.ReadString("ext: ", ref ra.State.Ext);
			}
			else
			{
				if (ra.InArgs.ContainsKey("-src")) ra.RootDir = new DirectoryInfo(ra.InArgs.GetFirstValue("-src"));
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
				ra.State.Ext = ra.InArgs.GetFirstValue("-ext");
			}

			ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.AllDirectories);

			string.Format("There are {0} matches.", ra.State.Files.Length).PrintLine();
			ra.Trace = interactive ? Utils.ReadWord("Trace first? (y/*): ", "y") : true;

			if (ra.Trace)
			{
				foreach (var f in ra.State.Files)
				{
					if (f.FullName == ra.Me) continue;
					var newpath = Path.ChangeExtension(f.FullName, ra.State.Ext);
					string.Format(f.FullName + " --> " + newpath).PrintLine(ConsoleColor.Yellow);
				}
			}

			if (!interactive || (interactive && Utils.ReadWord("Rename all? (y/*): ", "y")))
			{
				foreach (var f in ra.State.Files)
				{
					var newpath = Path.ChangeExtension(f.FullName, ra.State.Ext);
					File.Move(f.FullName, newpath);
				}
				Console.WriteLine("Done - {0} files renamed.", ra.State.Files.Length);
			}
			else Console.WriteLine("Aborting Rename. Press <Enter> to exit.");
		}
	}
}
