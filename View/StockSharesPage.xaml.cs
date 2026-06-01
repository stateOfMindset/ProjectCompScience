namespace ProjectCompScience.View;
using ProjectCompScience.Models;
using ProjectCompScience.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;

public partial class StockSharesPage : ContentPage
{
    ViewModelShare vm;

    public StockSharesPage()
    {
        InitializeComponent();
        vm = new ViewModelShare();
        BindingContext = vm;

        if (PortfolioPieChartView != null)
        {
            PortfolioPieChartView.Drawable = vm.MyPieChart;
        }

        vm.OnPortfolioDataChanged += () =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (PortfolioPieChartView != null)
                {
                    PortfolioPieChartView.Drawable = vm.MyPieChart;

                    PortfolioPieChartView.Invalidate();
                }
            });
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // טוען את הנתונים בכל פעם שאתה נכנס למסך
        if (BindingContext is ViewModelShare vm)
        {
            await vm.LoadDataAsync();
        }
    }
}