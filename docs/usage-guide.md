# `gfz` Command Line Interface Usage Guide

[TOC]

## Operating System

This document provides examples using Windows and calls the `gfz.exe` program. Calling the tool will depending on your operating system. Please review the table below and replace instances of `gfz.exe` with the appropriate call to the program.

| Operating System | Program Call |
| ---------------- | ------------ |
| Windows          | `gfz.exe`    |
| Linux            | `./gfz`      |
| macOS            | `gfz`        |



## About this Document

### Paths

Examples in this guide use relative paths `in/` and `out/` as the example base input and output directories. 

If you are unfamiliar with Unix style paths and/or using a command line terminal, consider learning how to navigate your operating system's terminal before proceeding as it will make the examples make more sense.

### Wildcards

When using the `--search-pattern` command, you may use the `*` and `?` wildcards.

`*` is the character group wildcard and will match any number of characters.

`?` is the character wildcard and will match any one character.

### Image Formats

`gfz-cli` uses the `SixLabors.ImageSharp` image library for image processing. As such, all image formats *can* be supported. For inputs, any image format will do. However, as this tool is a work in progress, not all formats are supported as output formats.

Review supported image types on SixLabors' website. https://docs.sixlabors.com/articles/imagesharp/imageformats.html



## General Usage

`gfz-cli` has a usage pattern for all actions. The program accepts 3 un-labelled ordered parameters followed by any number of relevant options.

```shell
gfz.exe [action] [input-path] [output-path] [other-options]
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
gfz.exe arc-compress ./in/dlg/
```

Compress a folder to `.arc` and set destination path.

```shell
# Compress contents of "in/dlg/"" creating "out/dlg.arc"
gfz.exe arc-compress in/dlg/ out/
```

### Decompress ARC

Decompress ***input-path*** (file or directory) at current path or at ***output-path*** (file or directory).

Decompress an `.arc` file.

```shell
# Decompress contents of "in/chara/face_tpl.arc" creating "in/chara/face_tpl/"
gfz.exe arc-decompress in/chara/face_tpl.arc
```

Decompress an `.arc` file and set the destination path.

```shell
# Decompress contents of "in/dlg/"" creating "out/dlg.arc"
gfz.exe arc-decompress in/chara/face_tpl.arc out/
```

Decompress all `.arc` files inside a folder.

```shell
# Decompress contents of all .arc files in "in/"
# "--search-pattern *.arc" is implicit
gfz.exe arc-decompress in/ --search-subdirs
```

Decompress all `.arc` files inside a folder and its subfolders and set the destination path. 

```shell
# Decompress contents of all .arc files in "in/" to "out/" and its subdirectories
# "--search-pattern *.arc" is implicit
gfz.exe arc-decompress in/ out/ --search-subdirs
```





## Car Data

Actions to convert the `./game/cardata.lz` binary (GX only) to a tab-separated value (TSV) table and to convert it back to a binary from an appropriately formatted TSV file.

### Create spreadsheet to edit vehicle parameters

Convert ***input-path*** file `cardata` or `cardata.lz` to `cardata.tsv` at the current path or at ***output-path*** if specified.

Note: The input file can be the compress `cardata.lz` or unpacked `cardata` file.

```shell
# Convert input file "in/game/cardata.tsv" to binary "in/game/cardata.lz"
gfz.exe cardata-bin-to-tsv in/game/cardata.lz
```

### Convert spreadsheet into `cardata.lz`

Convert ***input-path*** file `cardata.tsv` to `cardata.lz` at the current path or at ***output-path*** if specified.

```shell
# Convert input file "in/game/cardata.tsv" to binary "in/game/cardata.lz"
gfz.exe cardata-tsv-to-bin in/game/cardata.tsv
```



## Emblems

Actions for managing emblems.

### Types of Emblems

There are two kinds of emblems used in F-Zero AX and F-Zero GX. `gfz-cli` supports both.

#### Editor Emblems

Editor emblems are the emblems (typically) created inside the emblem editor. These files are saved to the GameCube memory card as `.gci` files. Each emblem contains a single image. The `.gci` format contains some other information for the GameCube file system.

#### Emblem Archives

F-Zero GX contains two binaries which house a number of emblems; `./emblem/chara.bin` and `./emblem/sample.bin`. These are the default emblems used in the machine editor. You can replace the emblems of these archives using more or less images than originally contained, too. Files too large will crash the game, though.

### Convert Emblem to Image

Extract the image contents of a `.gci` emblem. This creates 3 files; the `.gci` banner, the `.gci` icon, and the emblem image.

Extract images inside ***input-path*** (file or directory) at the current path or at ***output-path*** if specified.

```shell
# Extract images from emblem.gci
# "--search-pattern *fz*.dat.gci" is implicit
gfz.exe emblem-to-image in/emblem.gci
```

Extract all images from an emblem archive.

Extract images inside ***input-path*** (file or directory) at the current path or at ***output-path*** if specified.

```shell
# Extract images from "emblem.bin" creating "in/emblem/emblems/"
gfz.exe emblem-to-image in/emblem/emblems.bin
```

### Convert Image to Emblem

Convert ***input-path*** (file or directory) to an emblem archive at the current path or at ***output-path*** if specified.

```shell
# Convert source image to a .gci emblem
gfz.exe image-to-emblem-gci in/source.png
```

Convert ***input-path*** (file or directory) to an emblem archive at the current path or at ***output-path*** if specified.

```shell
# Convert source images inside target directory to a .bin emblem archive
gfz.exe image-to-emblem-bin in/source/ --search-pattern *.png
```

#### Image Options

The image to emblem code path supports using `SixLabors.ImageSharp`'s `ResizeOptions` information for resizing input images before being converted to emblems. As such, any option provided for [image resize options](#image-resize-options) will be used when processing images to emblems.



## ISO Images

Actions for handling GameCube ISO images.

### Extract Files from ISO

Extract all files including system files from ISO image.

```shell
# Extract files from ISO
# Creates "in/image/" with subdirectories "./sys/" and "./files"
gfz.exe extract-iso-files in/image.iso
```



## LZ Archives

Actions for managing `.lz` archive files.

### LZ Compression

Compress ***input-path*** (file or directory) to an `.lz` archive at the current path or at ***output-path*** (file or directory) if specified.

Compress a specific file.

```shell
# Compress the file at "in/stage/st01.tpl" to "in/stage/st01.tpl.lz"
gfz.exe lz-compress in/stage/st01.tpl
```

Compress all files in folder and subfolders.

```shell
# Compress all files ending in .gma inside the "in/" directory and its subdirectories
gfz.exe lz-compress in/ --search-pattern *.gma --search-subdirs
```

#### Specifying F-Zero AX/GX Formats

By default, `gfz-cli` serializes output files in formats for F-Zero GX. The user can specify which format they wish to serialize to using the `--format`. 

Add the option `--format ax` if you wish to compress a file for use in F-Zero AX.

```shell
# Compress file to .lz meant for use with F-Zero AX
gfz.exe lz-compress in/stage/st01.gma --format ax
```

### LZ Decompression

Decompress ***input-path*** (file or directory) at current path or at ***output-path*** (file or directory).

Decompress specific `.lz` file.

```shell
# Decompress contents of "in/stage/st01.gma.lz" creating "in/stage/st01.gma"
gfz.exe lz-decompress in/stage/st01.gma.lz
```

Decompress all `.lz` files in a folder and its subfolders.

```shell
# Decompress contents of all .lz files in "in/" and its subdirectories
# "--search-pattern *.lz" is implicit
gfz.exe lz-decompress in/ --search-subdirs
```



## TPL Texture Palettes

Actions for managing `.tpl` files.

### Unpack TPL

These arguments will create a folder in the same directory as the source file with the same name as the source file containing `.png` files of the source `.tpl`'s textures.

```shell
# Unpack all textures inside "in/bg/bg_mut.tpl" to "in/bg/bg_mut/"
gfz.exe tpl-unpack in/bg/bg_mut.tpl
```

You can also unpack all `.tpl` files in a directory.

```shell
# Unpack all textures inside "in/bg/bg_mut.tpl" to "in/bg/" and its subdirectories
# "--search-pattern "*.tpl" is implicit
gfz.exe tpl-unpack in/bg/ --search-subdirs
```

### Unpacking Options

| Option Token              | Brief Description                               |
| ------------------------- | ----------------------------------------------- |
| `--unpack-mipmaps`        | Unpack mipmaps. Off by default.                 |
| `--unpack-corrupted-cmpr` | Unpack corrupted CMPR textures. Off by default. |

#### Unpack Mipmaps

When unpacking a `.tpl` file, you can ask the unpacker to also save each textures' uncorrupted mipmaps. See [Unpack Corrupted CMPR Textures](#unpack-corrupted-cmpr-textures) for more information about corrupted mipmaps.

```shell
# Unpack "in/bg/bg_mut.tpl" textures and all textures' valid mipmaps
gfz.exe tpl-unpack in/bg/bg_mut.tpl --unpack-mipmaps
```

#### Unpack Corrupted CMPR Textures

F-Zero AX and F-Zero GX `.tpl` files do not store CMPR textures correctly. Due to a bug, they can under-allocate memory. Due to the nature of this bug, mipmaps are most susceptible to this issue.

You may specify that the unpacker output these corrupted/incomplete textures. The corrupted/incomplete pixel data will appear as magenta in the output files.

```shell
# # Unpack "in/bg/bg_mut.tpl" textures and all textures' mipmaps 
gfz.exe tpl-unpack in/bg/bg_mut.tpl --unpack-mipmaps --unpack-corrupted-cmpr
```



## Image Resize Options

Some `gfz-cli` actions take advantage of `SixLabors.ImageSharp`'s `ResizeOptions` when resizing images. This table shows the options available.

https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.ResizeOptions.html

| Option Token            | Brief Description                                            |
| ----------------------- | ------------------------------------------------------------ |
| `--resize`              | Whether to resize image.                                     |
| `--resize-mode`         | How the image should be resized. See [resize mode](#resize-mode). |
| `--resampler`           | The resampler to use when scaling image. See [resamplers](#resamplers). |
| `--width`               | The desired image width. May not be result width depending on `resize-mode` option. |
| `--height`              | The desired image height. May not be result height depending on `resize-mode` option. |
| `--compand`             | Whether to compress or expand individual pixel colors when scaling image. |
| `--pad-color`           | The padding color when scaling image.                        |
| `--position`            | Anchor positions to apply to resize image. See [position](#Position-(AnchorPositionMode)). |
| `--premultiplied-alpha` | Whether to use premultiplied alpha when scaling image.       |

### Resamplers

Select which image resampler to use. These options are for the `--resampler` command.

| Option              | Description                                                  |
| ------------------- | ------------------------------------------------------------ |
| `bicubic`           | Gets the Bicubic sampler that implements the bicubic kernel algorithm W(x). |
| `box`               | Gets the Box sampler that implements the box algorithm. Similar to nearest neighbor when upscaling. When downscaling the pixels will average, merging pixels together. |
| `catmullrom`        | Gets the Catmull-Rom sampler, a well known standard Cubic Filter often used as a interpolation function. |
| `hermite`           | Gets the Hermite sampler. A type of smoothed triangular interpolation filter that rounds off strong edges while preserving flat 'color levels' in the original image. |
| `lanczos2`          | Gets the Lanczos kernel sampler that implements smooth interpolation with a radius of 2 pixels. This algorithm provides sharpened results when compared to others when downsampling. |
| `lanczos3`          | Gets the Lanczos kernel sampler that implements smooth interpolation with a radius of 3 pixels This algorithm provides sharpened results when compared to others when downsampling. |
| `lanczos5`          | Gets the Lanczos kernel sampler that implements smooth interpolation with a radius of 5 pixels This algorithm provides sharpened results when compared to others when downsampling. |
| `lanczos8`          | Gets the Lanczos kernel sampler that implements smooth interpolation with a radius of 8 pixels This algorithm provides sharpened results when compared to others when downsampling. |
| `mitchellnetravali` | Gets the Mitchell-Netravali sampler. This seperable cubic algorithm yields a very good equilibrium between detail preservation (sharpness) and smoothness. |
| `nearestneightbour` | Gets the Nearest-Neighbour sampler that implements the nearest neighbor algorithm. This uses a very fast, unscaled filter which will select the closest pixel to the new pixels position. |
| `robidoux`          | Gets the Robidoux sampler. This algorithm developed by Nicolas Robidoux providing a very good equilibrium between detail preservation (sharpness) and smoothness comparable to `mitchellnetravali`. |
| `robidouxsharp`     | Gets the Robidoux Sharp sampler. A sharpened form of the `robidoux` sampler. |
| `spline`            | Gets the Spline sampler. A separable cubic algorithm similar to `mitchellnetravali` but yielding smoother results. |
| `triangle`          | Gets the Triangle sampler, otherwise known as Bilinear. This interpolation algorithm can be used where perfect image transformation with pixel matching is impossible, so that one can calculate and assign appropriate intensity values to pixels. |
| `welch`             | Gets the Welch sampler. A high speed algorithm that delivers very sharpened results. |

### Resize Mode

Specify how image resizing is processed. These options are for the `--resize-mode` command.

| Option    |                                                              |
| --------- | ------------------------------------------------------------ |
| `boxpad`  | Pads the image to fit the bound of the container without resizing the original source. When downscaling, performs the same functionality as `pad`. |
| `crop`    | Crops the resized image to fit the bounds of its container.  |
| `manual`  | The target location and size of the resized image has been manually set. |
| `max`     | Constrains the resized image to fit the bounds of its container maintaining the original aspect ratio. |
| `min`     | Resizes the image until the shortest side reaches the set given dimension. Upscaling is disabled in this mode and the original image will be returned if attempted. |
| `pad`     | Pads the resized image to fit the bounds of its container. If only one dimension is passed, will maintain the original aspect ratio. |
| `stretch` | Stretches the resized image to fit the bounds of its container. |

### Position (AnchorPositionMode)

Anchor positions to apply to resized images. These options are for the `--position` command.

https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.AnchorPositionMode.html

| Option        |                                                              |
| ------------- | ------------------------------------------------------------ |
| `bottom`      | Anchors the position of the image to the bottom of it's bounding container. |
| `bottomleft`  | Anchors the position of the image to the bottom left side of it's bounding container. |
| `bottomright` | Anchors the position of the image to the bottom right side of it's bounding container. |
| `center`      | Anchors the position of the image to the center of it's bounding container. |
| `left`        | Anchors the position of the image to the left of it's bounding container. |
| `right`       | Anchors the position of the image to the right of it's bounding container. |
| `top`         | Anchors the position of the image to the top of it's bounding container. |
| `topleft`     | Anchors the position of the image to the top left side of it's bounding container. |
| `topright`    | Anchors the position of the image to the top right side of it's bounding container. |





## Work-in-Progress Notes

Remove instances of setting output path and instead provide that info before actions, perhaps in [About this Document](#about-this-document)?
