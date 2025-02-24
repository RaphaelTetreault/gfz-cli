using Manifold.GFZCLI;

namespace Manifold.GfzCli.UnitTests;

public class Tests
{
    readonly string ArgLzCompressFile = $"{CliActionID.lz_compress} ./lz/lz-compress-file -p *.tpl";



    [SetUp]
    public void Setup()
    {
        OSPath cwd = new(Directory.GetCurrentDirectory());
        cwd.AppendRelativePathToDirectories(@"..\..\..\..\unit-tests\");
        Directory.SetCurrentDirectory(cwd);

        Console.WriteLine(cwd);
    }

    [Test] public void LzCompressFile() => GfzCliTestRunner.RunArgsAssertPass(ArgLzCompressFile);
    
}
