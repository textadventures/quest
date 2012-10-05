using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextAdventures.Quest.Scripts;

namespace TextAdventures.Quest
{
    public class UndoLogger
    {
        internal interface IUndoAction
        {
            void DoUndo(WorldModel worldModel);
            void DoRedo(WorldModel worldModel);
        }

        private class Transaction
        {
            private string m_command;
            private List<IUndoAction> m_attributes = new List<IUndoAction>();

            public Transaction(string command)
            {
                m_command = command;
            }

            public void AddUndoAction(IUndoAction action)
            {
                m_attributes.Add(action);
            }

            public int Count
            {
                get
                {
                    return m_attributes.Count;
                }
            }

            public void DoUndo(WorldModel worldModel)
            {
                const string undoTurnTemplate = "UndoTurn";
                m_attributes.Reverse();
                if (!worldModel.EditMode)
                {
                    if (worldModel.Template.DynamicTemplateExists(undoTurnTemplate))
                    {
                        worldModel.Print(worldModel.Template.GetDynamicText(undoTurnTemplate, m_command));
                    }
                }
                foreach (IUndoAction l in m_attributes)
                {
                    l.DoUndo(worldModel);
                }
            }

            public void DoRedo(WorldModel worldModel)
            {
                m_attributes.Reverse();     // Undo reverses attributes, so put them back in the correct order
                foreach (IUndoAction l in m_attributes)
                {
                    l.DoRedo(worldModel);
                }
            }

            internal string Description
            {
                get { return m_command; }
            }

            public Transaction PreviousTransaction { get; set; }
        }

        private bool m_logging = false;
        private Stack<Transaction> m_undoTransactions = new Stack<Transaction>();
        private Stack<Transaction> m_redoTransactions = new Stack<Transaction>();
        private Transaction m_currentTransaction;
        private WorldModel m_worldModel;
        public event EventHandler TransactionsUpdated;

        internal UndoLogger(WorldModel worldModel)
        {
            m_worldModel = worldModel;
        }

        public void StartTransaction(string command)
        {
            if (m_logging) throw new Exception("Starting transaction when previous transaction not finished");
            m_logging = true;
            Transaction previousTransaction = null;
            if (m_currentTransaction != null)
            {
                previousTransaction = (m_currentTransaction.Count > 0) ? m_currentTransaction : m_currentTransaction.PreviousTransaction;
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
            }
            OnTransactionsUpdated();
        }

        public void RollTransaction(string command)
        {
            if (m_currentTransaction != null) EndTransaction();
            StartTransaction(command);
        }

        private void OnTransactionsUpdated()
        {
            if (TransactionsUpdated != null) TransactionsUpdated(this, new EventArgs());
        }

        internal void AddUndoAction(IUndoAction action)
        {
            if (m_logging) m_currentTransaction.AddUndoAction(action);
        }

        public void RollbackTransaction()
        {
            if (m_currentTransaction != null) EndTransaction();
            Undo();
            if (m_currentTransaction != null)
            {
                m_currentTransaction = m_currentTransaction.PreviousTransaction;
            }
        }

        public void Undo()
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

            Transaction undoTransaction = m_undoTransactions.Pop();
            undoTransaction.DoUndo(m_worldModel);
            m_redoTransactions.Push(undoTransaction);
            OnTransactionsUpdated();
        }

        public void Redo()
        {
            if (m_redoTransactions.Count == 0)
            {
                throw new InvalidOperationException("No transactions to redo");
            }

            Transaction redoTransaction = m_redoTransactions.Pop();
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
            List<string> result = new List<string>();
            foreach (Transaction t in transactions)
            {
                result.Add(t.Description);
            }
            return result;
        }
    }
}
