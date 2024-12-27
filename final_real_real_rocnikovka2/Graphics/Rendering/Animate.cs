using final_real_real_rocnikovka2.Graphics.Objects;
using final_real_real_rocnikovka2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace final_real_real_rocnikovka2.Graphics.Rendering
{
    public static class Animate
    {
        private static TimeSpan AnimationTime => TimeSpan.FromMilliseconds(Globals.AnimationMs);
        private static readonly Storyboard AnimationStoryboard;

        static Animate()
        {
            AnimationStoryboard = new Storyboard();
        }
        public static Task Wait(int delay, int iteratorNumber)
        {
            if (delay > 14) return Task.Delay(delay);
            else
            {
                if (iteratorNumber % (15 - delay) == 0 || Globals.MultiIsChecked) return Task.Delay(1); // Globals.MulitIsChecked protoze jinak se mohou algoritmy zkazit, kdyz jich jede vic najednou a skippuji delay
                else return Task.CompletedTask;
            }
        }

        public static void AnimationRun()
        {
            AnimationStoryboard.Begin();
        }

        public static void AnimationClear()
        {
            AnimationStoryboard.Children.Clear();
        }
        public static async Task AnimationSkip()
        {
            AnimationStoryboard.SkipToFill();
            await Task.Delay(TimeSpan.FromMilliseconds(1));
        }

        public static void ScheduleForDeletion(GraphicElement gE)
        {
            AnimationStoryboard.Completed += (s, e) =>
            {
                gE.Delete();

            };
        }
        public static void ScheduleZIndex(GraphicElement gE, int zIndex)
        {
            AnimationStoryboard.Completed += (s, e) =>
            {
                Canvas.SetZIndex(gE.MainUIElement, zIndex);

            };
        }
        public static TimeSpan GetStoryboardDuration()
        {
            TimeSpan maxDuration = TimeSpan.Zero;

            foreach (var timeline in AnimationStoryboard.Children)
            {
                TimeSpan? duration = timeline.Duration.HasTimeSpan ? timeline.Duration.TimeSpan : (TimeSpan?)null;

                if (duration.HasValue)
                {
                    TimeSpan beginTime = timeline.BeginTime ?? TimeSpan.Zero;
                    TimeSpan endTime = beginTime + duration.Value;

                    if (endTime > maxDuration)
                    {
                        maxDuration = endTime;
                    }
                }
            }

            return maxDuration;
        }

        public static void BallStrokeColorChange(Ball ball, Color endColor, double duration, double beginTime)
        {
            ColorAnimation colorAnimation = CreateColorAnimation(
                gE:             ball,
                from:           ball.GetStrokeColor(),
                to:             endColor,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath("Stroke.Color"));

            AnimationStoryboard.Children.Add(colorAnimation);
        }

      
        public static void BallFillColorChange(Ball ball, Color endColor, double duration, double beginTime)
        {
            ColorAnimation colorAnimation = CreateColorAnimation(
                gE:             ball,
                from:           ball.GetFillColor(),
                to:             endColor,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath("Fill.Color"));

            AnimationStoryboard.Children.Add(colorAnimation);
        }


        public static void TextColorChange(Text text, Color endColor, double duration, double beginTime)
        {
            ColorAnimation colorAnimation = CreateColorAnimation(
                gE:             text,
                from:           text.GetColor(),
                to:             endColor,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath("Foreground.Color"));

            AnimationStoryboard.Children.Add(colorAnimation);
        }

        public static void OpacityChange(GraphicElement gE, double endOpacity, double duration, double beginTime)
        {
            DoubleAnimation doubleAnimation = CreateDoubleAnimation(
                gE:             gE, 
                from:           gE.MainUIElement.Opacity,
                to:             endOpacity, 
                duration:       duration, 
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(UIElement.OpacityProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            AnimationStoryboard.Children.Add(doubleAnimation);

        }

        public static void MoveGraphicElement(GraphicElement gE, double endX, double endY, double duration, double beginTime)
        {
            DoubleAnimation gEVerticalMovement = CreateDoubleAnimation(
                gE: gE,
                from: gE.Y,
                to: endY,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation gEHorizontalMovement = CreateDoubleAnimation(
                gE: gE,
                from: gE.X,
                to: endX,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

           

            AnimationStoryboard.Children.Add(gEVerticalMovement);
            AnimationStoryboard.Children.Add(gEHorizontalMovement);


            AnimationStoryboard.Completed += (s, e) =>  // provizorni reseni protoze nevim jak tohle udelat pomoci iterace pro kazdy pri animationrun :(
            {
                gE.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                gE.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                gE.SetPosition(endX, endY);

                AnimationStoryboard.Completed -= (s, e) => { };

            };
        }

        public static void MoveBallWithText(Ball ball, double endX, double endY, double duration, double beginTime)
        {
            DoubleAnimation ballVerticalMovement = CreateDoubleAnimation(
                gE:             ball,
                from:           ball.Y,
                to:             endY,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballHorizontalMovement = CreateDoubleAnimation(
                gE:             ball,
                from:           ball.X,
                to:             endX,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textVerticalMovement = CreateDoubleAnimation(
                gE:             ball.BallText,
                from:           ball.BallText.Y,
                to:             endY + Draw.BallRadius - ball.BallText.TextHeight/2,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textHorizontalMovement = CreateDoubleAnimation(
                gE:             ball.BallText,
                from:           ball.BallText.X,
                to:             endX + Draw.BallRadius - ball.BallText.TextWidth/2,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            AnimationStoryboard.Children.Add(ballVerticalMovement);
            AnimationStoryboard.Children.Add(ballHorizontalMovement);
            AnimationStoryboard.Children.Add(textVerticalMovement);
            AnimationStoryboard.Children.Add(textHorizontalMovement);
            
            ballVerticalMovement.Completed += (s, e) =>  // provizorni reseni protoze nevim jak tohle udelat pomoci iterace pro kazdy pri animationrun :(
            {
                ball.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ball.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);

                ball.BallText.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ball.BallText.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);

                ball.SetPosition(endX, endY);

                ballVerticalMovement.Completed -= (s, e) => { };

            };
        }

        public static void BallSwap(Ball ballA, Ball ballB, double duration, double beginTime, double arcHeight)
        {
            /////////////////// BALL ///////////////////

            DoubleAnimation ballAVerticalMovementUp = CreateDoubleAnimation(
                gE:             ballA,
                from:           ballA.Y,
                to:             ballA.Y - arcHeight * Draw.BallRadius,
                duration:       duration/2,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseOut });

            DoubleAnimation ballBVerticalMovementDown = CreateDoubleAnimation(
                gE:             ballB,
                from:           ballB.Y,
                to:             ballB.Y + arcHeight * Draw.BallRadius,
                duration:       duration / 2,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseOut });

            DoubleAnimation ballAHorizontalMovement = CreateDoubleAnimation(
                gE:             ballA,
                from:           ballA.X,
                to:             ballB.X,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballBHorizontalMovement = CreateDoubleAnimation(
                gE:             ballB,
                from:           ballB.X,
                to:             ballA.X,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballAVerticalMovementDown = CreateDoubleAnimation(
                gE:             ballA,
                from:           ballA.Y - arcHeight * Draw.BallRadius,
                to:             ballA.Y,
                duration:       duration / 2,
                beginTime:      beginTime + duration / 2,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseIn });

            DoubleAnimation ballBVerticalMovementUp = CreateDoubleAnimation(
                gE:             ballB,
                from:           ballB.Y + arcHeight * Draw.BallRadius,
                to:             ballB.Y,
                duration:       duration / 2,
                beginTime:      beginTime + duration / 2,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseIn });

            /////////////////// TEXT ///////////////////

            DoubleAnimation textAVerticalMovementUp = CreateDoubleAnimation(
                gE:             ballA.BallText,
                from:           ballA.BallText.Y,
                to:             ballA.BallText.Y - arcHeight * Draw.BallRadius,
                duration:       duration / 2,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseOut });

            DoubleAnimation textBVerticalMovementDown = CreateDoubleAnimation(
                gE:             ballB.BallText,
                from:           ballB.BallText.Y,
                to:             ballB.BallText.Y + arcHeight * Draw.BallRadius,
                duration:       duration / 2,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseOut });

            DoubleAnimation textAHorizontalMovement = CreateDoubleAnimation(
                gE:             ballA.BallText,
                from:           ballA.BallText.X,
                to:             ballB.X + Draw.BallRadius - ballA.BallText.TextWidth/2,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textBHorizontalMovement = CreateDoubleAnimation(
                gE:             ballB.BallText,
                from:           ballB.BallText.X,
                to:             ballA.X + Draw.BallRadius - ballB.BallText.TextWidth / 2,
                duration:       duration,
                beginTime:      beginTime,
                propertyPath:   new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textAVerticalMovementDown = CreateDoubleAnimation(
                gE:             ballA.BallText,
                from:           ballA.BallText.Y - arcHeight * Draw.BallRadius,
                to:             ballA.BallText.Y,
                duration:       duration / 2,
                beginTime:      beginTime + duration / 2,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseIn });

            DoubleAnimation textBVerticalMovementUp = CreateDoubleAnimation(
                gE:             ballB.BallText,
                from:           ballB.BallText.Y + arcHeight * Draw.BallRadius,
                to:             ballB.BallText.Y,
                duration:       duration / 2,
                beginTime:      beginTime + duration / 2,
                propertyPath:   new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseIn });

            /////////////////// BALL ///////////////////
            AnimationStoryboard.Children.Add(ballAVerticalMovementUp);
            AnimationStoryboard.Children.Add(ballBVerticalMovementDown);
            AnimationStoryboard.Children.Add(ballAHorizontalMovement);
            AnimationStoryboard.Children.Add(ballBHorizontalMovement);
            AnimationStoryboard.Children.Add(ballAVerticalMovementDown);
            AnimationStoryboard.Children.Add(ballBVerticalMovementUp);

            /////////////////// TEXT ///////////////////
            AnimationStoryboard.Children.Add(textAVerticalMovementUp);
            AnimationStoryboard.Children.Add(textBVerticalMovementDown);
            AnimationStoryboard.Children.Add(textAHorizontalMovement);
            AnimationStoryboard.Children.Add(textBHorizontalMovement);
            AnimationStoryboard.Children.Add(textAVerticalMovementDown);
            AnimationStoryboard.Children.Add(textBVerticalMovementUp);

            AnimationStoryboard.Completed += (s, e) =>  // provizorni reseni protoze nevim jak tohle udelat pomoci iterace pro kazdy pri animationrun :(
            {
                ballA.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ballA.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                ballB.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ballB.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);

                ballA.BallText.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ballA.BallText.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                ballB.BallText.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                ballB.BallText.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);

                AnimationStoryboard.Completed -= (s, e) => { };
            };
        }

        public static void BallSwapInTree(Ball child, Ball parent, double duration, double beginTime)
        {
            /////////////////// BALL ///////////////////

            DoubleAnimation ballAVerticalMovementUp = CreateDoubleAnimation(
                gE: child,
                from: child.Y,
                to: parent.Y,
                duration: duration / 2,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballBVerticalMovementDown = CreateDoubleAnimation(
                gE: parent,
                from: parent.Y,
                to: child.Y,
                duration: duration / 2,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballAHorizontalMovement = CreateDoubleAnimation(
                gE: child,
                from: child.X,
                to: parent.X,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation ballBHorizontalMovement = CreateDoubleAnimation(
                gE: parent,
                from: parent.X,
                to: child.X,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

          

            /////////////////// TEXT ///////////////////

            DoubleAnimation textAVerticalMovementUp = CreateDoubleAnimation(
                gE: child.BallText,
                from: child.BallText.Y,
                to: parent.Y + Draw.BallRadius - child.BallText.TextHeight/2,
                duration: duration / 2,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textBVerticalMovementDown = CreateDoubleAnimation(
                gE: parent.BallText,
                from: parent.BallText.Y,
                to: child.Y + Draw.BallRadius - parent.BallText.TextHeight/2,
                duration: duration / 2,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.TopProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textAHorizontalMovement = CreateDoubleAnimation(
                gE: child.BallText,
                from: child.BallText.X,
                to: parent.X + Draw.BallRadius - child.BallText.TextWidth / 2,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });

            DoubleAnimation textBHorizontalMovement = CreateDoubleAnimation(
                gE: parent.BallText,
                from: parent.BallText.X,
                to: child.X + Draw.BallRadius - parent.BallText.TextWidth / 2,
                duration: duration,
                beginTime: beginTime,
                propertyPath: new PropertyPath(Canvas.LeftProperty),
                easingFunction: new QuadraticEase { EasingMode = EasingMode.EaseInOut });


            /////////////////// BALL ///////////////////
            AnimationStoryboard.Children.Add(ballAVerticalMovementUp);
            AnimationStoryboard.Children.Add(ballBVerticalMovementDown);
            AnimationStoryboard.Children.Add(ballAHorizontalMovement);
            AnimationStoryboard.Children.Add(ballBHorizontalMovement);

            /////////////////// TEXT ///////////////////
            AnimationStoryboard.Children.Add(textAVerticalMovementUp);
            AnimationStoryboard.Children.Add(textBVerticalMovementDown);
            AnimationStoryboard.Children.Add(textAHorizontalMovement);
            AnimationStoryboard.Children.Add(textBHorizontalMovement);

            double parentX = parent.X;
            double parentY = parent.Y;

            double childX = child.X;
            double childY = child.Y;

            textAHorizontalMovement.Completed += (s, e) =>  // provizorni reseni protoze nevim jak tohle udelat pomoci iterace pro kazdy pri animationrun :(
            {
                child.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                child.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                parent.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                parent.MainUIElement.BeginAnimation(Canvas.TopProperty, null);

                child.BallText.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                child.BallText.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);
                parent.BallText.MainUIElement.BeginAnimation(Canvas.TopProperty, null);
                parent.BallText.MainUIElement.BeginAnimation(Canvas.LeftProperty, null);

                child.SetPosition(parentX, parentY);
                parent.SetPosition(childX, childY);
                textAHorizontalMovement.Completed -= (s, e) => { };
            };
        }


        private static ColorAnimation CreateColorAnimation(GraphicElement gE, Color from, Color to, double duration, double beginTime, PropertyPath propertyPath)
        {
            ColorAnimation colorAnimation = new ColorAnimation();

            colorAnimation.From = from;
            colorAnimation.To = to;
            colorAnimation.Duration = AnimationTime * duration;
            colorAnimation.BeginTime = AnimationTime * beginTime;
            colorAnimation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

            Storyboard.SetTarget(colorAnimation, gE.MainUIElement);
            Storyboard.SetTargetProperty(colorAnimation, propertyPath);

            return colorAnimation;
        }

     
        private static DoubleAnimation CreateDoubleAnimation(GraphicElement gE, double from, double to, double duration, double beginTime, PropertyPath propertyPath, IEasingFunction easingFunction)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = from;
            doubleAnimation.To = to;
            doubleAnimation.Duration = AnimationTime * duration;
            doubleAnimation.BeginTime = AnimationTime * beginTime;
            doubleAnimation.EasingFunction = easingFunction;

            Storyboard.SetTarget(doubleAnimation, gE.MainUIElement);
            Storyboard.SetTargetProperty(doubleAnimation, propertyPath);

            return doubleAnimation;
        }

    }
}
