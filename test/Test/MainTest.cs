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
using SharpPythonCompiler.Core;

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
            var solution = document.Project.Solution;

            solution = await SharpTransformer.TransformSolution(solution);

            var ass = await SharpCompiler.CompileProject(solution.Projects.FirstOrDefault());            

            var allTypes = ass.GetTypes();
            
            Assert.NotNull(ass.GetType("Test.my_class"));
        }
    }
}
