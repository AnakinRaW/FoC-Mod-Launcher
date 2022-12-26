using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Validation;

namespace Sklavenwalker.CommonUtilities.Wpf.Imaging;

internal class PixelBuffer
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _byteCount;

    public PixelBuffer(byte[] pixelBytes, int width, int height)
    {
        PixelBytes = pixelBytes;
        _width = width;
        _height = height;
        _byteCount = width * height * 4;
    }

    public byte[] PixelBytes { get; }

    public void AlphaBlendWithBackground(Color background)
    {
        for (var i = 0; i < _byteCount; i += 4)
        {
            var color = background.BlendWith(GetColor(i));
            SetColor(i, color);
        }
    }

    public void ReplacePixels(int sequenceLength, Predicate<Color[]> predicate, Func<Color[], Color[]> transform, Color background)
    {
        bool flag;
        do
        {
            flag = false;
            foreach (var pixelSequence in GetPixelSequences(sequenceLength))
            {
                var pixels = pixelSequence.GetPixels(background);
                if (predicate(pixels))
                {
                    flag = true;
                    pixelSequence.ReplacePixels(transform(pixels));
                }
            }
        }
        while (flag);
    }

    public Color GetColor(int i)
    {
        return Color.FromArgb(PixelBytes[i + 3], PixelBytes[i + 2], PixelBytes[i + 1], PixelBytes[i]);
    }

    public void SetColor(int i, Color color)
    {
        PixelBytes[i + 3] = color.A;
        PixelBytes[i + 2] = color.R;
        PixelBytes[i + 1] = color.G;
        PixelBytes[i] = color.B;
    }

    private IEnumerable<PixelSequence> GetPixelSequences(int sequenceLength)
    {
        for (var x = -1; x <= _width; ++x)
        {
            for (var y = -1; y <= _height; ++y)
            {
                if (y >= sequenceLength - 1)
                    yield return SequenceUpwardFrom(x, y, sequenceLength);
                if (y <= _height - sequenceLength + 1)
                    yield return SequenceDownwardFrom(x, y, sequenceLength);
                if (x >= sequenceLength - 1)
                    yield return SequenceLeftwardFrom(x, y, sequenceLength);
                if (x <= _width - sequenceLength + 1)
                    yield return SequenceRightwardFrom(x, y, sequenceLength);
            }
        }
    }

    private PixelSequence SequenceUpwardFrom(int x, int y, int length)
    {
        return new PixelSequence(this,
            Enumerable.Range(0, length).Select(i => GetIndex(x, y - i)).ToArray());
    }

    private PixelSequence SequenceDownwardFrom(int x, int y, int length)
    {
        return new PixelSequence(this,
            Enumerable.Range(0, length).Select(i => GetIndex(x, y + i)).ToArray());
    }

    private PixelSequence SequenceLeftwardFrom(int x, int y, int length)
    {
        return new PixelSequence(this,
            Enumerable.Range(0, length).Select(i => GetIndex(x - i, y)).ToArray());
    }

    private PixelSequence SequenceRightwardFrom(int x, int y, int length)
    {
        return new PixelSequence(this,
            Enumerable.Range(0, length).Select(i => GetIndex(x + i, y)).ToArray());
    }

    private int GetIndex(int x, int y)
    {
        return x < 0 || y < 0 || x >= _width || y >= _height ? -1 : (x + y * _width) * 4;
    }

    private class PixelSequence
    {
        private readonly PixelBuffer _source;
        private readonly int[] _indices;

        public PixelSequence(PixelBuffer source, int[] indices)
        {
            _source = Requires.NotNull(source, nameof(source));
            _indices = Requires.NotNull(indices, nameof(indices));
        }

        public Color[] GetPixels(Color background)
        {
            return _indices.Select(i => i != -1 ? _source.GetColor(i) : background).ToArray();
        }

        public void ReplacePixels(IReadOnlyList<Color> newValues)
        {
            if (newValues.Count != _indices.Length)
                throw new ArgumentException();
            for (var index = 0; index < newValues.Count; ++index)
            {
                if (_indices[index] != -1)
                    _source.SetColor(_indices[index], newValues[index]);
            }
        }
    }
}