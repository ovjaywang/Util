using System;
using System.Text;
using System.Text.RegularExpressions;
//静态函数的方式调用矩阵
namespace Matrix2
{
    public class Matrix2
    {
        public int rows;
        public int cols;
        public double[] mat;

        public Matrix2 L;
        public Matrix2 U;
        private int[] pi;
        private double detOfP = 1;

        public Matrix2(int iRows, int iCols)         // Matrix2 Class constructor
        {
            rows = iRows;
            cols = iCols;
            mat = new double[rows * cols];
        }

        public Boolean IsSquare()
        {
            return (rows == cols);
        }

        public double this[int iRow, int iCol]      // Access this Matrix2 as a 2D array
        {
            get { return mat[iRow * cols + iCol]; }
            set { mat[iRow * cols + iCol] = value; }
        }

        public Matrix2 GetCol(int k)
        {
            Matrix2 m = new Matrix2(rows, 1);
            for (int i = 0; i < rows; i++) m[i, 0] = this[i, k];
            return m;
        }

        public void SetCol(Matrix v, int k)
        {
            for (int i = 0; i < rows; i++) this[i, k] = v[i, 0];
        }

        public void MakeLU()                        // Function for LU decomposition
        {
            if (!IsSquare()) throw new MException("The Matrix2 is not square!");
            L = IdentityMatrix(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++) pi[i] = i;

            double p = 0;
            double pom2;
            int k0 = 0;
            int pom1 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                p = 0;
                for (int i = k; i < rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }
                if (p == 0) // samé nuly ve sloupci
                    throw new MException("The Matrix2 is singular!");

                pom1 = pi[k]; pi[k] = pi[k0]; pi[k0] = pom1;    // switch two rows in permutation Matrix2

                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i]; L[k, i] = L[k0, i]; L[k0, i] = pom2;
                }

                if (k != k0) detOfP *= -1;

                for (int i = 0; i < cols; i++)                  // Switch rows in U
                {
                    pom2 = U[k, i]; U[k, i] = U[k0, i]; U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                }
            }
        }

        public Matrix2 SolveWith(Matrix v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols) throw new MException("The Matrix2 is not square!");
            if (rows != v.rows) throw new MException("Wrong number of results in solution vector!");
            if (L == null) MakeLU();

            Matrix2 b = new Matrix2(rows, 1);
            for (int i = 0; i < rows; i++) b[i, 0] = v[pi[i], 0];   // switch two items in "v" due to permutation Matrix2

            Matrix2 z = SubsForth(L, b);
            Matrix2 x = SubsBack(U, z);

            return x;
        }

        // TODO check for redundancy with MakeLU() and SolveWith()
        public void MakeRref()                                    // Function makes reduced echolon form
        {
            int lead = 0;
            for (int r = 0; r < rows; r++)
            {
                if (cols <= lead) break;
                int i = r;
                while (this[i, lead] == 0)
                {
                    i++;
                    if (i == rows)
                    {
                        i = r;
                        lead++;
                        if (cols == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }
                for (int j = 0; j < cols; j++)
                {
                    double temp = this[r, j];
                    this[r, j] = this[i, j];
                    this[i, j] = temp;
                }
                double div = this[r, lead];
                for (int j = 0; j < cols; j++) this[r, j] /= div;
                for (int j = 0; j < rows; j++)
                {
                    if (j != r)
                    {
                        double sub = this[j, lead];
                        for (int k = 0; k < cols; k++) this[j, k] -= (sub * this[r, k]);
                    }
                }
                lead++;
            }
        }

        public Matrix2 Invert()                                   // Function returns the inverted Matrix2
        {
            if (L == null) MakeLU();

            Matrix2 inv = new Matrix2(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                Matrix2 Ei = Matrix2.ZeroMatrix(rows, 1);
                Ei[i, 0] = 1;
                Matrix2 col = SolveWith(Ei);
                inv.SetCol(col, i);
            }
            return inv;
        }


        public double Det()                         // Function for determinant
        {
            if (L == null) MakeLU();
            double det = detOfP;
            for (int i = 0; i < rows; i++) det *= U[i, i];
            return det;
        }

        public Matrix2 GetP()                        // Function returns permutation Matrix2 "P" due to permutation vector "pi"
        {
            if (L == null) MakeLU();

            Matrix2 Matrix2 = ZeroMatrix(rows, cols);
            for (int i = 0; i < rows; i++) Matrix2[pi[i], i] = 1;
            return Matrix2;
        }

        public Matrix2 Duplicate()                   // Function returns the copy of this Matrix2
        {
            Matrix2 Matrix2 = new Matrix2(rows, cols);
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    Matrix2[i, j] = this[i, j];
            return Matrix2;
        }

        public static Matrix2 SubsForth(Matrix A, Matrix2 b)          // Function solves Ax = b for A as a lower triangular Matrix2
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix2 x = new Matrix2(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static Matrix2 SubsBack(Matrix A, Matrix2 b)           // Function solves Ax = b for A as an upper triangular Matrix2
        {
            if (A.L == null) A.MakeLU();
            int n = A.rows;
            Matrix2 x = new Matrix2(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--) x[i, 0] -= A[i, j] * x[j, 0];
                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static Matrix2 ZeroMatrix(int iRows, int iCols)       // Function generates the zero Matrix2
        {
            Matrix2 Matrix2 = new Matrix2(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    Matrix2[i, j] = 0;
            return Matrix2;
        }

        public static Matrix2 IdentityMatrix(int iRows, int iCols)   // Function generates the identity Matrix2
        {
            Matrix2 Matrix2 = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
                Matrix2[i, i] = 1;
            return Matrix2;
        }

        public static Matrix2 RandomMatrix(int iRows, int iCols, int dispersion)       // Function generates the random Matrix2
        {
            Random random = new Random();
            Matrix2 Matrix2 = new Matrix2(iRows, iCols);
            for (int i = 0; i < iRows; i++)
                for (int j = 0; j < iCols; j++)
                    Matrix2[i, j] = random.Next(-dispersion, dispersion);
            return Matrix2;
        }

        public static Matrix2 Parse(string ps)                        // Function parses the Matrix2 from string
        {
            string s = NormalizeMatrixString(ps);
            string[] rows = Regex.Split(s, "\r\n");
            string[] nums = rows[0].Split(' ');
            Matrix2 Matrix2 = new Matrix2(rows.Length, nums.Length);
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    nums = rows[i].Split(' ');
                    for (int j = 0; j < nums.Length; j++) Matrix2[i, j] = double.Parse(nums[j]);
                }
            }
            catch (FormatException) { throw new MException("Wrong input format!"); }
            return Matrix2;
        }

        public override string ToString()                           // Function returns Matrix2 as a string
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    s.Append(String.Format("{0,5:E2}", this[i, j]) + " ");
                s.AppendLine();
            }
            return s.ToString();
        }

        public static Matrix2 Transpose(Matrix m)              // Matrix2 transpose, for any rectangular Matrix2
        {
            Matrix2 t = new Matrix2(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    t[j, i] = m[i, j];
            return t;
        }

        public static Matrix2 Power(Matrix m, int pow)           // Power Matrix2 to exponent
        {
            if (pow == 0) return IdentityMatrix(m.rows, m.cols);
            if (pow == 1) return m.Duplicate();
            if (pow == -1) return m.Invert();

            Matrix2 x;
            if (pow < 0) { x = m.Invert(); pow *= -1; }
            else x = m.Duplicate();

            Matrix2 ret = IdentityMatrix(m.rows, m.cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1) ret *= x;
                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static void SafeAplusBintoC(Matrix A, int xa, int ya, Matrix2 B, int xb, int yb, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] += B[yb + i, xb + j];
                }
        }

        private static void SafeAminusBintoC(Matrix A, int xa, int ya, Matrix2 B, int xb, int yb, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                    if (xb + j < B.cols && yb + i < B.rows) C[i, j] -= B[yb + i, xb + j];
                }
        }

        private static void SafeACopytoC(Matrix A, int xa, int ya, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows) C[i, j] += A[ya + i, xa + j];
                }
        }

        private static void AplusBintoC(Matrix A, int xa, int ya, Matrix2 B, int xb, int yb, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
        }

        private static void AminusBintoC(Matrix A, int xa, int ya, Matrix2 B, int xb, int yb, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
        }

        private static void ACopytoC(Matrix A, int xa, int ya, Matrix2 C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
                for (int j = 0; j < size; j++) C[i, j] = A[ya + i, xa + j];
        }

        // TODO assume Matrix2 2^N x 2^N and then directly call StrassenMultiplyRun(A,B,?,1,?)
        private static Matrix2 StrassenMultiply(Matrix A, Matrix2 B)                // Smart Matrix2 multiplication
        {
            if (A.cols != B.rows) throw new MException("Wrong dimension of Matrix2!");

            Matrix2 R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            int size = 1; int n = 0;
            while (msize > size) { size *= 2; n++; };
            int h = size / 2;


            Matrix2[,] mField = new Matrix2[n, 9];

            /*
             *  8x8, 8x8, 8x8, ...
             *  4x4, 4x4, 4x4, ...
             *  2x2, 2x2, 2x2, ...
             *  . . .
             */

            int z;
            for (int i = 0; i < n - 4; i++)          // rows
            {
                z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++) mField[i, j] = new Matrix2(z, z);
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new Matrix2(A.rows, B.cols);                  // result

            /// C11
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];

            return R;
        }
        public static void StrassenMultiplyRun(Matrix A, Matrix2 B, Matrix2 C, int l, Matrix2[,] f)    // A * B into C, level of recursion, Matrix2 field
        {
            int size = A.rows;
            int h = size / 2;

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            /// C11
            for (int i = 0; i < h; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];

            /// C12
            for (int i = 0; i < h; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];

            /// C21
            for (int i = h; i < size; i++)          // rows
                for (int j = 0; j < h; j++)     // cols
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];

            /// C22
            for (int i = h; i < size; i++)          // rows
                for (int j = h; j < size; j++)     // cols
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
        }
        public static Matrix2 StupidMultiply(Matrix m1, Matrix2 m2)                  // Stupid Matrix2 multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimensions of Matrix2!");

            Matrix2 result = ZeroMatrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
                for (int j = 0; j < result.cols; j++)
                    for (int k = 0; k < m1.cols; k++)
                        result[i, j] += m1[i, k] * m2[k, j];
            return result;
        }

        public static Matrix2 Multiply(Matrix m1, Matrix2 m2)                         // Matrix2 multiplication
        {
            if (m1.cols != m2.rows) throw new MException("Wrong dimension of Matrix2!");
            int msize = Math.Max(Math.Max(m1.rows, m1.cols), Math.Max(m2.rows, m2.cols));
            // stupid multiplication faster for small matrices
            if (msize < 32)
            {
                return StupidMultiply(m1, m2);
            }
            // stupid multiplication faster for non square matrices
            if (!m1.IsSquare() || !m2.IsSquare())
            {
                return StupidMultiply(m1, m2);
            }
            // Strassen multiplication is faster for large square Matrix2 2^N x 2^N
            // NOTE because of previous checks msize == m1.cols == m1.rows == m2.cols == m2.cols
            double exponent = Math.Log(msize) / Math.Log(2);
            if (Math.Pow(2, exponent) == msize)
            {
                return StrassenMultiply(m1, m2);
            }
            else
            {
                return StupidMultiply(m1, m2);
            }
        }
        public static Matrix2 Multiply(double n, Matrix2 m)                          // Multiplication by constant n
        {
            Matrix2 r = new Matrix2(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
                for (int j = 0; j < m.cols; j++)
                    r[i, j] = m[i, j] * n;
            return r;
        }
        public static Matrix2 Add(Matrix m1, Matrix2 m2)         // Sčítání matic
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols) throw new MException("Matrices must have the same dimensions!");
            Matrix2 r = new Matrix2(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
                for (int j = 0; j < r.cols; j++)
                    r[i, j] = m1[i, j] + m2[i, j];
            return r;
        }

        public static double TR(Matrix m){
            double sum = 0.0;
            for (int i = 0; i < m.rows; i++)
                    sum += m[i, i];
        return sum;
        }
        /// <summary>
        ///求实对称矩阵特征值与特征向量的雅可比法
        /// </summary>
        /// <param name="m">求取矩阵</param>
        /// <param name="dblEigenValue">一维数组，长度为矩阵的阶数，返回时存放特征值</param>
        /// <param name="mtxEigenVector">返回时存放特征向量矩阵，其中第i列为与数组dblEigenValue中第j个特征值对应的特征向量</param>
        /// <param name="nMaxIt">迭代次数</param>
        /// <param name="eps">计算精度</param>
        /// <returns>bool型，求解是否成功</returns>
        public static bool ComputeEvJacobi(Matrix m,double[] dblEigenValue, Matrix2 mtxEigenVector, int nMaxIt, double eps)
        {
            int i, j, p = 0, q = 0, u, w, t, s, l;
            double fm, cn, sn, omega, x, y, d;
            int cols = m.cols;
            int rows = m.rows;
            if (mtxEigenVector.rows!=m.rows)
                return false;

            l = 1;
            for (i = 0; i <cols ; i++)
            {
                mtxEigenVector[i,i] = 1.0;
                for (j = 0; j < cols; j++)
                    if (i != j)
                        mtxEigenVector[i * cols , j] = 0.0;
            }

            while (true)
            {
                fm = 0.0;
                for (i = 1; i <= cols - 1; i++)
                {
                    for (j = 0; j <= i - 1; j++)
                    {
                        d = Math.Abs(m[i * cols , j]);
                        if ((i != j) && (d > fm))
                        {
                            fm = d;
                            p = i;
                            q = j;
                        }
                    }
                }

                if (fm < eps)
                {
                    for (i = 0; i < cols; ++i)
                        dblEigenValue[i] = m[i, i];
                    return true;
                }

                if (l > nMaxIt)
                    return false;

                l = l + 1;
                u = p * cols + q;
                w = p * cols + p;
                t = q * cols + p;
                s = q * cols + q;
                x = -m[p*cols,q];
                y = (m[q*cols,q] - m[p*cols,p]) / 2.0;
                omega = x / Math.Sqrt(x * x + y * y);

                if (y < 0.0)
                    omega = -omega;

                sn = 1.0 + Math.Sqrt(1.0 - omega * omega);
                sn = omega / Math.Sqrt(2.0 * sn);
                cn = Math.Sqrt(1.0 - sn * sn);
                fm = m[p * cols , p];
                m[p * cols, p] = fm * cn * cn + m[q * cols, q] * sn * sn + m[p * cols, q] * omega;
                m[q * cols, q] = fm * sn * sn + m[q * cols, q] * cn * cn - m[p * cols, q] * omega;
                m[p * cols, q] = 0.0;
                m[q * cols , p] = 0.0;
                for (j = 0; j <= cols - 1; j++)
                {
                    if ((j != p) && (j != q))
                    {
                        u = p * cols + j; w = q * cols + j;
                        fm = m[p * cols, q];
                        m[p * cols, q] = fm * cn + m[p * cols, p] * sn;
                        m[p * cols, p] = -fm * sn + m[p * cols, p] * cn;
                    }
                }

                for (i = 0; i <= cols - 1; i++)
                {
                    if ((i != p) && (i != q))
                    {
                        u = i * cols + p;
                        w = i * cols + q;
                        fm = m[p * cols, q];
                        m[p * cols, q] = fm * cn + m[p * cols, p] * sn;
                        m[p * cols, p] = -fm * sn + m[p * cols, p] * cn;
                    }
                }

                for (i = 0; i <= cols - 1; i++)
                {
                    u = i * cols + p;
                    w = i * cols + q;
                    fm = mtxEigenVector[p * cols, q];
                    mtxEigenVector[p * cols, q] = fm * cn + mtxEigenVector[p * cols, p] * sn;
                    mtxEigenVector[p * cols, p] = -fm * sn + mtxEigenVector[p * cols, p] * cn;
                }
            }
        }
        public static string NormalizeMatrixString(string matStr)	// From Andy - thank you! :)
        {
            // Remove any multiple spaces
            while (matStr.IndexOf("  ") != -1)
                matStr = matStr.Replace("  ", " ");

            // Remove any spaces before or after newlines
            matStr = matStr.Replace(" \r\n", "\r\n");
            matStr = matStr.Replace("\r\n ", "\r\n");

            // If the data ends in a newline, remove the trailing newline.
            // Make it easier by first replacing \r\n’s with |’s then
            // restore the |’s with \r\n’s
            matStr = matStr.Replace("\r\n", "|");
            while (matStr.LastIndexOf("|") == (matStr.Length - 1))
                matStr = matStr.Substring(0, matStr.Length - 1);

            matStr = matStr.Replace("|", "\r\n");
            return matStr.Trim();
        }

        //   O P E R A T O R S

        public static Matrix2 operator -(Matrix m)
        { return Matrix2.Multiply(-1, m); }

        public static Matrix2 operator +(Matrix m1, Matrix2 m2)
        { return Matrix2.Add(m1, m2); }

        public static Matrix2 operator -(Matrix m1, Matrix2 m2)
        { return Matrix2.Add(m1, -m2); }

        public static Matrix2 operator *(Matrix m1, Matrix2 m2)
        { return Matrix2.Multiply(m1, m2); }

        public static Matrix2 operator *(double n, Matrix2 m)
        { return Matrix2.Multiply(n, m); }
    }

    //  The class for exceptions

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        { }
    }
}