﻿using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Lib
{
    interface Templates
    {
        public const string NAMESPACE_NAME = "Tests_{0}_Namespace";
        public const string CLASS_NAME = "Tests_{0}_Class";
        public const string METHOD_NAME = "Test_{0}_{1}_Method";
        public const string METHOD_BODY = "Assert.Fail(\"autogenerated\");";
        public const string METHOD_ANNOTATION = "TestMethod";
        public const string CLASS_ANNOTATION = "TestClass";
        public const string IMPORTS = "using {0};\nusing Microsoft.VisualStudio.TestTools.UnitTesting;\n";
    }

    public class Generator
    {
        private Parser parser;
        private Random random= new Random();
        public Generator(Parser parser)
        {
            this.parser = parser;
        }

        private string generateTestCode(CompilationUnitSyntax root)
        {
            var classDeclarations = parser.getClassDeclarations(root);

            var namespaceName = ((NamespaceDeclarationSyntax) root.Members[0]).Name;

            var testNamespace = SyntaxFactory.NamespaceDeclaration(
                    SyntaxFactory.ParseName(string.Format(Templates.NAMESPACE_NAME, namespaceName)))
                .NormalizeWhitespace();

            var testClass = SyntaxFactory.ClassDeclaration(
                string.Format(Templates.CLASS_NAME, namespaceName+random.Next().ToString()));

            var testClassAttribute = SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(Templates.CLASS_ANNOTATION))
                    ))
                .NormalizeWhitespace();
            testClass = testClass.AddAttributeLists(testClassAttribute);

            foreach (var classDeclaration in classDeclarations)
            {
                var methodDeclarations = parser.getPublicMethodsDeclarations(classDeclaration);

                foreach (var methodDeclaration in methodDeclarations)
                {
                    var testMethodAttribute = SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(Templates.METHOD_ANNOTATION))
                            ))
                        .NormalizeWhitespace();

                    var syntax = SyntaxFactory.ParseStatement(Templates.METHOD_BODY);

                    var testMethodDeclaration = SyntaxFactory.MethodDeclaration(
                            SyntaxFactory.ParseTypeName("void"),
                            string.Format(Templates.METHOD_NAME, classDeclaration.Identifier,
                                methodDeclaration.Identifier))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(syntax)
                        );

                    testMethodDeclaration = testMethodDeclaration.AddAttributeLists(testMethodAttribute);
                    testClass = testClass.AddMembers(testMethodDeclaration);
                }
            }

            testNamespace = testNamespace.AddMembers(testClass);
            var code = string.Format(Templates.IMPORTS, namespaceName) + testNamespace
                .NormalizeWhitespace()
                .ToFullString();

            return code;
        }

        public Task<string> testCodeFromClassCodeTask(string classCode)
        {
            return Task.Run(() => testCodeFromClassCode(classCode));
        }

        public string testCodeFromClassCode(string classCode)
        {

            var syntaxTree = parser.parse(classCode);
            var testClassSrcCode = generateTestCode(syntaxTree);
            return testClassSrcCode;

        }
    }
}