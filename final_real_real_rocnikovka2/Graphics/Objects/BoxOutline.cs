using final_real_real_rocnikovka2.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Graphics.Objects
{
    public class BoxOutline : GraphicElement
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="BoxOutline"/> class.
        /// The <paramref name="height"/> and <paramref name="width"/> parameters
        /// are multipliers of the <see cref="Draw.BallRadius"/> to determine the size of the box outline.
        /// </summary>
        /// <param name="canvas">The canvas on which the outline will be drawn.</param>
        /// <param name="opacity">The opacity of the box outline.</param>
        /// <param name="color">The color of the box outline.</param>
        /// <param name="width">The width multiplier based on <see cref="Draw.BallRadius"/>.</param>
        /// <param name="height">The height multiplier based on <see cref="Draw.BallRadius"/>.</param>
        public BoxOutline(Canvas canvas, double opacity, Color color, double width, double height) : base(canvas)
        {
            this.MainUIElement = CreateRectangle(opacity, color, width, height);
            Canvas.SetZIndex(MainUIElement, 0);
        }

        private static Rectangle CreateRectangle(double opacity, Color color, double width, double height)
        {
            return new Rectangle
            {
                Opacity = opacity,
                Fill = Brushes.Transparent,
                StrokeThickness = Draw.BallRadius * 0.05,
                Stroke = new SolidColorBrush(color),
                Width = Draw.BallRadius * width,
                Height = Draw.BallRadius * height
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
        public override void Delete()
        {
            MainCanvas.Children.Remove(MainUIElement);
        }
        public override void ChangeColor(Color color)
        {
            throw new NotImplementedException();
        }

    }
}
