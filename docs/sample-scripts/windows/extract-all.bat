:: Get variables from input. Note expected order.
set gfz=%1
set input=%2
set output=%3

:: Extract, decompress, and unpack all files
%gfz% extract-iso-files %input% %output%
%gfz% lz-decompress %output% --search-subdirs
%gfz% arc-decompress %output% --search-subdirs
%gfz% tpl-unpack %output% --search-subdirs --unpack-mipmaps --unpack-corrupted-cmpr
%gfz% emblem-bin-to-image %output% --search-subdirs --search-pattern "*chara.bin"
%gfz% emblem-bin-to-image %output% --search-subdirs --search-pattern "*sample.bin"
%gfz% cardata-bin-to-tsv %output% --search-subdirs --search-pattern "cardata.lz"
