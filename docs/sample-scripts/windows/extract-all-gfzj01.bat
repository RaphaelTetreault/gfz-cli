:: The path to gfz-cli executable
set gfz="C:/Downloads/gfz-cli-windows-x64/gfz.exe"
:: The input path to ROM
set input="C:/Desktop/gfzj01.iso"
:: The output path. Can (probably should be) new folder path
set output="C:/Desktop/gfzj01/"

:: Perform all the extracting, decompressing, unpacking, and converting.
echo on
call extract-all.bat %gfz% %input% %output%
pause