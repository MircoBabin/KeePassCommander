using KeePassCommandDll.Communication;

namespace KeePassCommand.Command
{
    public interface ICommand
    {
        void Run(ProgramArguments options, ISendCommand send);
    }
}
