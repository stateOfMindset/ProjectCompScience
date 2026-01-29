using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Auth;
using Firebase.Database;
using ProjectCompScience.Models;
using Firebase.Database.Query;

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
                //instance.CreateFakeData();
            }
            return instance;
        }

        #endregion

        #region properties
        public List<StockShare> stockShares = new List<StockShare>();


        FirebaseAuthClient? auth;
        FirebaseClient? client;
        public AuthCredential? loginAuthUser; //This is to keep the logged in user credential, so we can logout later
        public myUser fullDetaillsLoggedInUser;
        #endregion

        public void Init()
        {
            var config = new FirebaseAuthConfig()
            {
                ApiKey = "AIzaSyCsgKP-XDUCNIt0nsUSSUEVNJzpedO6Kzg",
                AuthDomain = "computersciencefinaleproject.firebaseapp.com", //כתובת התחברות
                Providers = new FirebaseAuthProvider[] //רשימת אפשריות להתחבר
              {
          new EmailProvider() //אנחנו נשתמש בשירות חינמי של התחברות עם מייל
              },
                UserRepository = new FileUserRepository("appUserData") //לא חובה, שם של קובץ בטלפון הפרטי שאפשר לשמור בו את מזהה ההתחברות כדי לא הכניס כל פעם את הסיסמא 
            };
            auth = new FirebaseAuthClient(config); //ההתחברות

            client =
              new FirebaseClient(@"https://computersciencefinaleproject-default-rtdb.europe-west1.firebasedatabase.app/", //כתובת מסד הנתונים
              new FirebaseOptions
              {
                  AuthTokenAsyncFactory = () => Task.FromResult(auth.User.Credential.IdToken)// מזהה ההתחברות של המשתמש עם השרת, הנתון נשמר במכשיר
              });
        }

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

        public async Task<bool> DeleteStockShareAsync(StockShare ItemToDelete)
        {
            if (stockShares != null)
            {
                if (stockShares.Contains(ItemToDelete))
                {
                    stockShares.Remove(ItemToDelete);
                    await Task.CompletedTask;
                    return true;
                }
            }
            return false;
        }

        public bool AddStockShare(StockShare ss)
        {
            stockShares.Add(ss);
            return true;
        }

        public async Task<bool> AddStockShareAsync(StockShare ItemToDelete)
        {
            if (stockShares != null)
            {
                try
                {
                    if (stockShares.Contains(ItemToDelete))
                    {
                        stockShares.Add(ItemToDelete);
                        var result = stockShares
                            .GroupBy(x => x.Id)
                            .Select(g => new StockShare
                            {
                                Id = g.Key,
                                Company = g.First().Company,
                                classType = g.First().classType,
                                price = g.First().price,
                                quantity = g.Sum(x => x.quantity)
                            });

                        await Task.CompletedTask;
                        return true;
                    }


                    else
                    {
                        stockShares.Add(ItemToDelete);
                        await Task.CompletedTask;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return false;
        }

        public async Task<bool> TryLogin(string userNameString, string passwordString)
        {
            if (userNameString == null || passwordString == null)
            {
                return false;
            }
            try
            {
                var authUser = await auth.SignInWithEmailAndPasswordAsync(userNameString, passwordString);
                loginAuthUser = authUser.AuthCredential;
                // We are logged in. Now go to DataBase and fetch data on user itself. Exampe 1 parameter: Name
                string uid = auth.User.Uid;
                string Name = await client
                    .Child("users")
                    .Child(uid)
                    .Child("Name")
                    .OnceSingleAsync<string>();
                //!!need to add here the transactions of the user , and or stocks that he holds , with stockID and quantity!!

                fullDetaillsLoggedInUser = new myUser()
                {
                    Email = auth.User.Info.Email,
                    Id = uid,
                    Name = Name
                };
                // Authentication successful 
                // We keep the token or Credential in loginAuthUser, so we can erase it later in logout
                // You can access the authenticated user's details via authUser.User
                // you should create a new user or person
                // Person person = new Person(){Email=authUser.User.info.Email, ...
                // Don't put the password in the Person :)

                // ((App)Application.Current).SetAuthenticatedShell();

                return true;
            }
            catch (FirebaseAuthException ex)
            {
                // Authentication failed
                return false;
            }
        }

        public async Task<bool> TryRegister(string userNameString, string passwordString, string Name)
        {
            try
            {
                // 1: Create a user in Firebase with an Email and Password.
                var respond = await auth.CreateUserWithEmailAndPasswordAsync(userNameString, passwordString);
                // 2: User was created and also user is also Logged in
                // 3: We Store the Uid of the user
                fullDetaillsLoggedInUser = new myUser()
                {
                    Email = respond.User.Info.Email,
                    Id = respond.User.Uid,
                    Name = Name
                };
                // 3: We can continue and add more details about the user but this time in the firebase Database
                // Example: saving the full name
                await client
                    .Child("users")
                    .Child(fullDetaillsLoggedInUser.Id)
                    .PutAsync(new
                    {
                        Name = Name
                        //Add here transaction history or stocks he holds
                    });

                return true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    ex.Message,
                    "OK"
                );

                return false;
            }
        }


        public bool Logout()
        {
            try
            {
                auth.SignOut();
                loginAuthUser = null;
                fullDetaillsLoggedInUser = null;
                return true;
            }
            catch
            {
                return false;
            }
        }



    }
}