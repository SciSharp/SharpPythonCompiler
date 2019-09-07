using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace SharpPythonCompiler.Core
{
    public static class SharpTransformer
    {
        public static async Task<Solution> TransformSolution(Solution solution)
        {
            var outputSolution = solution;

            foreach (var projectId in solution.ProjectIds)
            {
                outputSolution = await TransformProject(solution.GetProject(projectId));
            }

            return outputSolution;
        }

        public static async Task<Solution> TransformProject(Project project)
        {
            var solution = project.Solution;
            var compilation = await project.GetCompilationAsync();            
            
            foreach (var doc in project.Documents)
            {
                var root = await doc.GetSyntaxRootAsync();
                var model = compilation.GetSemanticModel(root.SyntaxTree);

                foreach (var classDeclaration in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
                {
                    var originalSymbol = model.GetDeclaredSymbol(classDeclaration);                
                    var newClassName = ConvertToPythonName(originalSymbol.Name);
                    solution = await Renamer.RenameSymbolAsync(project.Solution, originalSymbol, newClassName, solution.Workspace.Options);
                }
            }

            return solution;
        }

        private static string ConvertToPythonName(string name)
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
    }
}
