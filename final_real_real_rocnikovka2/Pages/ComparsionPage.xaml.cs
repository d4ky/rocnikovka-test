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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Pages
{
    /// <summary>
    /// Interaction logic for Comparsion.xaml
    /// </summary>
    public partial class ComparsionPage : Page
    {
        private double previousCanvasWidth;
        private double previousCanvasHeight;

        private List<int> Numbers1 { get; set; }
        private List<Box> Boxes1 { get; set; }
        private List<int> Numbers2 { get; set; }
        private List<Box> Boxes2 { get; set; }
        private List<int> Numbers3 { get; set; }
        private List<Box> Boxes3 { get; set; }
        private List<int> Numbers4 { get; set; }
        private List<Box> Boxes4 { get; set; }
        private List<int>[] Numbers { get; set; }
        private List<Box>[] Boxes { get; set; }
        private Canvas[] Canvases { get; set; }
        private ComboBox[] ComboBoxes { get; set; }

        private bool FirstLoad;
        private SortingAlgorithm SelectedAlgorithm1; 
        private SortingAlgorithm SelectedAlgorithm2;
        private SortingAlgorithm SelectedAlgorithm3;
        private SortingAlgorithm SelectedAlgorithm4;
        private SortingAlgorithm[] SelectedAlgorithms
        { get => new SortingAlgorithm[] { SelectedAlgorithm1, SelectedAlgorithm2, SelectedAlgorithm3, SelectedAlgorithm4 }; }

        private Dictionary<string, List<int>> TestDataDictionary { get; set; }


        private readonly List<SortingAlgorithm> SortingAlgorithms;

        public ComparsionPage(List<SortingAlgorithm> sortingAlgorithms)
        {
            InitializeComponent();
            this.SortingAlgorithms = sortingAlgorithms;
            TestDataDictionary = new(){
                { "Default", TestData.DEFAULT },
                { "Stairs", TestData.STAIRS },
                { "Reverse sorted", TestData.REVERSE_SORTED},
                { "Almost sorted", TestData.ALMOST_SRORTED }
            };
            DataContext = this;

            Numbers1 = [];
            Numbers2 = [];
            Numbers3 = [];
            Numbers4 = [];
            Boxes1 = [];
            Boxes2 = [];
            Boxes3 = [];
            Boxes4 = [];
            FirstLoad = true;

            Numbers = [Numbers1, Numbers2, Numbers3, Numbers4];
            Boxes = [Boxes1, Boxes2, Boxes3, Boxes4];

            Canvases = [MainCanvas1, MainCanvas2, MainCanvas3, MainCanvas4];
            ComboBoxes = [AlgorithmComboBox1, AlgorithmComboBox2, AlgorithmComboBox3, AlgorithmComboBox4];
            Globals.AlgorithmIsRunning1 = false;

            PopulateComboBoxes();
        }

        private void OnDataComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StopBtn_Click(null, null);

            for (int i = 0; i < Numbers.Length; i++)
            {
                Numbers[i] = new List<int>(TestDataDictionary[DataComboBox.SelectedItem.ToString()]);
            }

            for (int i = 0; i < Boxes.Length; i++)
            {
                Boxes[i].ForEach(e => e.Delete());
                Boxes[i].Clear();
            }
            for (int i = 0; i < Numbers.Length; i++)
            {
                InitializeRectObjects(Numbers[i], Canvases[i], Boxes[i]);
            }
        }


        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            if (FirstLoad)
            {
                DataComboBox.SelectedItem = "Default";
                FirstLoad = false;
            }

            previousCanvasWidth = MainCanvas1.ActualWidth;
            previousCanvasHeight = MainCanvas1.ActualHeight;
            Globals.AnimationMs = (int)SpeedSlider.Value;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Globals.Stop = true;
            Draw.ChangeColorForAll(Boxes.SelectMany(L => L), ColorPalette.DEFAULT_BAR_FILL);
        }

        private static void InitializeRectObjects(List<int> numbers, Canvas canvas, List<Box> boxes)
        {
            double width = canvas.ActualWidth / numbers.Count;
            double maxNumber = numbers.Max();
            double heightPerNum = (canvas.ActualHeight) / maxNumber;
            double xPos = 0;
            foreach (int number in numbers)
            {
                Box newBox = new(canvas, 1, ColorPalette.DEFAULT_BAR_FILL, width, number * heightPerNum);
                newBox.SetPosition(xPos, canvas.ActualHeight - number * heightPerNum);
                newBox.AddToCanvas();
                boxes.Add(newBox);

                xPos += width;
            }
        }

        private void PopulateComboBoxes()
        {
            for (int i = 0; i < ComboBoxes.Length; i++)
            {
                ComboBoxes[i].Items.Clear();
                foreach (var algorithm in SortingAlgorithms)
                {
                    ComboBoxes[i].Items.Add(algorithm);
                }
            }

            DataComboBox.Items.Clear();

            foreach (string dataName in TestDataDictionary.Keys)
            {
                DataComboBox.Items.Add($"{dataName}");
            }
        }

        private async void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            Globals.Stop = false;

            if (!Globals.AlgorithmIsRunning1)
            {
                Globals.AlgorithmIsRunning1 = true;

                List<Task> sortingTasks = new List<Task>();

                for (int i = 0; i < SelectedAlgorithms?.Length; i++)
                {
                    // Reset the algorithm before starting
                    SelectedAlgorithms[i]?.Reset(Numbers[i], Boxes[i]);

                    if (SelectedAlgorithms[i] != null)
                    {
                        sortingTasks.Add(SelectedAlgorithms[i].Sort());
                    }
                }

                await Task.WhenAll(sortingTasks);
                Globals.AlgorithmIsRunning1 = false;
            }
        }

        private void StopBtn_Click(Object sender, RoutedEventArgs e)
        {
            Globals.Stop = true;
            Globals.AlgorithmIsRunning1 = false;
            for (int i = 0; i < SelectedAlgorithms?.Length; i++)
            {
                if (SelectedAlgorithms[i] == null) continue;
                if (!SelectedAlgorithms[i].IsSorted())
                    Draw.ChangeColorForAll(Boxes.SelectMany(L => L), ColorPalette.DEFAULT_BAR_FILL);
            }

        }

        private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (Box box in Boxes.SelectMany(List => List))
            {
                box.Update(previousCanvasWidth, previousCanvasHeight);
            }
            previousCanvasWidth = MainCanvas1.ActualWidth;
            previousCanvasHeight = MainCanvas1.ActualHeight;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.AnimationMs = (int)SpeedSlider.Value;
        }

        private void OnComboBoxSelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
            CheckForDuplicateSelection(0);
        }

        private void OnComboBoxSelectionChanged2(object sender, SelectionChangedEventArgs e)
        {
            CheckForDuplicateSelection(1);
        }

        private void OnComboBoxSelectionChanged3(object sender, SelectionChangedEventArgs e)
        {
            CheckForDuplicateSelection(2);
        }

        private void OnComboBoxSelectionChanged4(object sender, SelectionChangedEventArgs e)
        {
            CheckForDuplicateSelection(3);
        }

        private void CheckForDuplicateSelection(int selectedIndex)
        {
            var selectedAlgorithm = ComboBoxes[selectedIndex].SelectedItem as SortingAlgorithm;

            for (int i = 0; i < ComboBoxes.Length; i++)
            {
                if (i != selectedIndex)
                {
                    if (ComboBoxes[i].SelectedItem == selectedAlgorithm)
                    {
                        ComboBoxes[selectedIndex].SelectedItem = null;

                        return;
                    }
                }
            }
            switch (selectedIndex + 1)
            {
                case 1:
                    SelectedAlgorithm1 = selectedAlgorithm;
                    break;
                case 2:
                    SelectedAlgorithm2 = selectedAlgorithm;
                    break;
                case 3:
                    SelectedAlgorithm3 = selectedAlgorithm;
                    break;
                case 4:
                    SelectedAlgorithm4 = selectedAlgorithm;
                    break;
            }
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
                }
                else
                {
                    List<int> numbers = Enumerable.Range(1, n).ToList();

                    Random rand = new Random();
                    numbers = numbers.OrderBy(x => rand.Next()).ToList();

                    StopBtn_Click(null, null);

                    for (int i = 0; i < Numbers.Length; i++)
                    {
                        Numbers[i] = new List<int>(numbers);
                    }

                    for (int i = 0; i < Boxes.Length; i++)
                    {
                        Boxes[i].ForEach(e => e.Delete());
                        Boxes[i].Clear();
                    }
                    for (int i = 0; i < Numbers.Length; i++)
                    {
                        InitializeRectObjects(Numbers[i], Canvases[i], Boxes[i]);
                    }
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
                if (numbers.Count < 2)
                {
                    MessageBox.Show("Array must have at least 2 elements");
                }
                else
                {
                    StopBtn_Click(null, null);

                    for (int i = 0; i < Numbers.Length; i++)
                    {
                        Numbers[i] = new List<int>(numbers);
                    }

                    for (int i = 0; i < Boxes.Length; i++)
                    {
                        Boxes[i].ForEach(e => e.Delete());
                        Boxes[i].Clear();
                    }
                    for (int i = 0; i < Numbers.Length; i++)
                    {
                        InitializeRectObjects(Numbers[i], Canvases[i], Boxes[i]);
                    }
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

                for (int i = 0; i < Numbers.Length; i++)
                {
                    Numbers[i] = new List<int>(numbers);
                }

                for (int i = 0; i < Boxes.Length; i++)
                {
                    Boxes[i].ForEach(e => e.Delete());
                    Boxes[i].Clear();
                }
                for (int i = 0; i < Numbers.Length; i++)
                {
                    InitializeRectObjects(Numbers[i], Canvases[i], Boxes[i]);
                }

            }
            catch
            {
                MessageBox.Show("SOMETHING WENT WRONG");
            }

            MinTextBox.Clear();
            MaxTextBox.Clear();
            NTextBox.Clear();
        }
    }
}
