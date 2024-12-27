using final_real_real_rocnikovka2.Algorithms;
using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Graphics.Rendering
{
    public static class Draw
    {
        private static double ballRadius;
        public static double VerticalGap = 1.5;
        public static double BallRadius
        {
            get => ballRadius;
            private set => ballRadius = value;
        }
        public static void UpdateBallRadius(SortingAlgorithm? sortingAlgorithm, int n, Canvas canvas)
        {
            switch (sortingAlgorithm)
            {
                case HeapSort:
                    BallRadius = Math.Min(canvas.ActualWidth / ((3 * 2 * Math.Pow(2, Math.Ceiling(Math.Log2(n + 1)) - 1)) + 1), canvas.ActualHeight / (2 * Math.Ceiling(Math.Log2(n + 1)) + 2 + VerticalGap * (Math.Ceiling(Math.Log2(n + 1)) + 2)));
                    break;
                case MergeSort:
                    BallRadius = Math.Min(canvas.ActualWidth / (6 * n), canvas.ActualHeight / (VerticalGap * (2 * Math.Ceiling(Math.Log2(n)) + 2) + 2 * (2 * Math.Ceiling(Math.Log2(n)) + 1)));
                    break;
                case QuickSort:
                    BallRadius = Math.Min(canvas.ActualWidth / (3 * n + 1), canvas.ActualHeight / (16 + VerticalGap));
                    break;
                default:
                    BallRadius = Math.Min(canvas.ActualWidth / (3 * n + 1), canvas.ActualHeight / 6);
                    break;
            }
        }

        public static void SwapXPos(GraphicElement gE1, GraphicElement gE2)
        {
            double tempX = gE1.X;

            gE1.SetPosition(gE2.X, gE1.Y);
            gE2.SetPosition(tempX, gE2.Y);
        }

        public static async void DrawDone(IEnumerable<GraphicElement> listGE, Color color)
        {
            Globals.EndAnimationIsRunning = true;
            foreach (GraphicElement gE in listGE)
            {
                gE.ChangeColor(color);
                await Task.Delay(1);
            }
            Globals.EndAnimationIsRunning = false;
        }
        public static async void ChangeColorForAll(IEnumerable<Ball> listGE, Color fillColor, Color strokeColor, bool withDelay = true)
        {
            foreach (Ball gE in listGE)
            {
                gE.ChangeColor(fillColor);
                gE.SetStrokeColor(strokeColor);

                if (withDelay) await Task.Delay(1);
            }
        }

        public static void ChangeColorForAll(IEnumerable<GraphicElement> listGE, Color color)
        {
            foreach (GraphicElement gE in listGE)
            {
                gE.ChangeColor(color);
            }
        }

        public static Ball CloneBall(Ball ball, Color fill, Color stroke)
        {
            Ball newBall = new(ball.MainCanvas, 1, fill, stroke, ball.BallRadiusRatio);
            newBall.BallText = new(ball.MainCanvas, 1, ball.BallText.GetColor(), ((TextBlock)ball.BallText.MainUIElement).Text, 0);
            newBall.SetPosition(ball.X, ball.Y);
            return newBall;
        }

        public static Ball CloneBall(Ball ball, double opacity = 1)
        {
            Ball newBall = new(ball.MainCanvas, opacity, ball.GetFillColor(), ball.GetStrokeColor(), ball.BallRadiusRatio);
            newBall.BallText = new(ball.MainCanvas, opacity, ball.BallText.GetColor(), ((TextBlock)ball.BallText.MainUIElement).Text, 0);
            newBall.SetPosition(ball.X, ball.Y);
            return newBall;
        }


    }
}
