using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CavemanChronicles
{
    public class TiledImageView : SKCanvasView
    {
        public static readonly BindableProperty SourceProperty =
            BindableProperty.Create(nameof(Source), typeof(string), typeof(TiledImageView), null, propertyChanged: OnSourceChanged);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly BindableProperty OpacityValueProperty =
            BindableProperty.Create(nameof(OpacityValue), typeof(double), typeof(TiledImageView), 1.0, propertyChanged: OnPropertyChanged);

        public double OpacityValue
        {
            get => (double)GetValue(OpacityValueProperty);
            set => SetValue(OpacityValueProperty, value);
        }

        private SKBitmap _bitmap;

        public TiledImageView()
        {
            PaintSurface += OnPaintSurface;
        }

        private static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = (TiledImageView)bindable;
            view.LoadImage((string)newValue);
        }

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((TiledImageView)bindable).InvalidateSurface();
        }

        private async void LoadImage(string source)
        {
            if (string.IsNullOrEmpty(source))
                return;

            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync(source);
                _bitmap = SKBitmap.Decode(stream);
                InvalidateSurface();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_bitmap == null)
                return;

            using var shader = SKShader.CreateBitmap(
                _bitmap,
                SKShaderTileMode.Repeat,
                SKShaderTileMode.Repeat
            );

            using var paint = new SKPaint
            {
                Shader = shader,
                Color = SKColors.White.WithAlpha((byte)(OpacityValue * 255))
            };

            canvas.DrawRect(0, 0, e.Info.Width, e.Info.Height, paint);
        }
    }
}