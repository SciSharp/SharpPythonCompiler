using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpPythonCompiler.Core
{
    class FieldNodeFinder : ISyntaxNodeFinder
    {
        public ClassNodeFinder ClassNodeFinder { get; private set; }

        public string Name { get; private set; }

        public FieldNodeFinder(ClassNodeFinder classNodeFinder, string name)
        {
            ClassNodeFinder = classNodeFinder;
            Name = name;
        }

        public CSharpSyntaxNode Find(CSharpSyntaxNode root)
        {
            foreach (var feedDeclaration in (ClassNodeFinder.Find(root) as ClassDeclarationSyntax).Members.OfType<FieldDeclarationSyntax>())
            {
                foreach (var variableDeclaration in feedDeclaration.Declaration.Variables)
                {
                    if (variableDeclaration.Identifier.Text == Name)
                    {
                        return variableDeclaration;
                    }
                }
            }

            return null;
        }
    }
}