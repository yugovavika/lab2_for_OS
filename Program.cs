using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace OSka
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 &&
                args[0] == "--generate")
            {
                RunGenerator(args);
                return;
            }

            Console.WriteLine(
                "Многопоточное умножение матриц");
            Console.WriteLine();

            Console.WriteLine("1 - Обычный запуск");
            Console.WriteLine("2 - Тестирование производительности");
            Console.WriteLine("3 - Возведение матрицы в степень");
            Console.WriteLine("4 - Замеры степени матрицы");

            int mode = Checkd.ReadNumberInRange(
                "Выберите режим: ", 1, 4);

            if (mode == 2)
            {
                RunBenchmark();
                return;
            }

            if (mode == 3)
            {
                RunPower();
                return;
            }

            if (mode == 4)
            {
                RunPowerBenchmark();
                return;
            }

            Console.WriteLine();

            int rows = Checkd.ReadNumberInRange(
            "Введите размер матрицы A: ", 1, 2000);

            int columns = Checkd.ReadNumberInRange(
                "Введите количество столбцов B: ", 1, 2000);

            int threadCount = Checkd.ReadNumberInRange(
                "Введите количество потоков: ", 1, 1024);

            Console.WriteLine();

            int[][] matrixA;
            int[,] matrixB;

            bool success = MatrixTasks.GenerateMatricesInProcess(
            rows,
            columns,
            out matrixA,
            out matrixB);

            if (!success)
            {
                Console.WriteLine(
                    "Не удалось получить матрицы.");
                return;
            }

            Console.WriteLine(
            "Матрицы получены от дочернего процесса.");

            Console.WriteLine(
                "Размер A: " + matrixA.Length + " строк");

            Console.WriteLine(
                "Размер B: "
                + matrixB.GetLength(0)
                + " x "
                + matrixB.GetLength(1));

            if (rows <= 10 && columns <= 10)
            {
                Console.WriteLine();
                Console.WriteLine("Матрица A:");
                MatrixTasks.PrintMatrixForCheck(matrixA);

                Console.WriteLine();
                Console.WriteLine("Матрица B:");
                MatrixTasks.PrintMatrixForCheck(matrixB);
            }

            MatrixTasks.RunStatic(
            matrixA,
            matrixB,
            threadCount,
            true);

            MatrixTasks.RunDynamic(
            matrixA,
            matrixB,
            threadCount,
            true);
        }


        private static void RunBenchmark()
        {
            int[] sizes = { 500, 1000, 2000 };
            int[] threadCounts =
            {
        1, 2, 3, 4, 5, 6, 7, 8, 16, 32
    };

            string filePath = "benchmark.csv";

            using (StreamWriter writer = new StreamWriter(
                filePath, false))
            {
                writer.WriteLine(
                    "Размер;Потоки;Запуск;Статика;Динамика");

                int sizeIndex;
                int threadIndex;
                int testIndex;

                for (sizeIndex = 0;
                    sizeIndex < sizes.Length;
                    sizeIndex++)
                {
                    int size = sizes[sizeIndex];

                    int[][] matrixA;
                    int[,] matrixB;

                    Console.WriteLine();
                    Console.WriteLine(
                        "Размер матриц: " + size);

                    Stopwatch transferStopwatch = Stopwatch.StartNew();

                    bool success =
                        MatrixTasks.GenerateMatricesInProcess(
                            size,
                            size,
                            out matrixA,
                            out matrixB);

                    transferStopwatch.Stop();

                    double transferTime =
                        transferStopwatch.Elapsed.TotalMilliseconds;

                    Console.WriteLine(
                        "Время генерации и передачи: "
                        + transferTime.ToString("F3")
                        + " мс");

                    if (!success)
                    {
                        Console.WriteLine(
                            "Не удалось создать матрицы.");
                        return;
                    }

                    for (threadIndex = 0;
                        threadIndex < threadCounts.Length;
                        threadIndex++)
                    {
                        int threads = threadCounts[threadIndex];

                        for (testIndex = 1;
                            testIndex <= 5;
                            testIndex++)
                        {
                            double staticTime =
                                MatrixTasks.RunStatic(
                                    matrixA,
                                    matrixB,
                                    threads,
                                    false);

                            double dynamicTime =
                                MatrixTasks.RunDynamic(
                                    matrixA,
                                    matrixB,
                                    threads,
                                    false);

                            writer.WriteLine(
                                size + ";"
                                + threads + ";"
                                + testIndex + ";"
                                + staticTime.ToString(
                                    "F3",
                                    CultureInfo.InvariantCulture)
                                + ";"
                                + dynamicTime.ToString(
                                    "F3",
                                    CultureInfo.InvariantCulture));

                            writer.Flush();

                            Console.WriteLine(
                                "Потоки: " + threads
                                + ", запуск: " + testIndex
                                + ", готово.");
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(
                "Тестирование завершено.");

            Console.WriteLine(
                "Результаты сохранены в benchmark.csv");
        }


        private static void RunPower()
        {
            Console.WriteLine();
            Console.WriteLine("Возведение матрицы в степень");

            int size = Checkd.ReadNumberInRange(
                "Введите размер квадратной матрицы: ", 1, 200);

            int power = Checkd.ReadNumberInRange(
                "Введите натуральную степень: ", 1, 100);

            int threadCount = Checkd.ReadNumberInRange(
                "Введите количество потоков: ", 1, 1024);

            int[,] matrix = new int[size, size];
            Random random = new Random();

            int row;
            int column;

            for (row = 0; row < size; row++)
            {
                for (column = 0; column < size; column++)
                {
                    matrix[row, column] = random.Next(0, 10);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Исходная матрица:");
            PrintMatrix(matrix);

            Console.WriteLine();
            Console.WriteLine("Выполняется возведение в степень...");

            Stopwatch stopwatch = Stopwatch.StartNew();

            long[,] result = MatrixTasks.PowerParallel(
                matrix,
                power,
                threadCount);

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Результирующая матрица:");
            PrintMatrix(result);

            Console.WriteLine();
            Console.WriteLine(
                "Время выполнения: "
                + stopwatch.Elapsed.TotalMilliseconds.ToString("F3")
                + " мс");

            Console.WriteLine("Возведение в степень завершено.");
        }



        private static void RunPowerBenchmark()
        {
            Console.WriteLine();
            Console.WriteLine("Замеры возведения матрицы в степень");

            int size = Checkd.ReadNumberInRange(
                "Введите размер матрицы: ", 1, 2000);

            int power = Checkd.ReadNumberInRange(
                "Введите степень матрицы: ", 1, 100);

            int logicalCount = Environment.ProcessorCount;
            int maximumThreads = logicalCount * 2;
            int[] threadCounts = new int[maximumThreads + 2];
            int index;

            for (index = 0; index < maximumThreads; index++)
            {
                threadCounts[index] = index + 1;
            }

            threadCounts[maximumThreads] = logicalCount * 4;
            threadCounts[maximumThreads + 1] = logicalCount * 8;

            int[,] matrix = new int[size, size];
            Random random = new Random();
            int row;
            int column;

            for (row = 0; row < size; row++)
            {
                for (column = 0; column < size; column++)
                {
                    matrix[row, column] = random.Next(0, 10);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Прогрев JIT-компиляции...");

            int[,] warmupMatrix = new int[2, 2];
            warmupMatrix[0, 0] = 1;
            warmupMatrix[0, 1] = 2;
            warmupMatrix[1, 0] = 3;
            warmupMatrix[1, 1] = 4;

            MatrixTasks.PowerSequential(warmupMatrix, 2);
            MatrixTasks.PowerParallel(warmupMatrix, 2, 2, false);

            Console.WriteLine();
            Console.WriteLine("Прогрев завершён.");
            Console.WriteLine("Начинаются реальные замеры.");

            string filePath = "power_benchmark.csv";

            using (StreamWriter writer = new StreamWriter(
                filePath, false))
            {
                writer.WriteLine(
                    "Размер;Степень;Потоки;Запуск;Время;Ускорение;Эффективность;Контрольная сумма");

                Stopwatch stopwatch = new Stopwatch();
                double[] times = new double[5];
                double sequentialAverage = 0;
                double parallelAverage = 0;
                double parallelTime = 0;
                double speedup = 0;
                double efficiency = 0;
                long[,] sequentialResult = null;
                long[,] parallelResult = null;
                long checksum = 0;
                int testIndex;

                Console.WriteLine();
                Console.WriteLine("Последовательный расчёт...");

                for (testIndex = 0; testIndex < 5; testIndex++)
                {
                    stopwatch.Restart();

                    sequentialResult = MatrixTasks.PowerSequential(
                        matrix, power);

                    stopwatch.Stop();

                    times[testIndex] =
                        stopwatch.Elapsed.TotalMilliseconds;

                    Console.WriteLine(
                        "Последовательный запуск " + (testIndex + 1)
                        + ": " + times[testIndex].ToString("F3")
                        + " мс");
                }

                sequentialAverage = GetTrimmedAverage(times);
                checksum = CalculatePowerChecksum(sequentialResult);

                Console.WriteLine();
                Console.WriteLine(
                    "Среднее последовательное время: "
                    + sequentialAverage.ToString("F3") + " мс");

                for (index = 0; index < threadCounts.Length; index++)
                {
                    int threads = threadCounts[index];

                    for (testIndex = 0; testIndex < 5; testIndex++)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "Потоков: " + threads
                            + ", запуск: " + (testIndex + 1));

                        stopwatch.Restart();

                        parallelResult = MatrixTasks.PowerParallel(
                            matrix, power, threads, false);

                        stopwatch.Stop();

                        times[testIndex] =
                            stopwatch.Elapsed.TotalMilliseconds;

                        long parallelChecksum =
                            CalculatePowerChecksum(parallelResult);

                        if (parallelChecksum != checksum)
                        {
                            Console.WriteLine(
                                "ВНИМАНИЕ: контрольные суммы не совпали!");
                        }

                        Console.WriteLine(
                            "Время: " + times[testIndex].ToString("F3")
                            + " мс");
                    }

                    parallelAverage = GetTrimmedAverage(times);
                    speedup = sequentialAverage / parallelAverage;
                    efficiency = speedup / threads;

                    writer.WriteLine(
                        size + ";"
                        + power + ";"
                        + threads + ";"
                        + "Среднее;"
                        + parallelAverage.ToString(
                            "F3", CultureInfo.InvariantCulture) + ";"
                        + speedup.ToString(
                            "F6", CultureInfo.InvariantCulture) + ";"
                        + efficiency.ToString(
                            "F6", CultureInfo.InvariantCulture) + ";"
                        + checksum);

                    writer.Flush();

                    Console.WriteLine();
                    Console.WriteLine(
                        "Среднее время для " + threads
                        + " потоков: "
                        + parallelAverage.ToString("F3") + " мс");

                    Console.WriteLine(
                        "Ускорение: " + speedup.ToString("F3")
                        + "; эффективность: "
                        + efficiency.ToString("F3"));
                }
            }

            Console.WriteLine();
            Console.WriteLine("Замеры завершены.");
            Console.WriteLine(
                "Файл результатов: power_benchmark.csv");
            Console.WriteLine(
                "Вывод прогресса не включён в замеры времени.");
        }


        private static double GetTrimmedAverage(double[] times)
        {
            double sum = 0;
            double minimum = times[0];
            double maximum = times[0];
            int index;

            for (index = 0; index < times.Length; index++)
            {
                sum += times[index];

                if (times[index] < minimum)
                {
                    minimum = times[index];
                }

                if (times[index] > maximum)
                {
                    maximum = times[index];
                }
            }

            return (sum - minimum - maximum) / 3.0;
        }


        private static long CalculatePowerChecksum(long[,] matrix)
        {
            long checksum = 0;
            int row;
            int column;

            for (row = 0; row < matrix.GetLength(0); row++)
            {
                for (column = 0; column < matrix.GetLength(1); column++)
                {
                    checksum = (checksum + matrix[row, column])
                        % 1000000007L;
                }
            }

            return checksum;
        }

        private static void PrintMatrix(int[,] matrix)
        {
            int row;
            int column;

            for (row = 0; row < matrix.GetLength(0); row++)
            {
                for (column = 0; column < matrix.GetLength(1); column++)
                {
                    Console.Write(
                        matrix[row, column].ToString().PadLeft(6));
                }

                Console.WriteLine();
            }
        }

        private static void PrintMatrix(long[,] matrix)
        {
            int row;
            int column;

            for (row = 0; row < matrix.GetLength(0); row++)
            {
                for (column = 0; column < matrix.GetLength(1); column++)
                {
                    Console.Write(
                        matrix[row, column].ToString().PadLeft(12));
                }

                Console.WriteLine();
            }
        }


        private static void RunGenerator(string[] args)
        {
            int rows = 0;
            int columns = 0;

            if (args.Length != 4 ||
                !int.TryParse(args[2], out rows) ||
                !int.TryParse(args[3], out columns) ||
                rows < 1 || rows > 2000 ||
                columns < 1 || columns > 2000)
            {
                Console.WriteLine(
                    "Неверные аргументы генератора.");
                return;
            }

            string filePath = args[1];

            MatrixTasks.GenerateToFile(
                filePath,
                rows,
                columns);
        }

    }
}
