namespace ProjectCompScience.View;

using ProjectCompScience.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System;

public partial class StockDetails : ContentPage
{
    public StockDetails()
    {
        InitializeComponent();

 
        var viewModel = new StockDetailsViewModel();
        BindingContext = viewModel;

    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (BindingContext is StockDetailsViewModel vm)
        {
            if (StockGraphView != null)
            {
                StockGraphView.Drawable = vm.MyGraphDrawable;
            }

            vm.OnGraphDataChanged -= TriggerGraphRedraw;

            vm.OnGraphDataChanged += TriggerGraphRedraw;
        }
    }


    private void TriggerGraphRedraw()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (StockGraphView != null)
            {
                StockGraphView.Invalidate();
            }
        });
    }
    private void OnTimeFilterClicked(object sender, EventArgs e)
    {
        foreach (var child in TimeFiltersContainer.Children)
        {
            if (child is Button btn)
            {
                btn.BackgroundColor = Colors.Transparent;
                btn.TextColor = Color.FromArgb("#A0A0A0");
            }
        }

        var clickedButton = (Button)sender;
        clickedButton.BackgroundColor = Colors.White;
        clickedButton.TextColor = Colors.Black;

        clickedButton.CornerRadius = 8;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is StockDetailsViewModel vm)
        {
            vm.ClearComparisons();
        }
    }
}