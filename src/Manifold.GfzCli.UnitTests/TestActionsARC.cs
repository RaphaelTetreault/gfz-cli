// TODO:
// 1. create version of Process which takes FilesCopyParams
//      consider moving args line into this type...
// 2. generate a new struct with 2 string[], a.args, b.filecopyprep
// 3. use struct to prepare files if needed, then do args


using Manifold.GFZCLI;

namespace Manifold.GfzCli.UnitTests;

public class Tests
{
    const string Input = "./res";
    const string Output = "./tests";
    const string Files = "files";
    const string Processed = "processed";
    const string Marker = "lz";
    readonly string[] AllGameCodes = ["gfze01", "gfzj01", "gfzp01", "gfzj8p"];
    readonly string[] GCGameCodes = ["gfze01", "gfzj01", "gfzp01"];

    public string[] Process(string template, CliActionID cliActionID, string[] gameCodes)
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

    //public string[] Process(string template, FileCopyParams @params, CliActionID cliActionID, string[] gameCodes)
    //{
    //    string[] args = new string[gameCodes.Length];
    //    for (int i = 0; i < args.Length; i++)
    //    {
    //        args[i] = template
    //            .Replace("<ACTION>", cliActionID.ToString().Replace("_", "-"))
    //            .Replace("<GAMECODE>", gameCodes[i]);
    //    }
    //    return args;
    //}


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


    string[] LzCompressArgs => Process(LzCompressTemplate, CliActionID.lz_compress, AllGameCodes);
    string[] LzDecompressArgs => Process(LzDecompressTemplate, CliActionID.lz_decompress, AllGameCodes);

    // TODO: move?
    [SetUp]
    public void Setup()
    {
        GfzCliTestRunner.SetCurrentWorkingDirectory();
        GfzCliTestRunner.CopyFiles(lzc);
    }

    [Test] public void _SetupBlank() => Console.WriteLine("Setup completed.");
    [Test] public void LzCompressFile() => GfzCliTestRunner.RunArgsAssertPass(LzCompressArgs);
    [Test] public void LzDecompressFile() => GfzCliTestRunner.RunArgsAssertPass(LzDecompressArgs);

}
