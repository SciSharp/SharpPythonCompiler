using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace SharpPythonCompiler.Core
{
    public static class SharpCompiler
    {
        public static async Task CompileSolution(Solution solution, string outputDir)
        { 
            solution = await SharpTransformer.TransformSolution(solution);

            foreach (var projectId in solution.ProjectIds)
            {
                await CompileProjectInternal(solution.GetProject(projectId), outputDir);
            }
        }

        public static async Task CompileProject(Project project, string outputDir)
        {
            var projectId = project.Id;
            var solution = await SharpTransformer.TransformProject(project);
            await CompileProjectInternal(solution.GetProject(projectId), outputDir);
        }

        private static Project GiveNewAssemblyName(Project project)
        {
            return project.WithAssemblyName(project.AssemblyName + ".Py");
        }

        internal static async Task CompileProjectInternal(Project project, string outputDir)
        {
            project = GiveNewAssemblyName(project);
            
            // ensure the directory does exist
            Directory.CreateDirectory(outputDir);

            var assemblyFilePath = Path.Combine(outputDir, project.AssemblyName + ".dll");

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
            var projectId = project.Id;
            var solution = await SharpTransformer.TransformProject(project);
            return await CompileProjectInternal(solution.GetProject(projectId));
        }

        public static async Task<Assembly> CompileProjectInternal(Project project)
        {
            project = GiveNewAssemblyName(project);

            // generate the assembly
            using (var stream = new MemoryStream())
            {
                var result = (await project.GetCompilationAsync()).Emit(stream);

                if (!result.Success)
                {
                    throw new Exception(result.Diagnostics.FirstOrDefault().GetMessage());
                }

                await stream.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);

                var rawAssembly = new byte[stream.Length];
                stream.Read(rawAssembly, 0, rawAssembly.Length);

                return Assembly.Load(rawAssembly);
            }
        }
    }
}
