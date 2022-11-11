using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeePassCommandDll.Communication
{
    public interface ISendCommand
    {
        Response Response { get; }
    }
}
