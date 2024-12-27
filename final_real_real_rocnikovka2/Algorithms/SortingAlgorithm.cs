using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace final_real_real_rocnikovka2.Algorithms
{
    public abstract class SortingAlgorithm
    {
        public String Name { get; protected set; }
        public static SortingAlgorithm Instance { get; }
        protected List<int> Numbers { get; set; }
        protected List<Box> Boxes { get; set; }
        protected List<Ball> Balls { get; set; }
        protected int StepState { get; set; }
        protected int N { get; set; }
        protected List<GraphicElement> GraphicElements { get; set; }
        public bool IsSortedBool { get; set; }
        private int _swapCount;
        private int _comparisonCount;

        public int SwapCount
        {
            get => _swapCount;
            set
            {
                if (_swapCount != value)
                {
                    _swapCount = value;
                    OnPropertyChanged(nameof(SwapCount)); 
                }
            }
        }

        public int ComparisonCount
        {
            get => _comparisonCount;
            set
            {
                if (_comparisonCount != value)
                {
                    _comparisonCount = value;
                    OnPropertyChanged(nameof(ComparisonCount)); 
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void Step();
        public abstract Task Sort();
        public abstract void Reset(List<int> numbers, List<Box> boxes);
        public abstract void Reset(List<int> numbers, List<Ball> balls, List<GraphicElement> graphicElements);
        public abstract void OnSelect(List<int> numbers, List<Ball> balls);

        public bool IsSorted()
        {
            if (Numbers == null) return false;
            for (int i = 0; i < Numbers.Count - 1; i++)
            {
                if (Numbers[i] > Numbers[i + 1])
                {
                    return false;
                }
            }
            return true;
        }

        protected static void SwapInList<T>(List<T> list, int indexA, int indexB)
        {
            (list[indexB], list[indexA]) = (list[indexA], list[indexB]);
        }     
    }
}
