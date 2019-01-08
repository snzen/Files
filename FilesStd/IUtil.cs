namespace Utils.Files
{
	public interface IUtil
	{
		int Run(RunArgs args);
		string Info { get; }
		string Name { get; }
	}
}
