using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Utils.Files
{
	public class ProgramNfo
	{
		public string Name;
		public string Descrtiption;
		public string Example;
		public Action<RunArgs> RunAction;
	}


	public class RunArgs
	{
		public IDictionary<string, List<string>> InArgs;
		public bool Trace = true;
		public DirectoryInfo RootDir = new DirectoryInfo(Directory.GetCurrentDirectory());
		public string Me = Process.GetCurrentProcess().MainModule.FileName;
		public State State = new State();

		public void ChangeRoot(string dir)
		{
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			RootDir = new DirectoryInfo(dir);
		}
	}

	public enum SortType
	{
		No = 0,
		AscName = 1,
		DescName = 2,
		Randomize = 3,
		AscCreateDate = 4,
		DescCreateDate = 5
	}

	public class State
	{
		public FileInfo[] Files;
		public string Prefix;
		public string Ext;
		public string NameFromFilePrefix;
		public string SearchPattern = "*.*";
		public string FileNamesFilePath;
		public string DestinationDir;
		public int NameCounter = 0;
		public int NameCounterStep = 1;
		public SortType Sort = SortType.No;
		public int Take = 0;
		public int PadZeroes = 6;
		Random Rdm = new Random();

		public FileInfo RandomFile()
		{
			if (Files != null) return Files[Rdm.Next(0, Files.Length)];
			else throw new Exception("No Files.");
		}

		public void SortFiles(SortType st)
		{
			if (Files == null) return;
			switch (st)
			{
				case SortType.AscName:
				Array.Sort<FileInfo>(Files, (a, b) => string.Compare(a.Name, b.Name));
				break;
				case SortType.DescName:
				Array.Sort<FileInfo>(Files, (a, b) => string.Compare(b.Name, a.Name));
				break;
				case SortType.Randomize:
				Utils.Randomize(Files);
				break;
				case SortType.AscCreateDate:
				Array.Sort<FileInfo>(Files, (a, b) => DateTime.Compare(a.CreationTime, b.CreationTime));
				break;
				case SortType.DescCreateDate:
				Array.Sort<FileInfo>(Files, (a, b) => DateTime.Compare(b.CreationTime, a.CreationTime));
				break;
				case SortType.No:
				default:
				break;
			}
		}

		public void IncrementCounter()
		{
			NameCounter += NameCounterStep;
		}
	}

}
