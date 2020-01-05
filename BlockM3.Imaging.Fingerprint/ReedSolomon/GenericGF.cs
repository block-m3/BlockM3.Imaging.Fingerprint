using System;

namespace BlockM3.Imaging.Fingerprint.ReedSolomon
{
    /*
     * Copyright 2007 ZXing authors
     *
     * Licensed under the Apache License, Version 2.0 (the "License");
     * you may not use this file except in compliance with the License.
     * You may obtain a copy of the License at
     *
     *      http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    internal sealed class GenericGF
    {
        //public static GenericGF AZTEC_DATA_8 = DATA_MATRIX_FIELD_256;
        //public static GenericGF MAXICODE_FIELD_64 = AZTEC_DATA_6;

        private const int INITIALIZATION_THRESHOLD = 0;

        //public static GenericGF AZTEC_DATA_12 = new GenericGF(0x1069, 4096, 1); // x^12 + x^6 + x^5 + x^3 + 1
        //public static GenericGF AZTEC_DATA_10 = new GenericGF(0x409, 1024, 1); // x^10 + x^3 + 1
        //public static GenericGF AZTEC_DATA_6 = new GenericGF(0x43, 64, 1); // x^6 + x + 1
        //public static GenericGF AZTEC_PARAM = new GenericGF(0x13, 16, 1); // x^4 + x + 1
        public static GenericGF QR_CODE_FIELD_256 = new GenericGF(0x011D, 256, 0); // x^8 + x^4 + x^3 + x^2 + 1
        public static GenericGF DATA_MATRIX_FIELD_256 = new GenericGF(0x012D, 256, 1); // x^8 + x^5 + x^3 + x^2 + 1
        private readonly int primitive;

        private int[] expTable;
        private bool initialized;
        private int[] logTable;
        private GenericGFPoly one;
        private GenericGFPoly zero;

        /// <summary>
        ///     Create a representation of GF(size) using the given primitive polynomial.
        /// </summary>
        /// <param name="primitive">
        ///     irreducible polynomial whose coefficients are represented by
        ///     *  the bits of an int, where the least-significant bit represents the constant
        ///     *  coefficient
        /// </param>
        /// <param name="size">the size of the field</param>
        /// <param name="genBase">
        ///     the factor b in the generator polynomial can be 0- or 1-based
        ///     *  (g(x) = (x+a^b)(x+a^(b+1))...(x+a^(b+2t-1))).
        ///     *  In most cases it should be 1, but for QR code it is 0.
        /// </param>
        public GenericGF(int primitive, int size, int genBase)
        {
            this.primitive = primitive;
            Size = size;
            GeneratorBase = genBase;

            if (size <= INITIALIZATION_THRESHOLD)
            {
                Initialize();
            }
        }

        internal GenericGFPoly Zero
        {
            get
            {
                CheckInit();
                return zero;
            }
        }

        internal GenericGFPoly One
        {
            get
            {
                CheckInit();
                return one;
            }
        }

        /// <summary>
        ///     Gets the size.
        /// </summary>
        public int Size { get; }

        /// <summary>
        ///     Gets the generator base.
        /// </summary>
        public int GeneratorBase { get; }

        private void Initialize()
        {
            expTable = new int[Size];
            logTable = new int[Size];

            int x = 1;

            for (int i = 0; i < Size; i++)
            {
                expTable[i] = x;
                x <<= 1; // x = x * 2; we're assuming the generator alpha is 2
                if (x >= Size)
                {
                    x ^= primitive;
                    x &= Size - 1;
                }
            }

            for (int i = 0; i < Size - 1; i++)
                logTable[expTable[i]] = i;

            // logTable[0] == 0 but this should never be used
            zero = new GenericGFPoly(this, new [] {0});
            one = new GenericGFPoly(this, new [] {1});

            initialized = true;
        }

        private void CheckInit()
        {
            if (initialized == false)
                Initialize();
        }

        /// <summary>
        ///     Builds the monomial.
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <param name="coefficient">The coefficient.</param>
        /// <returns>the monomial representing coefficient * x^degree</returns>
        internal GenericGFPoly BuildMonomial(int degree, int coefficient)
        {
            CheckInit();

            if (degree < 0)
                throw new ArgumentException();

            if (coefficient == 0)
                return zero;

            var coefficients = new int[degree + 1];
            coefficients[0] = coefficient;

            return new GenericGFPoly(this, coefficients);
        }

        /// <summary>
        ///     Implements both addition and subtraction -- they are the same in GF(size).
        /// </summary>
        /// <returns>sum/difference of a and b</returns>
        internal static int AddOrSubtract(int a, int b)
        {
            return a ^ b;
        }

        /// <summary>
        ///     Exps the specified a.
        /// </summary>
        /// <returns>2 to the power of a in GF(size)</returns>
        internal int Exp(int a)
        {
            CheckInit();
            return expTable[a];
        }

        /// <summary>
        ///     Logs the specified a.
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>base 2 log of a in GF(size)</returns>
        internal int Log(int a)
        {
            CheckInit();

            if (a == 0)
                throw new ArgumentException();

            return logTable[a];
        }

        /// <summary>
        ///     Inverses the specified a.
        /// </summary>
        /// <returns>multiplicative inverse of a</returns>
        internal int Inverse(int a)
        {
            CheckInit();

            if (a == 0)
                throw new ArithmeticException();

            return expTable[Size - logTable[a] - 1];
        }

        /// <summary>
        ///     Multiplies the specified a with b.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>product of a and b in GF(size)</returns>
        internal int Multiply(int a, int b)
        {
            CheckInit();

            if (a == 0 || b == 0)
                return 0;

            return expTable[(logTable[a] + logTable[b]) % (Size - 1)];
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"GF(0x{primitive.ToString("X")},{Size})";
    }
}