

using System.Runtime.CompilerServices;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using XRobot.Helpers;

namespace XRobot.Controls
{
    public class Sheet : SKCanvasView
    {

        #region Private Fields

        private readonly SKPaint _paint;

        private readonly SKPath _path;

        #endregion

        #region Bindable Properties

        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static new readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(RepeatButton), Colors.Maroon);


        #endregion

        #region Constructors

        public Sheet()
        {
            _paint = new SKPaint()
            {
                IsAntialias = true,
            };

            _path = new SKPath();
        }

        #endregion

        #region Methods

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;

            var canvas = e.Surface.Canvas;

            canvas.Clear();

            canvas.Save();

            _path.MoveTo(0, 0);

            _path.LineTo(info.Width, 0);

            _path.LineTo(info.Width, info.Height / 2.5f);

            _path.LineTo(0, info.Height);

            _path.Close();

            _paint.Style = SKPaintStyle.Fill;

            _paint.Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0),
                                                          new SKPoint(info.Width, ((float)info.Height).Scale(0.5f)),
                                                          new SKColor[]
                                                          {
                                                              BackgroundColor.ToBaseGradientColors(IsEnabled)[0],
                                                              BackgroundColor.ToBaseGradientColors(IsEnabled)[1],

                                                          }, SKShaderTileMode.Clamp);

            _paint.ImageFilter = SKImageFilter.CreateDropShadow(0, 1f, 3f, 3f, SKColors.Black);

            canvas.DrawPath(_path, _paint);

            canvas.Restore();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(BackgroundColor) ||
                propertyName == nameof(IsEnabled))
            {
                InvalidateSurface();
            }
        }

        #endregion
    }
}