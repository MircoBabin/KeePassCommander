# Virtual Machine

It is possible to use KeePassCommander inside a Virtual Machine. The host (running the Virtual Machine via e.g. VirtualBox) will then be running KeePass.

Communication between KeePassCommand.exe (running inside the Virtual Machine) and KeePass (running outside the Virtual Machine on the host) will then take place encrypted on the filesystem via a shared folder, and not via the named pipe.

p.s. Off course this system can be reversed for running KeePass inside the VM and querying it from the host (outside the VM). The principle stays the same, they both must have access to the same folder.

## Prerequisites on the host (outside the Virtual Machine)

* The host provides a shared folder. E.g. c:\\incoming\\VmShared\\KeePass
* The host machine is running KeePass. And inside the KeePass database an entry must be present. The title must start with KeePassCommander.FileSystem, the url points to the shared folder and the notes specify what entries are allowed to query. The notes have the same syntax as a [listgroup entry](ListGroup.md).

```
title: KeePassCommander.FileSystem [VM]
url: c:\incoming\VmShared\KeePass

notes:
KeePassCommanderListGroup=true
```
* After creating a new KeePassCommander.FileSystem entry, KeePass must be restarted for KeePassCommander to recognize the new entry.

## Prerequisites inside the Virtual Machine

* The Virtual Machine has access to the host shared folder via e.g. s:\\KeePass. 
* VirtualBox: define a shared folder named VmShared to c:\\incoming\\VmShared. Then configure the VM to have drive s: automatically and permanently mapped to \\\\vboxsvr\\VmShared
* Unpack KeePassCommander in a directory e.g. c:\\projects\\bin\\KeePassCommander. Inside the VM no KeePass installation is needed.
* Create a configfile KeePassCommand.config.xml in the same directory as KeePassCommand.exe, e.g. c:\\projects\\bin\\KeePassCommander\\KeePassCommand.config.xml. Configure &lt;filesystem&gt; to the shared folder.

```
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
    <filesystem>s:\KeePass</filesystem>
</Configuration>
```

p.s. Instead of a KeePassCommand.config.xml configuration file, the communication method can also be specified when running KeePassCommand.exe.

```
KeePassCommand.exe listgroup "-filesystem:s:\KeePass" "KeePassCommander.FileSystem [VM]"
```

## Test

* Adjust the "KeePassCommander.FileSystem \[VM\]" entry. Add the following line to notes to be able to listgroup. And restart KeePass.

```
KeePassCommanderListAddItem=KeePassCommander.FileSystem [VM]
```

After testing, remove this line from the notes!

* Inside the VM open a dosprompt and issue:

```
cd /d c:\projects\bin\KeePassCommander
KeePassCommand.exe listgroup "KeePassCommander.FileSystem [VM]"
```

This will show all entries that are allowed to be queried from within the VM.