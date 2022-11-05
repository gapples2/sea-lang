﻿using System.ComponentModel.DataAnnotations;
using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            bool showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;

                if (line == "#SHOWTREE")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing Node Trees" : "Hiding Node Trees");
                    continue;
                }

                var nodeTree = NodeTree.Parse(line);
                var compilation = new Compilation(nodeTree);
                var result = compilation.Evaluate(variables);

                var diagnostics = result.Diagnostics;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    LogNode(nodeTree.Root);
                    Console.ResetColor();
                }

                if (!diagnostics.Any()) Console.WriteLine(result.Value);
                else
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        Console.WriteLine();

                        Console.ForegroundColor = diagnostic.IsError ? ConsoleColor.DarkRed : ConsoleColor.DarkYellow;
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();
                        
                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        var message = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line.Substring(diagnostic.Span.End);
                        
                        Console.Write("    ");
                        Console.Write(prefix);

                        Console.ForegroundColor = diagnostic.IsError ? ConsoleColor.DarkRed : ConsoleColor.DarkYellow;
                        Console.Write(message);
                        Console.ResetColor();
                        
                        Console.Write(suffix);
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }
            }

            // CONSOLE WINDOW CONTROL
            //Console.WriteLine("Press C to close this window :)");
            //try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            //catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }

        private static void LogNode(Node node, string indent = "", bool last = false)
        {
            var marker = last ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Type);

            if (node is Token t && t.Value is not null)
            {
                Console.Write(" ");
                Console.Write(t.Value);   
            }
            
            Console.WriteLine();
            indent += last ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                LogNode(child, indent, child == lastChild);
            }
        }
    }
}
