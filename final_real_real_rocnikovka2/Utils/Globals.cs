
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_real_real_rocnikovka2.Utils
{
    public static class Globals
    {
        private static bool stop;
        private static bool algorithmIsRunning;
        private static int animationMs;
        private static bool multiIsChecked;
        private static bool algorithmIsRunning1;
        private static bool endAnimationIsRunning;

        public static bool EndAnimationIsRunning
        {
            get => endAnimationIsRunning;
            set => endAnimationIsRunning = value;
        }
        public static bool Stop
        {
            get => stop;
            set => stop = value;
        }

        public static bool AlgorithmIsRunning
        {
            get => algorithmIsRunning;
            set => algorithmIsRunning = value;
        }
        public static bool AlgorithmIsRunning1
        {
            get => algorithmIsRunning1;
            set => algorithmIsRunning1 = value;
        }

        public static int AnimationMs
        {
            get => animationMs;
            set => animationMs = value;
        }

        public static bool MultiIsChecked
        {
            get => multiIsChecked;
            set => multiIsChecked = value;
        }
    }
}
