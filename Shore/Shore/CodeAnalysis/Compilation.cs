using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Lowering;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;
        public Compilation? Previous { get; }
        public NodeTree NodeTree { get; }

        public Compilation(NodeTree nodeTree)
            : this(null, nodeTree)
        {
        }

        private Compilation(Compilation? previous, NodeTree nodeTree)
        {
            Previous = previous;
            NodeTree = nodeTree;
        }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, NodeTree.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public Compilation ContinueWith(NodeTree nodeTree) => new Compilation(this, nodeTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = NodeTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            var program = Binder.BindProgram(GlobalScope);
            if (program.Diagnostics.Any()) return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);
            program.Statement.WriteTo(writer);
        }
    }
}