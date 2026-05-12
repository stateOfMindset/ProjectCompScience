using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class StockDetails : ContentPage
{
    public StockDetails()
    {
        InitializeComponent();

        var viewModel = new StockDetailsViewModel();
        BindingContext = viewModel;

        viewModel.OnGraphDataChanged += ForceGraphRedraw;
    }

    private void ForceGraphRedraw()
    {
        StockGraphView.Invalidate();
    }
}