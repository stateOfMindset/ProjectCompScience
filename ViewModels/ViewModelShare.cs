using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectCompScience.Models;
using ProjectCompScience.Services;

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
            // still gotta make a new page for creating new StockShares , need to also create a new ViewModel for that page , and add the logic to it.
        }
    }
}
