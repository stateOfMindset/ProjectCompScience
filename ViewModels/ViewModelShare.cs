using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ProjectCompScience.Models;
using ProjectCompScience.Services;
using ProjectCompScience.View;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelShare : ViewModelBase
    {

        private ObservableCollection<StockShare> stockShares;
        public ObservableCollection<StockShare> StockShares
        {
            get { return stockShares; }
            set
            {
                stockShares = value;
                OnPropertyChanged();
            }
        }



        public ViewModelShare()
        {
            InitAsync();
            //StockShares = new ObservableCollection<StockShare>();
            //LocalDataService.GetLocalDataService().SetStockSharesRefrence(StockShares); NEEDS IMPLENTATION IN LOCALDATASERVICE.CS

            ButtonMovePageCommand = new Command(async () => {
                try
                {
                    await Shell.Current.GoToAsync("//addNewShares");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            ButtonDeleteShareCommand = new Command(async (item) =>
            {
                try
                {
                    await DeleteShare(item as StockShare);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            });
        }

        #region Asyncs
        private async Task DeleteShare(StockShare theItemToDelete)
        {
            bool successed = await LocalDataService.GetLocalDataService().DeleteStockShareAsync(theItemToDelete);
            if (successed)
            {
                StockShares.Remove(theItemToDelete);
            }
        }


        public void InitAsync()
        {
            StockShares = new ObservableCollection<StockShare>(LocalDataService.GetLocalDataService().GetStockShares());
        }
        #endregion
        #region commands
        public ICommand ButtonMovePageCommand { get; set; }

        public ICommand ButtonDeleteShareCommand { get; set; }

        #endregion
    }
}
