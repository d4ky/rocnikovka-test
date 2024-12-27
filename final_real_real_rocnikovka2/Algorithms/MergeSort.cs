using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class MergeSort : SortingAlgorithm
    {
        private static readonly MergeSort instance = new();
        public static new MergeSort Instance => instance;

        // STEP VARIABLES
        private int NumberOfLayers;
        private int CurrentLayer;
        private int CurrentIndex;
        private Dictionary<int, List<List<Ball>>> BallLayers;
        private Dictionary<(int left, int right), (int left, int right)> Connections;
        private int LeftIndex;
        private int RightIndex;
        private int CurrentJoinLayer;
        private BoxOutline LeftBox;
        private BoxOutline RightBox;
        private List<Ball> LeftParentSubList;
        private List<Ball> RightParentSubList;
        private Ball GreaterThanSymbol;
        private List<Line> ComparisonLines;

        private MergeSort()
        {
            Name = AlgorithmName.MERGE_SORT_NAME;
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
            NumberOfLayers =(int)(2 * Math.Ceiling(Math.Log2(N)) + 1);
            BallLayers = new() { { 0, new(){ Balls } }  };
            Connections = new();
            CurrentLayer = 0;
            LeftIndex = 0;
            RightIndex = 0;
            CurrentJoinLayer = 0;
            LeftBox = null;
            RightBox = null;
            LeftParentSubList = null;
            RightParentSubList = null;
            GreaterThanSymbol = null;
            ComparisonLines = new();
        }

        public override async Task Sort()
        {
            if (IsSorted()) return;

            int n = Numbers.Count;

            await MergeSortRecursion(0, n - 1);

            if (Globals.Stop) return;

            if (IsSorted())
                Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);
            else 
                Draw.DrawDone(Boxes, ColorPalette.DEFAULT_BAR_FILL);
        }

        private async Task MergeSortRecursion(int left, int right)
        {
            if (Globals.Stop) return;
            if (left >= right) return;
            int mid = left + (right - left) / 2;

            await MergeSortRecursion(left, mid);
            await MergeSortRecursion(mid + 1, right);

            await Merge(left, mid, right);
        }
        private async Task Merge(int left, int mid, int right)
        {
            int firstRight = mid + 1;
            ComparisonCount++;
            if (Numbers[mid] <= Numbers[firstRight]) return;

            while (left <= mid && firstRight <= right)
            {
                if (Globals.Stop) return;
                ComparisonCount++;
                if (Numbers[left] <= Numbers[firstRight])
                {
                    left++;
                } else
                {
                    int index = firstRight;

                    
                    while (index > left) // Takhle komplkovane to delam jen kvuli vykresu, jinak by to bylo lehci
                    {
                        SwapCount++;
                        if (Globals.Stop) return;
                        SwapInList(Numbers, index, index - 1);
                        SwapInList(Boxes, index, index - 1);
                        Draw.SwapXPos(Boxes[index], Boxes[index - 1]);

                        index--;
                    }

                    Boxes[firstRight].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    Boxes[index].ChangeColor(ColorPalette.SELECTED_BAR_FILL);

                    await Animate.Wait(Globals.AnimationMs, index);
                    if (Globals.Stop) return;

                    Boxes[firstRight].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    Boxes[index].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    left++;
                    mid++;
                    firstRight++;
                }

            }

        }

        public override void Step()
        {
            if (IsSortedBool) return;

            switch (StepState)
            {
                case 0:
                    CurrentIndex = 0;
                    Animate.AnimationClear();
                    foreach (var sublist in BallLayers[CurrentLayer])
                    {
                        int half = sublist.Count / 2;

                        if (half - 1 < 0)
                        {
                            Connections.Add(
                                (CurrentIndex, -1),
                                (CurrentIndex + half, CurrentIndex + sublist.Count - 1)
                            );
                        } else
                        {
                            Connections.Add(
                                (CurrentIndex, CurrentIndex + half - 1),
                                (CurrentIndex + half, CurrentIndex + sublist.Count - 1)
                            );
                        }

                        if (BallLayers.TryGetValue(CurrentLayer + 1, out var layer))
                        {
                            var firstHalf = sublist.GetRange(0, half); 
                            var secondHalf = sublist.GetRange(half, sublist.Count - half);
                            layer.Add(firstHalf);
                            layer.Add(secondHalf);
                        }
                        else
                        {
                            BallLayers.Add(CurrentLayer + 1, new List<List<Ball>> { sublist.GetRange(0, half) });
                            BallLayers[CurrentLayer + 1].Add(sublist.GetRange(half, sublist.Count - half));
                        }

                        CurrentIndex += sublist.Count;
                    }
                    //MessageBox.Show(string.Join('\n', BallLayers.Select(kvp => $"{kvp.Key} : {string.Join(' ', kvp.Value.Select(e => $"({string.Join(' ', e)})"))}")));
                    //MessageBox.Show(string.Join('\n', Connections.Select(kvp => $"({kvp.Key.left}, {kvp.Key.right}) : ({kvp.Value.left} : {kvp.Value.right})")));
                    Canvas canvas = Balls[0].MainCanvas;

                    
                    double xPos = (canvas.ActualWidth - Draw.BallRadius *(3 * (N * 2 - BallLayers[CurrentLayer + 1].Last().Count) - 1)) / 2;
                    double yPos = Draw.BallRadius * ((CurrentLayer + 2) * Draw.VerticalGap + 2 * (CurrentLayer + 1));
                    int temp = 0;
                    double parentGroupXPos = 0;
                    double parentGroupYPos = Draw.BallRadius * ((CurrentLayer + 1) * Draw.VerticalGap + 2 * (CurrentLayer)) + 2 * Draw.BallRadius;

                    for (int i = 0; i < BallLayers[CurrentLayer + 1].Count; i++)
                    {
                        
                        List<Ball> list = BallLayers[CurrentLayer + 1][i];
                        double thisGroupXPos = 0;
                        if (i % 2 == 0) temp = 0; 
                        for (int j = 0; j < list.Count; j++)
                        {
                            Ball newBall = Draw.CloneBall(list[j], 0);
                            GraphicElements.Add(newBall);
                            newBall.AddToCanvas();
                            Ball parentBall = BallLayers[CurrentLayer][i / 2][temp]; //HEEEEEEEEEEEEEREEEEEEEEE
                            newBall.SetPosition(parentBall.X, parentBall.Y);

                            Animate.MoveBallWithText(newBall, xPos, yPos, 1, 0);
                            Animate.OpacityChange(newBall, 1, 0.6, 0.3);
                            Animate.OpacityChange(newBall.BallText, 1, 0.6, 0.3);
                            if (j == list.Count / 2) thisGroupXPos = xPos;
                            list[j] = newBall;
                            xPos += 3 * Draw.BallRadius;
                            temp += 1;
                        }

                        if (i % 2 == 0)
                        {
                            parentGroupXPos = (BallLayers[CurrentLayer][i / 2][0].X + BallLayers[CurrentLayer][i / 2][BallLayers[CurrentLayer][i / 2].Count - 1].X)/2 + Draw.BallRadius;
                        }

                        
                        if (thisGroupXPos != 0)
                        {
                            if (list.Count % 2 == 0)
                            {
                                thisGroupXPos -= 1.5 * Draw.BallRadius;
                            }
                            
                            Line line = new(Balls[0].MainCanvas, 0, new(thisGroupXPos + Draw.BallRadius, yPos - 0.2 * Draw.BallRadius), new(parentGroupXPos, parentGroupYPos + 0.2 * Draw.BallRadius), 1, ColorPalette.PURE_WHITE, (BallLayers[CurrentLayer][i / 2].Count == 1));
                            GraphicElements.Add(line);
                            line.AddToCanvas();
                            Animate.OpacityChange(line, 1, 0.5, 1);
                            Canvas.SetZIndex(line.MainUIElement, -1);
                        }
                        

                        xPos += Draw.BallRadius * (3 * list.Count);
                    }

                    if (CurrentLayer == (int)(Math.Floor((double)NumberOfLayers / 2)) - 1)
                    {
                        StepState = 1;
                        CurrentIndex = 0;
                    }
                    CurrentLayer += 1;
                    Animate.AnimationRun();
                    break;
                case 1:
                    //MessageBox.Show(string.Join('\n', Connections.Select(kvp => $"({kvp.Key.left}, {kvp.Key.right}) : ({kvp.Value.left} : {kvp.Value.right})")));
                    
                    Animate.AnimationClear();
                    if (LeftBox != null)
                    {
                        Animate.OpacityChange(LeftBox, 0, 0.2, 0);
                        Animate.ScheduleForDeletion(LeftBox);

                    }
                    yPos = Draw.BallRadius * ((CurrentLayer + 1) * Draw.VerticalGap + 2 * (CurrentLayer));
                    if (Connections.TryGetValue((CurrentIndex, -1), out _))
                    {

                        Ball parentBall = GetItemAtOverallIndex(BallLayers[CurrentLayer], CurrentIndex);
                        Ball newBall = Draw.CloneBall(parentBall);
                        newBall.AddToCanvas();
                        GraphicElements.Add(newBall);

                        Canvas.SetZIndex(newBall.MainUIElement, 2);
                        Canvas.SetZIndex(newBall.BallText.MainUIElement, 2);
                        Animate.ScheduleZIndex(newBall, 0);
                        Animate.ScheduleZIndex(newBall.BallText, 0);

                        Ball tempBall = GetItemAtOverallIndex(BallLayers[CurrentLayer - CurrentJoinLayer - 1], CurrentIndex);
                        Animate.MoveBallWithText(newBall,
                            tempBall.X,
                            yPos + 2 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap,
                            1, 0.5);

                        Line nL = new(newBall.MainCanvas, 0, new(parentBall.X + Draw.BallRadius, parentBall.Y + 2.2 * Draw.BallRadius),
                            new(tempBall.X + Draw.BallRadius, yPos + 1.8 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap), 1, ColorPalette.PURE_WHITE, true);
                        nL.AddToCanvas();
                        Animate.OpacityChange(nL, 1, 0.5, 1);
                        GraphicElements.Add(nL);
                        Canvas.SetZIndex(nL.MainUIElement, -1);

                        BoxOutline bO = new(newBall.MainCanvas, 0, ColorPalette.SELECTED_STROKE, 2.4, 2.4);
                        Canvas.SetZIndex(bO.MainUIElement, 0);
                        Animate.OpacityChange(bO, 0.6, 0.2, 0);
                        GraphicElements.Add(bO);
                        bO.SetPosition(parentBall.X - 0.2 * Draw.BallRadius, parentBall.Y - 0.2 * Draw.BallRadius);
                        bO.AddToCanvas();
                        LeftBox = bO;

                        if (BallLayers.TryGetValue(CurrentLayer+1, out var llB))
                        {
                            llB.Add(new List<Ball>() { newBall });
                        } else
                        {
                            BallLayers.Add(CurrentLayer + 1, new List<List<Ball>>() { new() {newBall } });
                        }

                        Connections.Remove((CurrentIndex, -1));
                        CurrentIndex++;
                    } else if (Connections.TryGetValue((CurrentIndex, CurrentIndex + GetSublistContainingIndex(BallLayers[CurrentLayer], CurrentIndex).Count - 1), out var connect))
                    {
                        LeftIndex = 0;
                        RightIndex = 0;

                        Ball leftFirstParent = GetItemAtOverallIndex(BallLayers[CurrentLayer], CurrentIndex);
                        Ball rightFirstParent = GetItemAtOverallIndex(BallLayers[CurrentLayer], connect.left);
                        LeftParentSubList = GetSublistContainingIndex(BallLayers[CurrentLayer], CurrentIndex);
                        RightParentSubList = GetSublistContainingIndex(BallLayers[CurrentLayer], CurrentIndex + LeftParentSubList.Count);

                        BoxOutline bO1 = new(leftFirstParent.MainCanvas, 0, ColorPalette.SELECTED_STROKE, 3* LeftParentSubList.Count - 1 + 0.4, 2.4);
                        GraphicElements.Add(bO1);
                        bO1.SetPosition(leftFirstParent.X - 0.2 * Draw.BallRadius, leftFirstParent.Y - 0.2 * Draw.BallRadius);
                        Canvas.SetZIndex(bO1.MainUIElement, 0);
                        bO1.AddToCanvas();
                        LeftBox = bO1;

                        BoxOutline bO2 = new(rightFirstParent.MainCanvas, 0, ColorPalette.SELECTED_STROKE, 3 * RightParentSubList.Count - 1 + 0.4, 2.4);
                        GraphicElements.Add(bO2);
                        bO2.SetPosition(rightFirstParent.X - 0.2 * Draw.BallRadius, rightFirstParent.Y - 0.2 * Draw.BallRadius);
                        Canvas.SetZIndex(bO2.MainUIElement, 0);
                        bO2.AddToCanvas();
                        RightBox = bO2;

                        Animate.OpacityChange(bO1, 0.6, 0.2, 0);
                        Animate.OpacityChange(bO2, 0.6, 0.2, 0);

                        StepState = 2;
                        if (BallLayers.TryGetValue(CurrentLayer + 1, out var e))
                        {
                            e.Add(new());
                        }
                        else
                        {

                            BallLayers[CurrentLayer + 1] = new List<List<Ball>> { new List<Ball>() };
                        }
                        

                    }
                    Animate.AnimationRun();
                    break;
                case 2:
                    
                    Animate.AnimationClear();
                    
                    Animate.BallStrokeColorChange(LeftParentSubList[LeftIndex], ColorPalette.PURE_RED, 0, 0);
                    Animate.BallStrokeColorChange(RightParentSubList[RightIndex], ColorPalette.PURE_RED, 0, 0);

                    Animate.AnimationRun();
                    StepState = 3;
                    break;
                case 3:
                    Animate.AnimationClear();
                    Ball leftParent = LeftParentSubList[LeftIndex];
                    Ball rightParent = RightParentSubList[RightIndex];
                    Line newLine1 = new(leftParent.MainCanvas, 0,
                        new(leftParent.X + Draw.BallRadius, leftParent.Y + 2 * Draw.BallRadius),
                        new(leftParent.X + Draw.BallRadius, leftParent.Y + 2 * Draw.BallRadius + Draw.BallRadius*Draw.VerticalGap * 0.3),
                        1, ColorPalette.SOFTBLUE_STROKE, false);
                    Line newLine2 = new(rightParent.MainCanvas, 0,
                        new(rightParent.X + Draw.BallRadius, rightParent.Y + 2 * Draw.BallRadius),
                        new(rightParent.X + Draw.BallRadius, rightParent.Y + 2 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap * 0.3),
                        1, ColorPalette.SOFTBLUE_STROKE, false);
                    Line newLine3 = new(rightParent.MainCanvas, 0,
                        new(leftParent.X + Draw.BallRadius, leftParent.Y + 2 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap * 0.3),
                        new(rightParent.X + Draw.BallRadius, rightParent.Y + 2 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap * 0.3),
                        1, ColorPalette.SOFTBLUE_STROKE, false);

                    newLine1.AddToCanvas();
                    newLine2.AddToCanvas();
                    newLine3.AddToCanvas();

                    GraphicElements.Add(newLine1);
                    GraphicElements.Add(newLine2);
                    GraphicElements.Add(newLine3);

                    Animate.OpacityChange(newLine1, 1, 0.5, 0);
                    Animate.OpacityChange(newLine2, 1, 0.5, 0);
                    Animate.OpacityChange(newLine3, 1, 0.5, 0);

                    ComparisonLines.Add(newLine1);
                    ComparisonLines.Add(newLine2);
                    ComparisonLines.Add(newLine3);

                    GreaterThanSymbol = new(leftParent.MainCanvas, 0, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, 1);
                    GraphicElements.Add(GreaterThanSymbol);

                    if (int.Parse(leftParent.GetText()) < int.Parse(rightParent.GetText())) // hehehe uz to nezvladam
                    {
                        GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 0, Colors.WhiteSmoke, "<", 0);
                        
                        StepState = 4;
                    } else
                    {
                        GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 0, Colors.WhiteSmoke, ">", 0);
                        StepState = 5;
                    }

                    GreaterThanSymbol.SetPosition((leftParent.X + rightParent.X)/2, leftParent.Y + 1.5 * Draw.BallRadius + Draw.BallRadius * Draw.VerticalGap * 0.3);
                    GreaterThanSymbol.AddToCanvas();
                    Animate.OpacityChange(GreaterThanSymbol.BallText, 1, 0.5, 0);
                    Animate.AnimationRun();
                    break;
                case 4:
                    GreaterThanSymbol?.Delete();
                    ComparisonLines.ForEach(e => e.Delete());
                    Animate.AnimationClear();

                    Ball newBall1 = Draw.CloneBall(LeftParentSubList[LeftIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newBall1.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    GraphicElements.Add(newBall1);
                    Canvas.SetZIndex(newBall1.MainUIElement, -1);
                    newBall1.AddToCanvas();
                    yPos = Draw.BallRadius * ((CurrentLayer + 2) * Draw.VerticalGap + 2 * (CurrentLayer+1));
                    Canvas.SetZIndex(LeftParentSubList[LeftIndex].MainUIElement, 1);
                    Canvas.SetZIndex(LeftParentSubList[LeftIndex].BallText.MainUIElement, 1);
                    Animate.ScheduleZIndex(LeftParentSubList[LeftIndex], 0);
                    Animate.ScheduleZIndex(LeftParentSubList[LeftIndex].BallText, 0);
                    Animate.MoveBallWithText(LeftParentSubList[LeftIndex], GetItemAtOverallIndex(BallLayers[CurrentLayer - CurrentJoinLayer - 1], CurrentIndex).X, yPos, 1, 0);
                    if (CurrentLayer == NumberOfLayers - 2)
                    {
                        Animate.BallStrokeColorChange(LeftParentSubList[LeftIndex], ColorPalette.GREEN_STROKE, 0.2, 0.8);
                        Animate.BallFillColorChange(LeftParentSubList[LeftIndex], ColorPalette.GREEN_FILL, 0.2, 0.8);
                    }
                    else
                        Animate.BallStrokeColorChange(LeftParentSubList[LeftIndex], ColorPalette.DEFAULT_STROKE, 0.2, 0.8);

                 
                    BallLayers[CurrentLayer + 1].Last().Add(LeftParentSubList[LeftIndex]);
                    LeftIndex++;
                    CurrentIndex++;
                    
                    
                    if (RightIndex >= RightParentSubList.Count && LeftIndex >= LeftParentSubList.Count)
                    {
                        Animate.OpacityChange(LeftBox, 0, 0.2, 1);
                        Animate.OpacityChange(RightBox, 0, 0.2, 1);
                        Animate.ScheduleForDeletion(LeftBox);
                        Animate.ScheduleForDeletion(RightBox);

                        double xCoord = GetItemAtOverallIndex(BallLayers[CurrentLayer - CurrentJoinLayer - 1], CurrentIndex - 1).X - Draw.BallRadius * (3 * (BallLayers[CurrentLayer + 1].Last().Count() / 2) - 1);
                        if (BallLayers[CurrentLayer + 1].Last().Count() % 2 == 0)
                        {
                            xCoord += 1.5 * Draw.BallRadius;
                        }
                        double xCoord1 = newBall1.X - Draw.BallRadius * (3 * (LeftParentSubList.Count / 2) - 1);
                        if (LeftParentSubList.Count % 2 == 0)
                        {
                            xCoord1 += 1.5 * Draw.BallRadius;
                        }
                        Line newL1 = new(LeftParentSubList[LeftIndex - 1].MainCanvas, 0,
                            new(xCoord1, newBall1.Y + 2.2 * Draw.BallRadius),
                            new(xCoord, yPos - 0.2 * Draw.BallRadius),
                            1, ColorPalette.GREY_STROKE, false);
                        newL1.AddToCanvas();
                        GraphicElements.Add(newL1);
                        double x2coord = newBall1.X + 4 * Draw.BallRadius + Draw.BallRadius * (3 * (LeftParentSubList.Count) + 1) + Draw.BallRadius * (3 * (RightParentSubList.Count/2) - 1);
                        if (RightParentSubList.Count % 2 == 0)
                        {
                            x2coord -= 1.5 * Draw.BallRadius; 
                        }
                        Line newL2 = new(RightParentSubList[RightIndex - 1].MainCanvas, 0,
                        new(x2coord, newBall1.Y + 2.2 * Draw.BallRadius),
                        new(xCoord, yPos - 0.2 * Draw.BallRadius),
                        1, ColorPalette.GREY_STROKE, false);
                        newL2.AddToCanvas();
                        
                        GraphicElements.Add(newL2);

                        Canvas.SetZIndex(newL2.MainUIElement, -1);
                        Canvas.SetZIndex(newL1.MainUIElement, -1);
                        Animate.OpacityChange(newL1, 1, 0.5, 1);
                        Animate.OpacityChange(newL2, 1, 0.5, 1);

                        if (CurrentIndex == N)
                        {
                            CurrentIndex = 0;
                            LeftIndex = 0; 
                            RightIndex = 0;
                            CurrentLayer += 1;
                            CurrentJoinLayer += 2;
                            
                            if (CurrentLayer >= NumberOfLayers - 1)
                            {
                                IsSortedBool = true;
                                // O TOMHLE SE NEMLUVI
                                Numbers.Sort();
                                Balls.Sort((ball1, ball2) => Comparer<int>.Default.Compare(int.Parse(ball1.GetText()),int.Parse(ball2.GetText())));

                            }

                        } 
                        StepState = 1;

                    } else if (LeftIndex >= LeftParentSubList.Count)
                    {
                        StepState = 5;
                    } else if (RightIndex >= RightParentSubList.Count)
                    {
                        StepState = 4;
                    } else
                    {
                        StepState = 2;
                    }
                    Animate.AnimationRun();
                    break;
                case 5:
                    GreaterThanSymbol?.Delete();
                    ComparisonLines.ForEach(e => e.Delete());

                    Animate.AnimationClear();

                    Ball newBall2 = Draw.CloneBall(RightParentSubList[RightIndex], ColorPalette.GREY_FILL, ColorPalette.GREY_STROKE);
                    newBall2.BallText.ChangeColor(ColorPalette.GREY_STROKE);
                    GraphicElements.Add(newBall2);
                    Canvas.SetZIndex(newBall2.MainUIElement, -1);
                    newBall2.AddToCanvas();
                    yPos = Draw.BallRadius * ((CurrentLayer + 2) * Draw.VerticalGap + 2 * (CurrentLayer + 1));
                    Canvas.SetZIndex(RightParentSubList[RightIndex].MainUIElement, 2);
                    Canvas.SetZIndex(RightParentSubList[RightIndex].BallText.MainUIElement, 2);
                    Animate.ScheduleZIndex(RightParentSubList[RightIndex], 0);
                    Animate.ScheduleZIndex(RightParentSubList[RightIndex].BallText, 0);
                    Animate.MoveBallWithText(RightParentSubList[RightIndex], GetItemAtOverallIndex(BallLayers[CurrentLayer - CurrentJoinLayer - 1], CurrentIndex).X, yPos, 1, 0);
                    if (CurrentLayer == NumberOfLayers - 2)
                    {
                        Animate.BallStrokeColorChange(RightParentSubList[RightIndex], ColorPalette.GREEN_STROKE, 0.2, 0.8);
                        Animate.BallFillColorChange(RightParentSubList[RightIndex], ColorPalette.GREEN_FILL, 0.2, 0.8);
                    } else 
                        Animate.BallStrokeColorChange(RightParentSubList[RightIndex], ColorPalette.DEFAULT_STROKE, 0.2, 0.8);

                    
                    BallLayers[CurrentLayer + 1].Last().Add(RightParentSubList[RightIndex]);
                    RightIndex++;
                    CurrentIndex++;
                    
                    
                    if (RightIndex >= RightParentSubList.Count && LeftIndex >= LeftParentSubList.Count)
                    {
                        Animate.OpacityChange(LeftBox, 0, 0.2, 1);
                        Animate.OpacityChange(RightBox, 0, 0.2, 1);
                        Animate.ScheduleForDeletion(LeftBox);
                        Animate.ScheduleForDeletion(RightBox);

                        double xCoord = GetItemAtOverallIndex(BallLayers[CurrentLayer - CurrentJoinLayer - 1], CurrentIndex - 1).X - Draw.BallRadius * (3 * (BallLayers[CurrentLayer + 1].Last().Count() / 2) - 1);
                        if (BallLayers[CurrentLayer + 1].Last().Count() % 2 == 0)
                        {
                            xCoord += 1.5 * Draw.BallRadius;
                        }


                        double xCoord1 = newBall2.X - Draw.BallRadius * (3 * (RightParentSubList.Count / 2) - 1);
                        if (RightParentSubList.Count % 2 == 0)
                        {
                            xCoord1 += 1.5 * Draw.BallRadius;
                        }
                        Line newL3 = new(newBall2.MainCanvas, 0,
                            new(xCoord1, newBall2.Y + 2.2 * Draw.BallRadius),
                            new(xCoord, yPos - 0.2 * Draw.BallRadius),
                            1, ColorPalette.GREY_STROKE, false);
                        newL3.AddToCanvas();
                        GraphicElements.Add(newL3);
                        double xCoord2 = newBall2.X - 3 * Draw.BallRadius * (RightParentSubList.Count - 1) - Draw.BallRadius * (3 * LeftParentSubList.Count + 1) - Draw.BallRadius * (3 * (LeftParentSubList.Count / 2) + 1);
                        if (LeftParentSubList.Count % 2 == 0)
                        {
                            xCoord2 += 1.5 * Draw.BallRadius;
                        }
                        Line newL4 = new(newBall2.MainCanvas, 0, 
                            new(xCoord2, newBall2.Y + 2.2 * Draw.BallRadius),
                            new(xCoord, yPos - 0.2 * Draw.BallRadius),
                            1, ColorPalette.GREY_STROKE, false);
                        newL4.AddToCanvas();
                        GraphicElements.Add(newL4);


                        Canvas.SetZIndex(newL3.MainUIElement, -1);
                        Canvas.SetZIndex(newL4.MainUIElement, -1);
                        Animate.OpacityChange(newL3, 1, 0.5, 1);
                        Animate.OpacityChange(newL4, 1, 0.5, 1);
                        
                        
                        if (CurrentIndex == N)
                        {
                            CurrentIndex = 0;
                            LeftIndex = 0;
                            RightIndex = 0;
                            CurrentLayer += 1;
                            CurrentJoinLayer += 2;
                            
                            if (CurrentLayer >= NumberOfLayers - 1)
                            {
                                IsSortedBool = true;
                                // Ten jehoz jmeno nesmi byt vyrceno
                                Numbers.Sort();
                                Balls.Sort((ball1, ball2) => Comparer<int>.Default.Compare(int.Parse(ball1.GetText()), int.Parse(ball2.GetText()))); 
                            }
                        }
                        StepState = 1;
                        
                        
                    } else if (LeftIndex >= LeftParentSubList.Count)
                    {
                        StepState = 5;
                    } else if (RightIndex >= RightParentSubList.Count)
                    {
                        StepState = 4;
                    } else
                    {
                        StepState = 2;
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
        }

        private static T GetItemAtOverallIndex<T>(List<List<T>> listOfLists,  int index)
        {
            int currentIndex = 0;
            foreach (List<T> sublist in listOfLists)
            {
                if (currentIndex + sublist.Count > index)
                {
                    int localIndex = index - currentIndex;
                    return sublist[localIndex];  
                }
                currentIndex += sublist.Count;
            }

            return default(T);
        }
        public static List<T> GetSublistContainingIndex<T>(List<List<T>> listOfLists, int realIndex)
        {
            int cumulativeIndex = 0;

            foreach (var sublist in listOfLists)
            {
                if (realIndex < cumulativeIndex + sublist.Count)
                {
                    return sublist; 
                }
                cumulativeIndex += sublist.Count;
            }

            return default(List<T>);
        }

    }
}
