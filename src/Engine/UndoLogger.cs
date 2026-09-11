namespace QuestViva.Engine;

public class UndoLogger
{
    private readonly Stack<Transaction> _redoTransactions = new();
    private readonly Stack<Transaction> _undoTransactions = new();
    private readonly WorldModel _worldModel;
    private Transaction? _currentTransaction;

    private bool _logging;

    internal UndoLogger(WorldModel worldModel)
    {
        _worldModel = worldModel;
    }

    public event EventHandler? TransactionsUpdated;
    public event EventHandler? TransactionCommitted;

    public void StartTransaction(string command)
    {
        if (_logging)
        {
            throw new Exception("Starting transaction when previous transaction not finished");
        }

        _logging = true;
        Transaction? previousTransaction = null;
        if (_currentTransaction != null)
        {
            previousTransaction = _currentTransaction.Count > 0
                ? _currentTransaction
                : _currentTransaction.PreviousTransaction;
        }

        _currentTransaction = new Transaction(command);
        _currentTransaction.PreviousTransaction = previousTransaction;
    }

    public void EndTransaction()
    {
        _logging = false;
        if (_currentTransaction!.Count > 0)
        {
            _undoTransactions.Push(_currentTransaction);
            _redoTransactions.Clear();
            TransactionCommitted?.Invoke(this, EventArgs.Empty);
        }

        OnTransactionsUpdated();
    }

    public void RollTransaction(string command)
    {
        if (_currentTransaction != null)
        {
            EndTransaction();
        }

        StartTransaction(command);
    }

    private void OnTransactionsUpdated()
    {
        TransactionsUpdated?.Invoke(this, new EventArgs());
    }

    internal void AddUndoAction(Func<IUndoAction> getAction)
    {
        if (!_logging)
        {
            return;
        }

        // _logging is only true between StartTransaction and EndTransaction
        _currentTransaction!.AddUndoAction(getAction());
    }

    public async Task RollbackTransaction()
    {
        if (_logging)
        {
            EndTransaction();
        }

        await Undo();
        if (_currentTransaction != null)
        {
            _currentTransaction = _currentTransaction.PreviousTransaction;
        }
    }

    public async Task Undo()
    {
        const string nothingToUndoTemplate = "NothingToUndo";

        if (_undoTransactions.Count == 0)
        {
            if (_worldModel.Template.TemplateExists(nothingToUndoTemplate))
            {
                await _worldModel.PrintTemplateAsync("NothingToUndo");
            }
            else
            {
                throw new Exception("Nothing to undo");
            }

            return;
        }

        var undoTransaction = _undoTransactions.Pop();
        await undoTransaction.DoUndo(_worldModel);
        _redoTransactions.Push(undoTransaction);
        OnTransactionsUpdated();
    }

    public void Redo()
    {
        if (_redoTransactions.Count == 0)
        {
            throw new InvalidOperationException("No transactions to redo");
        }

        var redoTransaction = _redoTransactions.Pop();
        redoTransaction.DoRedo(_worldModel);
        _undoTransactions.Push(redoTransaction);
        OnTransactionsUpdated();
    }

    public IEnumerable<string> UndoList()
    {
        return GetTransactionList(_undoTransactions);
    }

    public IEnumerable<string> RedoList()
    {
        return GetTransactionList(_redoTransactions);
    }

    private IEnumerable<string> GetTransactionList(Stack<Transaction> transactions)
    {
        var result = new List<string>();
        foreach (var t in transactions)
        {
            result.Add(t.Description);
        }

        return result;
    }

    internal interface IUndoAction
    {
        void DoUndo(WorldModel worldModel);
        void DoRedo(WorldModel worldModel);
    }

    private class Transaction
    {
        private readonly List<IUndoAction> _attributes = [];

        public Transaction(string command)
        {
            Description = command;
        }

        public int Count => _attributes.Count;

        internal string Description { get; }

        public Transaction? PreviousTransaction { get; set; }

        public void AddUndoAction(IUndoAction action)
        {
            _attributes.Add(action);
        }

        public async Task DoUndo(WorldModel worldModel)
        {
            const string undoTurnTemplate = "UndoTurn";
            _attributes.Reverse();
            if (!worldModel.EditMode)
            {
                if (worldModel.Template.DynamicTemplateExists(undoTurnTemplate))
                {
                    await worldModel.PrintAsync(await worldModel.Template.GetDynamicTextAsync(undoTurnTemplate, Description));
                }
            }

            foreach (var l in _attributes)
            {
                l.DoUndo(worldModel);
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            _attributes.Reverse(); // Undo reverses attributes, so put them back in the correct order
            foreach (var l in _attributes)
            {
                l.DoRedo(worldModel);
            }
        }
    }
}