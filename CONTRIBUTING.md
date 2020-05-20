# KeePass Commander contributions

All contributions are accepted under [the MIT license](LICENSE.md "license").

## Rules

For each contribution the following rules apply:

1. The contribution must provide something meaningful to the end user, who is running KeePass.exe or KeePassCommand.exe
  * Internal refactorings will not be accepted. They are not meaningful to the end user.
  * Internal reorganisation of the files/maps structure will not be accepted. It is not meaningful to the end user.
  * Adding a Dependency Injection framework like Autofac will not be accepted. It is not meaningful to the end user.  
  * Unit Tests / Tests may be accepted. Each contributed test will be examined seperatly.
  
2. Only the Microsoft Windows platform will be supported.
  * Contributions regarding another OS like Linux/MacOS will not be accepted.
  
3. Only the .NET framework 4 will be supported.
  * Contributions regarding another .NET framework like .NET core will not be accepted.
  
4. Only the WinForms GUI framework will be supported.
  * Contributions regarding another GUI framework like XAML will not be accepted.
  
5. The project should compile out of the box, without additional download.
  * After cloning this project to a local harddisk, no additional downloads must be necessary. The project must compile at once.
  * NuGet packages download are forbidden.
  * NuGet packages are not forbidden, as long as they are inside the project and not downloaded.

**If one or more of these rules violates the principles/preferences of a person, that person is adviced to fork this project. And change the fork to his/her preferences.**