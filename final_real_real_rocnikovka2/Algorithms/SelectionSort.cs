using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_real_real_rocnikovka2.Algorithms
{
    public class SelectionSort : SortingAlgorithm
    {
        private static readonly SelectionSort instance = new();
        public static new SelectionSort Instance => instance;

        // STEP VARIABLES
        private int CurrentIndex;
        private int MinIndex;
        private int ComparisonIndex;
        private int LastMinIndex;

        private SelectionSort()
        {
            Name = AlgorithmName.SELECTION_SORT_NAME;
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
            MinIndex = 0;
            ComparisonIndex = 0;
        }

        public override async Task Sort()
        {
            if (IsSorted()) return;
            int n = Numbers.Count;
            for (int i = 0; i < n - 1; i++)
            {
                Boxes[i].ChangeColor(ColorPalette.PIVOT_BAR_FILL);
                int minNumberIndex = i;

                for (int j = i + 1; j < n; j++)
                {
                    if (Globals.Stop) return;
                    if (IsSorted()) // Ano, toto pridava casovou komplexitu, ale toto vizualni znazorneni to nejak neefektuje
                    {
                        Draw.DrawDone(Boxes, ColorPalette.SELECTED_BAR_FILL);
                        return;
                    };

                    Boxes[j].ChangeColor(ColorPalette.SELECTED_BAR_FILL);
                    
                    await Animate.Wait(Globals.AnimationMs, j);
                    if (Globals.Stop) return;
                    ComparisonCount++;
                    if (Numbers[j] < Numbers[minNumberIndex])
                    {
                        if (minNumberIndex != i)
                            Boxes[minNumberIndex].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);

                        minNumberIndex = j;
                        Boxes[j].ChangeColor(ColorPalette.MINIMUM_BAR_FILL);
                    } else
                    {
                        Boxes[j].ChangeColor(ColorPalette.DEFAULT_BAR_FILL);
                    }
                }

                Boxes[i].ChangeColor(ColorPalette.DEFAULT_BAR_FILL); 
                Boxes[minNumberIndex].ChangeColor(ColorPalette.SELECTED_BAR_FILL);

                if (minNumberIndex != i)
                {
                    SwapCount++;
                    SwapInList(Numbers, i, minNumberIndex);
                    SwapInList(Boxes, i, minNumberIndex);

                    Draw.SwapXPos(Boxes[i], Boxes[minNumberIndex]);
                }
            }
            Boxes[n-1].ChangeColor(ColorPalette.SELECTED_BAR_FILL);


        }

        public override void Step()
        {
            if (IsSortedBool) return;

            switch (StepState)
            {
                case 0:
                    Animate.AnimationClear();
                    Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.SELECTED_STROKE, 0.2, 0);
                    Animate.AnimationRun();
                    MinIndex = CurrentIndex;
                    ComparisonIndex = CurrentIndex + 1;
                    StepState = 1;
                    break;
                case 1:
                    
                    Animate.AnimationClear();
                    if (LastMinIndex >= 0)
                        Animate.BallStrokeColorChange(Balls[LastMinIndex], ColorPalette.DEFAULT_STROKE, 0.2, 0);
                    if (ComparisonIndex < N)
                        Animate.BallStrokeColorChange(Balls[ComparisonIndex], ColorPalette.SELECTED_STROKE, 0.2, 0);
                    Animate.BallStrokeColorChange(Balls[ComparisonIndex - 1], ColorPalette.DEFAULT_STROKE, 0.2, 0);
                    Animate.BallStrokeColorChange(Balls[MinIndex], ColorPalette.SOFTBLUE_STROKE, 0.2, 0);
                    if (ComparisonIndex < N)
                    {
                        if (Numbers[ComparisonIndex] < Numbers[MinIndex])
                        {
                            LastMinIndex = MinIndex;
                            MinIndex = ComparisonIndex;
                        } 
                        ComparisonIndex++;
                    } 
                    else
                    {
                        StepState = 2;
                    }
                    Animate.AnimationRun();
                    break;
                case 2:
                    Animate.AnimationClear();
                    Animate.BallFillColorChange(Balls[MinIndex], ColorPalette.GREEN_FILL, 1, 0.5);
                    Animate.BallStrokeColorChange(Balls[MinIndex], ColorPalette.GREEN_STROKE, 1, 0.5);
                    if (MinIndex != CurrentIndex)
                    {
                        Animate.BallSwap(Balls[CurrentIndex], Balls[MinIndex], 1, 0, 1.5);
                        SwapInList(Numbers, MinIndex, CurrentIndex);
                        SwapInList(Balls, MinIndex, CurrentIndex);
                        Draw.SwapXPos(Balls[CurrentIndex], Balls[MinIndex]);
                    }
                    CurrentIndex++;
                    if (CurrentIndex >= N - 1)
                    {
                        IsSortedBool = true;
                        Animate.BallFillColorChange(Balls[CurrentIndex], ColorPalette.GREEN_FILL, 1, 1);
                        Animate.BallStrokeColorChange(Balls[CurrentIndex], ColorPalette.GREEN_STROKE, 1, 1);
                    }
                    StepState = 0;
                    LastMinIndex = -1;
                    Animate.AnimationRun();
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
