using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;

namespace SharpPythonCompiler.Core
{
    interface ISyntaxNodeFinder
    {
        CSharpSyntaxNode Find(CSharpSyntaxNode root);
    }
}