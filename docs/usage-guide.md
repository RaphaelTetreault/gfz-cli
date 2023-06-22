# `gfz-cli` Commands

[TOC]

## Operating System

This document provides examples using Windows and calls the `gfz-cli.exe` program. Calling the tool will depending on your operating system. Please review the table below and replace instances of `gfz-cli.exe` with the appropriate call to the program.

| Operating System | Program Call  |
| ---------------- | ------------- |
| Windows          | `glz-cli.exe` |
| Linux            | `./gfz-cli`   |
| macOS            | `gfz-cli`     |



## About this Document

### Paths

Examples in this guide use relative paths `in/` and `out/` as the example base input and output directories. 

If you are unfamiliar with Unix style paths and/or using a command line terminal, consider learning how to navigate your operating system's terminal before proceeding as it will make the examples make more sense.

### Wildcards

When using the `--search-pattern` command, you may use the `*` and `?` wildcards.

`*` is the character group wildcard and will match any number of characters.

`?` is the character wildcard and will match any one character.



## General Usage

`gfz-cli` has a usage pattern for all actions. The program accepts 3 un-labelled ordered parameters followed by any number of relevant options.

```shell
gfz-cli.exe [action] [input-path] [output-path] [other-options]
```

**Action**: specifies the action to perform. *This parameter is **required***. 

**Input Path**: The target file or folder of the action. *This parameter is **required***. This can be a file path or folder path (directory). Some actions accept either file or folder, while others only accept one or the other.

**Output Path**: The target destination of the action. *This parameter is **optional***. This can be a file path or a folder path depending both on the input path and also on the action.

**Other Options**: Depending on the action performed, the user can pass additional arguments to further control how the specified action is performed. Generally these are optional, though some actions may require additional information.

### Possible Actions

| Actions Token         | Brief Description                                            |
| --------------------- | ------------------------------------------------------------ |
| `arc-compress`        | Compress contents of a folder into an `.arc` archive.        |
| `arc-decompress`      | Decompress contents of an `.arc` archive to a folder.        |
| `cardata-bin-to-tsv`  | Convert `./game/cardata.bin` into an editable TSV spreadsheet. (GX only.) |
| `cardata-tsv-to-bin`  | Convert a `cardata.tsv` spreadsheet into `cardata.bin` binary. (GX only.) |
| `extract-iso-files`   | Extract the `/sys` programs and file system `/files` from a GameCube ISO to a folder. |
| `emblem-to-image`     | Convert `.gci` and`.bin` emblems into `.png` files.          |
| `image-to-emblem-bin` | Convert multiple images into a single `.bin` emblem archive. |
| `image-to-emblem-gci` | Convert images into `.gci` emblems files.                    |
| `lz-compress`         | Compress contents of a folder into an `.lz` archive.         |
| `lz-decompress`       | Decompress contents of an `.lz` archive to a folder.         |
| `tpl-unpack`          | Unpack textures inside a `.tpl` archive to a folder as `.png` files. |

## General Options

There are a few options that apply to all actions.

| Option Token       | Brief Description                                            |
| ------------------ | ------------------------------------------------------------ |
| `--overwrite`      | Allow overwriting output files. Off by default.              |
| `--search-pattern` | When ***input-path*** is a directory, the search pattern applied to find files in that directory. Uses single (`?`) and multi (`*`) character wildcards. |
| `--search-subdirs` | When ***input-path*** is a directory, the search pattern applies to subdirectories. Off by default. |
| `--format`         | The target serialization format used. Either `ax` or `gx`. Set to `gx` by default. |



## ARC Archives

Actions for managing `.arc` archive files.

### Compress ARC

Compress ***input-path*** (directory) to an `.arc` archive beside target folder or at ***output-path*** (directory) if specified.

Compress a folder to `.arc`.

```shell
# Compress contents of "in/dlg/"" creating "in/dlg.arc"
gfz-cli.exe arc-compress ./in/dlg/
```

Compress a folder to `.arc` and set destination path.

```shell
# Compress contents of "in/dlg/"" creating "out/dlg.arc"
gfz-cli.exe arc-compress in/dlg/ out/
```

### Decompress ARC

Decompress ***input-path*** (file or directory) at current path or at ***output-path*** (file or directory).

Decompress an `.arc` file.

```shell
# Decompress contents of "in/chara/face_tpl.arc" creating "in/chara/face_tpl/"
gfz-cli.exe arc-decompress in/chara/face_tpl.arc
```

Decompress an `.arc` file and set the destination path.

```shell
# Decompress contents of "in/dlg/"" creating "out/dlg.arc"
gfz-cli.exe arc-decompress in/chara/face_tpl.arc out/
```

Decompress all `.arc` files inside a folder.

```shell
# Decompress contents of all .arc files in "in/"
# "--search-pattern *.arc" is implicit
gfz-cli.exe arc-decompress in/ --search-subdirs
```

Decompress all `.arc` files inside a folder and its subfolders and set the destination path. 

```shell
# Decompress contents of all .arc files in "in/" to "out/" and its subdirectories
# "--search-pattern *.arc" is implicit
gfz-cli.exe arc-decompress in/ out/ --search-subdirs
```





## Car Data

Actions to convert the `./game/cardata.lz` binary (GX only) to a tab-separated value (TSV) table and to convert it back to a binary from an appropriately formatted TSV file.

### Create spreadsheet to edit vehicle parameters

Convert ***input-path*** file `cardata` or `cardata.lz` to `cardata.tsv` at the current path or at ***output-path*** if specified.

Note: The input file can be the compress `cardata.lz` or unpacked `cardata` file.

```shell
# Convert input file "in/game/cardata.tsv" to binary "in/game/cardata.lz"
gfz-cli.exe cardata-bin-to-tsv in/game/cardata.lz
```

### Convert spreadsheet into `cardata.lz`

Convert ***input-path*** file `cardata.tsv` to `cardata.lz` at the current path or at ***output-path*** if specified.

```shell
# Convert input file "in/game/cardata.tsv" to binary "in/game/cardata.lz"
gfz-cli.exe cardata-tsv-to-bin in/game/cardata.tsv
```



## Emblems

Actions for managing emblems.

### Types of Emblems

#### Editor Emblems

#### Emblem Archives

### Convert Emblem to Image

### Convert Image to Emblem



## ISO Management

### Extract Files from ISO



## LZ Archives

Actions for managing `.lz` archive files.

### LZ Compression

Compress ***input-path*** (file or directory) to an `.lz` archive at the current path or at ***output-path*** (directory) if specified.

Compress a specific file.

```shell
# Compress the file at "in/stage/st01.tpl" to "in/stage/st01.tpl.lz"
gfz-cli.exe lz-compress in/stage/st01.tpl
```

Compress all `.gma` files in folder and subfolders.

```shell
# Compress all files ending in .gma inside the "in/" directory and its subdirectories
gfz-cli.exe lz-compress in/ --search-pattern *.gma --search-subdirs
```

#### Specifying F-Zero AX/GX Formats

By default, `gfz-cli` serializes output files in formats for F-Zero GX. The user can specify which format they wish to serialize to using the `--format`. 

Add the option `--format ax` if you wish to compress a file for use in F-Zero AX.

```shell
# Compress file to .lz meant for use with F-Zero AX
gfz-cli.exe lz-compress in/stage/st01.gma --format ax
```

### LZ Decompression

Decompress ***input-path*** (file or directory) at current path or at ***output-path*** (file or directory).

Decompress specific `.lz` file.

```shell
# Decompress contents of "in/stage/st01.gma.lz" creating "in/stage/st01.gma"
gfz-cli.exe lz-decompress in/stage/st01.gma.lz
```

Decompress all `.lz` files in a folder and its subfolders.

```shell
# Decompress contents of all .lz files in "in/" and its subdirectories
# "--search-pattern *.lz" is implicit
gfz-cli.exe lz-decompress in/ --search-subdirs
```



## TPL Texture Palettes

Actions for managing `.tpl` files.

### Unpack TPL

These arguments will create a folder in the same directory as the source file with the same name as the source file containing `.png` files of the source `.tpl`'s textures.

```shell
# Unpack all textures inside "in/bg/bg_mut.tpl" to "in/bg/bg_mut/"
gfz-cli.exe tpl-unpack in/bg/bg_mut.tpl
```

You can also unpack all `.tpl` files in a directory.

```shell
# Unpack all textures inside "in/bg/bg_mut.tpl" to "in/bg/" and its subdirectories
# "--search-pattern "*.tpl" is implicit
gfz-cli.exe tpl-unpack in/bg/ --search-subdirs
```

### Unpacking Options

| Option Token              | Brief Description                               |
| ------------------------- | ----------------------------------------------- |
| `--unpack-mipmaps`        | Unpack mipmaps. Off by default.                 |
| `--unpack-corrupted-cmpr` | Unpack corrupted CMPR textures. Off by default. |

#### Unpacking Mipmaps

When unpacking a `.tpl` file, you can ask the unpacker to also save each textures' uncorrupted mipmaps. See [Unpacking Corrupted CMPR Textures](#unpacking-corrupted-cmpr-textures) for more information about corrupted mipmaps.

```shell
# Unpack "in/bg/bg_mut.tpl" textures and all textures' valid mipmaps
gfz-cli.exe tpl-unpack in/bg/bg_mut.tpl --unpack-mipmaps
```

#### Unpacking Corrupted CMPR Textures

F-Zero AX and F-Zero GX `.tpl` files do not store CMPR textures correctly. Due to a bug, they can under-allocate memory. Due to the nature of this bug, mipmaps are most susceptible to this issue.

You may specify that the unpacker output these corrupted/incomplete textures. The corrupted/incomplete pixel data will appear as magenta in the output files.

```shell
# # Unpack "in/bg/bg_mut.tpl" textures and all textures' mipmaps 
gfz-cli.exe tpl-unpack in/bg/bg_mut.tpl --unpack-mipmaps --unpack-corrupted-cmpr
```



## Work-in-Progress Notes

Remove instances of setting output path and instead provide that info before actions, perhaps in [About this Document](#about-this-document)?
