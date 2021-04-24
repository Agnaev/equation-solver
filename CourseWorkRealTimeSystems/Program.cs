using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CourseWorkRealTimeSystems
{
    class Program
    {
        private static List<ThreadWithResult<double>> threads = new List<ThreadWithResult<double>>();
        public static void Main (string[] args)
        {
            Console.Write(
                value: "function: "
            );
            string quation = Console.ReadLine();
            Console.Write(
                value: "a = "
            );
            double.TryParse(
                s: Console.ReadLine().Replace('.', ','), 
                result: out double a
            );
            Console.Write(
                value: "b = "
            );
            double.TryParse(
                s: Console.ReadLine().Replace('.', ','), 
                result: out double b
            );
            Console.Write(
                value: "eps = "
            );
            double.TryParse(
                s: Console.ReadLine().Replace('.', ','), 
                result: out double eps
            );
            Console.Write(
                value: "Points count = "
            );
            int.TryParse(
                s: Console.ReadLine(), 
                result: out int pointsCount
            );
            Console.Write(
                value: "Threads count = "
            );
            int.TryParse(
                s: Console.ReadLine(), 
                result: out int threadsCount
            );

            double totalLength = b - a;
            double step = totalLength / threadsCount;
            double currentState = a;
            Random rnd = new Random();
            for (int i = 0; i < threadsCount; i++)
            {
                var th = new ThreadWithResult<double>(
                    thread: new Thread(
                        start: new ParameterizedThreadStart(
                            threadFunction
                        )
                    )
                );
                threads.Add(
                    item: th
                );
                th.Run(
                    param: new ThreadParams() {
                        quation = quation,
                        start = currentState,
                        end = currentState + step,
                        count = pointsCount,
                        rand = rnd,
                        threadId = i
                    }
                );
                currentState += step;
            }
            for (int i = 0; i < threadsCount; i++)
            {
                threads[i].Join();
                Console.WriteLine(
                    value: "thread[" + i + "] = " + threads[i].result
                );
            }
            var result = threads.Select(x => x.result).Min();

            Console.WriteLine("Result is " + Math.Round(result, eps.ToString().Length - 1));
        }

        private static void threadFunction (object param) 
        {
            ThreadParams data = (ThreadParams)param;
            double res = MathParser.Parse(
                equation: data.quation,
                args: new Argument<double>(
                    name: 'x', 
                    value: data.start
                )
            );
            double randValue;
            for (int _ = 0; _ < data.count; _++)
            {
                randValue = data.rand.NextDouble() * (data.end - data.start) + data.start;
                double tempRes = MathParser.Parse(
                    equation: data.quation,
                    args: new Argument<double>(
                        name: 'x', 
                        value: randValue
                    )
                );
                res = closerToZero(
                    a: res, 
                    b: tempRes
                );
            }
            threads[data.threadId].SetResult(
                result: res
            );
        }
        private static double closerToZero (double a, double b) => Math.Abs(a) > Math.Abs(b) ? b : a;
    }
}
