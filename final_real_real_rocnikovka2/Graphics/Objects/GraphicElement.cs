using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace final_real_real_rocnikovka2.Graphics.Objects
{
    public abstract class GraphicElement(Canvas canvas)
    {
        public Canvas MainCanvas { get; protected set; } = canvas;
        public UIElement MainUIElement { get; protected set; }

        public double X => Canvas.GetLeft(MainUIElement);
        public double Y => Canvas.GetTop(MainUIElement);

        public abstract void Delete();

        public abstract void AddToCanvas();

        public abstract void SetPosition(double x, double y);

        public abstract void ChangeColor(Color color);

    }
}
