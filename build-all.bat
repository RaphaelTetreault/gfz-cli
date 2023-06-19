echo on

:: Delete old builds folder and contents
:: /s	Deletes a directory tree (the specified directory and all its subdirectories, including all files).
:: /q	Specifies quiet mode. Does not prompt for confirmation when deleting a directory tree.
::      The /q parameter works only if /s is also specified.
rmdir /s /q builds

:: Move to project folder
pushd .
cd src/gfz-cli

:: Build all targets
dotnet publish -r linux-x64 -o ../../builds/linux-x64
dotnet publish -r osx-x64   -o ../../builds/osx-x64
dotnet publish -r rhel-x64  -o ../../builds/rhel-x64
dotnet publish -r win-x64   -o ../../builds/win-x64
dotnet publish -r win-x86   -o ../../builds/win-x86

:: Go back to root folder
popd

:: Compress folders as ZIP
pushd .
cd builds/

tar.exe -a -c -f	linux-x64.zip	linux-x64
tar.exe -a -c -f	osx-x64.zip	osx-x64
tar.exe -a -c -f	rhel-x64.zip	rhel-x64
tar.exe -a -c -f	win-x64.zip	win-x64
tar.exe -a -c -f	win-x86.zip	win-x86

popd

:: For debugging
:: PAUSE