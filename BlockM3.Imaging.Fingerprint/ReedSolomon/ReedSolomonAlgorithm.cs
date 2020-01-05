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

    public enum ErrorCorrectionCodeType
    {
        QRCode,
        DataMatrix
    }

    public static class ReedSolomonAlgorithm
    {
        /// <summary>
        /// Produces error correction codewords for a message using the Reed-Solomon algorithm.
        /// </summary>
        /// <param name="message">The message to compute the error correction codewords.</param>
        /// <param name="eccCount">The number of error correction codewords desired.</param>
        /// <param name="eccType">The type of Galois field to use to encode error correction codewords.</param>
        /// <returns>Returns the computed error correction codewords.</returns>
        public static byte[] Encode(byte[] message, int eccCount, ErrorCorrectionCodeType eccType)
        {
            GenericGF galoisField;

            if (eccType == ErrorCorrectionCodeType.QRCode)
                galoisField = GenericGF.QR_CODE_FIELD_256;
            else if (eccType == ErrorCorrectionCodeType.DataMatrix)
                galoisField = GenericGF.DATA_MATRIX_FIELD_256;
            else
                throw new ArgumentException($"Invalid '{nameof(eccType)}' argument.", nameof(eccType));

            var reedSolomonEncoder = new ReedSolomonEncoder(galoisField);
            return reedSolomonEncoder.EncodeEx(message, eccCount);
        }

        /// <summary>
        /// Produces error correction codewords for a message using the Reed-Solomon algorithm.
        /// </summary>
        /// <param name="message">The message to compute the error correction codewords.</param>
        /// <param name="eccCount">The number of error correction codewords desired.</param>
        /// <returns>Returns the computed error correction codewords.</returns>
        public static byte[] Encode(byte[] message, int eccCount)
        {
            return Encode(message, eccCount, ErrorCorrectionCodeType.DataMatrix);
        }

        /// <summary>
        /// Repairs a possibly broken message using the Reed-Solomon algorithm.
        /// </summary>
        /// <param name="message">The possibly broken message to repair.</param>
        /// <param name="ecc">The available error correction codewords.</param>
        /// <param name="eccType">The type of Galois field to use to decode message.</param>
        /// <returns>Returns the repaired message, or null if it cannot be repaired.</returns>
        public static byte[] Decode(byte[] message, byte[] ecc, ErrorCorrectionCodeType eccType)
        {
            GenericGF galoisField;

            if (eccType == ErrorCorrectionCodeType.QRCode)
                galoisField = GenericGF.QR_CODE_FIELD_256;
            else if (eccType == ErrorCorrectionCodeType.DataMatrix)
                galoisField = GenericGF.DATA_MATRIX_FIELD_256;
            else
                throw new ArgumentException($"Invalid '{nameof(eccType)}' argument.", nameof(eccType));

            var reedSolomonDecoder = new ReedSolomonDecoder(galoisField);
            return reedSolomonDecoder.DecodeEx(message, ecc);
        }

        /// <summary>
        /// Repairs a possibly broken message using the Reed-Solomon algorithm.
        /// </summary>
        /// <param name="message">The possibly broken message to repair.</param>
        /// <param name="ecc">The available error correction codewords.</param>
        /// <returns>Returns the repaired message, or null if it cannot be repaired.</returns>
        public static byte[] Decode(byte[] message, byte[] ecc)
        {
            return Decode(message, ecc, ErrorCorrectionCodeType.DataMatrix);
        }
    }
}
