namespace Utils.Files
{
	public interface IUtil
	{
		void Run(RunArgs args);
		string Info { get; }
		string Name { get; }
	}
}
