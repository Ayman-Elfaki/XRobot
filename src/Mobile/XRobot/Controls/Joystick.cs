



using System.Diagnostics;
using System.Windows.Input;
using System.Runtime.CompilerServices;

using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

using XRobot.Helpers;
using Microsoft.Maui.Controls;

namespace XRobot.Controls;

public class Joystick : SKCanvasView
{
 
    #region Private Fields

    private SKPaint _paint;

    private SKPaint _indicatorPaint;

    private SKPath _indicatorPath;

    private SKPoint _position;

    #endregion

    #region Events

    public event EventHandler<EventArgs> DigitalMoved;

    #endregion

    #region Bindable Properties

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Joystick));

    public object CommandParameter
    {
        get { return (object)GetValue(CommandParameterProperty); }
        set { SetValue(CommandParameterProperty, value); }
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Joystick));

    public Color IndicatorsColor
    {
        get { return (Color)GetValue(IndicatorsColorProperty); }
        set { SetValue(IndicatorsColorProperty, value); }
    }

    public static readonly BindableProperty IndicatorsColorProperty =
        BindableProperty.Create(nameof(IndicatorsColor), typeof(Color), typeof(Joystick), Colors.Gold);

    public new Color BackgroundColor
    {
        get { return (Color)GetValue(BackgroundColorProperty); }
        set { SetValue(BackgroundColorProperty, value); }
    }

    public static new readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(Joystick), Colors.Gold);


    public JoystickDirection Direction
    {
        get { return (JoystickDirection)GetValue(DirectionProperty); }
        set { SetValue(DirectionProperty, value); }
    }

    public static readonly BindableProperty DirectionProperty =
        BindableProperty.Create(nameof(Direction), typeof(JoystickDirection), typeof(Joystick), defaultValue: JoystickDirection.None);


    #endregion

    #region Constructors

    public Joystick()
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
    }

    #endregion

    #region Methods

    protected override void OnTouch(SKTouchEventArgs e)
    {
        if (e.InContact && IsEnabled)
        {

            var inSizeMax = new SKSize((float)Bounds.Width, (float)Bounds.Height);

            var outSizeMin = new SKSize(-inSizeMax.Width / 2, -inSizeMax.Height / 2);

            var outSizeMax = new SKSize(inSizeMax.Width / 2, inSizeMax.Height / 2);

            _position = e.Location.Map(new SKSize(0, 0), inSizeMax, outSizeMin, outSizeMax);

            // Hack to correct the position on the phone
            if (e.DeviceType == SKTouchDeviceType.Touch)
            {
                _position.Y += 100f;
                _position.X -= 100f;
            }

        }
        else 
        {
            _position = new SKPoint(0, 0);
        }

        InvalidateSurface();

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

        DrawIndicators(canvas, radius);

        DrawSocket(canvas, radius);


        canvas.Save();

        canvas.Translate(_position.X, _position.Y);

        DrawThumbLower(canvas, radius);

        DrawThumpUpper(canvas, radius);

        DrawBumps(canvas, radius);

        canvas.Restore();


        canvas.Restore();

    }


    private void DrawIndicators(SKCanvas canvas, float radius)
    {
        var threashHold = radius.Scale(0.05f);

        var previousDirection = Direction;

        canvas.Save();

        var baseRadius = radius.Scale(0.80f);

        var size = radius.Scale(0.08f);

        _indicatorPath = SKPath.ParseSvgPathData($"M-{size} 0L0 {size}L{size} 0L-{size} 0Z");

        _indicatorPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 1, baseRadius.Scale(0.005f), baseRadius.Scale(0.005f), SKColors.Gray);


        // Top

        canvas.Save();

        var isForward = _position.Y > threashHold;

        _indicatorPaint.Color = isForward ? IndicatorsColor.ToSKColor() : BackgroundColor.ToEngravedGradientColor(IsEnabled);

        Direction = isForward ? Direction & (~JoystickDirection.None) | JoystickDirection.Forward : Direction & (~JoystickDirection.Forward);

        canvas.Translate(0, baseRadius);

        canvas.RotateDegrees(0);

        canvas.DrawPath(_indicatorPath, _indicatorPaint);

        canvas.Restore();

        // Rigth

        canvas.Save();

        var isRigth = _position.X > threashHold;

        _indicatorPaint.Color = isRigth ? IndicatorsColor.ToSKColor() : BackgroundColor.ToEngravedGradientColor(IsEnabled);

        Direction = isRigth ? Direction & (~JoystickDirection.None) | JoystickDirection.Right : Direction & (~JoystickDirection.Right);

        canvas.Translate(baseRadius, 0);

        canvas.RotateDegrees(-90);

        canvas.DrawPath(_indicatorPath, _indicatorPaint);

        canvas.Restore();

        // Backward

        canvas.Save();

        var isBackward = _position.Y < -threashHold;

        _indicatorPaint.Color = isBackward ? IndicatorsColor.ToSKColor() : BackgroundColor.ToEngravedGradientColor(IsEnabled);

        Direction = isBackward ? Direction & (~JoystickDirection.None) | JoystickDirection.Backward : Direction & (~JoystickDirection.Backward);

        canvas.Translate(0, -baseRadius);

        canvas.RotateDegrees(180);

        canvas.DrawPath(_indicatorPath, _indicatorPaint);

        canvas.Restore();


        // Left

        canvas.Save();

        var isLeft = _position.X < -threashHold;

        _indicatorPaint.Color = isLeft ? IndicatorsColor.ToSKColor() : BackgroundColor.ToEngravedGradientColor(IsEnabled);

        Direction = isLeft ? Direction & (~JoystickDirection.None) | JoystickDirection.Left : Direction & (~JoystickDirection.Left);

        canvas.Translate(-baseRadius, 0);

        canvas.RotateDegrees(90);

        canvas.DrawPath(_indicatorPath, _indicatorPaint);

        canvas.Restore();

        if (!isForward && !isBackward && !isRigth && !isLeft)
        {
            Direction = JoystickDirection.None;
        }

        if (Direction != previousDirection)
        {
            DigitalMoved?.Invoke(this, new EventArgs());
            Execute(Command);
        }

        canvas.Restore();
    }

    private void DrawBumps(SKCanvas canvas, float radius)
    {
        canvas.Save();

        _paint.Style = SKPaintStyle.Fill;

        _paint.Shader = SKShader.CreateRadialGradient(new SKPoint(radius.Scale(0.01f), -radius.Scale(0.01f)),
                                                       radius.Scale(0.05f),
                                                       BackgroundColor.BumpsGradientColor(IsEnabled),
                                                       new float[] { 0f, 0.8f, 1f },
                                                       SKShaderTileMode.Clamp);



        canvas.Save();

        canvas.Translate(0, radius.Scale(0.25f));

        canvas.DrawCircle(0, 0, radius.Scale(0.05f), _paint);

        canvas.Restore();

        canvas.Save();

        canvas.Translate(0, -radius.Scale(0.25f));

        canvas.DrawCircle(0, 0, radius.Scale(0.05f), _paint);

        canvas.Restore();

        canvas.Save();

        canvas.Translate(radius.Scale(0.25f), 0);

        canvas.DrawCircle(0, 0, radius.Scale(0.05f), _paint);

        canvas.Restore();

        canvas.Save();

        canvas.Translate(-radius.Scale(0.25f), 0);

        canvas.DrawCircle(0, 0, radius.Scale(0.05f), _paint);

        canvas.Restore();

        _paint.ImageFilter = null;

        canvas.Restore();
    }

    private void DrawThumpUpper(SKCanvas canvas, float radius)
    {
        canvas.Save();

        _paint.Style = SKPaintStyle.Fill;

        _paint.Shader = SKShader.CreateLinearGradient(new SKPoint(radius.Scale(0.37f), 0),
                                          new SKPoint(-radius.Scale(0.37f), radius.Scale(0.37f)),
                                         BackgroundColor.ThumbGradientColor(IsEnabled).Reverse().ToArray(),
                                          new float[] { 0f, 1f },
                                          SKShaderTileMode.Clamp);


        canvas.DrawCircle(0, 0, radius.Scale(0.37f), _paint);

        DrawStroke(radius);

        canvas.Restore();
    }

    private void DrawThumbLower(SKCanvas canvas, float radius)
    {
        canvas.Save();

        _paint.Style = SKPaintStyle.Fill;

        _paint.Shader = SKShader.CreateLinearGradient(new SKPoint(radius.Scale(0.45f), 0),
                                            new SKPoint(-radius.Scale(0.45f), radius.Scale(0.45f)),
                                            BackgroundColor.ThumbGradientColor(IsEnabled),
                                            new float[] { 0f, 1f },
                                            SKShaderTileMode.Clamp);


        canvas.DrawCircle(0, 0, radius.Scale(0.45f), _paint);

        DrawStroke(radius);

        canvas.DrawCircle(0, 0, radius.Scale(0.45f), _paint);

        canvas.Restore();
    }

    private void DrawSocket(SKCanvas canvas, float radius)
    {
        canvas.Save();

        _paint.Style = SKPaintStyle.Fill;

        _paint.Shader = SKShader.CreateRadialGradient(new SKPoint(radius.Scale(0.2f), -radius.Scale(0.2f)),
                                                       radius.Scale(0.8f),
                                                       BackgroundColor.ToSocketGradientColor(IsEnabled),
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
        if (propertyName == nameof(BackgroundColor) ||
            propertyName == nameof(IndicatorsColor) ||
            propertyName == nameof(Width) ||
            propertyName == nameof(Height) ||
            propertyName == nameof(IsEnabled))
        {
            InvalidateSurface();

            EnableTouchEvents = true;
        }
    }

    #endregion

    #region Helper Methods

    private void Execute(ICommand command)
    {
        if (command == null)
            return;

        if (command.CanExecute(null))
        {
            command.Execute(CommandParameter);
        }
    }

    #endregion

}


[Flags]
public enum JoystickDirection : byte
{
    Forward = 1,
    Backward = 2,
    Left = 4,
    Right = 8,
    None = 16,
}
