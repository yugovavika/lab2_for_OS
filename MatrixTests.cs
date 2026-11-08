using OSka;
using Xunit;

namespace OSka.Tests
{
    public class MatrixTests
    {
        [Fact]
        public void PowerSequential_PowerOne_ReturnsSameMatrix()
        {
            int[,] matrix =
            {
                { 1, 2 },
                { 3, 4 }
            };

            long[,] result = MatrixTasks.PowerSequential(
                matrix, 1);

            AssertMatrixEqual(matrix, result);
        }

        [Fact]
        public void PowerSequential_PowerTwo_ReturnsCorrectResult()
        {
            int[,] matrix =
            {
                { 1, 2 },
                { 3, 4 }
            };

            long[,] expected =
            {
                { 7, 10 },
                { 15, 22 }
            };

            long[,] actual = MatrixTasks.PowerSequential(
                matrix, 2);

            AssertMatrixEqual(expected, actual);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(7)]
        
        public void PowerParallel_MatchesSequential(int threads)
        {
            int[,] matrix =
            {
        { 1, 2 },
        { 3, 4 }
    };

            long[,] expected = MatrixTasks.PowerSequential(
                matrix, 3);

            int iteration;
            iteration = 0;

            while (iteration < 100)
            {
                long[,] actual = MatrixTasks.PowerParallel(
                    matrix, 3, threads, false);

                AssertMatrixEqual(expected, actual);
                iteration++;
            }
        }

        [Fact]
        public void PowerParallel_IdentityMatrix_ReturnsIdentity()
        {
            int[,] matrix =
            {
                { 1, 0 },
                { 0, 1 }
            };

            long[,] result = MatrixTasks.PowerParallel(
                matrix, 5, 3, false);

            AssertMatrixEqual(matrix, result);
        }

        private static void AssertMatrixEqual(
            int[,] expected,
            long[,] actual)
        {
            Assert.Equal(expected.GetLength(0),
                actual.GetLength(0));
            Assert.Equal(expected.GetLength(1),
                actual.GetLength(1));

            int row;
            int column;

            for (row = 0; row < expected.GetLength(0); row++)
            {
                for (column = 0;
                    column < expected.GetLength(1);
                    column++)
                {
                    Assert.Equal((long)expected[row, column],
                        actual[row, column]);
                }
            }
        }

        private static void AssertMatrixEqual(
            long[,] expected,
            long[,] actual)
        {
            Assert.Equal(expected.GetLength(0),
                actual.GetLength(0));
            Assert.Equal(expected.GetLength(1),
                actual.GetLength(1));

            int row;
            int column;

            for (row = 0; row < expected.GetLength(0); row++)
            {
                for (column = 0;
                    column < expected.GetLength(1);
                    column++)
                {
                    Assert.Equal(expected[row, column],
                        actual[row, column]);
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(7)]
        public void RunStatic_MultipliesMatricesCorrectly(int threads)
        {
            int[][] matrixA =
            {
        new int[] { 1 },
        new int[] { 2, 3 }
    };

            int[,] matrixB =
            {
        { 4, 5 },
        { 6, 7 }
    };

            StringWriter output = new StringWriter();
            TextWriter original = Console.Out;

            try
            {
                Console.SetOut(output);
                MatrixTasks.RunStatic(
                    matrixA, matrixB, threads, true);
            }
            finally
            {
                Console.SetOut(original);
            }

            Assert.Contains("Контрольная сумма: 66",
                output.ToString());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(7)]
        public void RunDynamic_MultipliesMatricesCorrectly(int threads)
        {
            int[][] matrixA =
            {
                new int[] { 1 },
                new int[] { 2, 3 }
            };

            int[,] matrixB =
            {
                { 4, 5 },
                { 6, 7 }
            };

            StringWriter output = new StringWriter();
            TextWriter original = Console.Out;

            try
            {
                Console.SetOut(output);
                MatrixTasks.RunDynamic(
                    matrixA, matrixB, threads, true);
            }
            finally
            {
                Console.SetOut(original);
            }

            Assert.Contains("Контрольная сумма: 66",
                output.ToString());
        }

        [Fact]
        public void RunStatic_InvalidThreadCount_ReturnsMinusOne()
        {
            int[][] matrixA = { new int[] { 1 } };
            int[,] matrixB = { { 2 } };

            double result = MatrixTasks.RunStatic(
                matrixA, matrixB, 0, false);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void RunDynamic_InvalidThreadCount_ReturnsMinusOne()
        {
            int[][] matrixA = { new int[] { 1 } };
            int[,] matrixB = { { 2 } };

            double result = MatrixTasks.RunDynamic(
                matrixA, matrixB, 0, false);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void RunStatic_IncompatibleDimensions_ReturnsMinusOne()
        {
            int[][] matrixA = { new int[] { 1 } };
            int[,] matrixB = { { 2 }, { 3 } };

            double result = MatrixTasks.RunStatic(
                matrixA, matrixB, 1, false);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void RunDynamic_IncompatibleDimensions_ReturnsMinusOne()
        {
            int[][] matrixA = { new int[] { 1 } };
            int[,] matrixB = { { 2 }, { 3 } };

            double result = MatrixTasks.RunDynamic(
                matrixA, matrixB, 1, false);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void RunStatic_OneElement_ReturnsCorrectChecksum()
        {
            int[][] matrixA = { new int[] { 3 } };
            int[,] matrixB = { { 4 } };

            StringWriter output = new StringWriter();
            Console.SetOut(output);

            MatrixTasks.RunStatic(matrixA, matrixB, 1, true);

            Assert.Contains("Контрольная сумма: 12",
                output.ToString());
        }

        [Fact]
        public void RunDynamic_OneElement_ReturnsCorrectChecksum()
        {
            int[][] matrixA = { new int[] { 3 } };
            int[,] matrixB = { { 4 } };

            StringWriter output = new StringWriter();
            Console.SetOut(output);

            MatrixTasks.RunDynamic(matrixA, matrixB, 1, true);

            Assert.Contains("Контрольная сумма: 12",
                output.ToString());
        }

        [Fact]
        public void PowerSequential_OneElement_ReturnsCorrectResult()
        {
            int[,] matrix = { { 2 } };
            long[,] expected = { { 8 } };

            long[,] actual = MatrixTasks.PowerSequential(
                matrix, 3);

            AssertMatrixEqual(expected, actual);
        }

        [Fact]
        public void PowerParallel_OneElement_ReturnsCorrectResult()
        {
            int[,] matrix = { { 2 } };
            long[,] expected = { { 8 } };

            long[,] actual = MatrixTasks.PowerParallel(
                matrix, 3, 2, false);

            AssertMatrixEqual(expected, actual);
        }

        [Fact]
        public void PowerSequential_ZeroMatrix_ReturnsZeroMatrix()
        {
            int[,] matrix = { { 0, 0 }, { 0, 0 } };
            long[,] expected = { { 0, 0 }, { 0, 0 } };

            long[,] actual = MatrixTasks.PowerSequential(
                matrix, 3);

            AssertMatrixEqual(expected, actual);
        }

        [Fact]
        public void PowerParallel_ZeroMatrix_ReturnsZeroMatrix()
        {
            int[,] matrix = { { 0, 0 }, { 0, 0 } };
            long[,] expected = { { 0, 0 }, { 0, 0 } };

            long[,] actual = MatrixTasks.PowerParallel(
                matrix, 3, 2, false);

            AssertMatrixEqual(expected, actual);
        }

        [Fact]
        public void GenerateToFile_ReadFromFile_WorksCorrectly()
        {
            string filePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".bin");

            try
            {
                MatrixTasks.GenerateToFile(filePath, 3, 3);

                bool result = MatrixTasks.ReadFromFile(
                    filePath, out int[][] matrixA,
                    out int[,] matrixB);

                Assert.True(result);
                Assert.Equal(3, matrixA.Length);
                Assert.Equal(3, matrixB.GetLength(0));
                Assert.Equal(3, matrixB.GetLength(1));
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        [Fact]
        public void ReadFromFile_NonexistentFile_ReturnsFalse()
        {
            string filePath = Path.Combine(
                Path.GetTempPath(),
                Guid.NewGuid().ToString() + ".bin");

            int[][] matrixA;
            int[,] matrixB;

            bool result = MatrixTasks.ReadFromFile(
                filePath, out matrixA, out matrixB);

            Assert.False(result);
        }

        [Fact]
        public void GenerateMatricesInProcess_ReturnsMatrices()
        {
            bool result = MatrixTasks.GenerateMatricesInProcess(
                3, 3, out int[][] matrixA, out int[,] matrixB);

            Assert.True(result);
            Assert.Equal(3, matrixA.Length);
            Assert.Equal(3, matrixB.GetLength(0));
            Assert.Equal(3, matrixB.GetLength(1));
        }
    }


}
