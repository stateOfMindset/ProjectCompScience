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
            StockShares = new ObservableCollection<StockShare>(LocalDataService.GetLocalDataService().GetStockShares());
            //StockShares = new ObservableCollection<StockShare>();
            LocalDataService.GetLocalDataService().SetStockSharesRefrence(StockShares);

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
        }

        #region commands
        public ICommand ButtonMovePageCommand { get; set; }

        #endregion
    }
}
