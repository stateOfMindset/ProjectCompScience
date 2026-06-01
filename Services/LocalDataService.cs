using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using ProjectCompScience.Models;
using Microsoft.Maui.Storage;

namespace ProjectCompScience.Services
{
    internal class LocalDataService
    {
        // Dynamically finds the safe storage folder for whichever device (PC or Android) the app is running on
        public static readonly string EnvFilePath = Path.Combine(FileSystem.AppDataDirectory, "firebase.env");

        #region instance 
        private static LocalDataService? instance;
        static public LocalDataService GetLocalDataService()
        {
            if (instance == null)
            {
                instance = new LocalDataService();
            }
            return instance;
        }
        #endregion

        #region properties
        public List<StockShare> stockShares = new List<StockShare>();

        FirebaseAuthClient? auth;
        public FirebaseClient? client;
        public AuthCredential? loginAuthUser;
        public myUser fullDetaillsLoggedInUser;
        #endregion

        public async Task InitAsync()
        {
            var assembly = typeof(App).Assembly;
            string resourceName = "ProjectCompScience.Resources.Raw.firebase.env";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    DotNetEnv.Env.Load(stream);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("CRITICAL ERROR: Embedded .env file not found!");
                }
            }

            // Connect to Firebase
            var config = new FirebaseAuthConfig()
            {
                ApiKey = Environment.GetEnvironmentVariable("ApiKey_firebase"),
                AuthDomain = Environment.GetEnvironmentVariable("AuthDomain_firebase"),
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider()
                },
                UserRepository = new FileUserRepository("appUserData")
            };
            auth = new FirebaseAuthClient(config);

            string url = Environment.GetEnvironmentVariable("url_firebase");
            client = new FirebaseClient($"{url}",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(auth.User.Credential.IdToken)
                });
        }

        #region Authentication & User Profile
        public async Task<(bool isSuccess, string errorMessage)> TryLogin(string userNameString, string passwordString)
        {
            try
            {
                var authUser = await auth.SignInWithEmailAndPasswordAsync(userNameString, passwordString);
                loginAuthUser = authUser.AuthCredential;
                string uid = auth.User.Uid;

                var userDetails = await client.Child("users").Child(uid).Child("Details").OnceSingleAsync<myUser>();

                if (userDetails != null)
                {
                    fullDetaillsLoggedInUser = userDetails;
                }
                else
                {
                    fullDetaillsLoggedInUser = new myUser() { Email = auth.User.Info.Email, Id = uid, Name = "Unknown", Balance = 10000 };
                }

                Preferences.Set("UserId", uid);
                return (true, string.Empty);
            }
            catch (FirebaseAuthException)
            {
               
                return (false, "Incorrect email or password. Please try again.");
            }
            catch
            {
                return (false, "An unknown connection error occurred.");
            }
        }

        public async Task<(bool isSuccess, string errorMessage)> TryRegister(string userNameString, string passwordString, string Name)
        {
            try
            {
                var respond = await auth.CreateUserWithEmailAndPasswordAsync(userNameString, passwordString);

                fullDetaillsLoggedInUser = new myUser()
                {
                    Email = respond.User.Info.Email,
                    Id = respond.User.Uid,
                    Name = Name,
                    Balance = 10000.00
                };

                await client.Child("users").Child(fullDetaillsLoggedInUser.Id).Child("Details").PutAsync(fullDetaillsLoggedInUser);
                Preferences.Set("UserId", fullDetaillsLoggedInUser.Id);

                return (true, string.Empty);
            }
            catch (FirebaseAuthException)
            {
   
                return (false, "Registration failed. This email may already be in use, or your password is too weak.");
            }
            catch
            {
                return (false, "An unknown connection error occurred.");
            }
        }

        public bool Logout()
        {
            try
            {
                auth.SignOut();
                loginAuthUser = null;
                fullDetaillsLoggedInUser = null;
                Preferences.Remove("UserId");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task RemovePortfolioItemAsync(string userId, string ticker)
        {
            await client
                .Child("users")
                .Child(userId)
                .Child("Portfolio")
                .Child(ticker)
                .DeleteAsync();
        }
        #endregion


        #region Firebase Economy Methods
        public async Task<double> GetUserBalanceAsync(string userId)
        {
            try
            {
                return await client
                    .Child("users")
                    .Child(userId)
                    .Child("Details")
                    .Child("Balance")
                    .OnceSingleAsync<double>();
            }
            catch
            {
                return 10000.00;
            }
        }

        public async Task UpdateUserBalanceAsync(string userId, double newBalance)
        {
            await client
                .Child("users")
                .Child(userId)
                .Child("Details")
                .Child("Balance")
                .PutAsync(newBalance);

            if (fullDetaillsLoggedInUser != null)
            {
                fullDetaillsLoggedInUser.Balance = newBalance;
            }
        }

        public async Task RecordTransactionAsync(string userId, Transaction transaction)
        {
            await client
                .Child("users")
                .Child(userId)
                .Child("Transactions")
                .PostAsync(transaction);
        }

        public async Task UpdatePortfolioAsync(string userId, PortfolioItem item)
        {
            await client
                .Child("users")
                .Child(userId)
                .Child("Portfolio")
                .Child(item.Ticker)
                .PutAsync(item);
        }

        public async Task<List<PortfolioItem>> GetUserPortfolioAsync(string userId)
        {
            try
            {
                var items = await client
                    .Child("users")
                    .Child(userId)
                    .Child("Portfolio")
                    .OnceAsync<PortfolioItem>();

                return items.Select(x => x.Object).ToList();
            }
            catch
            {
                return new List<PortfolioItem>();
            }
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(string userId)
        {
            try
            {
                var items = await client
                    .Child("users")
                    .Child(userId)
                    .Child("Transactions")
                    .OnceAsync<Transaction>();

               
                return items
                    .Select(x => x.Object)
                    .OrderByDescending(t => t.Date) 
                    .ToList();
            }
            catch
            {
                return new List<Transaction>();
            }
        }


        public async Task<int> GetOwnedSharesAsync(string userId, string ticker)
        {
            try
            {
                var allTransactions = await GetUserTransactionsAsync(userId);

                var stockHistory = allTransactions.Where(t => t.Ticker == ticker).ToList();

                int totalBought = stockHistory.Where(t => t.TransactionType == "BUY").Sum(t => t.Shares);
                int totalSold = stockHistory.Where(t => t.TransactionType == "SELL").Sum(t => t.Shares);

                return totalBought - totalSold;
            }
            catch
            {
                return 0; 
            }
        }
        #endregion
    }
}