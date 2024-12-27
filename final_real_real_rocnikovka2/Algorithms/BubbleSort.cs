using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class BubbleSort : SortingAlgorithm
    {
        private static readonly BubbleSort instance = new();
        public static new SortingAlgorithm Instance => instance;


        // STEP VARIABLES
        private int CurrentIndex;
        private Ball GreaterThanSymbol;


        private BubbleSort()
        {
            Name = AlgorithmName.BUBBLE_SORT_NAME;
        }

        public override void Reset(List<int> numbers, List<Box> boxes)
        {
            Numbers = numbers;
            Boxes = boxes;
            N = Numbers.Count;
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
        }

        public override async Task Sort()
        {
            if (IsSorted())
            {
                return;
            }
            for (int i = 0; i < N - 1; i++)
            {
                for (int j = 0; j < N - i - 1; j++)
                {
                    if (Globals.Stop) return;

                    // Ano, je to dvakrat min efektivni, ale tuto vizualizaci to nejak nezpomali.
                    // Mohl jsem vyuzit bool ktery by kontroloval jestli v dane iteraci probehl 
                    // swap, ale pak by algoritmus udelal jeden prazdny pruchod, coz by nebylo hezke.
                    if (IsSorted()) 
                    {
                        Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);
                        return;
                    }

                    Boxes[j].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    Boxes[j+1].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    ComparisonCount++;
                    
                    if (Numbers[j] > Numbers[j + 1])
                    {
                        SwapInList(Numbers, j, j + 1);
                        SwapInList(Boxes, j, j + 1);

                        Draw.SwapXPos(Boxes[j], Boxes[j + 1]);
                        SwapCount++;
                    }

                    await Animate.Wait(Globals.AnimationMs, j);
                    if (Globals.Stop) return;
                    Boxes[j].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    Boxes[j + 1].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                }
            }
        }

        public override void Step()
        {
            if (IsSortedBool) return;

            switch (StepState)
            {
                case 0:
                    GreaterThanSymbol?.Delete();
                    if (CurrentIndex > 0)
                    {
                        Balls[CurrentIndex - 1].SetStrokeColor(ColorPalette.DEFAULT_STROKE);
                    } else if (CurrentIndex == 0)
                    {
                        Balls[N-1].SetStrokeColor(ColorPalette.DEFAULT_STROKE);
                        Balls[N - 2]?.SetStrokeColor(ColorPalette.DEFAULT_STROKE);
                    }
                    Animate.AnimationClear();
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    Animate.BallStrokeColorChange(Balls[CurrentIndex + 1], ColorPalette.SELECTED_STROKE, 0.5, 0);
                    Animate.AnimationRun();

                    StepState = 1;
                    break;
                case 1:
                    GraphicElements.Remove(GreaterThanSymbol);
                    GreaterThanSymbol = new(Balls[0].MainCanvas, 0, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, 1);
                    GreaterThanSymbol.BallText = new(GreaterThanSymbol.MainCanvas, 1, Colors.WhiteSmoke, ">", 0);
                    GreaterThanSymbol.SetPosition((Balls[CurrentIndex].X + Balls[CurrentIndex + 1].X) / 2, Balls[CurrentIndex].Y - 0.05 * Draw.BallRadius);
                    GreaterThanSymbol.AddToCanvas();
                    GraphicElements.Add(GreaterThanSymbol);
                    Animate.AnimationClear();
                    if (Numbers[CurrentIndex] > Numbers[CurrentIndex + 1])
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_GREEN, 0, 0);
                        StepState = 2;
                    }
                    else
                    {
                        Animate.TextColorChange(GreaterThanSymbol.BallText, ColorPalette.PURE_RED, 0, 0);
                        CurrentIndex++;
                        StepState = 0;

                        if (CurrentIndex >= N - 1)
                        {
                            CurrentIndex = 0;
                            if (IsSorted())
                            {
                                StepState = 3;
                            }
                        }
                    }
                    Animate.AnimationRun();
                    
                    break;
                case 2:
                    Animate.AnimationClear();
                    Animate.BallSwap(Balls[CurrentIndex], Balls[CurrentIndex + 1], 1, 0, 1.5);
                    Animate.BallStrokeColorChange(Balls[CurrentIndex + 1], ColorPalette.DEFAULT_STROKE, 0.5, 0.5);
                    Animate.OpacityChange(GreaterThanSymbol.BallText, 0, 1, 0);
                    Animate.AnimationRun();
                    SwapInList(Balls, CurrentIndex, CurrentIndex + 1);
                    SwapInList(Numbers, CurrentIndex, CurrentIndex + 1);
                    Draw.SwapXPos(Balls[CurrentIndex], Balls[CurrentIndex + 1]); // Hodne invalidni reseni (ballswap problem)

                    CurrentIndex++;
                    if (CurrentIndex >= N - 1)
                    {
                        CurrentIndex = 0;
                    }

                    StepState = 0;
                    break;
                case 3:
                    IsSortedBool = true;
                    GreaterThanSymbol?.Delete();
                    Draw.ChangeColorForAll(Balls, ColorPalette.GREEN_FILL, ColorPalette.GREEN_STROKE);
                    break;
            }
        }

        public override void OnSelect(List<int> numbers, List<Ball> balls)
        {
            double xPos = Draw.BallRadius;
            double yPos = balls[0].MainCanvas.ActualHeight / 2 - Draw.BallRadius;
            foreach (Ball ball in balls)
            {
                ball.SetPosition(xPos, yPos);
                xPos += 3 * Draw.BallRadius;
            }
        }
    }
}
