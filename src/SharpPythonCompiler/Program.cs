using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using SharpPythonCompiler.Core;

namespace SharpPythonCompiler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();

            var slnOrCsprojFile = args[0];
            var outputDir = args[1];

            if (!Path.IsPathRooted(slnOrCsprojFile))
                slnOrCsprojFile = Path.Combine(Environment.CurrentDirectory, slnOrCsprojFile);

            if (!Path.IsPathRooted(outputDir))
                outputDir = Path.Combine(Environment.CurrentDirectory, outputDir);       

            using (var workspace = MSBuildWorkspace.Create())
            {
                if (slnOrCsprojFile.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    await SharpCompiler.CompileSolution(await workspace.OpenSolutionAsync(slnOrCsprojFile), outputDir);
                }
                else
                {
                    await SharpCompiler.CompileProject(await workspace.OpenProjectAsync(slnOrCsprojFile), outputDir);
                }
            }
        }
    }
}
