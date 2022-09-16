:: CARDATA
:: Create spreadsheet to edit vehicle parameters
:: ./win-x64 gfz-cli.exe --cardata-bin-to-tsv D:\gfzj01\game\cardata.lz
:: Convert spreadsheet into `cardata.tsv.bin`
:: ./win-x64 gfz-cli.exe --cardata-tsv-to-bin D:\gfzj01\game\cardata.tsv


:: DECOMPRESS
:: Decompress specific file
:: ./win-x64 gfz-cli.exe --lzd D:\gfzj01\stage\st01.gma.lz
:: Decompress all lz files in folder
:: ./win-x64 gfz-cli.exe --lzd D:\gfzj01\stage\
:: Decompress all lz files in folder and subfolders
:: ./win-x64 gfz-cli.exe --lzd D:\gfzj01\ --searchSubdirs

:: COMPRESS
:: Compress specific file
:: ./win-x64 gfz-cli.exe --lzc D:\gfzj01\stage\st01.gma
:: Compress all gma files in folder
:: ./win-x64 gfz-cli.exe --lzc D:\gfzj01\stage\ --searchPattern *.gma
:: Compress all gma files in folder and subfolders
:: ./win-x64 gfz-cli.exe --lzc D:\gfzj01\ --searchPattern *.gma --searchSubdirs