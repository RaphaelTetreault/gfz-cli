:: Variables, all paths to program and files
:: Assumes line__.rel and cardata (decompressed) are in this folder
:: cardata can be compressed for this linerel-set-cardata

set gfz="..\gfz.exe"
set linerel="line__.rel"
set cardata="cardata"

%gfz% linerel-set-cardata %linerel% --region na --use-file %cardata%
%gfz% linerel-encrypt %linerel% -o --region na

pause