using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace Squircle.Avalonia.Controls;

internal class SquircleImpl : Control
{
    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SquircleImpl, CornerRadius>(nameof(CornerRadius), defaultValue: new CornerRadius(20));
    
    /// <summary>
    /// Corner radius.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    } 
    
    /// <summary>
    /// Defines the <see cref="CornerSmoothing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CornerSmoothingProperty =
        AvaloniaProperty.Register<SquircleImpl, double>(nameof(CornerSmoothing), defaultValue: 0.6);

    /// <summary>
    /// Corner smoothing.
    /// </summary>
    /// <remarks>
    /// Default value is 0.6 for ios-like effect.
    /// </remarks>
    public double CornerSmoothing
    {
        get => GetValue(CornerSmoothingProperty);
        set => SetValue(CornerSmoothingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="PreserveSmoothing"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> PreserveSmoothingProperty =
        AvaloniaProperty.Register<SquircleImpl, bool>(nameof(PreserveSmoothing), defaultValue: false);

    /// <summary>
    /// Indicate whether corner smoothing should be preserved
    /// even when there is not enough space.
    /// </summary>
    /// <remarks>
    /// Default value is false.
    /// </remarks>
    public bool PreserveSmoothing
    {
        get => GetValue(PreserveSmoothingProperty);
        set => SetValue(PreserveSmoothingProperty, value);
    }
    
    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<SquircleImpl, double>(nameof(BorderThickness), defaultValue: 0);
    
    /// <summary>
    /// Border thickness.
    /// </summary>
    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }
    
    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BorderBrushProperty =
        AvaloniaProperty.Register<SquircleImpl, IBrush>(nameof(BorderBrush), defaultValue: null);
    
    /// <summary>
    /// Border brush.
    /// </summary>
    public IBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    
    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> BackgroundProperty =
        AvaloniaProperty.Register<SquircleImpl, IBrush>(nameof(Background), defaultValue: null);
    
    /// <summary>
    /// Background brush.
    /// </summary>
    public IBrush Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    } 

    static SquircleImpl()
    {
        AffectsRender<SquircleImpl>(
            CornerRadiusProperty,
            CornerSmoothingProperty,
            PreserveSmoothingProperty,
            BorderThicknessProperty,
            BorderBrushProperty,
            BackgroundProperty);
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new SquircleDrawOperation(
            this,
            Bounds,
            CornerRadius,
            CornerSmoothing,
            PreserveSmoothing,
            BorderThickness,
            BorderBrush,
            Background));
        
        base.Render(context);
    }
}

// Figma's squircle implementation:
// https://figma.com/blog/desperately-seeking-squircles/
// https://github.com/phamfoo/figma-squircle/
// https://github.com/MartinRGB/Figma_Squircles_Approximation
internal class SquircleDrawOperation(
    Control squircle,
    Rect controlBounds,
    CornerRadius requestedCornerRadius,
    double requestedCornerSmoothing,
    bool preserveSmoothing,
    double borderThickness,
    IBrush borderBrush,
    IBrush background)
    : ICustomDrawOperation
{
    private record CornerProperties(
        float A,
        float B,
        float C,
        float D,
        float P)
    {
        /// <summary>
        /// Get sum of A and B.
        /// </summary>
        public float Ab { get; } = A + B;

        /// <summary>
        /// Get sum of B and C.
        /// </summary>
        public float Bc { get; } = B + C;
        
        /// <summary>
        /// Get sum of A, B and C.
        /// </summary>
        public float Abc { get; } = A + B + C;
        
        /// <summary>
        /// Get subtraction sum of A, B and C from P.
        /// </summary>
        public float PMinusAbc { get; } = P - A - B - C;
    }
    
    private readonly Control _squircle = squircle;
    private readonly Rect _controlBounds = controlBounds;
    private readonly CornerRadius _requestedCornerRadius = requestedCornerRadius;
    private readonly double _requestedCornerSmoothing = requestedCornerSmoothing;
    private readonly bool _preserveSmoothing = preserveSmoothing;
    private readonly double _borderThickness = borderThickness;
    private readonly IBrush _borderBrush = borderBrush;
    private readonly IBrush _background = background;
    
    private bool _disposed;

    public Rect Bounds => _controlBounds;
    
    public bool Equals(ICustomDrawOperation? other)
    {
        return other is SquircleDrawOperation operation &&
               operation._squircle == _squircle &&
               operation._controlBounds == _controlBounds &&
               operation._requestedCornerRadius == _requestedCornerRadius &&
               Math.Abs(operation._requestedCornerSmoothing - _requestedCornerSmoothing) < double.Epsilon &&
               operation._preserveSmoothing == _preserveSmoothing;
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        
        _disposed = true;
    }
    
    public bool HitTest(Point p) => _controlBounds.Contains(p);
    
    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature is null) return;
        
        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;
        
        // Preventing strange artifacts.
        if (canvas.GetLocalClipBounds(out var bounds) && 
            !bounds.Contains(SKRect.Create(
                bounds.Left,
                bounds.Top,
                (float)_controlBounds.Width,
                (float)_controlBounds.Height)))
        {
            Dispatcher.UIThread.Post(() => _squircle.InvalidateVisual());
        }
        
        var rect = new SKRect(
            left:   (float)_controlBounds.Left,
            top:    (float)_controlBounds.Top,
            right:  (float)_controlBounds.Width,
            bottom: (float)_controlBounds.Height);
        
        var maxRoundingAndSmoothing = Math.Min(rect.Width, rect.Height) / 2;
        var cornerRadius = new CornerRadius(
            Math.Min(maxRoundingAndSmoothing, _requestedCornerRadius.TopLeft),
            Math.Min(maxRoundingAndSmoothing, _requestedCornerRadius.TopRight),
            Math.Min(maxRoundingAndSmoothing, _requestedCornerRadius.BottomRight),
            Math.Min(maxRoundingAndSmoothing, _requestedCornerRadius.BottomLeft));
        
        // Top right corner.
        var topRightProps = GetCornerProperties(
            cornerRadius.TopRight,
            _requestedCornerSmoothing,
            _preserveSmoothing,
            maxRoundingAndSmoothing);
        
        // Bottom right corner.
        var bottomRightProps = GetCornerProperties(
            cornerRadius.BottomRight,
            _requestedCornerSmoothing,
            _preserveSmoothing,
            maxRoundingAndSmoothing);
        
        // Bottom left corner.
        var bottomLeftProps = GetCornerProperties(
            cornerRadius.BottomLeft,
            _requestedCornerSmoothing,
            _preserveSmoothing,
            maxRoundingAndSmoothing);
        
        // Top left corner.
        var topLeftProps = GetCornerProperties(
            cornerRadius.TopLeft,
            _requestedCornerSmoothing,
            _preserveSmoothing,
            maxRoundingAndSmoothing);
        
        var path = new SKPath();
        
        // Starting at the left end of top straight segment.
        path.MoveTo(rect.Left + topLeftProps.P, rect.Top);
        
        // Top right corner.
        path.LineTo(rect.Right - topRightProps.P, rect.Top);
        path.CubicTo(
            path.LastPoint.X + topRightProps.A, rect.Top,
            path.LastPoint.X + topRightProps.Ab, rect.Top,
            path.LastPoint.X + topRightProps.Abc, rect.Top + topRightProps.D);
        path.ArcTo(
            new SKPoint((float)cornerRadius.TopRight, (float)cornerRadius.TopRight),
            0,
            SKPathArcSize.Small,
            SKPathDirection.Clockwise,
            new SKPoint(rect.Right - topRightProps.D, rect.Top + topRightProps.PMinusAbc));
        path.CubicTo(
            rect.Right, path.LastPoint.Y + topRightProps.C,
            rect.Right, path.LastPoint.Y + topRightProps.Bc,
            rect.Right, path.LastPoint.Y + topRightProps.Abc);
        
        // Bottom right corner.
        path.LineTo(rect.Right, rect.Bottom - bottomRightProps.P);
        path.CubicTo(
            rect.Right, path.LastPoint.Y + bottomRightProps.A,
            rect.Right, path.LastPoint.Y + bottomRightProps.Ab,
            rect.Right - bottomRightProps.D, path.LastPoint.Y + bottomRightProps.Abc);
        path.ArcTo(
            new SKPoint((float)cornerRadius.BottomRight, (float)cornerRadius.BottomRight),
            0,
            SKPathArcSize.Small,
            SKPathDirection.Clockwise,
            new SKPoint(rect.Right - bottomRightProps.PMinusAbc, rect.Bottom - bottomRightProps.D));
        path.CubicTo(
            path.LastPoint.X - bottomRightProps.C, rect.Bottom,
            path.LastPoint.X - bottomRightProps.Bc, rect.Bottom,
            path.LastPoint.X - bottomRightProps.Abc, rect.Bottom);
        
        // Bottom left corner.
        path.LineTo(rect.Left + bottomLeftProps.P, rect.Bottom);
        path.CubicTo(
            path.LastPoint.X - bottomLeftProps.A, rect.Bottom,
            path.LastPoint.X - bottomLeftProps.Ab, rect.Bottom,
            path.LastPoint.X - bottomLeftProps.Abc, rect.Bottom - bottomLeftProps.D);
        path.ArcTo(
            new SKPoint((float)cornerRadius.BottomLeft, (float)cornerRadius.BottomLeft),
            0,
            SKPathArcSize.Small,
            SKPathDirection.Clockwise,
            new SKPoint(rect.Left + bottomLeftProps.D, rect.Bottom - bottomLeftProps.PMinusAbc));
        path.CubicTo(
            rect.Left, path.LastPoint.Y - bottomLeftProps.C,
            rect.Left, path.LastPoint.Y - bottomLeftProps.Bc,
            rect.Left, path.LastPoint.Y - bottomLeftProps.Abc);
        
        // Top left corner.
        path.LineTo(rect.Left, rect.Top + topLeftProps.P);
        path.CubicTo(
            rect.Left, path.LastPoint.Y - topLeftProps.A,
            rect.Left, path.LastPoint.Y - topLeftProps.Ab,
            rect.Left + topLeftProps.D, path.LastPoint.Y - topLeftProps.Abc);
        path.ArcTo(
            new SKPoint((float)cornerRadius.TopLeft, (float)cornerRadius.TopLeft),
            0,
            SKPathArcSize.Small,
            SKPathDirection.Clockwise,
            new SKPoint(rect.Left + topLeftProps.PMinusAbc, rect.Top + topLeftProps.D));
        path.CubicTo(
            path.LastPoint.X + topLeftProps.C, rect.Top,
            path.LastPoint.X + topLeftProps.Bc, rect.Top,
            path.LastPoint.X + topLeftProps.Abc, rect.Top);
        
        path.Close();
        
        canvas.ClipPath(path, SKClipOperation.Intersect, true);
        
        var fillPaint = new SKPaint();
        fillPaint.IsAntialias = true;
        
        canvas.DrawPath(path, fillPaint);
        
        var borderPaint = new SKPaint();
        borderPaint.IsAntialias = true;
        borderPaint.Color = new SKColor(255, 0, 0, 255);
        borderPaint.IsStroke = true;
        borderPaint.StrokeWidth = 5;

        canvas.DrawPath(path, borderPaint);
    }

    private static CornerProperties GetCornerProperties(
        double cornerRadius,
        double cornerSmoothing,
        bool preserveSmoothing,
        double maxRoundingAndSmoothing)
    {
        // In case of rectangle q = R = 90 deg (Figures 12.1 - 12.2).
        // p = (1 + cornerSmoothing) * q;
        var p = (1 + cornerSmoothing) * cornerRadius;

        if (!preserveSmoothing)
        {
            var maxCornerSmoothing = maxRoundingAndSmoothing / cornerRadius - 1;
            cornerSmoothing = Math.Min(cornerSmoothing, maxCornerSmoothing);
            p = Math.Min(p, maxRoundingAndSmoothing);
        }

        // In case of rounded rectangle this is always 90 deg.
        var arcSweep = 90 * (1 - cornerSmoothing);
        var arcSectionLength = Math.Sin(ToRadians(arcSweep / 2)) * cornerRadius * Math.Sqrt(2);

        // Distance between point P3 and P4 (Figure 11.1).
        var angleAlpha = (90 - arcSweep) / 2;
        var p3ToP4Distance = cornerRadius * Math.Tan(ToRadians(angleAlpha / 2));

        // Find a, b, c, d (Figure 11.1).
        var angleBeta = 45 * cornerSmoothing;
        
        var c = p3ToP4Distance * Math.Cos(ToRadians(angleBeta));
        var d = c * Math.Tan(ToRadians(angleBeta));

        var b = (p - arcSectionLength - c - d) / 3;
        var a = 2 * b;

        return new CornerProperties((float)a, (float)b, (float)c, (float)d, (float)p);
    }
    
    /// <summary>
    /// Get appropriate corner smoothing if
    /// length consumed by rounding and smoothing is greater than the shortest side. 
    /// </summary>
    /// <param name="cornerSmoothing"></param>
    /// <param name="cornerRadius"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    private static double GetEffectiveCornerSmoothing(
        double cornerSmoothing,
        double cornerRadius,
        SKRect rect)
    {
        var shortestSide = Math.Min(rect.Width, rect.Height);
        // In case of rectangle, rounding length is equal to corner radius.
        //var roundingLength = (float)requestedCornerRadius * Math.Sqrt((1 + Math.Cos(Math.PI / 2)) / (1 - Math.Cos(Math.PI / 2)));
        var roundingAndSmoothingLength = (1 + cornerSmoothing) * cornerRadius * 2;
        
        return Math.Min(roundingAndSmoothingLength, shortestSide) / 2 / cornerRadius - 1f;
    }
    
    private static double ToRadians(double degrees) =>
        degrees * Math.PI / 180;
}