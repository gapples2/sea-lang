﻿namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class FunctionDeclarationNode : MemberNode
    {
        public Token FunctionKeyword { get; }
        public Token FType { get; }
        public Token Identifier { get; }
        public Token OpenParenToken { get; }
        public SeparatedNodeList<ParameterNode> Parameters { get; }
        public Token CloseParenToken { get; }
        public BlockStatementNode? Body { get; }
        public override TokType Type => TokType.FunctionDeclaration;

        public FunctionDeclarationNode(NodeTree nodeTree, Token functionKeyword, Token type, Token identifier,
            Token openParenToken, SeparatedNodeList<ParameterNode> parameters, Token closeParenToken, BlockStatementNode? body)
            : base(nodeTree)
        {
            FunctionKeyword = functionKeyword;
            FType = type;
            Identifier = identifier;
            OpenParenToken = openParenToken;
            Parameters = parameters;
            CloseParenToken = closeParenToken;
            Body = body;
        }
    }
}