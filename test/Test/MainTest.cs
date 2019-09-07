using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Microsoft.CodeAnalysis.Rename;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;

namespace Test
{
    public class MainTest
    {
        private static Document GetProjectDocumentForCode(string code)
        {
            var workspace = new AdhocWorkspace();
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var references = new List<MetadataReference>() { mscorlib };
            var solution = workspace.CurrentSolution;

            var project = solution.AddProject("TestProject", "TestProject", LanguageNames.CSharp);
            project = project.AddMetadataReference(mscorlib);
            project = project.WithCompilationOptions(new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Debug
            ));
            return project.AddDocument("TestFile.cs", code);
        }

        private string ConvertToPythonName(string name)
        {
            //MyClass => my_class
            //_myClass => _my_class
            //myClass => my_class
            //_my_Class => _my_class

            var output = new char[name.Length * 2];
            var j = 0;

            var prevChar = '\0';

            const char UNDERSCORE = '_';

            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];

                if (char.IsUpper(c))
                {
                    if (i != 0 && prevChar != UNDERSCORE)
                    {
                        output[j++] = UNDERSCORE;
                    }

                    output[j++] = char.ToLower(c);
                }
                else
                {
                    output[j++] = c;
                }

                prevChar = c;
            }

            return new string(output, 0, j);
        }

        [Fact]
        public async Task TransformClassName()
        {
            var code = @"
    using System;

    namespace Test
    {
        public class MyClass
        {
            public MyClass()
            {            
            }

            public void MyTestMethod()
            {            
            }
        }
    }";

            var document = GetProjectDocumentForCode(code);
            var compilation = await document.Project.GetCompilationAsync();
            
            var root = await document.GetSyntaxRootAsync();
            var model = compilation.GetSemanticModel(root.SyntaxTree);

            var solution = document.Project.Solution;
            
            foreach (var oneClass in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            {
                var originalSymbol = model.GetDeclaredSymbol(oneClass);                
                var newClassName = ConvertToPythonName(originalSymbol.Name);
                solution = await Renamer.RenameSymbolAsync(solution, originalSymbol, newClassName, solution.Workspace.Options);
            }

            var assemblies = new List<Stream>();

            foreach (var projectId in solution.ProjectIds)
            {
                var stream = new MemoryStream();
                var result = (await solution.GetProject(projectId).GetCompilationAsync()).Emit(stream);
                Assert.True(result.Success);
                assemblies.Add(stream);
            }

            var assemblyMemory = assemblies.FirstOrDefault();
            var assemblyData = new byte[assemblyMemory.Length];
            assemblyMemory.Seek(0, SeekOrigin.Begin);
            assemblyMemory.Read(assemblyData, 0, assemblyData.Length);
            var ass = Assembly.Load(assemblyData);

            var allTypes = ass.GetTypes();

            Assert.NotNull(ass.GetType("Test.my_class"));
        }
    }
}
