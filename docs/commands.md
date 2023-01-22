# `gfz-cli` Commands

[TOC]

## Operating System

This document provides examples using Windows and calls the `gfz-cli.exe` program. Calling the tool will depending on your operating system. Please review the table below and replace instances of `gfz-cli.exe` with the appropriate call to the program.

| Operating System | Program Call  |
| ---------------- | ------------- |
| Windows          | `glz-cli.exe` |
| Linux            | `./gfz-cli`   |
| macOS            | `gfz-cli`     |



## Car Data (GX only)

This tool provide a way to convert the `cardata.lz` binary to a tab-separated value (TSV) table and a way to convert it back to a binary from an appropriately formatted TSV file.

### Create spreadsheet to edit vehicle parameters

This command creates a new file called `cardata.tsv` in the same directory as the input file. If `cardata.tsv` already exists there, it will be overwritten.

```shell
gfz-cli.exe --cardata-bin-to-tsv D:\gfzj01\game\cardata.lz
```

### Convert spreadsheet into `cardata.lz`

Note that this command will create both `cardata` and `cardata.lz` in the same directory as the input file.

```shell
gfz-cli.exe --cardata-tsv-to-bin D:\gfzj01\game\cardata.tsv
```



## LZ Decompression

Decompress specific `.lz` file.

```shell
gfz-cli.exe --lzd D:\gfzj01\stage\st01.gma.lz
```

Decompress all `.lz` files in a folder.

```shell
gfz-cli.exe --lzd D:\gfzj01\stage\
```

Decompress all `.lz` files in a folder and it's subfolders.

```shell
gfz-cli.exe --lzd D:\gfzj01\ --search-subdirs
```

Decompress all files ending in `.tpl.lz` in specified folder and it's subfolders.

```shell
gfz-cli.exe --lzd D:\gfzj01\ --search-pattern *.tpl.lz --search-subdirs 
```



## LZ Compression

Compress specific `.lz` file.

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\st01.gma
```

Compress files named using the format `st??.gma` in the specified folder. (`?` is a single-character wildcard.)

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\ --search-pattern st??.gma
```

Compress all `.gma` files in folder and subfolders. (`*` is the variable-length wildcard character.)

```shell
gfz-cli.exe --lzc D:\gfzj01\ --search-pattern *.gma --search-subdirs
```

### Specifying F-Zero AX/GX Formats

By default, `gfz-cli` serializes files in for F-Zero GX (all regions). The user can specify which format they wish to serialize to using the `--format` command. 

Compress a file to be F-Zero GX compatible explicitly.

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\st01.gma --format gx
```

Compress a file to be F-Zero AX compatible.

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\st01.gma --format ax
```



## TPL Files

To unpack a `.tpl` file, you can use the following command.

### Unpack TPL

These arguments will create a folder in the same directory as the source file with the same name as the source file containing `.png` files of the source `.tpl`'s textures.

```shell
gfz-cli.exe --tpl-unpack D:\gfzj01\bg\bg_mut.tpl
```

You can also unpack all `.tpl` files in a directory by calling the directory itself with appropriate search commands.

```shell
gfz-cli.exe --tpl-unpack D:\gfzj01\bg\ --search-pattern *.tpl
```

As usual, you can allow the command to include all subdirectories by using the `--search-subdirs` command.

```shell
gfz-cli.exe --tpl-unpack D:\gfzj01\bg\ --search-pattern *.tpl --search-subdirs
```



