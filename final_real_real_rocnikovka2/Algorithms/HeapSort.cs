using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class HeapSort : SortingAlgorithm
    {
        private static readonly HeapSort instance = new();
        public static new SortingAlgorithm Instance => instance;

        // STEP VARIABLES
        private int CurrentIndex;
        private Ball GreaterThanSymbol;
        private double SmallBallRatio = 0.7;
        private int NumOfLayers;
        private Dictionary<int, Point> TreeBallPos;
        private Dictionary<int, Point> BottomLinePos;
        private List<Ball> TopLineFakes;
        private List<Ball> BottomLineFakes;
        private int NumberDone;

        private HeapSort()
        {
            Name = AlgorithmName.HEAP_SORT_NAME;
        }

        public override void Reset(List<int> numbers, List<Box> boxes)
        {
            Numbers = numbers;
            Boxes = boxes;
            ComparisonCount = 0;
            SwapCount = 0;

        }
        public override void Reset(List<int> numbers, List<Ball> balls, List<GraphicElement> graphicElements)
        {
            foreach (var item in graphicElements)
            {
                item.Delete();
            }
            Numbers = numbers;
            Balls = balls;
            GraphicElements = graphicElements;
            IsSortedBool = false;
            N = Numbers.Count;
            CurrentIndex = 0;
            StepState = 0;
            TreeBallPos = [];
            NumOfLayers = (int)Math.Ceiling(Math.Log2(N + 1));
            TopLineFakes = [];
            BottomLineFakes = [];
            BottomLinePos = [];
            GreaterThanSymbol = null;
            NumberDone = 0;
        }

        public override async Task Sort()
        {
            if (IsSorted()) return;
            int n = Numbers.Count;

            for (int i = n/2 - 1; i >= 0; i--)
            {
                if (Globals.Stop) return;
                await Heapify(n, i);
            }

            for (int i = n -1; i >= 1; i--)
            {
                if (Globals.Stop) return;
                SwapInList(Numbers, 0, i);
                SwapInList(Boxes, 0, i);

                Draw.SwapXPos(Boxes[i], Boxes[0]);
                Boxes[0].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                Boxes[i].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                SwapCount++;

                await Animate.Wait(Globals.AnimationMs, i);
                if (Globals.Stop) return;

                await Heapify(i, 0);
            }
            Boxes[0].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
        }

        private async Task Heapify(int n, int i)
        {
            if (Globals.Stop) return;
            int largest = i;
            int leftChild = 2 * i + 1;
            int rightChild = 2 * i + 2;

            ComparisonCount++;
            if (leftChild < n && Numbers[leftChild] > Numbers[largest])
            {
                largest = leftChild;
            }
            ComparisonCount++;
            if (rightChild < n && Numbers[rightChild] > Numbers[largest])
            {
                largest = rightChild;
            }

            if (largest != i)
            {
                SwapCount++;
                SwapInList(Numbers, i, largest);
                SwapInList(Boxes, i, largest);

                Draw.SwapXPos(Boxes[i], Boxes[largest]);
                Boxes[largest].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                Boxes[i].ChangeColor(ColorPalette.SELECTED_BAR_FILL);

                await Animate.Wait(Globals.AnimationMs, i);
                if (Globals.Stop) return;

                Boxes[largest].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                Boxes[i].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);

                await Heapify(n, largest);
            }
        }

        public override void Step()
        {
            if (IsSortedBool) return;
            switch (StepState)
            {
                case 0:
                    //Selectne top line ball
                    Animate.AnimationClear();
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    Animate.AnimationRun();
                    StepState = 1;
                    break;
                case 1:
                    Animate.AnimationClear();
                    
                    Canvas.SetZIndex(Balls[CurrentIndex].BallText.MainUIElement, 1);
                    Canvas.SetZIndex(Balls[CurrentIndex].MainUIElement, 1);
                    Ball newBall = Draw.CloneBall(Balls[CurrentIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newBall.AddToCanvas();
                    newBall.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    TopLineFakes.Add(newBall);
                    GraphicElements.Add(newBall);
                    Animate.MoveBallWithText(Balls[CurrentIndex], TreeBallPos[CurrentIndex].X, TreeBallPos[CurrentIndex].Y, 1, 0);
                    //Balls[CurrentIndex].SetPosition(TreeBallPos[CurrentIndex].X, TreeBallPos[CurrentIndex ].Y);
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.DEFAULT_STROKE, 1, 0);
                    StepState = 0;
                    CurrentIndex++;

                    if (CurrentIndex == N)
                    {
                        StepState = 2;
                        CreateBottomLine();
                    } 
                    Animate.AnimationRun();
                    break;
                case 2:
                    Animate.AnimationClear();
                    foreach (Ball b in TopLineFakes)
                    {
                        Animate.OpacityChange(b, 0, 1, 0);
                        Animate.OpacityChange(b.BallText, 0, 1, 0);
                        Animate.ScheduleForDeletion(b);
                    }

                    foreach (Ball b in Balls)
                    {
                        Animate.MoveBallWithText(b, b.X, b.Y - 2 * Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap, 1, 1);
                    }

                    foreach (GraphicElement g in GraphicElements)
                    {
                        Animate.MoveGraphicElement(g, g.X, g.Y - 2 * Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap, 1, 1);
                    }

                    foreach (Ball b in BottomLineFakes)
                    {
                        Animate.MoveGraphicElement(b, b.X, b.Y - 1 * Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap, 1, 2);
                        Animate.OpacityChange(b, 1, 1.5, 2);
                    }


                    Animate.AnimationRun();
                    for (int i = 0; i < N; i++)
                    {
                        Point p = new(TreeBallPos[i].X, TreeBallPos[i].Y - 2 * Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap);
                        TreeBallPos[i] = p;
                    }
                    CurrentIndex = N - 1;
                    StepState = 3;
                    break;
                case 3:
                    GreaterThanSymbol?.Delete();
                    Animate.AnimationClear();
                    if (CurrentIndex + 1 != N && Balls[CurrentIndex + 1].GetStrokeColor() == ColorPalette.SELECTED_STROKE)
                        Animate.BallStrokeColorChange(Balls[CurrentIndex + 1], ColorPalette.DEFAULT_STROKE, 0.5, 0);
                    if (CurrentIndex + 1 != N && (int)Math.Floor((double)(CurrentIndex - 1) / 2) != (int)Math.Floor((double)(CurrentIndex) / 2) && Balls[(int)Math.Floor((double)(CurrentIndex) / 2)].GetStrokeColor() == ColorPalette.SELECTED_STROKE)
                        Animate.BallStrokeColorChange(Balls[(int)Math.Floor((double)(CurrentIndex) / 2)], ColorPalette.DEFAULT_STROKE, 0, 0);
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    if (Balls[(int)Math.Floor((double)(CurrentIndex - 1) / 2)].GetStrokeColor() != ColorPalette.SELECTED_STROKE)
                        Animate.BallStrokeColorChange(Balls[(int)Math.Floor((double)(CurrentIndex - 1) / 2)], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    Animate.AnimationRun();
                    StepState = 4;
                    break;
                case 4:
                    Animate.AnimationClear();
                    GreaterThanSymbol = new(Balls[0].MainCanvas, 0, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, 1);
                    
                    
                    GraphicElements.Add(GreaterThanSymbol);

                    int indexChild = CurrentIndex;
                    int indexParent = (int)Math.Floor((double)(CurrentIndex - 1) / 2);
                    double xPos = (TreeBallPos[indexParent].X + TreeBallPos[indexChild].X) / 2;
                    double yPos = (TreeBallPos[indexParent].Y + TreeBallPos[indexChild].Y) / 2;

                    

                    double deltaX = TreeBallPos[indexChild].X - TreeBallPos[indexParent].X;
                    double deltaY = TreeBallPos[indexChild].Y - TreeBallPos[indexParent].Y;
                    double angleInRadians = Math.Atan2(deltaY, deltaX);
                    double angleInDegrees = angleInRadians * (180 / Math.PI);



                    
                    if (Numbers[CurrentIndex] > Numbers[(int)Math.Floor((double)(CurrentIndex - 1) / 2)])
                    {
                        //nakreslit comparison se spravnym uhlem, cervnea
                        GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 1, Colors.WhiteSmoke, ">", angleInDegrees);
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_RED, 0, 0);
                        StepState = 5;
                    } else
                    {
                        if (Numbers[CurrentIndex] == Numbers[(int)Math.Floor((double)(CurrentIndex - 1) / 2)])
                        {
                            //nakreslit s rovna se
                            GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 1, Colors.WhiteSmoke, "=", angleInDegrees);
                            Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_GREEN, 0, 0);
                        } else
                        {
                            // nakreslit comparison se spravnymn uhlem, zelena
                            GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 1, Colors.WhiteSmoke, ">", angleInDegrees);
                            Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_GREEN, 0, 0);
                        }
       
                        if (CurrentIndex - 1 == 0)
                        {
                            StepState = 6; // horni dolni
                        } else
                        {
                            StepState = 3;
                            CurrentIndex--;
                        }
                    }
                    GreaterThanSymbol.SetPosition(xPos, yPos);
                    GreaterThanSymbol.AddToCanvas();
                    Animate.AnimationRun();
                    break;
                case 5:
                    //swap ve strome
                    Animate.AnimationClear();
                    Animate.BallSwapInTree(Balls[CurrentIndex], Balls[(int)Math.Floor((double)(CurrentIndex - 1) / 2)], 1, 0);
                    SwapInList(Numbers, CurrentIndex, (int)Math.Floor((double)(CurrentIndex - 1) / 2));
                    SwapInList(Balls, CurrentIndex, (int)Math.Floor((double)(CurrentIndex - 1) / 2));
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.DEFAULT_STROKE, 0.5, 0);
                    Animate.BallStrokeColorChange(Balls[(int)Math.Floor((double)(CurrentIndex - 1) / 2)], ColorPalette.DEFAULT_STROKE, 0.5, 0);
                    Animate.OpacityChange(GreaterThanSymbol.BallText, 0, 1, 0);
                    Animate.AnimationRun(); 

                    if (CurrentIndex - 1 == 0)
                    {
                        StepState = 6;
                    } else
                    {
                        StepState = 3;
                        CurrentIndex--;
                    }

                    break;
                case 6:
                    GreaterThanSymbol?.Delete();
                    Animate.AnimationClear();
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.DEFAULT_STROKE, 0, 0);
                    Animate.BallStrokeColorChange(Balls[0], ColorPalette.DEFAULT_STROKE, 0, 0);
                    Animate.BallStrokeColorChange(Balls[0], ColorPalette.PURE_RED, 0.5, 0);
                    Animate.BallStrokeColorChange(Balls[N - 1 - NumberDone], ColorPalette.PURE_RED, 0.5, 0);
                    Animate.AnimationRun();
                    StepState = 7;
                    break;
                case 7:
                    // horni dolni

                    Animate.AnimationClear();
                    
                    

                    Animate.MoveBallWithText(Balls[0], BottomLinePos[N - 1 - NumberDone].X, BottomLinePos[N - 1 - NumberDone].Y, 1, 1);
                    
                    if (NumberDone != N - 2)
                        Animate.MoveBallWithText(Balls[N - 1 - NumberDone], TreeBallPos[0].X, TreeBallPos[0].Y, 1, 2);
                    
                    Animate.BallFillColorChange(Balls[0], ColorPalette.GREEN_FILL, 0, 2);
                    Animate.BallStrokeColorChange(Balls[0], ColorPalette.GREEN_STROKE, 0, 2);
                    Animate.BallStrokeColorChange(Balls[N - 1 - NumberDone], ColorPalette.DEFAULT_STROKE, 1, 2);

                    
                    SwapInList(Balls, 0, N - 1 - NumberDone);
                    SwapInList(Numbers, 0, N - 1 - NumberDone);

                    NumberDone++;
                    if (N - NumberDone == 1)
                    {
                        StepState = 8;
                        Animate.MoveBallWithText(Balls[0], BottomLinePos[N - 1 - NumberDone].X, BottomLinePos[N - 1 - NumberDone].Y, 1, 3);
                        Animate.BallFillColorChange(Balls[0], ColorPalette.GREEN_FILL, 0, 4);
                        Animate.BallStrokeColorChange(Balls[0], ColorPalette.GREEN_STROKE, 0, 4);
                       


                    } else
                    {
                        CurrentIndex = N - 1 - NumberDone;
                        StepState = 3;
                    }
                    Animate.AnimationRun();
                    break;
                case 8:
                    IsSortedBool = true;
                    Animate.AnimationClear();
                    foreach (GraphicElement g in GraphicElements)
                    {
                        Animate.OpacityChange(g, 0, 1, 0);
                        if (!BottomLineFakes.Contains(g))
                            Animate.MoveGraphicElement(g, g.X, g.Y - 3 * Draw.BallRadius, 1, 0);
                    }

                    foreach (Ball b in Balls)
                    {
                        Animate.MoveBallWithText(b, b.X, Balls[0].MainCanvas.ActualHeight / 2 - Draw.BallRadius, 1, 1);
                    }
                    Animate.AnimationRun();
                    break;
            }

        }

        public override void OnSelect(List<int> numbers, List<Ball> balls)
        {
            Canvas canvas = balls[0].MainCanvas;

            double xPos = (canvas.ActualWidth - Draw.BallRadius * (3 * N - 1)) / 2;
            double yPos = Draw.BallRadius * Draw.VerticalGap;
            foreach (Ball ball in balls)
            {
                ball.SetPosition(xPos, yPos);
                xPos += 3 * Draw.BallRadius;
            }

            double smallBallRadius = SmallBallRatio * Draw.BallRadius;
            double numOfBallsInGap;
            double gapSize;
            int realIndex;

            yPos = 2 * Draw.BallRadius * (Draw.VerticalGap + 1);

            for (int i = 0; i < NumOfLayers; i++)
            {
                numOfBallsInGap = Math.Pow(2, NumOfLayers - i) - 1;
                gapSize = Draw.BallRadius * (3 * numOfBallsInGap + 1);
                xPos = (canvas.ActualWidth - (2 * Draw.BallRadius * Math.Pow(2, i) + gapSize * (Math.Pow(2, i) - 1))) / 2;

                for (int j = 0; j <= Math.Pow(2, i) - 1; j++)
                {
                    realIndex = (int)Math.Pow(2, i) - 1 + j;
                    TreeBallPos[realIndex] = new(xPos, yPos);


                    Ball ball = new(canvas, 1, ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE, SmallBallRatio);
                    ball.SetPosition(xPos + (Draw.BallRadius - smallBallRadius), yPos + (Draw.BallRadius - smallBallRadius));
                    ball.AddToCanvas();
                    GraphicElements.Add(ball);
                    xPos += gapSize + 2 * Draw.BallRadius;
                }
                yPos += Draw.BallRadius * (2 + Draw.VerticalGap);
            }

        }

        private void CreateBottomLine()
        {
            Canvas canvas = Balls[0].MainCanvas;

            double smallBallRadius = Draw.BallRadius * SmallBallRatio;
            double yPos = TreeBallPos[TreeBallPos.Keys.Max()].Y + 2 * Draw.BallRadius + (Draw.BallRadius * Draw.VerticalGap);
            double xPos = (canvas.ActualWidth - Draw.BallRadius * (3 * N - 1)) / 2;
            
            for (int i = 0; i < N; i++)
            {
                Ball ball = new(canvas, 0, ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE, SmallBallRatio);
                ball.SetPosition(xPos + (Draw.BallRadius - smallBallRadius), yPos + (Draw.BallRadius - smallBallRadius));
                ball.AddToCanvas();
                BottomLineFakes.Add(ball);
                GraphicElements.Add(ball);
                BottomLinePos[i] = new(xPos, yPos -  Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap );
                xPos += 3 * Draw.BallRadius;
            }
        }
    }
}
