using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Lib;

namespace TestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var srcDir = "./source/";
            var outputDir = "./result/";
            
            var fileNames = Directory.GetFiles(srcDir, "*.cs").Select(Path.GetFileName);
            var blockOptions = new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = 4};
            
            var loadSource = new TransformBlock<string, string>(
                async inputFilePath => await File.ReadAllTextAsync(inputFilePath), blockOptions);

            var parser = new Parser();
            var generator = new Generator(parser);
            
            var generateTestsBlock = new TransformBlock<string, string>(
                async classSrcCode => await generator.testCodeFromClassCodeTask(classSrcCode), blockOptions);

            var writeTestsIntoFile = new ActionBlock<string>(
                async testClassSrcCode =>
                {
                    await File.WriteAllTextAsync($"{outputDir}/{Guid.NewGuid()}.cs",
                        testClassSrcCode);
                }, blockOptions);

            var linkOptions = new DataflowLinkOptions {PropagateCompletion = true};

            loadSource.LinkTo(generateTestsBlock, linkOptions);
            generateTestsBlock.LinkTo(writeTestsIntoFile, linkOptions);

            foreach (var fileName in fileNames)
            {
                loadSource.SendAsync($"{srcDir}/{fileName}");
            }

            loadSource.Complete();

            try
            {
                writeTestsIntoFile.Completion.Wait();
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                {
                    switch (e)
                    {
                        case FileNotFoundException _:
                        case SyntaxException _:
                            Console.WriteLine(e.Message);
                            break;
                        default:
                            throw;
                    }
                }
            }
        }
    }
}
