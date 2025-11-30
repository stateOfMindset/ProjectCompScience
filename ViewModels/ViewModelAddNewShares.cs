using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectCompScience.Services;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelAddNewShares : ViewModelBase
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

        public ViewModelAddNewShares()
        {
            StockShares = new ObservableCollection<StockShare>(LocalDataService.GetLocalDataService().GetStockShares());
        }
    }
}
