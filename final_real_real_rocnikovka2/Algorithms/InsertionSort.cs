using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class InsertionSort : SortingAlgorithm
    {
        private static readonly InsertionSort instance = new();
        public static new SortingAlgorithm Instance => instance;

        // STEP VARIABLES
        private int CurrentIndex;
        private Ball GreaterThanSymbol;
        private int MaxIndex;

        private InsertionSort()
        {
            Name = AlgorithmName.INSERTION_SORT_NAME;
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
            MaxIndex = 0;
        }

        public override async Task Sort()
        {
            if (IsSorted()) return;

            int n = Numbers.Count;
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (Globals.Stop) return;
                    if (IsSorted()) // Ano, toto pridava casovou komplexitu, ale toto vizualni znazorneni to nejak neefektuje
                    {
                        Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);
                        return;
                    };
                    Boxes[j].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    Boxes[j - 1].ChangeColor(ColorPalette.SELECTED_BAR_FILL);

                    ComparisonCount++;
                    if (Numbers[j] < Numbers[j - 1]) // Takhle komplikovane to delam jen kvuli vykresu, jinak by to bylo lehci
                    {
                        SwapInList(Numbers, j, j - 1);
                        SwapInList(Boxes, j, j - 1);

                        Draw.SwapXPos(Boxes[j], Boxes[j - 1]);
                        SwapCount++;
                    } else
                    {
                        await Animate.Wait(Globals.AnimationMs, j);
                        if (Globals.Stop) return;
                        Boxes[j].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                        Boxes[j - 1].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                        break; 
                    }
                    await Animate.Wait(Globals.AnimationMs, j);
                    if (Globals.Stop) return;
                    Boxes[j].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    Boxes[j - 1].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                }
            }
            if (IsSorted())
                Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);

        }

        public override void Step()
        {
            if (IsSortedBool) return;

            switch (StepState)
            {
                case 0:
                    GreaterThanSymbol?.Delete();
                    Animate.AnimationClear();
                    if (CurrentIndex >= N)
                    {
                        IsSortedBool = true;
                        return;
                    }
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    MaxIndex = CurrentIndex;
                    StepState = 1;
                    if (CurrentIndex == 0)
                    {
                        
                        CurrentIndex++;
                        StepState = 0;
                    } else
                    {
                        Animate.BallFillColorChange(Balls[MaxIndex-1], ColorPalette.GREEN_FILL, 0.5, 0);
                        Animate.BallStrokeColorChange(Balls[MaxIndex - 1], ColorPalette.GREEN_STROKE, 0.5, 0);
                    }
                    Animate.AnimationRun();
                    break;
                case 1:
                    GreaterThanSymbol?.Delete();
                    Animate.AnimationClear();
                    Animate.BallStrokeColorChange(Balls[CurrentIndex - 1], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    Animate.AnimationRun();

                    StepState = 2;
                    break;
                case 2:
                    GreaterThanSymbol?.Delete();
                    GraphicElements.Remove(GreaterThanSymbol);
                    GreaterThanSymbol = new(Balls[0].MainCanvas, 0, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, 1);
                    GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 1, Colors.WhiteSmoke, ">", 0);
                    GreaterThanSymbol.SetPosition((Balls[CurrentIndex - 1].X + Balls[CurrentIndex].X) / 2, Balls[CurrentIndex].Y - 0.05 * Draw.BallRadius);
                    GreaterThanSymbol.AddToCanvas();
                    GraphicElements.Add(GreaterThanSymbol);

                    Animate.AnimationClear();
                    if (Numbers[CurrentIndex] < Numbers[CurrentIndex - 1])
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_GREEN, 0, 0);
                        StepState = 3;
                    }
                    else
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_RED, 0, 0);
                        Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.GREEN_STROKE, 0.5, 0);
                        Animate.BallStrokeColorChange(Balls[CurrentIndex - 1], ColorPalette.GREEN_STROKE, 0.5, 0);
                        Animate.BallFillColorChange(Balls[CurrentIndex], ColorPalette.GREEN_FILL, 0.5, 0);
                        Animate.OpacityChange(GreaterThanSymbol.BallText, 0, 1, 1);
                        MaxIndex++;
                        CurrentIndex = MaxIndex;
                        StepState = 0;
                    }
                    Animate.AnimationRun();
                    break;
                case 3:
                    Animate.AnimationClear();
                    Animate.BallSwap(Balls[CurrentIndex-1], Balls[CurrentIndex], 1, 0, 1.5);
                    Animate.OpacityChange(GreaterThanSymbol.BallText, 0, 1, 0);
                    Animate.BallStrokeColorChange(Balls[CurrentIndex - 1], ColorPalette.GREEN_STROKE, 1, 0);
                    if (CurrentIndex == 1)
                    {
                        Animate.BallFillColorChange(Balls[CurrentIndex], ColorPalette.GREEN_FILL, 0.5, 0.5);
                        Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.GREEN_STROKE, 0.5, 0.5);
                    }
                    Animate.AnimationRun();
                    SwapInList(Numbers, CurrentIndex, CurrentIndex - 1);
                    SwapInList(Balls, CurrentIndex, CurrentIndex - 1);
                    Draw.SwapXPos(Balls[CurrentIndex - 1], Balls[CurrentIndex]); // hodne invalidni x2

                    CurrentIndex--;
                    if (CurrentIndex != 0)
                    {
                        StepState = 1;
                    } else
                    {
                        StepState = 0;
                        MaxIndex++;
                        CurrentIndex = MaxIndex;
                    }

                    break;
            }
        }

        public override void OnSelect(List<int> numbers, List<Ball> balls)
        {
            double xPos = Draw.BallRadius;
            double yPos = balls[0].MainCanvas.ActualHeight / 2 - Draw.BallRadius;
            foreach (Ball ball in Balls)
            {
                ball.SetPosition(xPos, yPos);
                xPos += 3 * Draw.BallRadius;
            }
        }
    }
}
