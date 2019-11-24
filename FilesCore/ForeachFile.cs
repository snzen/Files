using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Utils.Files
{
	public class ForeachFile : IUtil
	{
		public string Name => "foreach";
		public string Info =>
			"Enumerates all matching files in a given root and launches a process with the provided args." +
			"Args: not interactive (-ni), concurrent instances [1] (-ci), root [current] (-root), file search (-sp) [*.*], " +
			"recursive (-rec) [y/n]" + Environment.NewLine +
			"program to call [files] (-proc), program args (-pargs) [put in quotes and add a single space before the pargs string] " +
			"current dir full path as arg name (-cdf),  current dir as arg (-cd)" + Environment.NewLine +
			"Example: files -p foreach -ni -cdf $$ -cd $ -root <path> -proc <files> -pargs \" -p colpick -ni  -in $$ -out $$ -cols 0,4,1 \"";

		public int Run(RunArgs ra)
		{
			bool interactive = !ra.InArgs.ContainsKey("-ni");
			DirectoryInfo root = ra.RootDir;
			string prog = "files";
			string progArgs = null;
			string cdf = null;
			string cd = null;
			var recursive = "n";
			var ci = 1;
			var sp = string.Empty;

			if (interactive)
			{
				var src = string.Empty;
				var cc = string.Empty;

				Utils.ReadString("Source dir (current): ", ref src);
				Utils.ReadString("File search pattern (*.*): ", ref sp);
				Utils.ReadString("Recursive? (y/*): ", ref recursive);
				root = !string.IsNullOrEmpty(src) ? new DirectoryInfo(src) : ra.RootDir;
				Utils.ReadString("Program to run for each dir (files): ", ref prog);
				Utils.ReadString("Concurrent instances (1): ", ref cc);

				if (!string.IsNullOrWhiteSpace(cc)) ci = int.Parse(cc);

				Utils.ReadString("Program arguments: ", ref progArgs, true);
				Utils.ReadString("Argument string to be replaced by the full current dir path: ", ref cdf);
				Utils.ReadString("Argument string to be replaced by the current dir name: ", ref cd);
			}
			else
			{
				if (ra.InArgs.ContainsKey("-root"))
					root = new DirectoryInfo(ra.InArgs.GetFirstValue("-root"));
				if (ra.InArgs.ContainsKey("-proc"))
					prog = ra.InArgs.GetFirstValue("-proc");
				progArgs = ra.InArgs.GetFirstValue("-pargs");
				if (ra.InArgs.ContainsKey("-sp")) sp = ra.InArgs.GetFirstValue("-sp");
				if (ra.InArgs.ContainsKey("-rec")) recursive = ra.InArgs.GetFirstValue("-rec");
				if (ra.InArgs.ContainsKey("-cdf")) cdf = ra.InArgs.GetFirstValue("-cdf");
				if (ra.InArgs.ContainsKey("-cd")) cd = ra.InArgs.GetFirstValue("-cd");
				if (ra.InArgs.ContainsKey("-ci")) ci = int.Parse(ra.InArgs.GetFirstValue("-ci"));
			}

			var so = recursive == "y" ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var F = new List<FileInfo>(root.EnumerateFiles(sp, so));

			Parallel.For(0, F.Count, new ParallelOptions() { MaxDegreeOfParallelism = ci }, (i) =>
			{
				try
				{
					var f = F[i];
					var pargs = progArgs;
					if (!string.IsNullOrWhiteSpace(cdf)) pargs = pargs.Replace(cdf, "\"" + f.FullName + "\"");
					if (!string.IsNullOrWhiteSpace(cd)) pargs = pargs.Replace(cd, f.Directory.FullName);

					var proc = new ProcessStartInfo(prog, pargs);

					$"Starting {prog} {pargs}".PrintLine();

					using (var process = Process.Start(proc))
						process.WaitForExit();
				}
				catch (Exception ex)
				{
					ex.Message.PrintLine(ConsoleColor.Red);
					ex.StackTrace.PrintSysWarn();
				}
			});

			return 0;
		}
	}
}
