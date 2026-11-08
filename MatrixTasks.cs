using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace OSka
{
    internal static class MatrixTasks
    {
        private static int completedRows;
        private static int lastPercent;
        private static object progressLock;

        static MatrixTasks()
        {
            completedRows = 0;
            lastPercent = 0;
            progressLock = new object();
        }

        private class WorkerState
        {
            public int[,] First;
            public int[,] Second;
            public long[,] Result;
            public int StartRow;
            public int EndRow;
            public int TotalRows;

            public WorkerState(
                int[,] first,
                int[,] second,
                long[,] result,
                int startRow,
                int endRow,
                int totalRows)
            {
                First = first;
                Second = second;
                Result = result;
                StartRow = startRow;
                EndRow = endRow;
                TotalRows = totalRows;
            }
        }

        private class StaticWorkerState
        {
            public int[][] First;
            public int[,] Second;
            public long[,] Result;
            public int StartRow;
            public int EndRow;
            public int TotalRows;
            public bool ShowOutput;

            public StaticWorkerState(
            int[][] first,
            int[,] second,
            long[,] result,
            int startRow,
            int endRow,
            int totalRows,
            bool showOutput)
            {
                First = first;
                Second = second;
                Result = result;
                StartRow = startRow;
                EndRow = endRow;
                TotalRows = totalRows;
                ShowOutput = showOutput;
            }
        }

        private class DynamicWorkState
        {
            public int[][] First;
            public int[,] Second;
            public long[,] Result;
            public int TotalRows;
            public int NextRow;
            public bool ShowOutput;

            public DynamicWorkState(
                int[][] first,
                int[,] second,
                long[,] result,
                int totalRows,
                bool showOutput)
            {
                First = first;
                Second = second;
                Result = result;
                TotalRows = totalRows;
                NextRow = -1;
                ShowOutput = showOutput;
            }
        }

        public static void GenerateToFile(
            string filePath,
            int rows,
            int columns)
        {
            Random random = new Random();

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (BinaryWriter writer = new BinaryWriter(
                File.Create(filePath)))
            {
                writer.Write(rows);
                writer.Write(columns);

                int row;
                int column;

                for (row = 0; row < rows; row++)
                {
                    for (column = 0; column <= row; column++)
                    {
                        writer.Write(random.Next(0, 10));
                    }
                }

                for (row = 0; row < rows; row++)
                {
                    for (column = 0; column < columns; column++)
                    {
                        writer.Write(random.Next(0, 10));
                    }
                }
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine(
                    "Ошибка: файл не был создан.");
                return;
            }

            Console.WriteLine("Матрицы записаны в файл.");
        }


        public static bool ReadFromFile(
    string filePath,
    out int[][] matrixA,
    out int[,] matrixB)
        {
            matrixA = new int[0][];
            matrixB = new int[0, 0];

            if (!File.Exists(filePath))
            {
                Console.WriteLine(
                    "Ошибка: файл не найден.");
                return false;
            }

            try
            {
                using (BinaryReader reader = new BinaryReader(
                    File.OpenRead(filePath)))
                {
                    int rows = reader.ReadInt32();
                    int columns = reader.ReadInt32();

                    if (rows < 1 || rows > 2000 ||
                        columns < 1 || columns > 2000)
                    {
                        Console.WriteLine(
                            "Ошибка: неверные размеры матриц.");
                        return false;
                    }

                    matrixA = new int[rows][];
                    matrixB = new int[rows, columns];

                    int row;
                    int column;

                    for (row = 0; row < rows; row++)
                    {
                        matrixA[row] = new int[row + 1];

                        for (column = 0;
                            column <= row;
                            column++)
                        {
                            matrixA[row][column] =
                                reader.ReadInt32();
                        }
                    }

                    for (row = 0; row < rows; row++)
                    {
                        for (column = 0;
                            column < columns;
                            column++)
                        {
                            matrixB[row, column] =
                                reader.ReadInt32();
                        }
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine(
                    "Ошибка при чтении файла.");
                return false;
            }
            catch (ArgumentException)
            {
                Console.WriteLine(
                    "Ошибка в имени или пути файла.");
                return false;
            }

            Console.WriteLine(
                "Матрицы успешно прочитаны из файла.");

            return true;
        }

        public static void PrintMatrixForCheck(int[][] matrix)
        {
            PrintMatrix(matrix);
        }

        public static void PrintMatrixForCheck(int[,] matrix)
        {
            PrintMatrix(matrix);
        }

        private static void PrintMatrix(int[][] matrix)
        {
            int row;
            int column;

            for (row = 0; row < matrix.Length; row++)
            {
                for (column = 0;
                    column < matrix[row].Length;
                    column++)
                {
                    Console.Write(
                        matrix[row][column].ToString().PadLeft(6));
                }

                Console.WriteLine();
            }
        }

        public static bool GenerateMatricesInProcess(
            int rows,
            int columns,
            out int[][] matrixA,
            out int[,] matrixB)
        {
            matrixA = new int[0][];
            matrixB = new int[0, 0];

            string filePath = Path.Combine(
                Path.GetTempPath(),
                "matrices_" + Guid.NewGuid().ToString()
                + ".bin");

            string? executablePath = Environment.ProcessPath;

            if (string.IsNullOrEmpty(executablePath))
            {
                Console.WriteLine(
                    "Не удалось определить путь программы.");
                return false;
            }

            try
            {
                ProcessStartInfo startInfo =
                    new ProcessStartInfo();

                startInfo.FileName = executablePath;
                startInfo.UseShellExecute = false;

                startInfo.ArgumentList.Add("--generate");
                startInfo.ArgumentList.Add(filePath);
                startInfo.ArgumentList.Add(rows.ToString());
                startInfo.ArgumentList.Add(columns.ToString());

                using (Process childProcess = new Process())
                {
                    childProcess.StartInfo = startInfo;
                    childProcess.Start();
                    childProcess.WaitForExit();

                    if (childProcess.ExitCode != 0)
                    {
                        Console.WriteLine(
                            "Ошибка дочернего процесса.");
                        return false;
                    }
                }

                return ReadFromFile(
                    filePath,
                    out matrixA,
                    out matrixB);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.WriteLine(
                    "Не удалось запустить дочерний процесс.");
                return false;
            }
            catch (IOException)
            {
                Console.WriteLine(
                    "Ошибка работы с файлом.");
                return false;
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        public static double RunStatic(
        int[][] matrixA,
        int[,] matrixB,
        int threadCount,
        bool showOutput)
        {
            int rows = matrixA.Length;
            int columns = matrixB.GetLength(1);

            if (rows != matrixB.GetLength(0) ||
                threadCount < 1)
            {
                Console.WriteLine(
                    "Ошибка: неверные размеры матриц.");
                return -1;
            }

            long[,] result = new long[rows, columns];

            Thread[] threads = new Thread[threadCount];

            StaticWorkerState[] states =
                new StaticWorkerState[threadCount];

            int threadIndex;
            int startRow;
            int endRow;

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                startRow = threadIndex * rows / threadCount;

                endRow =
                    (threadIndex + 1) * rows / threadCount;

                states[threadIndex] = new StaticWorkerState(
                matrixA,
                matrixB,
                result,
                startRow,
                endRow,
                rows,
                showOutput);

                threads[threadIndex] = new Thread(
                    CalculateStaticWorker);
            }

            Interlocked.Exchange(ref completedRows, 0);

            lock (progressLock)
            {
                lastPercent = 0;
            }

            if (showOutput)
            {
                Console.WriteLine();
                Console.WriteLine("Статическое распределение");
                Console.WriteLine("Прогресс: 0%");
            }

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex].Start(
                    states[threadIndex]);
            }

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex].Join();
            }

            stopwatch.Stop();

            if (showOutput)
            {
                Console.WriteLine();
                Console.WriteLine("Умножение завершено.");

                Console.WriteLine(
                $"Время выполнения: "
                + $"{stopwatch.Elapsed.TotalMilliseconds.ToString("F3")} мс");

                Console.WriteLine(
                    "Контрольная сумма: "
                    + CalculateChecksum(result));
            }

            if (showOutput && rows <= 10 && columns <= 10)
            {
                Console.WriteLine();
                Console.WriteLine("Матрица C:");
                PrintMatrix(result);
            }

            return stopwatch.Elapsed.TotalMilliseconds;
        }



        private static void CalculateJaggedRows(
    int[][] first,
    int[,] second,
    long[,] result,
    int row)
        {
            int column;
            int index;

            for (index = 0;
                index < first[row].Length;
                index++)
            {
                for (column = 0;
                    column < second.GetLength(1);
                    column++)
                {
                    result[row, column] +=
                        (long)first[row][index]
                        * second[index, column];
                }
            }
        }

        private static void CalculateStaticWorker(object state)
        {
            StaticWorkerState data = (StaticWorkerState)state;

            int row;
            int completed = 0;
            int percent = 0;

            for (row = data.StartRow;
                row < data.EndRow;
                row++)
            {
                CalculateJaggedRows(
                    data.First,
                    data.Second,
                    data.Result,
                    row);

                completed = Interlocked.Increment(
                    ref completedRows);

                percent = completed * 100 / data.TotalRows;

                if (data.ShowOutput)
                {
                    lock (progressLock)
                    {
                        if (percent > lastPercent)
                        {
                            lastPercent = percent;

                            Console.WriteLine(
                                "Прогресс: " + percent + "%");
                        }
                    }
                }
            }
        }

        private static void PrintChecksum(long[,] matrix)
        {
            long checksum = 0;
            int row = 0;
            int column = 0;

            for (row = 0; row < matrix.GetLength(0); row++)
            {
                for (column = 0;
                    column < matrix.GetLength(1);
                    column++)
                {
                    checksum += matrix[row, column];
                }
            }

            Console.WriteLine(
                "Контрольная сумма: " + checksum);
        }

        public static double RunDynamic(
    int[][] matrixA,
    int[,] matrixB,
    int threadCount,
    bool showOutput)
        {
            int rows = matrixA.Length;
            int columns = matrixB.GetLength(1);

            if (rows != matrixB.GetLength(0) ||
                threadCount < 1)
            {
                Console.WriteLine(
                    "Ошибка: неверные размеры матриц.");
                return -1;
            }

            long[,] result = new long[rows, columns];

            DynamicWorkState data = new DynamicWorkState(
                matrixA,
                matrixB,
                result,
                rows,
                showOutput);

            Thread[] threads = new Thread[threadCount];

            Interlocked.Exchange(ref completedRows, 0);

            lock (progressLock)
            {
                lastPercent = 0;
            }

            if (showOutput)
            {
                Console.WriteLine();
                Console.WriteLine("Динамическое распределение");
                Console.WriteLine("Прогресс: 0%");
            }

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            int threadIndex;

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex] = new Thread(
                    CalculateDynamicWorker);

                threads[threadIndex].Start(data);
            }

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex].Join();
            }

            stopwatch.Stop();

            if (showOutput)
            {
                Console.WriteLine();
                Console.WriteLine("Умножение завершено.");

                Console.WriteLine(
    $"Время выполнения: "
    + $"{stopwatch.Elapsed.TotalMilliseconds.ToString("F3")} мс");

                PrintChecksum(result);
            }

            if (showOutput && rows <= 10 && columns <= 10)
            {
                Console.WriteLine();
                Console.WriteLine("Матрица C:");
                PrintMatrix(result);
            }

            return stopwatch.Elapsed.TotalMilliseconds;
        }

        private static void CalculateDynamicWorker(object state)
        {
            DynamicWorkState data = (DynamicWorkState)state;

            int row = 0;
            int completed = 0;
            int percent = 0;

            while (true)
            {
                row = Interlocked.Increment(
                    ref data.NextRow);

                if (row >= data.TotalRows)
                {
                    break;
                }

                CalculateJaggedRows(
                    data.First,
                    data.Second,
                    data.Result,
                    row);

                completed = Interlocked.Increment(
                    ref completedRows);

                percent = completed * 100 / data.TotalRows;

                if (data.ShowOutput)
                {
                    lock (progressLock)
                    {
                        if (percent > lastPercent)
                        {
                            lastPercent = percent;

                            Console.WriteLine(
                                "Прогресс: " + percent + "%");
                        }
                    }
                }
            }
        }

        public static void Run(
            int rows,
            int common,
            int columns,
            int threadCount)
        {
            int[,] matrixA = new int[rows, common];
            int[,] matrixB = new int[common, columns];

            long[,] result = new long[rows, columns];

            Random random = new Random();

            FillMatrix(matrixA, random);
            FillMatrix(matrixB, random);

            Console.WriteLine("Матрицы созданы.");
            Console.WriteLine();

            if (rows <= 10
                && common <= 10
                && columns <= 10)
            {
                Console.WriteLine("Матрица A:");
                PrintMatrix(matrixA);

                Console.WriteLine();
                Console.WriteLine("Матрица B:");
                PrintMatrix(matrixB);
            }

            Thread[] threads = new Thread[threadCount];

            WorkerState[] states =
                new WorkerState[threadCount];

            int threadIndex;
            int startRow;
            int endRow;

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                startRow = threadIndex * rows / threadCount;

                endRow =
                    (threadIndex + 1) * rows / threadCount;

                states[threadIndex] = new WorkerState(
                    matrixA,
                    matrixB,
                    result,
                    startRow,
                    endRow,
                    rows);

                threads[threadIndex] = new Thread(
                    CalculateWorker);
            }

            Interlocked.Exchange(ref completedRows, 0);

            lock (progressLock)
            {
                lastPercent = 0;
            }

            Console.WriteLine();
            Console.WriteLine("Прогресс: 0%");

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex].Start(
                    states[threadIndex]);
            }

            for (threadIndex = 0;
                threadIndex < threadCount;
                threadIndex++)
            {
                threads[threadIndex].Join();
            }

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine("Умножение завершено.");

            Console.WriteLine(
                "Время: "
                + stopwatch.ElapsedMilliseconds
                + " мс");

            Console.WriteLine(
                "Контрольная сумма: "
                + CalculateChecksum(result));

            if (rows <= 10 && columns <= 10)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Результирующая матрица C:");

                PrintMatrix(result);
            }

            Console.WriteLine();
            Console.WriteLine("Программа завершена.");
        }

        private static void FillMatrix(
            int[,] matrix,
            Random random)
        {
            int row;
            int column;

            for (row = 0;
                row < matrix.GetLength(0);
                row++)
            {
                for (column = 0;
                    column < matrix.GetLength(1);
                    column++)
                {
                    matrix[row, column] = random.Next(0, 10);
                }
            }
        }

        private static void PrintMatrix(int[,] matrix)
        {
            int row;
            int column;

            for (row = 0;
                row < matrix.GetLength(0);
                row++)
            {
                for (column = 0;
                    column < matrix.GetLength(1);
                    column++)
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

            for (row = 0;
                row < matrix.GetLength(0);
                row++)
            {
                for (column = 0;
                    column < matrix.GetLength(1);
                    column++)
                {
                    Console.Write(
                        matrix[row, column].ToString().PadLeft(6));
                }

                Console.WriteLine();
            }
        }

        private static void CalculateWorker(object state)
        {
            WorkerState data = (WorkerState)state;

            int row;

            for (row = data.StartRow;
                row < data.EndRow;
                row++)
            {
                CalculateRows(
                    data.First,
                    data.Second,
                    data.Result,
                    row,
                    row + 1);

                int completed = Interlocked.Increment(
                    ref completedRows);

                int percent =
                    completed * 100 / data.TotalRows;

                lock (progressLock)
                {
                    if (percent > lastPercent)
                    {
                        lastPercent = percent;

                        Console.WriteLine(
                            "Прогресс: " + percent + "%");
                    }
                }
            }
        }

        private static void CalculateRows(
            int[,] first,
            int[,] second,
            long[,] result,
            int startRow,
            int endRow)
        {
            int row;
            int column;
            int index;

            for (row = startRow;
                row < endRow;
                row++)
            {
                for (index = 0;
                    index < first.GetLength(1);
                    index++)
                {
                    for (column = 0;
                        column < second.GetLength(1);
                        column++)
                    {
                        result[row, column] +=
                            (long)first[row, index]
                            * second[index, column];
                    }
                }
            }
        }

        private static long CalculateChecksum(long[,] matrix)
        {
            long checksum;
            int row;
            int column;

            checksum = 0;

            for (row = 0;
                row < matrix.GetLength(0);
                row++)
            {
                for (column = 0;
                    column < matrix.GetLength(1);
                    column++)
                {
                    checksum += matrix[row, column];
                }
            }

            return checksum;
        }
        private const long Modulo = 1000000007L;

        private class PowerBuffers
        {
            public long[,] Current;
            public long[,] Next;

            public PowerBuffers(long[,] initial, int size)
            {
                Current = initial;
                Next = new long[size, size];
            }

            public void Swap()
            {
                long[,] temporary = Current;
                Current = Next;
                Next = temporary;
            }
        }

        private class PowerWorkerState
        {
            public PowerBuffers Buffers;
            public long[,] Original;
            public Barrier SyncBarrier;
            public int StartRow;
            public int EndRow;
            public int Size;
            public int Power;
            public int TotalRows;
            public int CompletedRows;
            public int LastPercent;
            public object ProgressLock;
            public bool ShowProgress;

            public PowerWorkerState(
                PowerBuffers buffers,
                long[,] original,
                Barrier syncBarrier,
                int startRow,
                int endRow,
                int size,
                int power,
                int totalRows,
                object progressLock,
                bool showProgress)
            {
                Buffers = buffers;
                Original = original;
                SyncBarrier = syncBarrier;
                StartRow = startRow;
                EndRow = endRow;
                Size = size;
                Power = power;
                TotalRows = totalRows;
                CompletedRows = 0;
                LastPercent = 0;
                ProgressLock = progressLock;
                ShowProgress = showProgress;
            }
        }

        public static long[,] PowerSequential(
            int[,] matrix,
            int power)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            if (rows != columns)
            {
                throw new ArgumentException(
                    "Матрица должна быть квадратной.");
            }

            if (power < 1)
            {
                throw new ArgumentException(
                    "Степень должна быть натуральным числом.");
            }

            long[,] result = new long[rows, rows];
            long[,] original = new long[rows, rows];
            long[,] current = new long[rows, rows];
            long[,] next = new long[rows, rows];

            int row;
            int column;
            int index;

            for (row = 0; row < rows; row++)
            {
                for (column = 0; column < rows; column++)
                {
                    original[row, column] = Normalize(
                        matrix[row, column]);
                    current[row, column] = original[row, column];
                }
            }

            int step;

            for (step = 1; step < power; step++)
            {
                for (row = 0; row < rows; row++)
                {
                    for (column = 0; column < rows; column++)
                    {
                        next[row, column] = 0;

                        for (index = 0; index < rows; index++)
                        {
                            next[row, column] =
                                (next[row, column]
                                + current[row, index]
                                * original[index, column])
                                % Modulo;
                        }
                    }
                }

                long[,] temporary = current;
                current = next;
                next = temporary;
            }

            for (row = 0; row < rows; row++)
            {
                for (column = 0; column < rows; column++)
                {
                    result[row, column] = current[row, column];
                }
            }

            return result;
        }

        public static long[,] PowerParallel(
            int[,] matrix,
            int power,
            int threadCount,
            bool showProgress = true)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(nameof(matrix));
            }

            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            if (rows != columns)
            {
                throw new ArgumentException(
                    "Матрица должна быть квадратной.");
            }

            if (power < 1)
            {
                throw new ArgumentException(
                    "Степень должна быть натуральным числом.");
            }

            if (threadCount < 1)
            {
                throw new ArgumentException(
                    "Число потоков должно быть положительным.");
            }

            long[,] initial = new long[rows, rows];
            long[,] original = new long[rows, rows];
            int row;
            int column;

            for (row = 0; row < rows; row++)
            {
                for (column = 0; column < rows; column++)
                {
                    original[row, column] = Normalize(
                        matrix[row, column]);
                    initial[row, column] = original[row, column];
                }
            }

            if (power == 1 || rows == 0)
            {
                return initial;
            }

            PowerBuffers buffers = new PowerBuffers(initial, rows);
            object progressLockLocal = new object();
            int totalRows = (power - 1) * rows;
            Interlocked.Exchange(ref completedRows, 0);

            lock (progressLock)
            {
                lastPercent = 0;
            }

            Thread[] threads = new Thread[threadCount];
            PowerWorkerState[] states =
                new PowerWorkerState[threadCount];

            using (Barrier syncBarrier = new Barrier(
                threadCount,
                delegate(Barrier barrier)
                {
                    buffers.Swap();
                }))
            {
                int threadIndex;
                int startRow;
                int endRow;

                for (threadIndex = 0;
                    threadIndex < threadCount;
                    threadIndex++)
                {
                    startRow = threadIndex * rows / threadCount;
                    endRow = (threadIndex + 1) * rows / threadCount;

                    states[threadIndex] = new PowerWorkerState(
                        buffers,
                        original,
                        syncBarrier,
                        startRow,
                        endRow,
                        rows,
                        power,
                        totalRows,
                        progressLockLocal,
                        showProgress);

                    threads[threadIndex] = new Thread(
                        CalculatePowerWorker);
                }

                if (showProgress)
                {
                    Console.WriteLine(
                        "Прогресс возведения в степень: 0%");
                }

                for (threadIndex = 0;
                    threadIndex < threadCount;
                    threadIndex++)
                {
                    threads[threadIndex].Start(states[threadIndex]);
                }

                for (threadIndex = 0;
                    threadIndex < threadCount;
                    threadIndex++)
                {
                    threads[threadIndex].Join();
                }
            }

            return buffers.Current;
        }

        private static void CalculatePowerWorker(object state)
        {
            PowerWorkerState data = (PowerWorkerState)state;
            int step;
            int row;
            int column;
            int index;
            long sum;
            int completed;
            int percent;

            for (step = 1; step < data.Power; step++)
            {
                for (row = data.StartRow; row < data.EndRow; row++)
                {
                    for (column = 0; column < data.Size; column++)
                    {
                        sum = 0;

                        for (index = 0; index < data.Size; index++)
                        {
                            sum = (sum
                                + data.Buffers.Current[row, index]
                                * data.Original[index, column])
                                % Modulo;
                        }

                        data.Buffers.Next[row, column] = sum;
                    }

                    if (data.ShowProgress)
                    {
                        completed = Interlocked.Increment(
                            ref completedRows);
                        percent = completed * 100 / data.TotalRows;

                        lock (progressLock)
                        {
                            if (percent > lastPercent)
                            {
                                lastPercent = percent;
                                Console.WriteLine(
                                    "Прогресс возведения в степень: "
                                    + percent + "%");
                            }
                        }
                    }
                }

                data.SyncBarrier.SignalAndWait();
            }
        }

        private static long Normalize(long value)
        {
            long result = value % Modulo;

            if (result < 0)
            {
                result += Modulo;
            }

            return result;
        }

    }
}