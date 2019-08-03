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
			"Args: not interactive (-ni), source (-src), search pattern (-sp), report errors (-err)";

		public int Run(RunArgs ra)
		{
			var interactive = !ra.InArgs.ContainsKey("-ni");
			var err = ra.InArgs.ContainsKey("-err");
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
			var All = new List<FileInfo>();
			var searchOpt = SearchOption.AllDirectories;

			try
			{
				var D = d.EnumerateDirectories("*", searchOpt).GetEnumerator();

				foreach (var file in d.EnumerateFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
					All.Add(file);

				while (true)
					try
					{
						if (!D.MoveNext()) break;

						foreach (var file in D.Current.EnumerateFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly))
							All.Add(file);
					}
					catch (Exception ex)
					{
						if (err) ex.Message.PrintLine(ConsoleColor.Red);
					}
			}
			catch (Exception ex)
			{
				if (err) ex.Message.PrintLine(ConsoleColor.Red);
			}

			Console.WriteLine();
			string.Format("There are {0} matches:", All.Count).PrintLine();
			Console.WriteLine();

			foreach (var f in All) Console.WriteLine(f.FullName);

			Console.WriteLine();

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
