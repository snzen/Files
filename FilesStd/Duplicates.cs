using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Files
{
	public class Duplicates : IUtil
	{
		public string Name => "duplicates";
		public string Info =>
			"Detects file duplicates in one or more folders by comparing sizes, names or data hashes." + Environment.NewLine +
			"There are extension and size filters as well as an option for partial hashing by skip/taking portions of the files.";

		public void Run(RunArgs ra)
		{
			var srcDirs = new List<DirectoryInfo>();
			var srcs = string.Empty;
			var skipLessThanSize = -1;
			var skipMoreThanSize = -1;
			var ignExt = string.Empty;
			var iExt = new List<string>();

			Utils.ReadString("Enter folders to search into, separated by semicolon: ", ref srcs, true);
			Utils.ReadString("Search pattern (*.*): ", ref ra.State.SearchPattern);
			var recursive = !Utils.ReadWord("Recursive search (default is yes)? (n/*): ", "n");

			Utils.ReadInt($"skip files with size < ({skipLessThanSize})Kb: ", ref skipLessThanSize, false);
			Utils.ReadInt($"skip files with size > ({skipMoreThanSize})Kb: ", ref skipMoreThanSize, false);
			Utils.ReadString("skip extensions (.xyz): ", ref ignExt, false);

			if (!string.IsNullOrEmpty(ignExt))
				foreach (var ext in ignExt.Split(';'))
					iExt.Add(ext.Trim());

			var compHash = Utils.ReadWord("Compare file names (default) or MD5 hashes? (h/*): ", "h");
			var inParallel = 1;
			var useStreamReduction = false;
			var take = 1000;
			var skip = 6000;

			if (compHash)
			{
				Utils.ReadInt($"concurrent readers (1-{Environment.ProcessorCount}): ", ref inParallel);
				Console.WriteLine(
					"By default the hash is computed over the whole file. {0}You can use skip and take parameters " +
					"to take a portion of the data.", Environment.NewLine);

				useStreamReduction = !Utils.ReadWord("Do you want to use skip/take? (n/*): ", "n");

				if (useStreamReduction)
				{
					"The reader always starts with a TAKE (position 0).".PrintLine();
					Utils.ReadInt($"take bytes ({take}): ", ref take, false);
					Utils.ReadInt($"skip bytes ({skip}): ", ref skip, false);

					if (skip < 0 || take < 0) throw new ArgumentOutOfRangeException("Negative skip or take value.");
				}
			}

			if (inParallel < 1 || inParallel > Environment.ProcessorCount)
				throw new ArgumentOutOfRangeException("inParallel", $"The concurrent readers should be between 0 and {Environment.ProcessorCount}");

			foreach (var p in srcs.Split(';'))
				if (Directory.Exists(p))
					srcDirs.Add(new DirectoryInfo(p));
				else throw new DirectoryNotFoundException(p);// will throw if the path is invalid

			var All = new List<List<FileInfo>>();
			var searchOpt = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

			// Some subdirectories may be restricted and the GetFiles will throw with AccesDenied
			// so loop the individual folders separately.
			foreach (var d in srcDirs)
				try
				{
					foreach (var sd in d.EnumerateDirectories("*", searchOpt))
						try
						{
							var L = new List<FileInfo>();

							foreach (var file in sd.EnumerateFiles(ra.State.SearchPattern, searchOpt))
							{
								if (iExt.Count > 0 && iExt.Contains(file.Extension)) continue;
								if (skipLessThanSize > 0 && file.Length < skipLessThanSize) continue;
								if (skipMoreThanSize > 0 && file.Length > skipMoreThanSize) continue;

								L.Add(file);
							}

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

			var dict = new Dictionary<string, List<FileInfo>>();
			var sb = new StringBuilder();
			var totalFiles = 0;

			// In all cases the length will be the same
			foreach (var src in All)
				foreach (var fi in src)
				{
					var key = $"{fi.Length}";
					if (!dict.ContainsKey(key))
						dict.Add(key, new List<FileInfo>());
					else totalFiles++;

					dict[key].Add(fi);
				}


			totalFiles += totalFiles;

			// Either compare the names or the data hashes
			var hashDict = new Dictionary<string, List<FileInfo>>();
			var totalDuplicates = 0;
			var counter = 0;
			var lockHashLoop = new object();
			Console.CursorVisible = false;
			var cursorTop = Console.CursorTop;

			foreach (var kv in dict)
				if (kv.Value.Count > 1)
					if (compHash)
					{
						Console.SetCursorPosition(0, cursorTop);

						Parallel.For(0, kv.Value.Count, new ParallelOptions() { MaxDegreeOfParallelism = inParallel }, (i) =>
						{
							try
							{
								Stream stream = File.OpenRead(kv.Value[i].FullName);
								if (useStreamReduction) stream = new StreamReductor(stream, skip, take);

								using (var md5 = MD5.Create())
								using (stream)
								{
									lock (lockHashLoop)
									{
										$"{++counter}/{totalFiles} [{kv.Value[i].Length / 1000}]Kb file{i} = {kv.Value[i].FullName}".PrintLine(ConsoleColor.Yellow);
									}

									var h = md5.ComputeHash(stream);
									var key = BitConverter.ToString(h);

									lock (lockHashLoop)
									{
										if (!hashDict.ContainsKey(key)) hashDict.Add(key, new List<FileInfo>());
										else totalDuplicates++;

										hashDict[key].Add(kv.Value[i]);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
								return;
							}
						});

						var cursorTopNow = Console.CursorTop;
						for (int top = cursorTop; top < cursorTopNow; top++)
							for (int left = 0; left < Console.WindowWidth; left++)
							{
								Console.SetCursorPosition(left, top);
								Console.Write(' ');
							}
					}
					else
						foreach (var f in kv.Value)
						{
							var key = f.Name.ToLowerInvariant();

							if (!hashDict.ContainsKey(key))
								hashDict.Add(key, new List<FileInfo>());
							else totalDuplicates++;

							hashDict[key].Add(f);
							$"comparing {++counter}/{totalFiles}".Print(ConsoleColor.Yellow);
							Console.SetCursorPosition(0, Console.CursorTop);
						}

			Console.SetCursorPosition(0, cursorTop);
			Console.CursorVisible = true;

			sb.AppendLine("Files with the same name and length are grouped together.");
			sb.AppendLine();

			foreach (var kv in hashDict)
				if (kv.Value.Count > 1)
				{
					foreach (var p in kv.Value)
						sb.AppendLine(p.FullName);

					sb.AppendLine();
				}


			$"There are {totalDuplicates} files with clones.".PrintLine();

			if (totalDuplicates > 0)
			{
				ra.Trace = Utils.ReadWord("Trace? (y/*): ", "y");

				if (ra.Trace) sb.ToString().PrintLine(ConsoleColor.Yellow);

				if (Utils.ReadWord("Save results? (y/*): ", "y"))
				{
					var fn = string.Empty;
					Utils.ReadString("result file path: ", ref fn, true);
					File.WriteAllText(fn, sb.ToString());
				}
				else Console.WriteLine("Aborting. Press <Enter> to exit.");
			}
		}
	}
}
