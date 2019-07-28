using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace Utils.Files
{
	public class PadNumbers : IUtil
	{
		public string Name => "pad";
		public string Info => "Adds leading zeros to the numbers in the file names.";

		public int Run(RunArgs ra)
		{
			ra.State.DestinationDir = ra.RootDir.FullName;
			Utils.ReadString("destination dir: ", ref ra.State.DestinationDir);
			Utils.ReadInt("number length with zero padding (6) : ", ref ra.State.PadZeroes);
			Utils.ReadString("search pattern (*.*): ", ref ra.State.SearchPattern);

			var useexisting = false;
			if (ra.State.Files != null && ra.State.Files.Length > 0)
			{
				string.Format(
					"There are {0} files from a previous subprogram run. {1}Frst of the old files: {2} ",
					ra.State.Files.Length, Environment.NewLine, ra.State.Files[0]).PrintLine();
				useexisting = Utils.ReadWord("Use them? (y/*) ", "y");
			}

			if (!useexisting) ra.State.Files = ra.RootDir.GetFiles(ra.State.SearchPattern, SearchOption.TopDirectoryOnly);
			else if (Utils.ReadWord(string.Format("Change the root dir ({0}) ? (y/*)", ra.RootDir.FullName), "y"))
			{
				var newroot = string.Empty;
				Utils.ReadString("Enter new root dir: ", ref newroot, true);
				ra.ChangeRoot(newroot);
			}

			string.Format("There are {0} matches.", ra.State.Files.Length).PrintLine();
			ra.Trace = Utils.ReadWord("Trace first? (y/*): ", "y");

			var FI = new List<FileInfo>();

			if (ra.Trace) PadFiles(ra, FI);
			if (Utils.ReadWord("Pad all? (y/*): ", "y"))
			{
				PadFiles(ra, FI, true);
				ra.State.Files = FI.ToArray();
				Console.WriteLine("Done - {0} files renamed.", ra.State.Files.Length);
			}
			else Console.WriteLine("Aborting pad.");

			return 0;
		}

		static void PadFiles(RunArgs ra, List<FileInfo> FI, bool forreal = false)
		{
			var tcounter = ra.State.NameCounter;
			foreach (var f in ra.State.Files)
			{
				if (f.FullName == ra.Me) continue;
				var dotidx = f.Name.IndexOf('.');
				var fn = f.Name.Remove(dotidx);
				var suff = f.Name.Substring(dotidx);
				fn = PadZeroes(fn, ra.State.PadZeroes);
				var newname = string.Format("{0}{1}{2}", ra.State.Prefix, fn, suff);
				var newpath = Path.Combine(ra.State.DestinationDir, newname);
				if (!forreal) string.Format(f.Name + " --> " + newpath).PrintLine(ConsoleColor.Yellow);
				else
				{
					File.Move(f.FullName, newpath);
					FI.Add(new FileInfo(newpath));
				}
			}
		}


		public static string PadZeroes(string fnWoExt, int zeroes)
		{
			Match m = Regex.Match(fnWoExt, @"\d+");
			if (m.Success)
			{
				var padded = m.Value.PadLeft(zeroes, '0');
				return fnWoExt.Replace(m.Value, padded);
			}
			return fnWoExt;
		}
	}
}
