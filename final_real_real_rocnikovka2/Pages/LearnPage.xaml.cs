using final_real_real_rocnikovka2.Algorithms;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Pages
{
    /// <summary>
    /// Interaction logic for ExplanatorySortingPage.xaml
    /// </summary>
    // Udelat vsechny algoritmy :(
    // udelat lepsi toggle znazorneni autosteppingu
    public partial class ExplanatorySortingPage : Page
    {
        private double previousCanvasWidth;
        private double previousCanvasHeight;
        private double previousBallRadius;
        private bool IsAutoStepping;
        private bool FirstLoad;

        private readonly List<SortingAlgorithm> SortingAlgorithms;
        private SortingAlgorithm? SelectedAlgorithm { get; set; }
        private List<int> Numbers { get; set; }
        private List<Ball> Balls { get; set; }
        private List<GraphicElement> GraphicElements { get; set; }

        public ExplanatorySortingPage(List<SortingAlgorithm> sortingAlgorithms)
        {
            InitializeComponent();
            this.SortingAlgorithms = sortingAlgorithms;
            PopulateComboBox();

            GraphicElements = [];
            Numbers = [4, 2, 5, 1, 3, 2];
            Balls = [];
            FirstLoad = true;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (FirstLoad)
            {
                InitializeBallObjects(Numbers);
                FirstLoad = false;
            }

            previousCanvasWidth = MainCanvas.ActualWidth;
            previousCanvasHeight = MainCanvas.ActualHeight;
            previousBallRadius = Draw.BallRadius;
            Globals.AnimationMs = (int)SpeedSlider.Value;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            IsAutoStepping = false;
            ResetBtn_Click(null, null);
        }

        private void InitializeBallObjects(List<int> numbers)
        {
            Draw.UpdateBallRadius(null, numbers.Count, MainCanvas);
            double xPos = Draw.BallRadius;
            double yPos = MainCanvas.ActualHeight / 2 - Draw.BallRadius;
            foreach (int number in numbers)
            {
                Ball newBall = new(MainCanvas, 1, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, 1);
                newBall.BallText = new(MainCanvas, 1, ColorPalette.DEFAULT_TEXT_COLOR, number.ToString(), 0);

                newBall.SetPosition(xPos, yPos);
                newBall.AddToCanvas();
                Balls.Add(newBall);

                xPos += 3 * Draw.BallRadius;
            }
        }

        private void PopulateComboBox()
        {
            AlgorithmComboBox.Items.Clear();
            foreach (var algorithm in SortingAlgorithms)
            {
                AlgorithmComboBox.Items.Add(algorithm);
            }
        }

        private async void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            AutoStepButton.Content = "Auto Step: OFF";
            AutoStepButton.Foreground = new SolidColorBrush(Colors.Red);
            IsAutoStepping = false;
            GraphicElements.ForEach(gE => gE.Delete());
            GraphicElements.Clear();
            Animate.AnimationSkip();
            List<Ball> temp = [];
            foreach (Ball b in Balls) // linej fix na jeden malinky error =)
            {
                Ball newBall = Draw.CloneBall(b);
                newBall.AddToCanvas();
                temp.Add(newBall);  
                b.Delete();
            }
            Balls = temp;
            SelectedAlgorithm?.Reset(Numbers, Balls, GraphicElements);
            if (SelectedAlgorithm == (SortingAlgorithm)AlgorithmComboBox.SelectedItem)
                SelectedAlgorithm?.OnSelect(Numbers, Balls);
            Draw.ChangeColorForAll(Balls, ColorPalette.DEFAULT_FILL, ColorPalette.DEFAULT_STROKE, false);
            
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is not Slider slider) return;

            Globals.AnimationMs = (int)slider.Value;
        }

        private async void OnAlgorithmComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetBtn_Click(null, null);
            await Task.Delay(1);
            SelectedAlgorithm = (SortingAlgorithm)AlgorithmComboBox.SelectedItem;
            Draw.UpdateBallRadius(SelectedAlgorithm, Numbers.Count, MainCanvas);
            SelectedAlgorithm?.Reset(Numbers, Balls, GraphicElements);
            SelectedAlgorithm?.OnSelect(Numbers, Balls);
        }

        private async void StepBtn_Click(object sender, RoutedEventArgs e)
        {
            IsAutoStepping = false;
            AutoStepButton.Content = "Auto Step: OFF";
            AutoStepButton.Foreground = new SolidColorBrush(Colors.Red);
            await Animate.AnimationSkip();
            SelectedAlgorithm?.Step();
        }
        
        private async void AutoStepBtn_Click(Object sender, RoutedEventArgs e)
        {
            if (SelectedAlgorithm == null) return;
            if (!IsAutoStepping)
            {
                IsAutoStepping = true;
                AutoStepButton.Content = "Auto Step: ON";
                AutoStepButton.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            } else
            {
                IsAutoStepping = false;
                AutoStepButton.Content = "Auto Step: OFF";
                AutoStepButton.Foreground = new SolidColorBrush(Colors.Red);
            }
            while (IsAutoStepping)
            {
                SelectedAlgorithm?.Step();
                await Task.Delay(TimeSpan.FromTicks(Math.Max(Animate.GetStoryboardDuration().Ticks, TimeSpan.FromMilliseconds(Globals.AnimationMs).Ticks)));

                if (SelectedAlgorithm.IsSortedBool) break;
            }
            IsAutoStepping = false;
        }

        private void ScrambleBtn_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int n = Numbers.Count;

            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = Numbers[i];
                Numbers[i] = Numbers[j];
                Numbers[j] = temp;
                Ball tempBall = Balls[i];
                Balls[i] = Balls[j];
                Balls[j] = tempBall;

                Draw.SwapXPos(Balls[i], Balls[j]);
            }
            ResetBtn_Click(null, null);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void SeriesTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GenerateNumberSeries();
            }
        }

        private void GenerateNumberSeries()
        {
            SeriesTextBox.Text.Replace(" ", "");
            if (int.TryParse(SeriesTextBox.Text, out int n))
            {
                if (n < 4)
                {
                    MessageBox.Show("N must be at least 4");
                }
                else if (n >= 51)
                {
                    MessageBox.Show("N must be smaller than 50");
                }
                else
                {
                    List<int> numbers = Enumerable.Range(1, n).ToList();

                    Random rand = new Random();
                    numbers = numbers.OrderBy(x => rand.Next()).ToList();

                    IsAutoStepping = false;
                    AutoStepButton.Content = "Auto Step: OFF";
                    AutoStepButton.Foreground = new SolidColorBrush(Colors.Red);

                    Numbers = new List<int>(numbers);
                    Balls.ForEach(e => e.Delete());
                    Balls.Clear();
                    InitializeBallObjects(Numbers);
                    OnAlgorithmComboBoxSelectionChanged(null, null);
                  

                }
            }
            SeriesTextBox.Clear();
        }

        private void ExactInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ParseExactInput();
            }
        }

        private void ParseExactInput()
        {
            try
            {
                List<int> numbers = Array.ConvertAll(ExactInputTextBox.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();
                if (numbers.Count < 4)
                {
                    MessageBox.Show("Array must have at least 4 elements");
                }
                else if (numbers.Count > 50)
                {
                    MessageBox.Show("Array can't have more than 50 elements");
                }
                else
                {
                    foreach (int number in numbers)
                    {
                        if (number > 9999)
                        {
                            MessageBox.Show("All numbers must be less than 10000");
                            ExactInputTextBox.Clear();
                            return;
                        }
                    }
                    IsAutoStepping = false;
                    AutoStepButton.Content = "Auto Step: OFF";
                    AutoStepButton.Foreground = new SolidColorBrush(Colors.Red);

                    Numbers = new List<int>(numbers);
                    Balls.ForEach(e => e.Delete());
                    Balls.Clear();
                    InitializeBallObjects(Numbers);
                    OnAlgorithmComboBoxSelectionChanged(null, null);
                }
            }
            catch (OverflowException)
            {
                MessageBox.Show("All numbers must be less than 10000");
            }
            catch (Exception)
            {
                MessageBox.Show("SOMETHING WENT WRONG");
            }
            ExactInputTextBox.Clear();
        }
    }
}
