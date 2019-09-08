using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SharpPythonCompiler.Core
{
    class MethodParameterNodeFinder : ISyntaxNodeFinder
    {
        public MethodNodeFinder MethodNodeFinder { get; private set; }

        public string Name { get; private set; }

        public MethodParameterNodeFinder(MethodNodeFinder methodNodeFinder, string name)
        {
            MethodNodeFinder = methodNodeFinder;
            Name = name;
        }

        public CSharpSyntaxNode Find(CSharpSyntaxNode root)
        {
            return (MethodNodeFinder.Find(root) as MethodDeclarationSyntax)
                .ParameterList
                .Parameters
                .FirstOrDefault(n => n.Identifier.Text == Name);
        }
    }
}