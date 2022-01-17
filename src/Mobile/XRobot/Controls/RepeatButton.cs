


using System.Runtime.CompilerServices;


using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;


using XRobot.Helpers;

namespace XRobot.Controls
{
    public class RepeatButton : SKCanvasView
    {

        #region Private Fields

        private SKPaint _paint;

        private SKPoint _position;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Pressed;

        #endregion

        #region Bindable Properties

        public Color IconColor
        {
            get { return (Color)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }

        public static readonly BindableProperty IconColorProperty =
            BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(RepeatButton), Colors.Maroon);

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly BindableProperty PathProperty =
            BindableProperty.Create(nameof(Path), typeof(string), typeof(RepeatButton), "M-50 0L0 50L50 0L-50 0Z", BindingMode.TwoWay);

        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        public static readonly BindableProperty IsPressedProperty =
            BindableProperty.Create(nameof(IsPressed), typeof(bool), typeof(RepeatButton), false);

        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static new readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(RepeatButton), Colors.Maroon);


        #endregion

        #region Constructors

        public RepeatButton()
        {
            EnableTouchEvents = true;

            _paint = new SKPaint()
            {
                IsAntialias = true
            };
        }

        #endregion

        #region Methods

        protected override void OnTouch(SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Pressed)
            {
                IsPressed = true;

                InvalidateSurface();

                Pressed?.Invoke(this, new EventArgs());
            }

            if (e.ActionType == SKTouchAction.Released)
            {
                IsPressed = false;

                InvalidateSurface();
            }

            e.Handled = true;

        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;

            var canvas = e.Surface.Canvas;

            var radius = (info.Width / 2.0f);

            var freeRadius = radius.Scale(0.15f);

            _position = _position.Clamp(-freeRadius, freeRadius);

            canvas.Clear();

            canvas.Save();

            canvas.Translate(radius, radius);

            canvas.Scale(1, -1);

            DrawBase(canvas, radius);

            DrawSocket(canvas, radius);

            DrawIcon(canvas, radius);

            canvas.Restore();

        }

        private void DrawIcon(SKCanvas canvas, float radius)
        {

            canvas.Save();

            var baseRadius = radius.Scale(0.80f);

            canvas.Scale(baseRadius.Scale(0.2f));

            _paint.Style = SKPaintStyle.Stroke;

            _paint.StrokeWidth = baseRadius.Scale(0.01f);

            _paint.ImageFilter = null;

            _paint.Color = IsPressed ? IconColor.ToSKColor() : BackgroundColor.ToBaseGradientColors(IsEnabled)[1];

            canvas.DrawPath(SKPath.ParseSvgPathData(Path), _paint);

            canvas.Restore();
        }

        private void DrawSocket(SKCanvas canvas, float radius)
        {
            canvas.Save();

            _paint.Style = SKPaintStyle.Fill;

            var colors = (IsEnabled, IsPressed) switch
            {
                (var x, var y) when y == true => BackgroundColor.ToSocketGradientColor(x),
                (var x, var y) when y == false => BackgroundColor.ToSocketGradientColor(x).Reverse().ToArray(),
            };

            _paint.Shader = SKShader.CreateRadialGradient(new SKPoint(radius.Scale(0.2f), -radius.Scale(0.2f)),
                                                           radius.Scale(0.8f),
                                                           colors,
                                                           new float[] { 0f, 0.5f, 1f },
                                                           SKShaderTileMode.Clamp);


            canvas.DrawCircle(0, 0, radius.Scale(0.70f), _paint);

            DrawStroke(radius);

            canvas.DrawCircle(0, 0, radius.Scale(0.70f), _paint);

            canvas.Restore();
        }

        private void DrawBase(SKCanvas canvas, float radius)
        {
            canvas.Save();

            _paint.Style = SKPaintStyle.Fill;

            _paint.Shader = SKShader.CreateLinearGradient(new SKPoint(-radius.Scale(1.2f), radius.Scale(1.2f)),
                                                          new SKPoint(radius.Scale(1.2f), -radius.Scale(1.2f)),
                                                          BackgroundColor.ToBaseGradientColors(IsEnabled),
                                                          new float[] { 0f, 0.6f, 1f },
                                                          SKShaderTileMode.Clamp);



            canvas.DrawCircle(0, 0, radius.Scale(0.95f), _paint);

            DrawStroke(radius);

            canvas.DrawCircle(0, 0, radius.Scale(0.95f), _paint);

            canvas.Restore();
        }

        private void DrawStroke(float radius)
        {

            _paint.StrokeWidth = radius.Scale(0.01f);

            _paint.Style = SKPaintStyle.Stroke;

            _paint.Shader = null;

            _paint.Color = BackgroundColor.WithLuminosity(0.3f).ToSKColor();

        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(BackgroundColor) ||
                propertyName == nameof(IsEnabled) ||
                propertyName == nameof(IsPressed))
            {
                InvalidateSurface();
            }

        }

        #endregion

    }
}
