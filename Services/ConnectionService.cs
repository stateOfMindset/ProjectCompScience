using ProjectCompScience.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Services
{
    internal class ConnectionService
    {
           
    private static ConnectionService instance;
        private bool wasConnected = true;

        public static ConnectionService GetInstance()
        {
            if (instance == null)
            {
                instance = new ConnectionService();
            }
            return instance;
        }

        private ConnectionService()
        {
            // Subscribe to connectivity changes
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            Connectivity.ConnectivityChanged += OnConnectivityChanged2;
            wasConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            bool isConnected = e.NetworkAccess == NetworkAccess.Internet;

            if (!isConnected && wasConnected)
            {
                // Internet just disconnected
                await ShowDisconnectedAlert();
            }
            else if (isConnected && !wasConnected)
            {
                // Internet just reconnected
                await ShowReconnectedMessage();
            }

            wasConnected = isConnected;
        }

        private async void OnConnectivityChanged2(object? sender, ConnectivityChangedEventArgs e)
        {
            bool isConnected = e.NetworkAccess == NetworkAccess.Internet;

            if (!isConnected && wasConnected)
            {
                // Internet just disconnected — show overlay
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var mainPage = Application.Current?.MainPage;
                    // only if there isn't already a modal page on top (like an alert) to avoid stacking multiple modals on top of each other
                    if (Shell.Current != null && Shell.Current.Navigation.ModalStack.Count == 0)
                    {
                        await Shell.Current.Navigation.PushModalAsync(new NoInternetPage(), animated: true);
                    }
                });
            }
            else if (isConnected && !wasConnected)
            {
                // Internet just reconnected — hide overlay
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var mainPage = Application.Current?.MainPage;
                    if (mainPage != null)
                    {
                        await mainPage.Navigation.PopModalAsync(animated: true);
                    }
                });
            }

            wasConnected = isConnected;
        }

        private async Task ShowDisconnectedAlert()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                                "No Internet Connection",
                                "Your internet connection has been lost. Please reconnect to the internet to resume operations.",
                                "OK"
                            );
                });
            }
        }

        private async Task ShowReconnectedMessage()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                                "Connected",
                                "Internet connection has been restored.",
                                "OK"
                            );
                });
            }
        }

        private bool IsConnected()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }

        public async Task<bool> CheckConnectivityAndAlert(string operationName = "this operation")
        {
            if (!IsConnected())
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                                      "No Internet Connection",
                                      $"Cannot perform {operationName} without internet connection. Please check your network settings.",
                                      "OK"
                                  );
                    });
                }
                return false;
            }
            return true;
        }

        // in case we want to disable the service and stop listening to connectivity changes. Example working offline mode in the app where we don't want to show connectivity alerts
        public void Dispose()
        {
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}

