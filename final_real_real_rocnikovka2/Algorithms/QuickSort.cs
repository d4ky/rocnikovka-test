using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class QuickSort : SortingAlgorithm
    {
        private static readonly QuickSort instance = new();
        public static new QuickSort Instance => instance;

        // STEP VARIABLES
        private int CurrentIndex;
        private double SmallBallRatio = 0.7;
        private List<List<Ball>> TopLine;
        private List<List<Ball>> BottomLine;
        private int CurrentSublistIndex;
        private double LeftXPos;
        private double RightXPos;
        private BoxOutline HighlightBox;
        private Ball GreaterThanSymbol;
        private double GTSyPos;
        private double BottomLineYPos;
        private List<Ball> BottomVisualPivBalls;
        private List<Ball> VisualPivBalls;
        private List<Ball> ResultBalls;
        private double FakeLineYPos;

        private QuickSort()
        {
            Name = AlgorithmName.QUICK_SORT_NAME;
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
            TopLine = new() { Balls };
            BottomLine = new();
            CurrentSublistIndex = 0;
            HighlightBox = null;
            GreaterThanSymbol = null;
            LeftXPos = 0;
            RightXPos = 0;
            BottomLineYPos = Balls[0].MainCanvas.ActualHeight / 2 + 0.5 * Draw.BallRadius * Draw.VerticalGap;
            GTSyPos = BottomLineYPos - Draw.BallRadius - Draw.BallRadius * Draw.VerticalGap * 0.5;
            VisualPivBalls = new();
            BottomVisualPivBalls = new();
            ResultBalls = new();
            Balls.ForEach(e => e.BallText.ChangeColor(ColorPalette.PURE_WHITE));
            FakeLineYPos = Balls[0].MainCanvas.ActualHeight - 2 * Draw.BallRadius;
        }

        public override async Task Sort()
        {
            if (IsSorted()) return;

            int n = Numbers.Count;
            
            await QuickSortRecursion(0, n - 1);

            if (IsSorted())
                Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);
            else
                Draw.ChangeColorForAll(Boxes, ColorPalette.DEFAULT_BAR_FILL);
        }

        private async Task QuickSortRecursion(int left, int right)
        {
            if (IsSorted()) return;
            if (Globals.Stop) return;
            if (left >= right) return;

            int partionIndex = await Partion(left, right);
            if (Globals.Stop) return;

            await QuickSortRecursion(left, partionIndex - 1);
            await QuickSortRecursion(partionIndex + 1, right);
     
        }

        private async Task<int> Partion(int left, int right)
        {
            int pivot = Numbers[right];
            int i = left - 1;

            Boxes[right].ChangeColor(ColorPalette.PIVOT_BAR_FILL);

            for (int j = left; j < right; j++)
            {
                if (Globals.Stop) return 0;
                ComparisonCount++;
                if (Numbers[j] < pivot)
                {
                    i++;
                    SwapCount++;
                    SwapInList(Numbers, i, j);
                    SwapInList(Boxes, i, j);
                    Draw.SwapXPos(Boxes[i], Boxes[j]);

                    Boxes[i].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    Boxes[j].ChangeColor(ColorPalette.SELECTED_BAR_FILL);

                    await Animate.Wait(Globals.AnimationMs, j);
                    if (Globals.Stop) return 0;

                    Boxes[i].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    Boxes[j].ChangeColor(ColorPalette.DEFAULT_BAR_FILL); 

                }
            }

            Boxes[right].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
            SwapCount++;
            SwapInList(Numbers, i + 1, right);
            SwapInList(Boxes, i + 1, right);
            Draw.SwapXPos(Boxes[i + 1], Boxes[right]);

            return i + 1;
        }

        public override void Step()
        {
            if (IsSortedBool) return;

            switch (StepState)
            {
                case 0:
                    // obdelnik kolem current ssublist
                    // select pivots a vybarvit (rightmost) cervene
                    // sedy > nad sublistem

                    GreaterThanSymbol?.Delete();
                    HighlightBox?.Delete();

                    Animate.AnimationClear();
                    HighlightBox = new(Balls[0].MainCanvas, 0, ColorPalette.SELECTED_STROKE, 3 * TopLine[CurrentSublistIndex].Count - 1 + 0.4, 2.4);
                    HighlightBox.SetPosition(TopLine[CurrentSublistIndex][0].X - 0.2 * Draw.BallRadius, TopLine[CurrentSublistIndex][0].Y - 0.2 * Draw.BallRadius);
                    Animate.OpacityChange(HighlightBox, 0.6, 0.2, 0);
                    Canvas.SetZIndex(HighlightBox.MainUIElement, -1);
                    HighlightBox.AddToCanvas();
                    GraphicElements.Add(HighlightBox);
                    

                    double subListXMid = (TopLine[CurrentSublistIndex].First().X + TopLine[CurrentSublistIndex].Last().X) / 2;
                    if (TopLine[CurrentSublistIndex].Count > 1)
                    {
                        GreaterThanSymbol = new(Balls[0].MainCanvas, 0, ColorPalette.PURE_WHITE, ColorPalette.PURE_WHITE, 1);
                        GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 0, ColorPalette.PURE_WHITE, ">", 0);
                        GraphicElements.Add(GreaterThanSymbol);

                        GreaterThanSymbol.SetPosition(subListXMid, GTSyPos);
                        GreaterThanSymbol.AddToCanvas();
                        Animate.OpacityChange(GreaterThanSymbol.BallText, 1, 0.2, 0.4);
                    } 


                    Animate.BallStrokeColorChange(TopLine[CurrentSublistIndex].Last(), ColorPalette.PURE_RED, 0.2, 0);
                    BottomLine.Add(new());
                    BottomLine.Add(new());

                    LeftXPos = TopLine[CurrentSublistIndex][0].X;
                    RightXPos = TopLine[CurrentSublistIndex].Last().X;

                    Animate.AnimationRun();

                    StepState = 1;
                    break;
                case 1:
                    // v danem sublistu select curr index
                    Animate.AnimationClear();
                    if (TopLine[CurrentSublistIndex].Count != 1)
                        Animate.BallStrokeColorChange(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.SELECTED_STROKE, 0.2, 0);
                    Animate.AnimationRun();

                    if (TopLine[CurrentSublistIndex].Count == 1)
                    {
                        StepState = 5;
                    } else
                    {

                        StepState = 2;
                    }
                    break;
                case 2:
                    // vybarvim > nad sublistem spravnou barvou
                    // poslu na posun
                    Animate.AnimationClear();
                    if (int.Parse(TopLine[CurrentSublistIndex][CurrentIndex].GetText()) >= int.Parse(TopLine[CurrentSublistIndex].Last().GetText()))
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_GREEN, 0.2, 0);
                        StepState = 3;
                    } else
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_RED, 0.2, 0);
                        StepState = 4;
                    }
                    Animate.AnimationRun();
                    break;
                case 3:
                    // true
                    // posun na spravny index 
                    Animate.AnimationClear();

                    Ball newRightBall = Draw.CloneBall(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newRightBall.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    Canvas.SetZIndex(newRightBall.MainUIElement, -1);
                    Canvas.SetZIndex(newRightBall.BallText.MainUIElement, -1);
                    GraphicElements.Add(newRightBall);
                    newRightBall.AddToCanvas();

                    BottomLine.Last().Insert(0, TopLine[CurrentSublistIndex][CurrentIndex]);
                    TopLine[CurrentSublistIndex][CurrentIndex] = newRightBall;

                    Animate.MoveBallWithText(BottomLine.Last()[0], RightXPos, BottomLineYPos, 1, 0);
                    Canvas.SetZIndex(BottomLine.Last()[0].MainUIElement, 1);
                    Canvas.SetZIndex(BottomLine.Last()[0].BallText.MainUIElement, 1);

                    Animate.ScheduleZIndex(BottomLine.Last()[0], 0);
                    Animate.ScheduleZIndex(BottomLine.Last()[0].BallText, 0);

                    Animate.BallStrokeColorChange(BottomLine.Last()[0], ColorPalette.DEFAULT_STROKE, 0.2, 1);

                    Animate.AnimationRun();

                    StepState = 1;
                    CurrentIndex++;
                    RightXPos -= Draw.BallRadius * 3;

                    if (CurrentIndex == TopLine[CurrentSublistIndex].Count - 1)
                    {
                        StepState = 5;
                        Animate.OpacityChange(GreaterThanSymbol, 0, 0.5, 0);
                        Animate.ScheduleForDeletion(GreaterThanSymbol);
                    }

                    break;
                case 4:
                    // falsch
                    // posun na spravny index
                    // pokud je currindex + 1 pivot tak poslu na pivot animaci
                    Animate.AnimationClear();

                    Ball newLeftBall = Draw.CloneBall(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newLeftBall.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    Canvas.SetZIndex(newLeftBall.MainUIElement, -1);
                    Canvas.SetZIndex(newLeftBall.BallText.MainUIElement, -1);
                    GraphicElements.Add(newLeftBall);
                    newLeftBall.AddToCanvas();

                    BottomLine[BottomLine.Count - 2].Add(TopLine[CurrentSublistIndex][CurrentIndex]);
                    TopLine[CurrentSublistIndex][CurrentIndex] = newLeftBall;

                    Animate.MoveBallWithText(BottomLine[BottomLine.Count - 2].Last(), LeftXPos, BottomLineYPos, 1, 0);
                    Canvas.SetZIndex(BottomLine[BottomLine.Count - 2].Last().MainUIElement, 1);
                    Canvas.SetZIndex(BottomLine[BottomLine.Count - 2].Last().BallText.MainUIElement, 1);

                    Animate.ScheduleZIndex(BottomLine[BottomLine.Count - 2].Last(), 0);
                    Animate.ScheduleZIndex(BottomLine[BottomLine.Count - 2].Last().BallText, 0);

                    Animate.BallStrokeColorChange(BottomLine[BottomLine.Count - 2].Last(), ColorPalette.DEFAULT_STROKE, 0.2, 1);

                    
                    StepState = 1;
                    CurrentIndex++;
                    LeftXPos += Draw.BallRadius * 3;

                    if (CurrentIndex == TopLine[CurrentSublistIndex].Count - 1)
                    {
                        StepState = 5;
                        Animate.OpacityChange(GreaterThanSymbol, 0, 0.5, 0);
                        Animate.ScheduleForDeletion(GreaterThanSymbol);
                    }
                    Animate.AnimationRun();
                    break;
                case 5:
                    // pivot animace
                    
                    Animate.AnimationClear();

                    Ball newPivBall = Draw.CloneBall(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newPivBall.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    Canvas.SetZIndex(newPivBall.MainUIElement, -1);
                    Canvas.SetZIndex(newPivBall.BallText.MainUIElement, -1);
                    GraphicElements.Add(newPivBall);
                    newPivBall.AddToCanvas();
                    VisualPivBalls.Add(newPivBall);

                    Animate.OpacityChange(HighlightBox, 0, 0.4, 1);
                    Animate.ScheduleForDeletion(HighlightBox);

                    Animate.MoveBallWithText(TopLine[CurrentSublistIndex][CurrentIndex], LeftXPos, BottomLineYPos, 1, 0);
                   
                    
                    Animate.AnimationRun();
                    StepState = 6;
                    break;
                case 6:
                    // dalsik sublist
                    // pokud neni dalsi sublist tak animace radku
                    Animate.AnimationClear();

                    Ball newBottomPivBall = Draw.CloneBall(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    //((TextBlock)newBottomPivBall.BallText.MainUIElement).Text = "P";
                    newBottomPivBall.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    Canvas.SetZIndex(newBottomPivBall.MainUIElement, -1);
                    Canvas.SetZIndex(newBottomPivBall.BallText.MainUIElement, -1);
                    GraphicElements.Add(newBottomPivBall);
                    newBottomPivBall.AddToCanvas();
                    BottomVisualPivBalls.Add(newBottomPivBall);

                    Animate.BallStrokeColorChange(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.DEFAULT_STROKE, 0.3, 0);

                    Animate.MoveBallWithText(TopLine[CurrentSublistIndex][CurrentIndex], LeftXPos, FakeLineYPos, 1, 0);
                    Animate.BallStrokeColorChange(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREEN_STROKE, 0, 1);
                    Animate.BallFillColorChange(TopLine[CurrentSublistIndex][CurrentIndex], ColorPalette.GREEN_FILL, 0, 1);

                    ResultBalls.Add(TopLine[CurrentSublistIndex][CurrentIndex]);

                    TopLine[CurrentSublistIndex][CurrentIndex] = Draw.CloneBall(TopLine[CurrentSublistIndex][CurrentIndex], 0);
                  


                    if (CurrentSublistIndex == TopLine.Count - 1)
                    {
                        StepState = 7;
                        if (BottomLine.Where(e => e.Count > 0).Count() == 0)
                        {
                            StepState = 9;
                            
                        }
                    } else
                    {
                        // vsyechny ruzyn ++ atd :(
                        CurrentSublistIndex++;
                        CurrentIndex = 0;
                        StepState = 0;
                        
                        //tam smazat box atd
                    }
                    Animate.AnimationRun();

                    break;
                case 7:
                    // animace radku
                    // prvni sublist
                    Animate.AnimationClear();


                    for (int i = 0; i < TopLine.Count; i++)
                    {
                        for (int j = 0; j < TopLine[i].Count; j++)
                        {
                            Animate.MoveBallWithText(TopLine[i][j], TopLine[i][j].X, TopLine[i][j].Y - Draw.BallRadius * (2 + Draw.VerticalGap), 1, 0.4);
                            Animate.OpacityChange(TopLine[i][j], 0, 1, 0.4);
                            Animate.OpacityChange(TopLine[i][j].BallText, 0, 1, 0.4);
                            Animate.ScheduleForDeletion(TopLine[i][j]);
                        }
                    }

                    foreach (Ball b in VisualPivBalls)
                    {
                        Animate.MoveBallWithText(b, b.X, b.Y - Draw.BallRadius * (2 + Draw.VerticalGap), 1, 0.4);
                        Animate.OpacityChange(b, 0, 1, 0.4);
                        Animate.OpacityChange(b.BallText, 0, 1, 0.4);
                        Animate.ScheduleForDeletion(b);
                    }

                    for (int i = 0; i < BottomLine.Count; i++)
                    {
                        for (int j = 0; j < BottomLine[i].Count; j++)
                        {
                            Animate.MoveBallWithText(BottomLine[i][j], BottomLine[i][j].X, BottomLine[i][j].Y - Draw.BallRadius * (2 + Draw.VerticalGap), 1, 0.4);
                        }
                    }

                    foreach (Ball b in BottomVisualPivBalls)
                    {
                        Animate.MoveBallWithText(b, b.X, b.Y - Draw.BallRadius * (2 + Draw.VerticalGap), 1, 0.4);
                    }

                    TopLine.Clear();
                    

                    VisualPivBalls = BottomVisualPivBalls;
                    BottomVisualPivBalls = new();

                    foreach (var sublist in BottomLine)
                    {
                        if (sublist.Count > 0)
                        {
                            TopLine.Add(new List<Ball>(sublist));
                        }
                    }
                    BottomLine = new();
                    CurrentSublistIndex = 0;
                    CurrentIndex = 0;
                    StepState = 8;

                    Animate.AnimationRun();
                    break;
                case 8:
                    Animate.AnimationClear();

                    foreach (Ball b in VisualPivBalls)
                    {
                        Ball newBottomPivBall2 = Draw.CloneBall(b);
                        Canvas.SetZIndex(newBottomPivBall2.MainUIElement, -1);
                        Canvas.SetZIndex(newBottomPivBall2.BallText.MainUIElement, -1);
                        GraphicElements.Add(newBottomPivBall2);
                        newBottomPivBall2.AddToCanvas();
                        BottomVisualPivBalls.Add(newBottomPivBall2);

                        Animate.MoveBallWithText(newBottomPivBall2, newBottomPivBall2.X, newBottomPivBall2.Y + Draw.BallRadius * Draw.VerticalGap + 2 * Draw.BallRadius, 1, 0);
                    }

                  

                    Animate.AnimationRun();
                    StepState = 0;
                    break;
                case 9:
                    Animate.AnimationClear();
                    foreach (Ball b in VisualPivBalls.Concat(BottomVisualPivBalls))
                    {
                        Animate.OpacityChange(b, 0, 1, 0);
                        Animate.OpacityChange(b.BallText, 0, 1, 0);
                        Animate.ScheduleForDeletion(b);
                    }

                    foreach (Ball b in ResultBalls)
                    {
                        Animate.MoveBallWithText(b, b.X, b.MainCanvas.Height / 2 - Draw.BallRadius, 1, 1);
                    }

                    foreach (Ball b in GraphicElements.OfType<Ball>().Where(e => e.GetFillColor() == ColorPalette.GREY_FILL))
                    {
                        Animate.OpacityChange(b, 0, 1, 0);
                        Animate.ScheduleForDeletion(b);
                    }

                    Animate.AnimationRun();

                    Numbers.Sort();
                    Balls.Sort((ball1, ball2) => Comparer<int>.Default.Compare(int.Parse(ball1.GetText()), int.Parse(ball2.GetText())));
                    
                    IsSortedBool = true;
                    
                    break;
            }
        }

        public override void OnSelect(List<int> numbers, List<Ball> balls)
        {
            Canvas canvas = balls[0].MainCanvas;
            double xPos = (canvas.ActualWidth - Draw.BallRadius * (3 * N - 1)) / 2;
            double yPos = canvas.ActualHeight / 2 - 0.5 * Draw.BallRadius * Draw.VerticalGap - 2 * Draw.BallRadius;
            double smallBallRadius = Draw.BallRadius * SmallBallRatio;
            foreach (Ball ball in Balls)
            {
                ball.SetPosition(xPos, yPos);
                GraphicElements.Add(ball);

                Ball newBall = new(canvas, 1, ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE, SmallBallRatio);
                newBall.SetPosition(xPos + (Draw.BallRadius - smallBallRadius), FakeLineYPos + (Draw.BallRadius - smallBallRadius));
                newBall.AddToCanvas();
                Canvas.SetZIndex(newBall.MainUIElement, -1);
                GraphicElements.Add(newBall);

                xPos += 3 * Draw.BallRadius;
            }
        }

    }
}
