using final_real_real_rocnikovka2.Algorithms;
using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Pages
{
    /// <summary>
    /// Interaction logic for ClassicSortingPage.xaml
    /// </summary>
    /// 

    //super speed text kdyz bude animation speed mensi nez 14
    //zavorka za super speed ktera oznami ze super speed je disabled kdyz multisichecked
    //obdelnik v levem hornim rohu s comparison, swap, lookup count
    public partial class ClassicSortingPage : Page
    {
        private double previousCanvasWidth;
        private double previousCanvasHeight;
        private bool FirstLoad;

        private readonly List<SortingAlgorithm> SortingAlgorithms;
        public SortingAlgorithm? SelectedAlgorithm { get; set; }
        private List<int> Numbers { get; set; }
        private List<Box> Boxes { get; set; }
        private Dictionary<string, List<int>> TestDataDictionary { get; set; }

        private SortingAlgorithm RunningAlgorithm;


        
        public ClassicSortingPage(List<SortingAlgorithm> sortingAlgorithms)
        {
            InitializeComponent();
            this.SortingAlgorithms = sortingAlgorithms;
            
            TestDataDictionary = new(){
                { "Default", TestData.DEFAULT },
                { "Stairs", TestData.STAIRS },
                { "Reverse sorted", TestData.REVERSE_SORTED},
                { "Almost sorted", TestData.ALMOST_SRORTED }
            };
            

            PopulateComboBox();
            DataContext = this;

            Numbers = [];
            Boxes = [];
           

            StatsTextBlock.Text = $"Stats (n = {Numbers.Count})";
            SwapCountTextBlock.Text = $"Swap Count: 0";
            ComparisonCountTextBlock.Text = $"Comparison count: 0";

            FirstLoad = true;
            SpeedText.Text = $"{(int)SpeedSlider.Value} ms";
        }

        private void OnDataComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopBtn_Click(null, null);
           
            Numbers = new List<int>(TestDataDictionary[DataComboBox.SelectedItem.ToString()]);

            Boxes.ForEach(e => e.Delete());
            Boxes.Clear();
            InitializeRectObjects(Numbers);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

         private void PopulateComboBox()
        {
            AlgorithmComboBox.Items.Clear();
            foreach (var algorithm in SortingAlgorithms)
            {
                AlgorithmComboBox.Items.Add(algorithm);
            }

            DataComboBox.Items.Clear();

            foreach (string dataName in TestDataDictionary.Keys)
            {
                DataComboBox.Items.Add($"{dataName}");
            }
            
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (FirstLoad)
            {
                DataComboBox.SelectedItem = "Default";
                FirstLoad = false;
            }
                
            previousCanvasWidth = MainCanvas.ActualWidth;
            previousCanvasHeight = MainCanvas.ActualHeight;
            Globals.AnimationMs = (int)SpeedSlider.Value;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Globals.Stop = true;
            Draw.ChangeColorForAll(Boxes, ColorPalette.DEFAULT_BAR_FILL);
        }

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Box box in Boxes)
            {
                box.Update(previousCanvasWidth, previousCanvasHeight);
            }
            previousCanvasWidth = MainCanvas.ActualWidth;
            previousCanvasHeight = MainCanvas.ActualHeight;

        }

        private void InitializeRectObjects(List<int> numbers)
        {
            double width = MainCanvas.ActualWidth/numbers.Count;
            double maxNumber = numbers.Max();
            double heightPerNum = (MainCanvas.ActualHeight) / maxNumber;
            double xPos = 0;
            foreach (int number in numbers)
            {
                Box newBox = new(MainCanvas, 1, ColorPalette.DEFAULT_BAR_FILL, width, number * heightPerNum);
                newBox.SetPosition(xPos, MainCanvas.ActualHeight - number * heightPerNum);
                newBox.AddToCanvas();
                Boxes.Add(newBox);

                xPos += width;
            }
            StatsTextBlock.Text = $"Stats (n = {Numbers.Count})";
        }

        private void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedAlgorithm = (SortingAlgorithm)AlgorithmComboBox.SelectedItem;
            if (!Globals.MultiIsChecked)
                OnDataComboBoxSelectionChanged(null, null);
        }

        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAlgorithm == null) return;
            Globals.Stop = false;
            if (!Globals.AlgorithmIsRunning || Globals.MultiIsChecked)
            {
                RunningAlgorithm = SelectedAlgorithm;
                Globals.AlgorithmIsRunning = true;

                SelectedAlgorithm.PropertyChanged += OnAlgorithmStatsChanged;

                SelectedAlgorithm.Reset(Numbers, Boxes);
                await SelectedAlgorithm.Sort();

                SelectedAlgorithm.PropertyChanged -= OnAlgorithmStatsChanged;

                Globals.AlgorithmIsRunning = false;
            } 
        }

        private void OnAlgorithmStatsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == RunningAlgorithm)
            {
                SwapCountTextBlock.Text = $"Swap Count: {RunningAlgorithm.SwapCount.ToString()}";
                ComparisonCountTextBlock.Text = $"Comparison Count: {RunningAlgorithm.ComparisonCount.ToString()}";
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.AnimationMs = (int)SpeedSlider.Value;

            if (SpeedText == null) return;

            if (Globals.AnimationMs >= 15)
            {
                SpeedText.Inlines.Clear();
                SpeedText.Text = $"{Globals.AnimationMs} ms";
                return;
            }

            if (Globals.MultiIsChecked)
            {
                SpeedText.Inlines.Clear();
                SpeedText.Inlines.Add(new Run("1 ms ("));


                Run redRun = new Run("multi is checked")
                {
                    Foreground = new SolidColorBrush(Colors.Red)
                };
                SpeedText.Inlines.Add(redRun);

                SpeedText.Inlines.Add(new Run(")"));

                return;
            }
            string text = "Super fast!";
            int maxValue = 15;

            SpeedText.Inlines.Clear();

            double greenness = 1 - (Globals.AnimationMs / (double)maxValue);

            for (int i = 0; i < text.Length; i++)
            {
                Color color = Color.FromRgb(
                    (byte)(255 * (1 - greenness)),
                    255,
                    (byte)(255 * (1 - greenness)) 
                );

                Run run = new Run(text[i].ToString())
                {
                    Foreground = new SolidColorBrush(color)
                };

                SpeedText.Inlines.Add(run);
            }

        }

        private async void StopBtn_Click(Object sender, RoutedEventArgs e)
        {
            if (SelectedAlgorithm == null) return;
            Globals.Stop = true;
            if (!SelectedAlgorithm.IsSorted())
                Draw.ChangeColorForAll(Boxes, ColorPalette.DEFAULT_BAR_FILL);
            await Task.Delay(1);

        }

        private void CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            Globals.MultiIsChecked = false;
            Globals.Stop = true;
            OnDataComboBoxSelectionChanged(null, null);

            SpeedSlider_ValueChanged(null, null);
            if (SelectedAlgorithm == null) return;
            if (SelectedAlgorithm.IsSorted())
                Draw.ChangeColorForAll(Boxes, ColorPalette.DEFAULT_BAR_FILL);
        }

        private void CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            Globals.MultiIsChecked = true; 
            SpeedSlider_ValueChanged(null, null);
        }

        private void ResetDataBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Globals.EndAnimationIsRunning) return;
            OnDataComboBoxSelectionChanged(null, null);
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
                if (n >= 1001)
                {
                    MessageBox.Show("N must be smaller than 1000");
                } else
                {
                    List<int> numbers = Enumerable.Range(1, n).ToList();
                    
                    Random rand = new Random();
                    numbers = numbers.OrderBy(x => rand.Next()).ToList();

                    StopBtn_Click(null, null);

                    Numbers = new List<int>(numbers);
                    Boxes.ForEach(e => e.Delete());
                    Boxes.Clear();
                    InitializeRectObjects(Numbers);
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
            try {
                List<int> numbers = Array.ConvertAll(ExactInputTextBox.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), int.Parse).ToList();
                if (numbers.Count < 2)
                {
                    MessageBox.Show("Array must have at least 2 elements");
                } else
                {
                    StopBtn_Click(null, null);

                    Numbers = new List<int>(numbers);
                    Boxes.ForEach(e => e.Delete());
                    Boxes.Clear();
                    InitializeRectObjects(Numbers);
                }
            }
            catch (OverflowException)
            {
                MessageBox.Show("All numbers must be less than 2^31");
            }
            catch (Exception)
            {
                MessageBox.Show("SOMETHING WENT WRONG");
            }
            ExactInputTextBox.Clear();
        }

        private void RandomTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GenerateRandomNumbers();
            }
        }

        private void GenerateRandomNumbers()
        {
            if (MinTextBox.Text.Length < 1 || MaxTextBox.Text.Length < 1 || NTextBox.Text.Length < 1)
            {
                MessageBox.Show("Please fill in all three fields");
                return;
            }
            try
            {
                int n = int.Parse(NTextBox.Text.Replace(" ", ""));
                int min = int.Parse(MinTextBox.Text.Replace(" ", ""));
                int max = int.Parse(MaxTextBox.Text.Replace(" ", ""));

                if (n >= 1001)
                {
                    MessageBox.Show("N must be smaller than 1000");
                    return;
                }
                    Random rand = new Random();
                List<int> numbers = Enumerable.Range(0, n).Select(_ => rand.Next(min, max + 1)).ToList();

                StopBtn_Click(null, null);

                Numbers = new List<int>(numbers);
                Boxes.ForEach(e => e.Delete());
                Boxes.Clear();
                InitializeRectObjects(Numbers);

            } catch
            {
                MessageBox.Show("SOMETHING WENT WRONG");
            }

            MinTextBox.Clear();
            MaxTextBox.Clear();
            NTextBox.Clear();
        }
    }
}
