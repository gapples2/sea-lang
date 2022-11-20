namespace Shore.CodeAnalysis.Syntax
{
    public enum TokType
    {
        UnknownToken,
        EndOfFileToken,
        WhitespaceToken,
        
        NumberToken,
        IdentifierToken,
        FalseKeyword,
        TrueKeyword,
        
        DashToken,
        StarToken,
        SlashToken,
        PlusToken,
        CloseParenToken,
        OpenParenToken,
        EqualsToken,
        
        BangToken,
        DoubleAmpersandToken,
        DoublePipeToken,
        DoubleEqualsToken,
        BangEqualsToken,

        LiteralExpression,
        BinaryExpression,
        ParenthesisExpression,
        UnaryExpression,
        NameExpression,
        AssignmentExpression,
        CompilationUnit,
        ExpressionStatement,
        BlockStatement,
        CloseBraceToken,
        OpenBraceToken,
        VariableDeclarationStatement,
        //LetKeyword,
        ReadOnlyKeyword,
        GreaterThanOrEqualToken,
        GreaterThanToken,
        LessThanOrEqualToken,
        LessThanToken,
        IfStatement,
        ElseStatement,
        ElseKeyword,
        IfKeyword,
        WhileStatement,
        WhileKeyword,
        ForStatement,
        ForKeyword,
        UntilKeyword,
        RightShiftToken,
        LeftShiftToken,
        PipeToken,
        AmpersandToken,
        CaratToken,
        TildeToken,
        StringToken,
        CallExpression,
        CommaToken,
        BoolKeyword,
        StringKeyword,
        Int8Keyword,
        Int16Keyword,
        Int32Keyword,
        Int64Keyword,
        Parameter,
        GlobalStatement,
        FunctionDeclaration,
        FunctionKeyword,
        VoidKeyword,
        ContinueStatement,
        BreakStatement,
        BreakKeyword,
        ContinueKeyword,
        ReturnStatement,
        ReturnKeyword,
    }
}