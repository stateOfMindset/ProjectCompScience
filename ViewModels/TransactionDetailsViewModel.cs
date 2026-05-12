using ProjectCompScience.Models;
using System.Collections.Generic;

namespace ProjectCompScience.ViewModels
{
    internal class TransactionDetailsViewModel : ViewModelBase, IQueryAttributable
    {
        private Transaction currentTransaction;
        public Transaction CurrentTransaction
        {
            get => currentTransaction;
            set { currentTransaction = value; OnPropertyChanged(); }
        }

 
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("SelectedTx"))
            {
                CurrentTransaction = query["SelectedTx"] as Transaction;               
                query.Clear();
            }
        }
    }
}