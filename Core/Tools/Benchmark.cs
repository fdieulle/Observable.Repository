using System;
using System.Diagnostics;

namespace Observable.Tools
{
    /// <summary>
    /// Helper class to measure performances.
    /// </summary>
    public static class Benchmark
    {
        #region Stopwatch extensions

        public static void TakeMeasure(this Stopwatch sw, string message)
        {
            sw.Stop();
            Console.WriteLine("{0} Elapsed: {1} ms", message, sw.Elapsed.TotalMilliseconds);
            sw.Reset();
            sw.Start();
        }

        #endregion

        private const int DEFAULT_TIMES = 100;
        private const Units DEFAULT_UNITS = Units.Auto;
        private const int DEFAULT_JITTER_TIMES = 5;

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure(
            this Action action,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS, 
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action begin = null, Action end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke();
                action();
                end?.Invoke();
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke();

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action();
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke();
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg">Function argument</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T>(
            this Action<T> action, 
            T arg,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T> begin = null,
            Action<T> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg);
                action(arg);
                end?.Invoke(arg);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2>(
            this Action<T1, T2> action, T1 arg1, T2 arg2,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2> begin = null,
            Action<T1, T2> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2);
                action(arg1, arg2);
                end?.Invoke(arg1, arg2);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3>(
            this Action<T1, T2, T3> action, 
            T1 arg1, T2 arg2, T3 arg3,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3> begin = null,
            Action<T1, T2, T3> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3);

                action(arg1, arg2, arg3);

                end?.Invoke(arg1, arg2, arg3);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4>(
            this Action<T1, T2, T3, T4> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4> begin = null,
            Action<T1, T2, T3, T4> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4);

                action(arg1, arg2, arg3, arg4);

                end?.Invoke(arg1, arg2, arg3, arg4);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5>(
            this Action<T1, T2, T3, T4, T5> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5> begin = null,
            Action<T1, T2, T3, T4, T5> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5);

                action(arg1, arg2, arg3, arg4, arg5);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6>(
            this Action<T1, T2, T3, T4, T5, T6> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6> begin = null,
            Action<T1, T2, T3, T4, T5, T6> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);

                action(arg1, arg2, arg3, arg4, arg5, arg6);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="arg7">Function argument 7</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6, T7>(
            this Action<T1, T2, T3, T4, T5, T6, T7> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6, T7> begin = null,
            Action<T1, T2, T3, T4, T5, T6, T7> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="arg7">Function argument 7</param>
        /// <param name="arg8">Function argument 8</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6, T7, T8>(
            this Action<T1, T2, T3, T4, T5, T6, T7, T8> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> begin = null,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<TResult>(
            this Func<TResult> action,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action begin = null, 
            Action end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke();
                action();
                end?.Invoke();
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke();

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action();
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke();
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg">Function argument</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T, TResult>(
            this Func<T, TResult> action, T arg,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T> begin = null,
            Action<T> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg);
                action(arg);
                end?.Invoke(arg);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, TResult>(
            this Func<T1, T2, TResult> action, 
            T1 arg1, T2 arg2,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2> begin = null,
            Action<T1, T2> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2);
                action(arg1, arg2);
                end?.Invoke(arg1, arg2);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, TResult>(
            this Func<T1, T2, T3, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3> begin = null,
            Action<T1, T2, T3> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3);

                action(arg1, arg2, arg3);

                end?.Invoke(arg1, arg2, arg3);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4> begin = null,
            Action<T1, T2, T3, T4> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4);

                action(arg1, arg2, arg3, arg4);

                end?.Invoke(arg1, arg2, arg3, arg4);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, TResult>(
            this Func<T1, T2, T3, T4, T5, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5> begin = null,
            Action<T1, T2, T3, T4, T5> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5);

                action(arg1, arg2, arg3, arg4, arg5);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6> begin = null, 
            Action<T1, T2, T3, T4, T5, T6> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);

                action(arg1, arg2, arg3, arg4, arg5, arg6);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="arg7">Function argument 7</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6, T7, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, T7, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6, T7> begin = null,
            Action<T1, T2, T3, T4, T5, T6, T7> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        /// <summary>
        /// Measure a function by running it many times. By default 5 jitter run which will be ignore in the metrics then run 100 times.
        /// </summary>
        /// <param name="action">Function to measure</param>
        /// <param name="arg1">Function argument 1</param>
        /// <param name="arg2">Function argument 2</param>
        /// <param name="arg3">Function argument 3</param>
        /// <param name="arg4">Function argument 4</param>
        /// <param name="arg5">Function argument 5</param>
        /// <param name="arg6">Function argument 6</param>
        /// <param name="arg7">Function argument 7</param>
        /// <param name="arg8">Function argument 8</param>
        /// <param name="times">Number of function calls.</param>
        /// <param name="units">Measure units, by default Auto</param>
        /// <param name="jitterTimes">Number of jitter calls, 5 by default.</param>
        /// <param name="checkMemory">Defines if you want to measure memory also.</param>
        /// <param name="begin">Function call before each measured function.</param>
        /// <param name="end">Function call after each measured function.</param>
        /// <returns>Return the measure metrics.</returns>
        public static Metrics Measure<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> action, 
            T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8,
            int times = DEFAULT_TIMES, Units units = DEFAULT_UNITS,
            int jitterTimes = DEFAULT_JITTER_TIMES, bool checkMemory = false,
            Action<T1, T2, T3, T4, T5, T6, T7, T8> begin = null, 
            Action<T1, T2, T3, T4, T5, T6, T7, T8> end = null)
        {
            for (var i = 0; i < jitterTimes; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            var metrics = new double[times];
            var memory = checkMemory ? CreateMemoryMetrics(times) : null;
            var sw = new Stopwatch();
            for (var i = 0; i < times; i++)
            {
                begin?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

                if (checkMemory)
                {
                    ForceGc();
                    FillGcInfo(memory, i);
                }

                sw.Start();
                action(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
                sw.Stop();

                if (checkMemory)
                    FillGcInfo(memory, i);

                metrics[i] = sw.Elapsed.TotalMilliseconds;
                sw.Reset();

                end?.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            return GenerateMetrics(metrics, units, memory);
        }

        #region Helpers

        private static Metrics GenerateMetrics(double[] metrics, Units units, double[][] memory)
        {
            Metrics[] underlyings = null;
            if (memory != null)
            {
                underlyings = new Metrics[memory.Length];
                underlyings[0] = new Metrics(memory[0], Units.Millis, "Total Memory: ");
                for (var i = 0; i <= GC.MaxGeneration; i++ )
                    underlyings[i+1] = new Metrics(memory[i+1], Units.Millis, "GC Gen" + i + " count:");
            }
            return new Metrics(metrics, units, underlyings: underlyings);
        }
        
        private static double[][] CreateMemoryMetrics(int times)
        {
            var metrics = new double[GC.MaxGeneration + 2][];
            metrics[0] = new double[times];
            for(var i=0; i <= GC.MaxGeneration; i++)
                metrics[i+1] = new double[times];
            return metrics;
        }

        private static void FillGcInfo(double[][] memory, int idx)
        {
            memory[0][idx] = GC.GetTotalMemory(false) - memory[0][idx];
            for (var j = 0; j <= GC.MaxGeneration; j++)
                memory[j + 1][idx] = GC.CollectionCount(j) - memory[j + 1][idx];
        }

        private static void ForceGc()
        {
            for (var i = 0; i <= GC.MaxGeneration; i++ )
                GC.Collect(i, GCCollectionMode.Forced, true);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion
    }
}
