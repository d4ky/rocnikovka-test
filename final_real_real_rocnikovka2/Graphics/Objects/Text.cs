using final_real_real_rocnikovka2.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Graphics.Objects
{
    public class Text : GraphicElement
    {
        public double TextWidth => MainUIElement.DesiredSize.Width;
        public double TextHeight => MainUIElement.DesiredSize.Height;

        public double RotationAngle { get; private set; }


        public double FontSize
        {
            get => ((TextBlock)MainUIElement).FontSize;
            set => ((TextBlock)MainUIElement).FontSize = value;
        }

        public Text(Canvas canvas, double opacity, Color color, string content, double rotationAngle) : base(canvas)
        {
            this.RotationAngle = rotationAngle;
            this.MainUIElement = CreateTextBlock(opacity, color, content);
            Measure();
        }

        private static TextBlock CreateTextBlock(double opacity, Color color, string content)
        {
            return new TextBlock
            {
                Text = content,
                Foreground = new SolidColorBrush(color),
                Opacity = opacity,
                FontWeight = FontWeights.Bold,
                FontSize = Draw.BallRadius / 3 * 2,
                TextAlignment = TextAlignment.Center
            };
        }

        public override void SetPosition(double x, double y)
        {
            Measure();
            Canvas.SetLeft(MainUIElement, x);
            Canvas.SetTop(MainUIElement, y);

            if (RotationAngle != 0)
            {
                RotateTransform rotateTransform = new()
                {
                    Angle = RotationAngle,
                    CenterX = TextWidth / 2,
                    CenterY = TextHeight / 2
                };
                MainUIElement.RenderTransform = rotateTransform;
            }
        }

        public void Measure()
        {
            this.FontSize =  Draw.BallRadius / 3 * 2;
            MainUIElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }

        public override void AddToCanvas()
        {
            MainCanvas.Children.Add(MainUIElement);
        }

        public override void Delete()
        {
            MainCanvas.Children.Remove(MainUIElement);
        }
        public override void ChangeColor(Color color)
        {
            ((TextBlock)MainUIElement).Foreground = new SolidColorBrush(color);
        }

        public Color GetColor()
        {
            return ((SolidColorBrush)((TextBlock)MainUIElement).Foreground).Color;
        }
    }
}
