using ProjectCompScience.Services;
using System;
using ProjectCompScience.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectCompScience.ViewModels
{
    internal class TransactionHistoryViewModel : ViewModelBase
    {
        public ObservableCollection<Transaction> Transactions { get; set; } = new();

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(); }
        }

        public ICommand TransactionTappedCommand { get; }

        public TransactionHistoryViewModel()
        {
            TransactionTappedCommand = new Command<Transaction>(async (selectedTx) => await GoToDetails(selectedTx));
        }

        public async Task LoadHistoryAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            string userId = Preferences.Get("UserId", "");
            var history = await LocalDataService.GetLocalDataService().GetUserTransactionsAsync(userId);

            Transactions.Clear();
            foreach (var item in history)
            {
                Transactions.Add(item);
            }

            IsBusy = false;
        }

        private async Task GoToDetails(Transaction tx)
        {
            if (tx == null) return;

            var navigationParams = new Dictionary<string, object>
            {
                { "SelectedTx", tx }
            };

            await Shell.Current.GoToAsync("TransactionDetailsViewl", navigationParams);
        }
    }

}
