using final_real_real_rocnikovka2.Algorithms;
using final_real_real_rocnikovka2.Graphics.Rendering;
using final_real_real_rocnikovka2.Pages;
using final_real_real_rocnikovka2.Utils;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace final_real_real_rocnikovka2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ClassicSortingPage _classicSortingPage;
        private readonly ExplanatorySortingPage _explanatorySortingPage;
        private readonly ComparsionPage _comparisonPage;

        private readonly SortingAlgorithmLoader _algorithmLoader;
        private readonly List<SortingAlgorithm> sortingAlgorithms;

        public MainWindow()
        {
            InitializeComponent();

            _algorithmLoader = new SortingAlgorithmLoader(Assembly.GetExecutingAssembly());
            var preferredOrder = new Dictionary<Type, int>
            {
                { typeof(BubbleSort), 0 },
                { typeof(SelectionSort), 1 },
                { typeof(InsertionSort), 2 },
                { typeof(HeapSort), 3 },
                { typeof(MergeSort), 4 },
                { typeof(QuickSort), 5 }
            };
            sortingAlgorithms = _algorithmLoader.LoadAlgorithms();

            sortingAlgorithms = sortingAlgorithms
                .OrderBy(algorithm =>
                {
                    return preferredOrder.ContainsKey(algorithm.GetType()) ? preferredOrder[algorithm.GetType()] : int.MaxValue;
                })
                .ToList();


            _classicSortingPage = new ClassicSortingPage(sortingAlgorithms);
            _explanatorySortingPage = new ExplanatorySortingPage(sortingAlgorithms);
            _comparisonPage = new ComparsionPage(sortingAlgorithms);


            MainFrame.Content = _explanatorySortingPage;
            ExplanatoryButton.Tag = "Selected";
        }

        private void ClassicBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = _classicSortingPage;
            ClassicButton.Tag = "Selected";
            ExplanatoryButton.Tag = null;
            ComparisonButton.Tag = null;
        }

        private void ExplanatoryBtn_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = _explanatorySortingPage;
            ExplanatoryButton.Tag = "Selected";
            ClassicButton.Tag = null;
            ComparisonButton.Tag = null;
        }

        private void ComparisonBtn_Click(Object sender, RoutedEventArgs e)
        {
            MainFrame.Content = _comparisonPage;
            ComparisonButton.Tag = "Selected";
            ClassicButton.Tag = null;
            ExplanatoryButton.Tag = null;
        }
    }
}