using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

class ConcurrentThreadTest_4Threads
{
    enum YesNoPrompt
    {
        Yes,
        No,
        Invalid
    };

    struct ThreadCounterContainer
    {
        public ThreadCounterContainer(Int16 count1, Int16 count2, Int16 count3, Int16 count4)
        {
            Thread1_Counter = count1;
            Thread2_Counter = count2;
            Thread3_Counter = count3;
            Thread4_Counter = count4;
        }

        public Int16 Thread1_Counter { get; set; }
        public Int16 Thread2_Counter { get; set; }
        public Int16 Thread3_Counter { get; set; }
        public Int16 Thread4_Counter { get; set; }
    }

    private static ThreadCounterContainer CounterContainer = new ThreadCounterContainer(0, 0, 0, 0);

    private static Mutex mut = new Mutex();
    private static List<Thread> threadList = new List<Thread>();

    private static bool usingMutex;

    private static int numThreads = 4;
    private static int numIterations = 1000;

    static void Main()
    {
        GetUserRequirements();

        Stopwatch watch = new Stopwatch();
        watch.Start();

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("________________________________________________________\n");

        for (int i = 0; i < numThreads; i++)
        {
            Thread newThread = new Thread(new ThreadStart(ThreadInstance));

            newThread.Name = String.Format("Thread {0}", i + 1);
            threadList.Add(newThread);
            newThread.Start();
        }

        foreach (Thread thread in threadList)
        {
            thread.Join();
        }

        watch.Stop();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Number of threads:     ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(numThreads);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Number of interations: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(numIterations);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("Using Mutex?:          ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{(usingMutex ? "Yes" : "No")}\n");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Execution Time: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{watch.Elapsed.TotalMilliseconds} m/s");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("________________________________________________________");

        Console.ForegroundColor = ConsoleColor.Red;
    }

    private static void GetUserRequirements()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("How many threads do you wish to use?");

        numThreads = PromptUserValue(1, 4);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("How many interations do you wish to use?");

        numIterations = PromptUserValue(1, 32767);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Do you want to use Mutex?");

        usingMutex = (PromptUserYesNo() == YesNoPrompt.Yes ? true : false);

        Console.Clear();
    }

    private static YesNoPrompt PromptUserYesNo()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;

            string userResponse = Console.ReadLine();
            string userAnswer = userResponse.Trim().ToUpperInvariant();

            Console.WriteLine("");

            YesNoPrompt promptAnswer = (userAnswer == "Y" ? YesNoPrompt.Yes : (userAnswer == "N" ? YesNoPrompt.No : YesNoPrompt.Invalid));

            if (promptAnswer == YesNoPrompt.Invalid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Invalid input. Please enter Y or N.");
            }
            else
            {
                return promptAnswer;
            }
        }
    }

    private static int PromptUserValue(int minValue, int maxValue)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;

            int? promptValue;

            try
            {
                promptValue = Int32.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                promptValue = null;
            }

            Console.WriteLine("");

            if (minValue > promptValue || promptValue > maxValue)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid input. Please enter a number between {minValue} and {maxValue}.");
            }
            else if (promptValue == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Invalid input. Please enter a number.");
            }
            else
            {
                return promptValue ?? default(int);
            }
        }
    }

    private static void ThreadInstance()
    {
        for (int i = 0; i < numIterations; i++)
        {
            IncrementCounter(i + 1);
        }
    }

    private static void IncrementCounter(int iterationCount)
    {
        try
        {
            if (usingMutex)
            {
                mut.WaitOne();
            }

            switch (Thread.CurrentThread.Name)
            {
                case "Thread 1":
                    CounterContainer.Thread1_Counter++;
                    break;
                case "Thread 2":
                    CounterContainer.Thread2_Counter++;
                    break;
                case "Thread 3":
                    CounterContainer.Thread3_Counter++;
                    break;
                case "Thread 4":
                    CounterContainer.Thread4_Counter++;
                    break;
                default:
                    throw new Exception("Invalid thread name.");
            }

            if (usingMutex)
            {
                mut.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex);
        }
    }
}