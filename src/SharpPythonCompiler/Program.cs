using System;
using SharpPythonCompiler.Core;

namespace SharpPythonCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            SharpCompiler.CompileSolution(args[0], args[1]).Wait();
        }
    }
}
