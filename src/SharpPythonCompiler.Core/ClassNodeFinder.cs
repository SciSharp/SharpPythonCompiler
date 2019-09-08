using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpPythonCompiler.Core
{
    class ClassNodeFinder : ISyntaxNodeFinder
    {
        public string Name { get; private set; }

        public ClassNodeFinder(string name)
        {
            Name = name;
        }

        public CSharpSyntaxNode Find(CSharpSyntaxNode root)
        {
            return root.DescendantNodesAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(n => n.Identifier.Text == Name);
        }
    }
}