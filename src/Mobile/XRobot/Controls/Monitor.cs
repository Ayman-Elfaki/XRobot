

using System.Reflection;
using System.Runtime.CompilerServices;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using XRobot.Helpers;

namespace XRobot.Controls
{
    public class Monitor : SKCanvasView
    {
        #region Private Fields

        private SKPaint _paint;

        private SKPaint _indicatorPaint;

        private SKPaint _textPaint;

        private SKTypeface _typeface;

        private float _oldValue = 0f;

        #endregion

        #region Bindable Properties

        public new Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public static new readonly BindableProperty BackgroundColorProperty =
            BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(Monitor), Colors.Maroon);

        public Color IndicatorsColor
        {
            get { return (Color)GetValue(IndicatorsColorProperty); }
            set { SetValue(IndicatorsColorProperty, value); }
        }

        public static readonly BindableProperty IndicatorsColorProperty =
            BindableProperty.Create(nameof(IndicatorsColor), typeof(Color), typeof(Monitor), Colors.Maroon);

        public float Value
        {
            get { return (float)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(float), typeof(Monitor), 0.0f, BindingMode.TwoWay, propertyChanged: OnpropertyChanged, coerceValue: OncoerceValue);

        private static object OncoerceValue(BindableObject bindable, object value)
        {
            var reader = bindable as Monitor;

            var currentValue = (float)value;

            if (currentValue > 100f || currentValue < 0f)
            {
                return reader._oldValue;
            }

            reader._oldValue = currentValue;

            return reader._oldValue = currentValue; ;
        }

        private static void OnpropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Monitor monitor)
            {
                monitor.InvalidateSurface();
            }
        }


        #endregion

        #region Constructors

        public Monitor()
        {
            EnableTouchEvents = true;

            _paint = new SKPaint()
            {
                IsAntialias = true
            };

            _indicatorPaint = new SKPaint()
            {
                IsAntialias = true
            };

            _textPaint = new SKPaint()
            {
                IsAntialias = true,
                IsStroke = true,
            };

            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(Monitor)).Assembly;

            var stream = assembly.GetManifestResourceStream("XRobot.Resources.Fonts.OriginTech.ttf");

            _typeface = SKTypeface.FromStream(stream);

            SizeChanged += (s, a) => InvalidateSurface();
        }

        #endregion

        #region Methods

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            var info = e.Info;

            var canvas = e.Surface.Canvas;

            var radius = (info.Width / 2.0f);


            canvas.Clear();


            canvas.Save();

            canvas.Translate(radius, radius);

            canvas.Scale(1, -1);


            DrawBase(canvas, radius);

            DrawIndicators(canvas, radius);

            DrawSocket(canvas, radius);


            canvas.Restore();

        }


        private void DrawIndicators(SKCanvas canvas, float radius)
        {

            canvas.Save();

            var baseRadius = radius.Scale(0.75f);

            var size = radius.Scale(0.15f);


            _indicatorPaint.Style = SKPaintStyle.Stroke;

            _indicatorPaint.StrokeCap = SKStrokeCap.Round;

            _indicatorPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 0.5f, 0.5f, SKColors.Black);

            _indicatorPaint.StrokeWidth = radius.Scale(0.04f);


            for (int i = 0; i < 25; i++)
            {
                canvas.RotateDegrees(10);

                canvas.Save();

                canvas.RotateDegrees(230);

                canvas.Translate(0, baseRadius);

                _indicatorPaint.Color = i <= Value.Map(0, 99, 25, 0) ? BackgroundColor.ToBaseGradientColors(IsEnabled)[1] : IndicatorsColor.ToSKColor();

                canvas.DrawLine(0, 0, 0, size, _indicatorPaint);

                canvas.Restore();

            }


            canvas.Restore();
        }

        private void DrawSocket(SKCanvas canvas, float radius)
        {
            canvas.Save();

            _paint.Style = SKPaintStyle.Fill;

            _paint.Color = BackgroundColor.ToBaseGradientColors(IsEnabled)[0];

            canvas.DrawCircle(0, 0, radius.Scale(0.70f), _paint);

            canvas.Save();

            canvas.Scale(1, -1);

            _textPaint.StrokeWidth = 2;

            _textPaint.Style = SKPaintStyle.StrokeAndFill;

            _textPaint.TextSize = radius.Scale(0.5f);

            _textPaint.Typeface = _typeface;

            _textPaint.Color = IndicatorsColor.ToSKColor();


            canvas.DrawText(Value.ToString("000."), new SKPoint(-radius.Scale(0.45f), radius.Scale(0.20f)), _textPaint);

            canvas.Restore();

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

            _paint.ImageFilter = null;

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
            if (propertyName == nameof(Value) ||
                propertyName == nameof(IsEnabled) ||
                propertyName == nameof(BackgroundColor) ||
                propertyName == nameof(IndicatorsColor) )
            {
                InvalidateSurface();
            }
        }

        #endregion
    }
}