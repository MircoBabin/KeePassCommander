# KeePassCommander changelog

## Version 4.2
Release date: 6 april 2023

* fix: open KeePassCommand.config.xml in same directory as KeePassCommandDll.dll

## Version 4.1
Release date: 28 march 2023

* fix: C# programming language connector.

## Version 4.0
Release date: 18 november 2022

* add: communication via the named pipe is now encrypted. First a shared key is determined via the Diffie-Hellman (and Merkle) protocol. Then further communication uses Aes encryption with the agreed shared key.
* add: encrypted communication via the filesystem. To be able to communicate from within a [Virtual Machine](docs/VirtualMachine.md). And possibly from within a RDP session.

## Version 3.1
Release date: 14 october 2022

* add: listgroup.
* add: KeePassEntry.php function KeePassEntry::ListGroup()

## Version 3.0
Release date: 5 september 2022

* breaking change: getfield now returns the value as UTF-8, base64 encoded. Because the value can contain newlines and/or tabs.
* fix: getnote with multiple entrynames.
* add: getfieldraw.
* add: KeePassEntry.ps1 ability to also retrieve fields and attachments.
* add: KeePassEntry.py ability to also retrieve fields and attachments.
* add: KeePassEntry.cs ability to also retrieve fields and attachments.
* add: KeePassCommandDll.dll ability to also retrieve fields and attachments.

## Version 2.8
Release date: 2 september 2022

* fix: description of KeePassCommander - see also pull request #9 by [rasa](https://github.com/rasa)
* add: KeePassEntry.php ability to also retrieve fields and attachments.

## Version 2.7
Release date: 14 july 2022

* implemented the KeePass check for updates mechanism.
* updated the information exposed by assembly properties.

## Version 2.6
Release date: 4 may 2022

* renamed release distribution to KeePassCommander-x.x.zip
* for automatic installation scripts https://github.com/MircoBabin/KeePassCommander/releases/latest/download/release.download.zip.url-location is a textfile and will contain an url to the latest release zip file

## Version 2.5
Release date: 14 feb 2022

* fix: getattachmentraw - see also pull request #6 by [kkum](https://github.com/kkum)
* fix: getfield

## Version 2.4
Release date: 11 dec 2020

* Added Python connector

## Version 2.3
Release date: 8 oct 2020

* Prevent infinite Field Reference resolving
* Add logging via --KeePassCommanderDebug=c:\incoming\KeePassCommander.log
* Solved restarting listing on named pipe

## Version 2.2
Release date: 20 may 2020

* resolve Field References
* added C# connector

## Version 2.1 [patch 3] 
Release date: 29 jan 2020

* trying to fix issue #3 "Doesn't work with a different KeePass version"

## Version 2.0 
Release date: 8 nov 2019

* added: Get file attachments
* added: Get string fields
* added: Get notes
* added: Help
* added: -out-utf8:outputfile
* added: Programming languages connectors

## Version 1.0
Release date: 11 jan 2019

* Initial release
