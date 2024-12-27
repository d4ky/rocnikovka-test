using final_real_real_rocnikovka2.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Graphics.Objects
{
    class Line : GraphicElement
    {

        private double Thickness { get; set;}
        public Line(Canvas canvas, double opacity, Point start, Point end, double thickness, Color color, bool isDashed) : base(canvas)
        {
            this.MainUIElement = CreateSmoothLine(opacity, start, end, thickness, color, isDashed);
            this.Thickness = thickness;
            Canvas.SetZIndex(MainUIElement, -1);
        }
        private Path CreateSmoothLine(double opacity, Point start, Point end, double thickness, Color color, bool isDashed)
        {
            double strokeThickness = 0.07 * Draw.BallRadius * thickness;
             var dashArray = isDashed
                ? new DoubleCollection { 1, 2}
                : new DoubleCollection();

            return new Path
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = strokeThickness, 
                SnapsToDevicePixels = true,       
                StrokeLineJoin = PenLineJoin.Round,
                StrokeDashArray = dashArray, 
                
                Opacity = opacity,       

                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = start,  
                            Segments = new PathSegmentCollection
                            {
                                new LineSegment(end, true)  
                            }
                        }
                    }
                }
            };
        }


        public override void AddToCanvas()
        {
            MainCanvas.Children.Add(MainUIElement);
        }

        public override void ChangeColor(Color color)
        {
            ((Path)MainUIElement).Stroke = new SolidColorBrush(color);
        }

        public override void Delete()
        {
            MainCanvas.Children.Remove(MainUIElement);
        }

        public override void SetPosition(double x, double y)
        {
            throw new NotImplementedException();
        }

    }
}
