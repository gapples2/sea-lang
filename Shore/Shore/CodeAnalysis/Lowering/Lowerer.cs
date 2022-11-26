﻿using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;
        
        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++_labelCount}";
            return new BoundLabel(name);
        }
        
        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse()) stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement == null)
            {
                var endLabel = GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, false);
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var result = new BoundBlockStatement(
                        ImmutableArray.Create(gotoFalse, node.ThenStatement, endLabelStatement));
                return RewriteStatement(result);
            }
            else
            {
                var elseLabel = GenerateLabel();
                var endLabel = GenerateLabel();

                var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, false);
                var gotoEndStatement = new BoundGotoStatement(endLabel);
                var elseLabelStatement = new BoundLabelStatement(elseLabel);
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var result = new BoundBlockStatement(ImmutableArray.Create(
                    gotoFalse, node.ThenStatement, gotoEndStatement, elseLabelStatement, node.ElseStatement, endLabelStatement));
                return RewriteStatement(result);
            }
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var bodyLabel = GenerateLabel();

            var gotoContinue = new BoundGotoStatement(node.ContinueLabel);
            var bodyLabelStatement = new BoundLabelStatement(bodyLabel);
            var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            var gotoTrue = new BoundConditionalGotoStatement(bodyLabel, node.Condition);
            var breakLabelStatement = new BoundLabelStatement(node.BreakLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoContinue, bodyLabelStatement, node.Body, continueLabelStatement, gotoTrue, breakLabelStatement
            ));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
            var variableExpression = new BoundVariableExpression(node.Variable);
            var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int64);
            var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(TokType.LessThanOrEqualToken, TypeSymbol.Int64, TypeSymbol.Int64),
                new BoundVariableExpression(upperBoundSymbol)
            );           
            var continueLabelStatement = new BoundLabelStatement(node.ContinueLabel);
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(TokType.PlusToken, TypeSymbol.Int64, TypeSymbol.Int64),
                        new BoundLiteralExpression((long)1)
                    )
                )
            );
            var whileBody = new BoundBlockStatement(
                    ImmutableArray.Create<BoundStatement>(node.Body, continueLabelStatement, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody, node.BreakLabel, GenerateLabel());
            var result =
                new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration,
                    upperBoundDeclaration, whileStatement));

            return RewriteStatement(result);
        }
    }
}