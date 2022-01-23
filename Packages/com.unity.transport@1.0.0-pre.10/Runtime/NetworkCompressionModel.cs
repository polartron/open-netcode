using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Unity.Networking.Transport
{
    /// <summary>
    /// Used to provide Huffman compression when using packed DataStream functions
    /// </summary>
    public unsafe struct NetworkCompressionModel : IDisposable
    {
        internal static readonly byte[] k_BucketSizes =
        {
            0, 0, 1, 2, 3, 4, 6, 8, 10, 12, 15, 18, 21, 24, 27, 32
        };

        internal static readonly uint[] k_BucketOffsets =
        {
            0, 1, 2, 4, 8, 16, 32, 96, 352, 1376, 5472, 38240, 300384, 2397536, 19174752, 153392480
        };
        internal static readonly int[] k_FirstBucketCandidate =
        {
            // 0   1   2   3   4   5   6   7   8   9   10  11  12  13  14  15  16  17  18 19 20 21 22 23 24 25 26 27 28 29 30 31 32
            15, 15, 15, 15, 14, 14, 14, 13, 13, 13, 12, 12, 12, 11, 11, 11, 10, 10, 10, 9, 9, 8, 8, 7, 7, 6, 5, 4, 3, 2, 1, 1, 0
        };
        internal static readonly byte[] k_DefaultModelData = { 16, // 16 symbols
                                                               2, 3, 3, 3,   4, 4, 4, 5,     5, 5, 6, 6,     6, 6, 6, 6,
                                                               0, 0 }; // no contexts
        internal const int k_AlphabetSize = 16;
        internal const int k_MaxHuffmanSymbolLength = 6;
        internal const int k_MaxContexts = 1;

        /// <summary>
        /// Disposes this instance
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkCompressionModel"/> class
        /// </summary>
        /// <param name="allocator">The allocator</param>
        public NetworkCompressionModel(Allocator allocator)
        {
            for (int i = 0; i < k_AlphabetSize; ++i)
            {
                bucketSizes[i] = k_BucketSizes[i];
                bucketOffsets[i] = k_BucketOffsets[i];
            }
            byte[] modelData = k_DefaultModelData;

            //int numContexts = NetworkConfig.maxContexts;
            int numContexts = 1;
            byte[,] symbolLengths = new byte[numContexts, k_AlphabetSize];

            int readOffset = 0;
            {
                // default model
                int defaultModelAlphabetSize = modelData[readOffset++];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (defaultModelAlphabetSize != k_AlphabetSize)
                    throw new InvalidOperationException("The alphabet size of compression models must be " + k_AlphabetSize);
#endif

                for (int i = 0; i < k_AlphabetSize; i++)
                {
                    byte length = modelData[readOffset++];
                    for (int context = 0; context < numContexts; context++)
                    {
                        symbolLengths[context, i] = length;
                    }
                }

                // other models
                int numModels = modelData[readOffset] | (modelData[readOffset + 1] << 8);
                readOffset += 2;
                for (int model = 0; model < numModels; model++)
                {
                    int context = modelData[readOffset] | (modelData[readOffset + 1] << 8);
                    readOffset += 2;

                    int modelAlphabetSize = modelData[readOffset++];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (modelAlphabetSize != k_AlphabetSize)
                        throw new InvalidOperationException("The alphabet size of compression models must be " + k_AlphabetSize);
#endif
                    for (int i = 0; i < k_AlphabetSize; i++)
                    {
                        byte length = modelData[readOffset++];
                        symbolLengths[context, i] = length;
                    }
                }
            }

            // generate tables
            var tmpSymbolLengths = new byte[k_AlphabetSize];
            var tmpSymbolDecodeTable = new ushort[1 << k_MaxHuffmanSymbolLength];
            var symbolCodes = new byte[k_AlphabetSize];

            for (int context = 0; context < numContexts; context++)
            {
                for (int i = 0; i < k_AlphabetSize; i++)
                    tmpSymbolLengths[i] = symbolLengths[context, i];

                GenerateHuffmanCodes(symbolCodes, 0, tmpSymbolLengths, 0, k_AlphabetSize, k_MaxHuffmanSymbolLength);
                GenerateHuffmanDecodeTable(tmpSymbolDecodeTable, 0, tmpSymbolLengths, symbolCodes, k_AlphabetSize, k_MaxHuffmanSymbolLength);
                for (int i = 0; i < k_AlphabetSize; i++)
                {
                    encodeTable[context * k_AlphabetSize + i] = (ushort)((symbolCodes[i] << 8) | symbolLengths[context, i]);
                }
                for (int i = 0; i < (1 << k_MaxHuffmanSymbolLength); i++)
                {
                    decodeTable[context * (1 << k_MaxHuffmanSymbolLength) + i] = tmpSymbolDecodeTable[i];
                }
            }
        }

        private static void GenerateHuffmanCodes(byte[] symboLCodes, int symbolCodesOffset, byte[] symbolLengths, int symbolLengthsOffset, int alphabetSize, int maxCodeLength)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (alphabetSize > 256 || maxCodeLength > 8)
                throw new InvalidOperationException("Can only generate huffman codes up to alphabet size 256 and maximum code length 8");
#endif

            var lengthCounts = new byte[maxCodeLength + 1];
            var symbolList = new byte[maxCodeLength + 1, alphabetSize];

            //byte[] symbol_list[(MAX_HUFFMAN_CODE_LENGTH + 1u) * MAX_NUM_HUFFMAN_SYMBOLS];
            for (int symbol = 0; symbol < alphabetSize; symbol++)
            {
                int length = symbolLengths[symbol + symbolLengthsOffset];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (length > maxCodeLength)
                    throw new InvalidOperationException("Maximum code length exceeded");
#endif
                symbolList[length, lengthCounts[length]++] = (byte)symbol;
            }

            uint nextCodeWord = 0;
            for (int length = 1; length <= maxCodeLength; length++)
            {
                int length_count = lengthCounts[length];
                for (int i = 0; i < length_count; i++)
                {
                    int symbol = symbolList[length, i];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    if (symbolLengths[symbol + symbolLengthsOffset] != length)
                        throw new InvalidOperationException("Incorrect symbol length");
#endif
                    symboLCodes[symbol + symbolCodesOffset] = (byte)ReverseBits(nextCodeWord++, length);
                }
                nextCodeWord <<= 1;
            }
        }

        private static uint ReverseBits(uint value, int num_bits)
        {
            value = ((value & 0x55555555u) << 1) | ((value & 0xAAAAAAAAu) >> 1);
            value = ((value & 0x33333333u) << 2) | ((value & 0xCCCCCCCCu) >> 2);
            value = ((value & 0x0F0F0F0Fu) << 4) | ((value & 0xF0F0F0F0u) >> 4);
            value = ((value & 0x00FF00FFu) << 8) | ((value & 0xFF00FF00u) >> 8);
            value = (value << 16) | (value >> 16);
            return value >> (32 - num_bits);
        }

        // decode table entries: (symbol << 8) | length
        private static void GenerateHuffmanDecodeTable(ushort[] decodeTable, int decodeTableOffset, byte[] symbolLengths, byte[] symbolCodes, int alphabetSize, int maxCodeLength)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (alphabetSize > 256 || maxCodeLength > 8)
                throw new InvalidOperationException("Can only generate huffman codes up to alphabet size 256 and maximum code length 8");
#endif

            uint maxCode = 1u << maxCodeLength;
            for (int symbol = 0; symbol < alphabetSize; symbol++)
            {
                int length = symbolLengths[symbol];
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (length > maxCodeLength)
                    throw new InvalidOperationException("Maximum code length exceeded");
#endif
                if (length > 0)
                {
                    uint code = symbolCodes[symbol];
                    uint step = 1u << length;
                    do
                    {
                        decodeTable[decodeTableOffset + code] = (ushort)(symbol << 8 | length);
                        code += step;
                    }
                    while (code < maxCode);
                }
            }
        }

        internal fixed ushort encodeTable[k_MaxContexts * k_AlphabetSize];
        internal fixed ushort decodeTable[k_MaxContexts * (1 << k_MaxHuffmanSymbolLength)];
        internal fixed byte bucketSizes[k_AlphabetSize];
        internal fixed uint bucketOffsets[k_AlphabetSize];
        
        /// <summary>
        /// Calculates the bucket using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The bucket index</returns>
        public int CalculateBucket(uint value)
        {
            int bucketIndex = k_FirstBucketCandidate[math.lzcnt(value)];
            if (bucketIndex + 1 < k_AlphabetSize && value >= bucketOffsets[bucketIndex + 1])
                bucketIndex++;

            return bucketIndex;
        }
    }
}
