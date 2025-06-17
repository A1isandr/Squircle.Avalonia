using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace Squircle.Avalonia.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SquircleCornerRadiusTextBox.TextChanged += (_, _) =>
        {
            try
            {
                Squircle.CornerRadius = CornerRadius.Parse(SquircleCornerRadiusTextBox.Text ?? string.Empty);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        };
        
        SquircleCornerSmoothingSlider.ValueChanged += (_, _) =>
            Squircle.CornerSmoothing = SquircleCornerSmoothingSlider.Value;
    }
}