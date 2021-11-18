using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lib
{
    public class Parser
    {
        public CompilationUnitSyntax parse(string srcText)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(srcText);
            var tree = syntaxTree.GetCompilationUnitRoot();
            var diagnostics = tree.GetDiagnostics();

            if (0 != diagnostics.Count())
            {
                var acc = "";
                foreach(var d in diagnostics)
                {
                    acc += d + "\n";
                }
                throw new SyntaxException(acc);
            }

            return tree;
        }

        public IEnumerable<MethodDeclarationSyntax> getPublicMethodsDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            return from methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Modifiers.Any(methodModifier => methodModifier.Text == "public")
                select methodDeclaration;
        }

        public IEnumerable<ClassDeclarationSyntax> getClassDeclarations(CompilationUnitSyntax baseNode)
        {
            return from classDeclaration in baseNode.DescendantNodes().OfType<ClassDeclarationSyntax>()
                select classDeclaration;
        }
    }
}