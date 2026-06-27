#nullable disable
namespace QuestViva.Engine;

public class UndoLogger
{
    private readonly Stack<Transaction> m_redoTransactions = new();
    private readonly Stack<Transaction> m_undoTransactions = new();
    private readonly WorldModel m_worldModel;
    private Transaction m_currentTransaction;

    private bool m_logging;

    internal UndoLogger(WorldModel worldModel)
    {
        m_worldModel = worldModel;
    }

    public event EventHandler TransactionsUpdated;
    public event EventHandler TransactionCommitted;

    public void StartTransaction(string command)
    {
        if (m_logging)
        {
            throw new Exception("Starting transaction when previous transaction not finished");
        }

        m_logging = true;
        Transaction previousTransaction = null;
        if (m_currentTransaction != null)
        {
            previousTransaction = m_currentTransaction.Count > 0
                ? m_currentTransaction
                : m_currentTransaction.PreviousTransaction;
        }

        m_currentTransaction = new Transaction(command);
        m_currentTransaction.PreviousTransaction = previousTransaction;
    }

    public void EndTransaction()
    {
        m_logging = false;
        if (m_currentTransaction.Count > 0)
        {
            m_undoTransactions.Push(m_currentTransaction);
            m_redoTransactions.Clear();
            TransactionCommitted?.Invoke(this, EventArgs.Empty);
        }

        OnTransactionsUpdated();
    }

    public void RollTransaction(string command)
    {
        if (m_currentTransaction != null)
        {
            EndTransaction();
        }

        StartTransaction(command);
    }

    private void OnTransactionsUpdated()
    {
        if (TransactionsUpdated != null)
        {
            TransactionsUpdated(this, new EventArgs());
        }
    }

    internal void AddUndoAction(Func<IUndoAction> getAction)
    {
        if (!m_logging)
        {
            return;
        }

        m_currentTransaction.AddUndoAction(getAction());
    }

    public async Task RollbackTransaction()
    {
        if (m_logging)
        {
            EndTransaction();
        }

        await Undo();
        if (m_currentTransaction != null)
        {
            m_currentTransaction = m_currentTransaction.PreviousTransaction;
        }
    }

    public async Task Undo()
    {
        const string NothingToUndoTemplate = "NothingToUndo";

        if (m_undoTransactions.Count == 0)
        {
            if (m_worldModel.Template.TemplateExists(NothingToUndoTemplate))
            {
                m_worldModel.PrintTemplate("NothingToUndo");
            }
            else
            {
                throw new Exception("Nothing to undo");
            }

            return;
        }

        var undoTransaction = m_undoTransactions.Pop();
        await undoTransaction.DoUndo(m_worldModel);
        m_redoTransactions.Push(undoTransaction);
        OnTransactionsUpdated();
    }

    public void Redo()
    {
        if (m_redoTransactions.Count == 0)
        {
            throw new InvalidOperationException("No transactions to redo");
        }

        var redoTransaction = m_redoTransactions.Pop();
        redoTransaction.DoRedo(m_worldModel);
        m_undoTransactions.Push(redoTransaction);
        OnTransactionsUpdated();
    }

    public IEnumerable<string> UndoList()
    {
        return GetTransactionList(m_undoTransactions);
    }

    public IEnumerable<string> RedoList()
    {
        return GetTransactionList(m_redoTransactions);
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
        private readonly List<IUndoAction> m_attributes = new();

        public Transaction(string command)
        {
            Description = command;
        }

        public int Count => m_attributes.Count;

        internal string Description { get; }

        public Transaction PreviousTransaction { get; set; }

        public void AddUndoAction(IUndoAction action)
        {
            m_attributes.Add(action);
        }

        public async Task DoUndo(WorldModel worldModel)
        {
            const string undoTurnTemplate = "UndoTurn";
            m_attributes.Reverse();
            if (!worldModel.EditMode)
            {
                if (worldModel.Template.DynamicTemplateExists(undoTurnTemplate))
                {
                    worldModel.Print(await worldModel.Template.GetDynamicTextAsync(undoTurnTemplate, Description));
                }
            }

            foreach (var l in m_attributes)
            {
                l.DoUndo(worldModel);
            }
        }

        public void DoRedo(WorldModel worldModel)
        {
            m_attributes.Reverse(); // Undo reverses attributes, so put them back in the correct order
            foreach (var l in m_attributes)
            {
                l.DoRedo(worldModel);
            }
        }
    }
}