using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectCompScience.Models;

namespace ProjectCompScience.Services
{
    internal class LocalDataService
    {
        #region instance 
        private static LocalDataService? instance;
        static public LocalDataService GetLocalDataService()
        {
            if (instance == null)
            {
                instance = new LocalDataService();
                instance.CreateFakeData();
            }
            return instance;
        }


        #endregion

        public List<StockShare> stockShares = new List<StockShare>();
        private void CreateFakeData()
        {
            StockShare ss1 = new StockShare()
            {
                Id = "1",
                Company = new Company("Apple", "10c"),
                classType = "A",
                price = 100,
                quantity = 4
            };
            StockShare ss2 = new StockShare()
            {
                Id = "2",
                Company = new Company("NVIDIA", "15c"),
                classType = "A",
                price = 200,
                quantity = 4
            };
            StockShare ss3 = new StockShare()
            {
                Id = "3",
                Company = new Company("Microsoft", "20c"),
                classType = "A",
                price = 150,
                quantity = 4
            };
            stockShares.Add(ss1);
            stockShares.Add(ss2);
            stockShares.Add(ss3);

        }

        public List<StockShare> GetStockShares()
        {
            return stockShares;
        }
        public void RemoveStockShare(StockShare ss)
        {
            stockShares.Remove(ss);
        }
        public void AddMessage(StockShare ss)
        {
            stockShares.Add(ss);
        }
    }
}