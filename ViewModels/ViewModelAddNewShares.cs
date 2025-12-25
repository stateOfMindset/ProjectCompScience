using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectCompScience.Services;
using System.Windows.Input;

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
            ButtonCreateShareCommand = new Command(CreateShare);
        }

        #region Propfulls

        private string IdField;

        public string idField
        {
            get { return idField; }
            set
            {
                idField = value;
                OnPropertyChanged();

            }
        }

        private string CompanyFieldText;

        public string companyFieldText
        {
            get { return companyFieldText; }
            set
            {
                companyFieldText = value;
                OnPropertyChanged();

            }
        }

        private string ClassTypeFieldText;

        public string classTypeFieldText
        {
            get { return classTypeFieldText; }
            set
            {
                classTypeFieldText = value;
                OnPropertyChanged();

            }
        }

        private string PriceFieldText;

        public string priceFieldText
        {
            get { return priceFieldText; }
            set
            {
                priceFieldText = value;
                OnPropertyChanged();

            }
        }

        private string QuantityFieldText;

        public string quantityFieldText
        {
            get { return quantityFieldText; }
            set
            {
                quantityFieldText = value;
                OnPropertyChanged();

            }
        }


        #endregion

        #region Commands
            public ICommand ButtonCreateShareCommand { get; set; }
        #endregion

        private void CreateShare()
        {
            StockShare newShare = new StockShare()
            {
                Id = idField,
                Company = new Company(companyFieldText, "N/A"),
                classType = classTypeFieldText,
                price = Convert.ToInt32(priceFieldText),
                quantity = Convert.ToInt32(quantityFieldText)
            };

            StockShares.Add(newShare);
        }
    }
}
