using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Controls;

// Copied from WPF's github.
public sealed class SystemDropShadowChrome : Decorator
{
    public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color),
        typeof(SystemDropShadowChrome), new FrameworkPropertyMetadata(Color.FromArgb(0x71, 0x00, 0x00, 0x00),
            FrameworkPropertyMetadataOptions.AffectsRender, ClearBrushes));

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius),
        typeof(CornerRadius), typeof(SystemDropShadowChrome), new FrameworkPropertyMetadata(new CornerRadius(),
            FrameworkPropertyMetadataOptions.AffectsRender, ClearBrushes), IsCornerRadiusValid);

    private const double ShadowDepth = 5.0;
    private const int TopLeft = 0;
    private const int Top = 1;
    private const int TopRight = 2;
    private const int Left = 3;
    private const int Center = 4;
    private const int Right = 5;
    private const int BottomLeft = 6;
    private const int Bottom = 7;
    private const int BottomRight = 8;
    private static Brush[]? _commonBrushes;
    private static CornerRadius _commonCornerRadius;
    private static object _resourceAccess = new();
    private Brush[]? _brushes;

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    private static bool IsCornerRadiusValid(object value)
    {
        var cr = (CornerRadius)value;
        return !(cr.TopLeft < 0.0 || cr.TopRight < 0.0 || cr.BottomLeft < 0.0 || cr.BottomRight < 0.0 ||
                 double.IsNaN(cr.TopLeft) || double.IsNaN(cr.TopRight) || double.IsNaN(cr.BottomLeft) ||
                 double.IsNaN(cr.BottomRight) ||
                 double.IsInfinity(cr.TopLeft) || double.IsInfinity(cr.TopRight) || double.IsInfinity(cr.BottomLeft) ||
                 double.IsInfinity(cr.BottomRight));
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static void ClearBrushes(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        ((SystemDropShadowChrome)o)._brushes = null;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var cornerRadius = CornerRadius;

        var shadowBounds = new Rect(new Point(ShadowDepth, ShadowDepth),
            new Size(RenderSize.Width, RenderSize.Height));
        var color = Color;

        if (shadowBounds.Width > 0 && shadowBounds.Height > 0 && color.A > 0)
        {
            var centerWidth = shadowBounds.Right - shadowBounds.Left - 2 * ShadowDepth;
            var centerHeight = shadowBounds.Bottom - shadowBounds.Top - 2 * ShadowDepth;

            var maxRadius = Math.Min(centerWidth * 0.5, centerHeight * 0.5);
            cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
            cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, maxRadius);
            cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);
            cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);
            var brushes = GetBrushes(color, cornerRadius);

            var centerTop = shadowBounds.Top + ShadowDepth;
            var centerLeft = shadowBounds.Left + ShadowDepth;
            var centerRight = shadowBounds.Right - ShadowDepth;
            var centerBottom = shadowBounds.Bottom - ShadowDepth;

            var guidelineSetX = new[]
            {
                centerLeft,
                centerLeft + cornerRadius.TopLeft,
                centerRight - cornerRadius.TopRight,
                centerLeft + cornerRadius.BottomLeft,
                centerRight - cornerRadius.BottomRight,
                centerRight
            };

            var guidelineSetY = new[]
            {
                centerTop,
                centerTop + cornerRadius.TopLeft,
                centerTop + cornerRadius.TopRight,
                centerBottom - cornerRadius.BottomLeft,
                centerBottom - cornerRadius.BottomRight,
                centerBottom
            };

            drawingContext.PushGuidelineSet(new GuidelineSet(guidelineSetX, guidelineSetY));

            cornerRadius.TopLeft += ShadowDepth;
            cornerRadius.TopRight += ShadowDepth;
            cornerRadius.BottomLeft += ShadowDepth;
            cornerRadius.BottomRight += ShadowDepth;

            var topLeft = new Rect(shadowBounds.Left, shadowBounds.Top, cornerRadius.TopLeft, cornerRadius.TopLeft);
            drawingContext.DrawRectangle(brushes[TopLeft], null, topLeft);

            var topWidth = guidelineSetX[2] - guidelineSetX[1];
            if (topWidth > 0)
            {
                var top = new Rect(guidelineSetX[1], shadowBounds.Top, topWidth, ShadowDepth);
                drawingContext.DrawRectangle(brushes[Top], null, top);
            }

            var topRight = new Rect(guidelineSetX[2], shadowBounds.Top, cornerRadius.TopRight, cornerRadius.TopRight);
            drawingContext.DrawRectangle(brushes[TopRight], null, topRight);

            var leftHeight = guidelineSetY[3] - guidelineSetY[1];
            if (leftHeight > 0)
            {
                var left = new Rect(shadowBounds.Left, guidelineSetY[1], ShadowDepth, leftHeight);
                drawingContext.DrawRectangle(brushes[Left], null, left);
            }

            var rightHeight = guidelineSetY[4] - guidelineSetY[2];
            if (rightHeight > 0)
            {
                var right = new Rect(guidelineSetX[5], guidelineSetY[2], ShadowDepth, rightHeight);
                drawingContext.DrawRectangle(brushes[Right], null, right);
            }

            var bottomLeft = new Rect(shadowBounds.Left, guidelineSetY[3], cornerRadius.BottomLeft,
                cornerRadius.BottomLeft);
            drawingContext.DrawRectangle(brushes[BottomLeft], null, bottomLeft);

            var bottomWidth = guidelineSetX[4] - guidelineSetX[3];
            if (bottomWidth > 0)
            {
                var bottom = new Rect(guidelineSetX[3], guidelineSetY[5], bottomWidth, ShadowDepth);
                drawingContext.DrawRectangle(brushes[Bottom], null, bottom);
            }

            var bottomRight = new Rect(guidelineSetX[4], guidelineSetY[4], cornerRadius.BottomRight,
                cornerRadius.BottomRight);
            drawingContext.DrawRectangle(brushes[BottomRight], null, bottomRight);

            if (cornerRadius.TopLeft == ShadowDepth &&
                cornerRadius.TopLeft == cornerRadius.TopRight &&
                cornerRadius.TopLeft == cornerRadius.BottomLeft &&
                cornerRadius.TopLeft == cornerRadius.BottomRight)
            {
                var center = new Rect(guidelineSetX[0], guidelineSetY[0], centerWidth, centerHeight);
                drawingContext.DrawRectangle(brushes[Center], null, center);
            }
            else
            {
                var figure = new PathFigure();

                if (cornerRadius.TopLeft > ShadowDepth)
                {
                    figure.StartPoint = new Point(guidelineSetX[1], guidelineSetY[0]);
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[1], guidelineSetY[1]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[1]), true));
                }
                else
                {
                    figure.StartPoint = new Point(guidelineSetX[0], guidelineSetY[0]);
                }

                if (cornerRadius.BottomLeft > ShadowDepth)
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[3]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[3], guidelineSetY[3]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[3], guidelineSetY[5]), true));
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[5]), true));
                }

                if (cornerRadius.BottomRight > ShadowDepth)
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[4], guidelineSetY[5]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[4], guidelineSetY[4]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[4]), true));
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[5]), true));
                }


                if (cornerRadius.TopRight > ShadowDepth)
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[2]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[2], guidelineSetY[2]), true));
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[2], guidelineSetY[0]), true));
                }
                else
                {
                    figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[0]), true));
                }

                figure.IsClosed = true;
                figure.Freeze();

                var geometry = new PathGeometry();
                geometry.Figures.Add(figure);
                geometry.Freeze();

                drawingContext.DrawGeometry(brushes[Center], null, geometry);
            }

            drawingContext.Pop();
        }
    }

    private static GradientStopCollection CreateStops(Color c, double cornerRadius)
    {
        var gradientScale = 1 / (cornerRadius + ShadowDepth);
        var gsc = new GradientStopCollection { new(c, (0.5 + cornerRadius) * gradientScale) };
        var stopColor = c;
        stopColor.A = (byte)(.74336 * c.A);
        gsc.Add(new GradientStop(stopColor, (1.5 + cornerRadius) * gradientScale));
        stopColor.A = (byte)(.38053 * c.A);
        gsc.Add(new GradientStop(stopColor, (2.5 + cornerRadius) * gradientScale));
        stopColor.A = (byte)(.12389 * c.A);
        gsc.Add(new GradientStop(stopColor, (3.5 + cornerRadius) * gradientScale));
        stopColor.A = (byte)(.02654 * c.A);
        gsc.Add(new GradientStop(stopColor, (4.5 + cornerRadius) * gradientScale));
        stopColor.A = 0;
        gsc.Add(new GradientStop(stopColor, (5 + cornerRadius) * gradientScale));
        gsc.Freeze();
        return gsc;
    }

    private static Brush[] CreateBrushes(Color c, CornerRadius cornerRadius)
    {
        var brushes = new Brush[9];

        brushes[Center] = new SolidColorBrush(c);
        brushes[Center].Freeze();

        var sideStops = CreateStops(c, 0);
        var top = new LinearGradientBrush(sideStops, new Point(0, 1), new Point(0, 0));
        top.Freeze();
        brushes[Top] = top;

        var left = new LinearGradientBrush(sideStops, new Point(1, 0), new Point(0, 0));
        left.Freeze();
        brushes[Left] = left;

        var right = new LinearGradientBrush(sideStops, new Point(0, 0), new Point(1, 0));
        right.Freeze();
        brushes[Right] = right;

        var bottom = new LinearGradientBrush(sideStops, new Point(0, 0), new Point(0, 1));
        bottom.Freeze();
        brushes[Bottom] = bottom;

        GradientStopCollection topLeftStops;
        topLeftStops = cornerRadius.TopLeft == 0 ? sideStops : CreateStops(c, cornerRadius.TopLeft);

        var topLeft = new RadialGradientBrush(topLeftStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(1, 1),
            GradientOrigin = new Point(1, 1)
        };
        topLeft.Freeze();
        brushes[TopLeft] = topLeft;

        GradientStopCollection topRightStops;
        if (cornerRadius.TopRight == 0)
            topRightStops = sideStops;
        else if (cornerRadius.TopRight == cornerRadius.TopLeft)
            topRightStops = topLeftStops;
        else
            topRightStops = CreateStops(c, cornerRadius.TopRight);

        var topRight = new RadialGradientBrush(topRightStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(0, 1),
            GradientOrigin = new Point(0, 1)
        };
        topRight.Freeze();
        brushes[TopRight] = topRight;

        GradientStopCollection bottomLeftStops;
        if (cornerRadius.BottomLeft == 0)
            bottomLeftStops = sideStops;
        else if (cornerRadius.BottomLeft == cornerRadius.TopLeft)
            bottomLeftStops = topLeftStops;
        else if (cornerRadius.BottomLeft == cornerRadius.TopRight)
            bottomLeftStops = topRightStops;
        else
            bottomLeftStops = CreateStops(c, cornerRadius.BottomLeft);

        var bottomLeft = new RadialGradientBrush(bottomLeftStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(1, 0),
            GradientOrigin = new Point(1, 0)
        };
        bottomLeft.Freeze();
        brushes[BottomLeft] = bottomLeft;

        GradientStopCollection bottomRightStops;
        if (cornerRadius.BottomRight == 0)
            bottomRightStops = sideStops;
        else if (cornerRadius.BottomRight == cornerRadius.TopLeft)
            bottomRightStops = topLeftStops;
        else if (cornerRadius.BottomRight == cornerRadius.TopRight)
            bottomRightStops = topRightStops;
        else if (cornerRadius.BottomRight == cornerRadius.BottomLeft)
            bottomRightStops = bottomLeftStops;
        else
            bottomRightStops = CreateStops(c, cornerRadius.BottomRight);

        var bottomRight = new RadialGradientBrush(bottomRightStops)
        {
            RadiusX = 1,
            RadiusY = 1,
            Center = new Point(0, 0),
            GradientOrigin = new Point(0, 0)
        };
        bottomRight.Freeze();
        brushes[BottomRight] = bottomRight;

        return brushes;
    }

    private Brush[] GetBrushes(Color c, CornerRadius cornerRadius)
    {
        if (_commonBrushes == null)
        {
            lock (_resourceAccess)
            {
                if (_commonBrushes == null)
                {
                    _commonBrushes = CreateBrushes(c, cornerRadius);
                    _commonCornerRadius = cornerRadius;
                }
            }
        }

        if (c == ((SolidColorBrush)_commonBrushes[Center]).Color && cornerRadius == _commonCornerRadius)
        {
            _brushes = null;
            return _commonBrushes;
        }

        return _brushes ??= CreateBrushes(c, cornerRadius);
    }
}