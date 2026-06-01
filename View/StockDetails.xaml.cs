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

    // 4. הפונקציה שמציירת בפועל
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
}