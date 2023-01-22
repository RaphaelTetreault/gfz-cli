echo on

:: Delete builds folder and contents
:: /s	Deletes a directory tree (the specified directory and all its subdirectories, including all files).
:: /q	Specifies quiet mode. Does not prompt for confirmation when deleting a directory tree. The /q parameter works only if /s is also specified.
rmdir /s /q builds

:: Move to project folder
pushd .
cd src/gfz-cli

:: Build all targets
dotnet publish -r linux-x64
dotnet publish -r osx.10.10-x64
dotnet publish -r osx.11.0-x64
dotnet publish -r osx.12-x64
dotnet publish -r rhel-x64
dotnet publish -r win-x86
dotnet publish -r win-x64

:: Go back to root folder
popd

:: Copy the publish folder to the root of the git repo
:: https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/xcopy
:: /s	Copies subdirectories. This option automatically excludes empty directories.

robocopy .\src\gfz-cli\bin\Debug\net6.0\linux-x64\publish\	.\builds\linux-x64\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\osx.10.10-x64\publish\	.\builds\osx.10.10-x64\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\osx.11.0-x64\publish\	.\builds\osx.11.0-x64\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\osx.12-x64\publish\	.\builds\osx.12-x64\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\rhel-x64\publish\	.\builds\rhel-x64\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\win-x86\publish\	.\builds\win-x86\	/s
robocopy .\src\gfz-cli\bin\Debug\net6.0\win-x64\publish\	.\builds\win-x64\	/s

:: Compress folders as ZIP
pushd .
cd builds/

tar.exe -a -c -f	linux-x64.zip		linux-x64
tar.exe -a -c -f	osx10.10-x64.zip	osx10.10-x64
tar.exe -a -c -f	osx.11.0-x64.zip	osx.11.0-x64
tar.exe -a -c -f	osx.12-x64.zip		osx.12-x64
tar.exe -a -c -f	rhel-x64.zip		rhel-x64
tar.exe -a -c -f	win-x86.zip		win-x86
tar.exe -a -c -f	win-x64.zip		win-x64

popd

:: For debugging
:: PAUSE