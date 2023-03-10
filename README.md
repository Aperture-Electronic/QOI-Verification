# QOI Research & Verification Tools

## Requirement
OS: Windows/Linux  
Runtime: .NET Runtime 7.0

All executable files are under *ProjectPath*/bin/Debug/net7.0/

## Test Bitmap Converter
This tool can read all images in a directory and convert them to 24-bpp bitmap (.BMP) files.

Usage: 
* Linux  
> dotnet TestBitmapConverter.dll **\<root_path>** **\<out_path>**
* Windows  
> ./TestBitmapConverter **\<root_path>** **\<out_path>**

**root_path**: Root path of image set. It can has many sub-directory in it and contains multiple PNG images.
**out_path**: Output path, all Bitmap images will output to the path. And the original sub-directory name will appears at the perfix of filename with "\_\_\_" separator.

## Batch Compression Test
* Linux
> dotnet BatchCompressionTest.dll **<test_program>** **<test_set_path>**
* Windows
> ./BatchCompressionTest **<test_program>** **<test_set_path>**

**test_program**: Test program path. Please note that you need use the corresponding platform program to test, i.e. under Linux you should use Linux executable program compiled by GCC, etc. The test program has some command-line format which shows in follows.

**test_set_path**: Test set path. This path contains many Bitmap files as test set. It can be generated by the *TestBitmapConverter*.

## Test Program Requirement & Format
The test program has a certain command-line format that batch test program can use shell argument to control and run it.

### Arguments
There are 3 arguments on the test program, like
> ./test **\<command>** **\<source>** **\<target>**

**command**: The command argument has 3 choice
1. -e: Encode (Compression)
2. -d: Decode (Decompression)
3. -c: Compare (Compare the image in pixel level)

**source**, **target**:
The source and target is the input file path to the test program. Under *Encode* mode, the *source* is the the Bitamp file to compress, and *target* is the output file. Under *Decode* mode, the *source* file is the compressed file to decode, and the *target* file is output original Bitmap file. And, under *Compare* mode, the *source* and *target* are two files to compare between.

### Standard output
The standard output of *Encode* mode has a requirement.  
Under *Encode* mode, program must outputing an line of statistic information head like
> -- QOI Encoding Statistic --

and outputing each encoding type and its count by the format list as follows
> type\_name = count
> 
This output is used to statistic.

### Return
The program must return 0 if it working correctly.
And the program can return other values (not equal 0) when error occurred, to let the *BatchCompressionTest* can detect the error.
