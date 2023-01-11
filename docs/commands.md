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
gfz-cli.exe --lzd D:\gfzj01\ --searchSubdirs
```



## LZ Compression

Compress specific `.lz` file.

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\st01.gma
```

Compress all `.gma` files in folder.

```shell
gfz-cli.exe --lzc D:\gfzj01\stage\ --searchPattern *.gma
```

Compress all, for example, `.gma` files in folder and subfolders.

```shell
gfz-cli.exe --lzc D:\gfzj01\ --searchPattern *.gma --searchSubdirs
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

