From: [F-Zero GX - Emblem file format](https://docs.google.com/document/d/1c4a7d6xZ-rnK-E5p7d__6V1qhLxcdOHwAmv-FR8K50s/edit?tab=t.0#heading=h.sykb28zg8xhj)

# Emblem file format

Mainly written by Joselle (other sources credited where relevant)

## **Summary**

Format: (Byte offset from start of file, Number of bytes)

* (0x0, 8\) "GFZE8P" followed by 0xFF 02 (for North American region)  
* (0x8, 2\*16) filename as string, starting at fze and ending at .dat, followed by 0x00  
* (0x28, 4\) Timestamp for GX’s in-game file listing  
* (0x2C, 9\) 0x00 00 00 60 00 02 00 03 04  
* (0x35, 1\) Number of times file has been copied  
* (0x36, 2\) Start block of file in memory card  
* (0x38, 8\) 0x00 03 FF FF 00 00 00 04  
* (0x40, 2\) Checksum  
* (0x42, 2\) 0x04 01  
* (0x44, 2\*16) "F-ZERO GX" (the file's game title), followed by 0x00s  
* (0x64, 60\) "YY/MM/DD HH:MM" (the file's comment), followed by 0x00s  
* (0xA0, 24\*256) Banner pixel data (96 x 32 pixels)  
* (0x18A0, 8\*256) Icon pixel data (32 x 32 pixels)  
* (0x20A0, 2\*16\*256) Emblem pixel data (64 x 64 pixels)  
* (0x40A0, until 0x603F) 0x00 padding

Total size: 0x6040 bytes \= 0x40 header \+ (3 blocks \* 0x2000 per block)

## **Header (0x0-0x3F) in general**

All laid out in this struct:

[https://github.com/suloku/gcmm/blob/master/source/gci.h](https://github.com/suloku/gcmm/blob/master/source/gci.h)

Generally, this document won’t emphasize the parts that are the same for every GX emblem file. But do note that bytes that are listed as constants here won’t necessarily be constants for every Gamecube memcard file.

## **Filename (0x8, 2\*16)**

Format: fze0200002000\<HEX\>.dat

\<HEX\> seems to be a file creation timestamp: (Number of seconds since 2000/01/01 00:00) \* 40,500,000, and then converted to hexadecimal.

(Of course I couldn’t confirm that all the digits of \<HEX\> are this timestamp, due to the extreme precision involved. The first 7 out of 14 digits I’m pretty sure of, but after that the digits may as well be random. I also don’t know what happens in the year 2057 when it has to go to 16 digits.)

This timestamp should match up with the file’s Comment. It might not match up with the timestamp field (at 0x28) if the file has been copied.

This is just the inner part of the actual filename that you get when exporting with Dolphin emulator or GC Memory Manager (Homebrew). The full filename is: 8P-GFZE-fze0200002000\<HEX\>.dat.gci

The game has some rules on what emblem filenames it accepts, but they don’t have to be in the exact format above. This is nice to know, because it’s hard to distinguish which file is which just from an integer timestamp; being able to name files more descriptively can help when you have many emblem files.

Filenames GX accepts:

* fze02000020001.dat  
* fze020000000012345678901234.dat  
* fze020000200012345678901234.dat  
* fze020006789012345678901234.dat  
* fze113456789012345678901234.dat  
* fze123456789012345678901234.dat  
* fze1234567890abcdef.dat  
* fze1234567890abcdefg.dat  
* fze12345-my-emblem.dat  
* fze12-my-emblem.dat  
* fze1-my-emblem.dat  
* Probably the best format for having descriptive filenames: **fze123-my-emblem.dat**

Filenames that make GX load the emblem in a buggy way:

* fze003456789012345678901234.dat (first 2 rows loaded, part of the 3rd and 4th rows loaded, no other pixels loaded)  
* fze0-my-emblem.dat (about half of the pixels loaded)

Filenames GX doesn’t load in the emblem list:

* fze000000000000000000000000.dat  
* fze000000000012345678901234.dat  
* fze000000200012345678901234.dat  
* fze0200002000123456789012345678  
* fze123456789012345678901234  
* fze123456789012345678901234.gci  
* fze1234567890123456789012345dat  
* fze-234567890123456789012345678

With certain filenames, GX will crash on the memory card loading screen when it tries to load the emblem list, depending on how many emblem files there are on the memory card. As tested by StarkNebula:

* If “fze” is followed by a non-number, such as in fze-23456789012345678901234.dat, then the loading screen will crash if there are more than 2 emblem files.   
* With “fze0” followed by non-numbers, it loads fine with at least 20-something emblem files (possibly more).  
* With “fze020” followed by non-numbers, it loads fine with at least 98 emblem files (possibly more).

## **Timestamp for GX’s in-game file listing (0x28, 4\)**

When you are in the Emblem Editor and you tell F-Zero GX to load an existing emblem, it lists the emblems on your memory card(s) along with a date and time.

This field represents an emblem’s date and time as number of seconds since 2000/01/01 00:00.

Examples:

2015/03/26 20:43 \+ 46 seconds \= (((15\*365+4+31+28+25 days) \* 24 \+ 20 hrs) \* 60 \+ 43 mins) \* 60 \+ 46 secs in hex \= 0x1CA72C02

2005/12/09 23:37 \+ 52 seconds \= (((5\*365+2+31+28+31+30+31+30+31+31+30+31+30+8 days) \* 24 \+ 23 hrs) \* 60 \+ 37 mins) \* 60 \+ 52 secs in hex \= 0x0B2CD1D0

Note that the number of seconds (46 and 52 in the examples) isn’t in the displayed timestamp, but we can figure it out using the hex number in the file.

This timestamp might not match up with the file’s Comment, if the file has been copied between memory cards before. (Comment \= creation time. This timestamp, however, seems to change when copied between memory cards.)

## **Start block of file in memory card (0x36, 2\)**

Not sure if this Dolphin Memcard Manager error is related \- it happens sometimes when I try to delete an emblem file that I generated myself:

“Order of files in the File Directory do not match the block order

Right click and export all of the saves,

and import the saves to a new memcard”

## **Comment address (0x3C, 4\)**

Always 4 for GX emblem files, but a note on how this works for those curious:

A GC memcard reader expects that (0x40 \+ this value) is the byte address of the file Title (expected to be 32 bytes, padded with 0x00 at the end) followed by the file Comment.

I tried increasing this value by 1, and it made Dolphin’s Memcard Manager miss the first character of the Title and the first character of the Comment \- e.g. “-ZERO GX”, “5/03/26 20:45”.

## **Checksum (0x40, 2\)**

At this point the standard .gci header has ended (according to [https://code.google.com/p/gcmm/source/browse/trunk/source/gci.h](https://code.google.com/p/gcmm/source/browse/trunk/source/gci.h)) so the content type is game-specific.

This checksum is the reason why F-Zero GX detects your emblem file as corrupt if you take an existing emblem file and try to modify its pixel values.

The workings of the F-Zero GX memory card file checksum were found by Ralf (of GSCentral). Assuming I have my terms right, it’s a 16-bit CRC with a start value of 0xFFFF, and a generator polynomial of 0x8408. The checksum is performed on all bytes from right after this checksum, to the end of the file.

The C code to perform it is under the FZEROGX\_MakeSaveGameValid function here (line 949 as of 2015/04/03): [https://code.google.com/p/gcmm/source/browse/trunk/source/mcard.c](https://code.google.com/p/gcmm/source/browse/trunk/source/mcard.c)

When F-Zero GX loads a memory card file, if the checksum does not agree with the checked bytes, it identifies that as a corrupt file. In the case of emblem files, the file appears in the emblem file list as “Corrupt File” and when you highlight it, the game says the file is corrupt and must be deleted. The game doesn’t let you open a corrupt emblem file.

## **0x42**

This is the same (0x04 01\) for any emblem file I’ve seen, but other GX files have different values. My game data is 0x01 0B, garage is 0x03 01, ghosts are 0x02 01, replays are 0x05 04\.

Other versions (besides North America) might have different codes, such as 0x05 03 for JP replays?

## **Pixel data**

### **Pixel representation**

Each pixel is represented by 2 bytes (16 bits). The bit-level format is:

NRRRRRGGGGGBBBBB

R \= red value divided by 8\. For example, if this is 00101, then the red value of this pixel is 5\*8 \= 40\.

G \= green value divided by 8\.

B \= blue value divided by 8\.

N \= null flag. If R, G, and B are all 0, and N is 1, it’s a black pixel; if N is 0, it’s a blank pixel.

Yes, this means that emblems are only ever saved with RGB values that are multiples of 8 (0, 8, 16, …, 248), despite the fact that the slider shows 0 to 255\. If you specify an RGB value that’s not a multiple of 8, then it’s rounded down to a multiple of 8 when saving. (You can confirm this by re-loading your emblem and then using the Syringe tool (X button)).

Be careful with N; this isn’t exactly like an alpha value. If you have R=G=B=31 and N \= 0, it’s white instead of a blank pixel. It seems that you need all the bits to be 0 to specify a blank pixel.

### **Pixel order**

It goes by 4x4 blocks of pixels. It starts from the upper leftmost 4x4, and goes like this through those 4x4 pixels:

| 1 | 2 | 3 | 4 |
| :---- | :---- | :---- | :---- |
| 5 | 6 | 7 | 8 |
| 9 | 10 | 11 | 12 |
| 13 | 14 | 15 | 16 |

Then it moves to the 4x4 square right of that, and keeps going right until it reaches the end of the top 4 rows. Then it moves to the next 4 rows. If you divided the 64x64 emblem pixels into 256 squares of 4x4 size, these squares would be traversed in this order:

| 1 | 2 | 3 | 4 | ... | 16 |
| :---- | :---- | :---- | :---- | :---- | :---- |
| 17 | 18 | 19 | 20 | ... | 32 |
| ... | ... | ... | ... | ... | ... |
| 241 | 242 | 243 | 244 | ... | 256 |

## **Banner pixel data**

This is the banner (image that’s 3 times as wide as it is high) that you see with the file in a Gamecube memory manager. It’s 96 x 32 pixels. For emblem files, the left 2/3rds of the banner are the same for every emblem file, and the right 1/3rd is a small display of the emblem.

## **Icon pixel data**

This is the icon (square image) that you see with the file in a Gamecube memory manager. It’s 32 x 32 pixels. The icon is the same for every emblem file.

## **Emblem pixel data**

The emblem editor doesn’t allow you to color the pixels at the very edge, so in effect, you get 62 x 62 instead of 64 x 64 pixels with the emblem editor.

However, if you create an emblem that’s 64 x 64, then the edges’ color spill out far beyond the emblem itself when you attach the emblem to your machine ([screenshot](https://twitter.com/yoshifan28/status/583896784201584640)). So it’s best to use only 62 x 62 pixels (unless you can make creative use of that color spill effect...).

One question that may arise about the 2-byte-per-pixel format is this: Why didn’t they use 3 bytes per pixel so that they could use a 0-255 RGB range? After all, the emblem file format is just over 2 blocks large (1 block \= 8 KB \= 2\*16\*256 bytes), and if you used 3 bytes per pixel on the emblem pixel data, then it would still be within 3 blocks.

There are two reasons I can see.

**You actually need 3 bytes plus 1 bit for this, because blank pixels are permitted.** So either you go to 4 bytes per pixel and end up going just over 3 blocks, or you must do some tricky encoding scheme where a pixel is somewhere in between 3 and 4 bytes.

**It’s not just about the emblem file itself.** You can attach an emblem to a custom machine, and save that emblem-attachment data to a garage file. The emblem’s pixel data must be copied to the garage file, because this garage file’s validity must not depend on the original emblem file’s existence on the same memory card. Such a garage file can have up to 16 emblems (4 machines x 4 emblems each). Similarly, a replay file can have a custom machine with up to 4 emblems.

Thus, even if 2 bytes per pixel (emblem pixel data \= 1 block) versus 3+ bytes per pixel (emblem pixel data \= 1.5+ blocks) didn’t increase the emblem file’s block count, it would certainly increase the block count of replay and garage files. Indeed, it seems that a replay gets 4 blocks (= 4 emblems) added if a custom machine is used. And a garage file is 18 blocks, probably due to the 16 blocks’ worth of emblems that can be there.

The better questions are: Why does the game still add space for all the possible emblems to your garage/replay files, even when your machines only have 1 or 2 emblems (or no emblems)? And why doesn’t it encode the emblem pixel data efficiently like a PNG, instead of uncompressed like a BMP? (Though, the latter point could maybe be forgiven since it was 2003 when they made this game; I’m not sure if PNG was widespread at the time.)

Another note on filesize: a completely blank emblem’s non-zero data ends just before this emblem pixel data section, thus making the non-zero data end slightly after 1 block; but such an emblem file is still considered 3 blocks large.

## **0x00 padding (0x40A0)**

This consists entirely of zero-value bytes in any emblem file created in GX, but you’re free to add whatever you want (such as string comments) in these padding bytes. (Tested by StarkNebula)

Just remember that these padding bytes are included in the checksum computation.

## **Code to create an emblem file from an image file (PNG, JPG, etc.)**

Python code: [http://pastebin.com/BftBzmxw](http://pastebin.com/BftBzmxw)

A web version is being planned as well.

## **Other related Links**

Gamecube memory card structure

[http://hitmen.c02.at/files/yagcd/yagcd/chap12.html\#sec12](http://hitmen.c02.at/files/yagcd/yagcd/chap12.html#sec12)

