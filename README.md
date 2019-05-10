# KeePass Commander
KeePass Commander is a plugin for the [KeePass password store](https://keepass.info/ "KeePass"). 
It's purpose is to provide a communication channel for php-scripts, ... to be able to query the KeePass password store from the commandline.

![Screenshot](screenshot.png)

# Download binary
For Windows (.NET framework 3.5 / .NET framework 4), [the latest version can be found here](https://github.com/MircoBabin/KeePassCommander/releases/latest "Lastest Version").
The plugin works with KeePass 2.41. Because the plugin barely uses anything from KeePass, it will probably work with all future KeePass versions.

Download the zip and unpack it in the KeePass directory where KeePass.exe is located.

The minimum .NET framework required is 3.5. This is the first framework to implement the System.IO.Pipes namespace.

# Commandline arguments
KeePassCommand.exe get {-out:outputfilename} "KeePass-entry-title" "KeePass-entry-title" ...

KeePass-entry-title must match exactly, there is no fuzzy logic. All open databases in KeePass are searched.

If -out: is ommitted then the output will be at the console (STDOUT).

When using -out:, don't forget to delete the outputfile from the (hopefully Bitlocker encrypted, with Bitlocker boot password) harddisk after reading the contents!

e.g. KeePassCommand.exe get "Sample Entry"

# Examples

Examples are found in the github directory **example**.

- example.kdbx is a KeePass database. It's master password is **example**.
- [KeePassEntry.php](example/KeePassEntry.php) can be used to query the KeePass password store from PHP. With minimal modifications you can use it anywhere.


# Why
The plugin [KeePassHttp](https://github.com/pfn/keepasshttp/) already exists for querying the password store. 
I did not want to use this plugin, because it embeds a http server inside KeePass. 
And I don't want to "pair" with a code, because I want to communicate from the commandline, without configuration.

So I build KeePassCommander.dll plugin which runs a Windows named-pipe-server inside KeePass. And a KeePassCommand.exe commandline tool to communicate with KeePassCommander.dll. 

I'm using this plugin among other things to automate DeployHQ. In KeePass I store the DeployHQ API key. From a php script the API key is queried and then used. 

# Contributions
Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md "contributing") before making any contribution!

# License
[The license is MIT.](LICENSE.md "license")





