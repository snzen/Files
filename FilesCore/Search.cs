using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Utils.Files
{
	public class Search : IUtil
	{
		public string Name => "search";
		public string Info =>
			"Search for files recursively." + Environment.NewLine +
			"Args: not interactive (-ni), source (-src), search pattern (-sp)";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			string src = ra.RootDir.FullName;

			if (interactive)
			{
				Utils.ReadString("Source dir (current): ", ref src);
				Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
			}
			else
			{
				ra.ChangeRoot(ra.InArgs.GetFirstValue("-src"));
				if (ra.InArgs.ContainsKey("-sp")) ra.State.SearchPattern = ra.InArgs.GetFirstValue("-sp");
			}

			var d = new DirectoryInfo(src);
			var All = new List<List<FileInfo>>();
			var searchOpt = SearchOption.AllDirectories;

			try
			{
				foreach (var sd in d.EnumerateDirectories("*", searchOpt).Union(new DirectoryInfo[] { d }))
					try
					{
						var L = new List<FileInfo>();

						foreach (var file in sd.EnumerateFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
							L.Add(file);

						All.Add(L);
					}
					catch (Exception ex)
					{
						ex.Message.PrintLine(ConsoleColor.Red);
					}
			}
			catch (Exception ex)
			{
				ex.Message.PrintLine(ConsoleColor.Red);
			}

			string.Format("There are {0} matches:", All.Sum(x => x.Count)).PrintLine();

			foreach (var FI in All)
				foreach (var f in FI)
				{
					if (f.FullName == ra.Me) continue;

					Console.WriteLine(f.FullName);
				}

			Console.WriteLine("Done");

			return 0;
		}

		void search(RunArgs ra, DirectoryInfo d)
		{
			var All = new List<List<FileInfo>>();
			var searchOpt = SearchOption.AllDirectories;

			try
			{
				foreach (var sd in d.EnumerateDirectories("*", searchOpt).Union(new DirectoryInfo[] { d }))
					try
					{
						var L = new List<FileInfo>();

						foreach (var file in sd.EnumerateFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
							L.Add(file);

						All.Add(L);
					}
					catch (Exception ex)
					{
						ex.Message.PrintLine(ConsoleColor.Red);
					}
			}
			catch (Exception ex)
			{
				ex.Message.PrintLine(ConsoleColor.Red);
			}
		}
	}
}
