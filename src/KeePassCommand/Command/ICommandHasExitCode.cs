using KeePassCommandDll.Communication;

namespace KeePassCommand.Command
{
    public interface ICommandHasExitCode
    {
        int ExitCode { get; }
    }
}
