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
    public class Box : GraphicElement
    {
       


        public double Width
        {
            get => ((Rectangle)MainUIElement).Width;
            set => ((Rectangle)MainUIElement).Width = value;
        }

        public double Height
        {
            get => ((Rectangle)MainUIElement).Height;
            set => ((Rectangle)MainUIElement).Height = value;
        }

        public Box(Canvas canvas, double opacity, Color color, double width, double height) : base(canvas)
        {
            this.MainUIElement = CreateRectangle(opacity, color, width, height);
        }

        private static Rectangle CreateRectangle(double opacity, Color color, double width, double height)
        {
            return new Rectangle
            {
                Fill = new SolidColorBrush(color),
                Opacity = opacity,
                Width = width,
                Height = height,
                Stroke = Brushes.Transparent
            };
        }

        public override void SetPosition(double x, double y)
        {
            Canvas.SetLeft(MainUIElement, x);
            Canvas.SetTop(MainUIElement, y);
        }

        public override void AddToCanvas()
        {
            MainCanvas.Children.Add(MainUIElement);
        }

        public void Update(double prevWidth, double prevHeight)
        {
            double widthScale = (MainCanvas.ActualWidth / prevWidth);
            double heightScale = (MainCanvas.ActualHeight / prevHeight);
            this.Width *= widthScale;
            this.Height *= heightScale;

            SetPosition(X * widthScale, Y * heightScale);
        }

        public override void Delete()
        {
            MainCanvas.Children.Remove(MainUIElement);
        }
        public override void ChangeColor(Color color)
        {
            ((Rectangle)MainUIElement).Fill = new SolidColorBrush(color);
        }

    }
}
