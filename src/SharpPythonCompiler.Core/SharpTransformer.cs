using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        private static async Task<Solution> TransformNode(Solution solution, SemanticModel model, CSharpSyntaxNode node)
        {
            var originalSymbol = model.GetDeclaredSymbol(node);
            var newName = ConvertToPythonName(originalSymbol.Name);
            return await Renamer.RenameSymbolAsync(solution, originalSymbol, newName, solution.Workspace.Options);
        }

        private static async Task<IEnumerable<ISyntaxNodeFinder>> GetItemsForTransform(Project project, Compilation compilation, Document doc)
        {
            var nodeFinders = new List<ISyntaxNodeFinder>();
            
            var root = await doc.GetSyntaxRootAsync();
            var model = compilation.GetSemanticModel(root.SyntaxTree);

            foreach (var classDeclaration in root.DescendantNodesAndSelf().OfType<ClassDeclarationSyntax>())
            {
                var classNodeFinder = new ClassNodeFinder(classDeclaration.Identifier.Text);

                var publicMembers = classDeclaration.Members.Where(m => m.Modifiers.Any(mm => mm.Text == "public"));
                
                var memberCount = 0;

                foreach (var member in publicMembers)
                {
                    if (member is MethodDeclarationSyntax methodDelcaration)
                    {
                        var parameterCount = 0;

                        var methodNodeFinder = new MethodNodeFinder(classNodeFinder, methodDelcaration.Identifier.Text);

                        foreach (var parameter in methodDelcaration?.ParameterList?.Parameters)
                        {
                            if (!parameter.Identifier.Text.Equals(ConvertToPythonName(parameter.Identifier.Text)))
                            {
                                var parameterNodeFinder = new MethodParameterNodeFinder(methodNodeFinder, parameter.Identifier.Text);
                                nodeFinders.Add(parameterNodeFinder);
                                parameterCount++;
                            }
                        }

                        if (!methodDelcaration.Identifier.Text.Equals(ConvertToPythonName(methodDelcaration.Identifier.Text))
                            || parameterCount > 0)
                        {
                            nodeFinders.Add(methodNodeFinder);
                            memberCount++;
                        }
                    }
                    else if (member is PropertyDeclarationSyntax propertyDeclaration)
                    {
                        if (!propertyDeclaration.Identifier.Text.Equals(ConvertToPythonName(propertyDeclaration.Identifier.Text)))
                        {
                            var propertyNodeFinder = new PropertyNodeFinder(classNodeFinder, propertyDeclaration.Identifier.Text);
                            nodeFinders.Add(propertyNodeFinder);
                            memberCount++;
                        }
                    }
                }

                if (!classDeclaration.Identifier.Text.Equals(ConvertToPythonName(classDeclaration.Identifier.Text))
                    || memberCount > 0)
                {
                    nodeFinders.Add(classNodeFinder);
                }
            }

            return nodeFinders;
        }

        public static async Task<Solution> TransformProject(Project project)
        {
            var projectId = project.Id;
            var compilation = await project.GetCompilationAsync();

            var documents = new List<KeyValuePair<DocumentId, IEnumerable<ISyntaxNodeFinder>>>();

            foreach (var doc in project.Documents)
            {
                documents.Add(new KeyValuePair<DocumentId, IEnumerable<ISyntaxNodeFinder>>(doc.Id, await GetItemsForTransform(project, compilation, doc)));
            }

            var solution = project.Solution;

            foreach (var d in documents)
            {
                var nodeFinders = d.Value;

                foreach (var finder in nodeFinders)
                {
                    var doc = project.GetDocument(d.Key);
                    var root = await doc.GetSyntaxRootAsync();
                    var model = compilation.GetSemanticModel(root.SyntaxTree);

                    solution = await TransformNode(solution, model, finder.Find(root as CSharpSyntaxNode));
                    project = solution.GetProject(projectId);
                    compilation = await project.GetCompilationAsync();
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
