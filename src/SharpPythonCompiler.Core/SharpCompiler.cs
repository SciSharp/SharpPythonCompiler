using System;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SharpPythonCompiler.Core
{
    public static class SharpCompiler
    {
        public static async Task CompileSolution(string solutionFile, string outputDir)
        {
            var workspace = new AdhocWorkspace();
            workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Default, filePath: solutionFile));   
            var solution = await SharpTransformer.TransformSolution(workspace.CurrentSolution);

            foreach (var projectId in solution.ProjectIds)
            {
                await CompileProject(solution.GetProject(projectId), outputDir);
            }
        }

        public static async Task CompileProject(Project project, string outputDir)
        {
            var assemblyName = project.AssemblyName;
            
            // ensure the directory does exist
            Directory.CreateDirectory(outputDir);

            var assemblyFilePath = Path.Combine(outputDir, assemblyName + ".dll");

            // generate the assembly
            using (var stream = File.Create(assemblyFilePath))
            {
                (await project.GetCompilationAsync()).Emit(stream);
                await stream.FlushAsync();
                stream.Close();
            }
        }

        public static async Task<Assembly> CompileProject(Project project)
        {
            var assemblyName = project.AssemblyName;

            // generate the assembly
            using (var stream = new MemoryStream())
            {
                (await project.GetCompilationAsync()).Emit(stream);
                await stream.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                var rawAssembly = new byte[stream.Length];
                stream.Read(rawAssembly, 0, rawAssembly.Length);

                return Assembly.Load(rawAssembly);
            }
        }
    }
}
