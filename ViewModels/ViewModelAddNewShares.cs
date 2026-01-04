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
            ButtonCreateShareCommand = new Command(async () =>
            {
                try
                {
                    StockShares = new ObservableCollection<StockShare>(LocalDataService.GetLocalDataService().GetStockShares());
                    CreateShare();
                    await Shell.Current.GoToAsync("//StockSharesPage");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });



        }
        #region Propfulls
        public string IdField
        {
            get { return idField; }
            set
            {
                idField = value;
                OnPropertyChanged();
            }
        }



        private string idField;

        public string CompanyFieldText
        {
            get { return companyFieldText; }
            set
            {
                companyFieldText = value;
                OnPropertyChanged();
            }
        }

        private string companyFieldText;

        public string ClassTypeFieldText
        {
            get { return classTypeFieldText; }
            set
            {
                classTypeFieldText = value;
                OnPropertyChanged();
            }
        }

        private string classTypeFieldText;

        public string PriceFieldText
        {
            get { return priceFieldText; }
            set
            {
                priceFieldText = value;
                OnPropertyChanged();
            }
        }

        private string priceFieldText;

        public string QuantityFieldText
        {
            get { return quantityFieldText; }
            set
            {
                quantityFieldText = value;
                OnPropertyChanged();
            }
        }

        private string quantityFieldText;


        #endregion

        #region Commands
        public ICommand ButtonCreateShareCommand { get; set; }
        #endregion

        private async void CreateShare()
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
            LocalDataService.GetLocalDataService().AddStockShare(newShare);
        }

        public void ClearFields()
        {
            IdField = string.Empty;
            CompanyFieldText = string.Empty;
            ClassTypeFieldText = string.Empty;
            PriceFieldText = string.Empty;
            QuantityFieldText = string.Empty;
        }
    }
}
