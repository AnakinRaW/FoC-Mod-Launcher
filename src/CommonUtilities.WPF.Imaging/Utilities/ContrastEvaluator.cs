using System;
using System.Buffers;
using System.Windows.Media;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

internal class ContrastEvaluator
{
    private const byte AllowedBackgroundChannelVariance = 2;
    private readonly int _byteCount;
    private readonly PixelBuffer _pixels;
    private readonly int _width;
    private readonly int _height;
    private readonly int _stride;
    private readonly Color _background;
    private readonly byte _minimumContrastEvaluationAlpha;
    private readonly double _minimumPassingPixelRatio;
    private readonly double _minimumContrastRatio;
    private readonly ArrayPool<byte> _antiAntiAliasingBufferPool;
    private readonly Lazy<PixelBuffer> _antiAntiAliasingBufferLazy;

    public ContrastEvaluator(byte[] pixels, int width, int height, Color background,
        byte minimumContrastEvaluationAlpha, double minimumPassingPixelRatio, double minimumContrastRatio)
    {
        _stride = width * 4;
        _byteCount = _stride * height;
        _pixels = new PixelBuffer(pixels, width, height);
        _width = width;
        _height = height;
        _background = Color.FromArgb(byte.MaxValue, background.R, background.G, background.B);
        _minimumContrastEvaluationAlpha = minimumContrastEvaluationAlpha;
        _minimumPassingPixelRatio = minimumPassingPixelRatio;
        _minimumContrastRatio = minimumContrastRatio;
        _antiAntiAliasingBufferPool = ArrayPool<byte>.Shared;
        _antiAntiAliasingBufferLazy = new Lazy<PixelBuffer>(RemoveAntiAliasing);
    }

    public bool ImageHasSufficientContrast()
    {
        var num1 = 0;
        var num2 = 0;
        for (var i = 0; i + 4 <= _byteCount; i += 4)
        {
            if (!IsBackgroundPixel(i) && IsOnEdgeOrAdjacentToBackgroundPixel(i))
            {
                ++num1;
                if (HasSufficientContrast(i))
                    ++num2;
            }
        }
        return num1 == 0 || num2 / (double)num1 >= _minimumPassingPixelRatio;
    }

    private bool HasSufficientContrast(int i)
    {
        return _background.ContrastWith(GetColor(i)) > _minimumContrastRatio;
    }

    private Color GetColor(int i)
    {
        return _antiAntiAliasingBufferLazy.Value.GetColor(i);
    }

    private bool IsOnEdgeOrAdjacentToBackgroundPixel(int i)
    {
        return IsBackgroundPixelOrOutOfBounds(Above(i)) ||
               IsBackgroundPixelOrOutOfBounds(Below(i)) ||
               IsBackgroundPixelOrOutOfBounds(LeftOf(i)) ||
               IsBackgroundPixelOrOutOfBounds(RightOf(i));
    }

    private bool IsBackgroundPixelOrOutOfBounds(int i)
    {
        return IsOutOfBounds(i) || IsBackgroundPixel(i);
    }

    private bool IsBackgroundPixel(int i)
    {
        return Alpha(i) < _minimumContrastEvaluationAlpha || IsCloseToBackground(GetColor(i));
    }

    private byte Alpha(int i)
    {
        return _antiAntiAliasingBufferLazy.Value.PixelBytes[i + 3];
    }

    private bool IsOutOfBounds(int i)
    {
        return i < 0 || i >= _stride * _height;
    }

    private int Above(int i)
    {
        return i - _stride;
    }

    private int Below(int i)
    {
        return i + _stride;
    }

    private int LeftOf(int i)
    {
        return i % _stride != 0 ? i - 4 : -1;
    }

    private int RightOf(int i)
    {
        return (i + 4) % _stride != 0 ? i + 4 : -1;
    }

    private bool IsCloseToBackground(Color color)
    {
        return Math.Abs(color.R - _background.R) <= AllowedBackgroundChannelVariance &&
               Math.Abs(color.G - _background.G) <= AllowedBackgroundChannelVariance &&
               Math.Abs(color.B - _background.B) <= AllowedBackgroundChannelVariance;
    }

    private PixelBuffer RemoveAntiAliasing()
    {
        var resource = _antiAntiAliasingBufferPool.Rent(_byteCount);
        try
        {
            Array.Copy(_pixels.PixelBytes, resource, _byteCount);
            var pixelBuffer = new PixelBuffer(resource, _width, _height);
            pixelBuffer.AlphaBlendWithBackground(_background);
            pixelBuffer.ReplacePixels(3, IsAntiAliasingSequence, ChangeMiddleValueToBackground, _background);
            return pixelBuffer;
        }
        finally
        {
            _antiAntiAliasingBufferPool.Return(resource);
        }
    }

    private bool IsAntiAliasingSequence(Color[] pixels)
    {
        if (pixels.Length != 3)
            throw new ArgumentException("Pixel array should have three elements");
        if (!IsCloseToBackground(pixels[0]) || IsCloseToBackground(pixels[1]))
            return false;
        var num = _background.ContrastWith(pixels[1]);
        return _background.ContrastWith(pixels[2]) > num;
    }

    private Color[] ChangeMiddleValueToBackground(Color[] pixels)
    {
        return pixels.Length == 3
            ? new[]
            {
                pixels[0],
                _background,
                pixels[2]
            }
            : throw new ArgumentException("Pixel array should have three elements");
    }
}