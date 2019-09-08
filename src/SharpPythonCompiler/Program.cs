using System;
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
        static void Main(string[] args)
        {
            var slnOrCsprojFile = args[0];
            var outputDir = args[1];

            if (!Path.IsPathRooted(slnOrCsprojFile))
                slnOrCsprojFile = Path.Combine(AppContext.BaseDirectory, slnOrCsprojFile);

            MSBuildLocator.RegisterDefaults();

            if (slnOrCsprojFile.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                SharpCompiler.CompileSolution(LoadSolution(slnOrCsprojFile).Result, outputDir).Wait();
            }
            else
            {
                SharpCompiler.CompileProject(LoadProject(slnOrCsprojFile).Result, outputDir).Wait();
            }            
        }

        private static async Task<Solution> LoadSolution(string solutionFile)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                return await workspace.OpenSolutionAsync(solutionFile);
            }
        }

        private static async Task<Project> LoadProject(string projectFile)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                return await workspace.OpenProjectAsync(projectFile);
            }
        }
    }
}
