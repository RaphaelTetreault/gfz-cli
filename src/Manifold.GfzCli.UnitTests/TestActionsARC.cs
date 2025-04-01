// TODO:
// 1. create version of Process which takes FilesCopyParams
//      consider moving args line into this type...
// 2. generate a new struct with 2 string[], a.args, b.filecopyprep
// 3. use struct to prepare files if needed, then do args


using Manifold.GFZCLI;
using Newtonsoft.Json;

namespace Manifold.GfzCli.UnitTests;

public readonly record struct CliDebugData
{
    public readonly required string[] CliArgs { get; init; }
}



public class Tests
{
    const string Input = "./res";
    const string Output = "./tests";
    const string Files = "files";
    const string Processed = "processed";
    const string Marker = "lz";
    static readonly string[] AllGameCodes = ["gfze01", "gfzj01", "gfzp01", "gfzj8p"];
    static readonly string[] GCGameCodes = ["gfze01", "gfzj01", "gfzp01"];

    const string FilesDir = "files/";
    const string ProcessedDir = "processed/";


    public string[] PrepareTestAndCliArgs(string template, CliActionID cliActionID, string[] gameCodes)
    {
        string[] args = new string[gameCodes.Length];
        for (int i = 0; i < args.Length; i++)
        {
            args[i] = template
                .Replace("<ACTION>", cliActionID.ToString().Replace("_", "-"))
                .Replace("<GAMECODE>", gameCodes[i]);
        }
        return args;
    }



    // TODO: use diff input after solving file copy setup.
    readonly string LzCompressTemplate = $"<ACTION> ./{Output}/{Marker}/<ACTION>/ -p *.tpl -o";
    //readonly string LzCompressTemplate = $"<ACTION> ./{Input}/<GAMECODE>/{Processed}/bg/ ./{Output}/{Marker}/<ACTION>/ -p *.tpl -o";
    readonly string LzDecompressTemplate = $"<ACTION> ./{Input}/<GAMECODE>/{Files}/bg/ ./{Output}/{Marker}/<ACTION>/ -p *.tpl.lz -o";

    readonly FileCopyParams lzc = new()
    {
        FilesSource = $"./{Input}/gfzj01/{Processed}/bg/",
        FilesDestination = $"./{Output}/{Marker}/{CliActionID.lz_compress.ToString().Replace("_", "-")}/",
        //FilesSource = $"./{Input}/<GAMECODE>/{Processed}/bg/",
        //FilesDestination = $"./{Output}/{Marker}/<ACTION>/",
        SearchPattern = "*.tpl",
        SearchOption = SearchOption.TopDirectoryOnly,
        Overwrite = false,
        FileCopyLimit = 3,
    };


    string[] LzCompressArgs => PrepareTestAndCliArgs(LzCompressTemplate, CliActionID.lz_compress, AllGameCodes);
    string[] LzDecompressArgs => PrepareTestAndCliArgs(LzDecompressTemplate, CliActionID.lz_decompress, AllGameCodes);

    // TODO: move?
    [SetUp]
    public void Setup()
    {
        GfzCliTestRunner.SetCurrentWorkingDirectory();
    }

    //[Test] public void _SetupBlank() => Console.WriteLine("Setup completed.");
    //[Test] public void LzCompressFile() => GfzCliTestRunner.RunArgsAssertPass(LzCompressArgs);
    //[Test] public void LzDecompressFile() => GfzCliTestRunner.RunArgsAssertPass(LzDecompressArgs);




    [Test] public void LzCompressFile() => GfzCliTestRunner.RunArgsAssertPass(new CliDebugParams()
    {
        CliActionID = CliActionID.lz_compress,
        CliArg = $"<ACTION> <TESTDIR> -p *.tpl -o",
        GameCodes = AllGameCodes,
        CopySubdirectory = ProcessedDir + "bg/",
        TestSubdirectory = "", // subdir in generated test folder
        SrcCopySearchPattern = "*.tpl",
        SrcCopySearchOption = SearchOption.TopDirectoryOnly,
        DstCopyOverwrite = false,
        SrcCopyLimit = 3,
    }.PrepareAndGenerateTestCliArgs());

    //public static readonly CliDebugParams LZC = new()
    //{
    //    CliActionID = CliActionID.lz_compress,
    //    CliArg = $"<ACTION> <TESTDIR> -p *.tpl -o",
    //    GameCodes = AllGameCodes,
    //    SrcSubdirectory = FilesDir + "bg/",
    //    DstSubdirectory = ProcessedDir, // other subdirs generated?
    //    SrcCopySearchPattern = "*.tpl",
    //    SrcCopySearchOption = SearchOption.TopDirectoryOnly,
    //    DstCopyOverwrite = false,
    //    SrcCopyLimit = 3,
    //}; 




}
