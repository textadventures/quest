using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Scripts;

namespace AxeSoftware.Quest
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

            public void Add(Element appliesTo, string property, object oldValue, object newValue)
            {
                m_attributes.Add(new UndoFieldSet(appliesTo.Name, property, oldValue, newValue));
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
                m_attributes.Reverse();
                worldModel.Print(worldModel.Template.GetDynamicText("UndoTurn", m_command));
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
            m_currentTransaction = new Transaction(command);
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

        private void OnTransactionsUpdated()
        {
            if (TransactionsUpdated != null) TransactionsUpdated(this, new EventArgs());
        }

        internal void AddUndoAction(IUndoAction action)
        {
            if (m_logging) m_currentTransaction.AddUndoAction(action);
        }

        public void Undo()
        {
            if (m_undoTransactions.Count == 0)
            {
                m_worldModel.PrintTemplate("NothingToUndo");
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
