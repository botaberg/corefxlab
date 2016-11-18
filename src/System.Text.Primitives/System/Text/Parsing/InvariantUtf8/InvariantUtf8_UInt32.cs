// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Utf8;

namespace System.Text
{
    public static partial class PrimitiveParser
    {
        public static partial class InvariantUtf8
        {
            #region helpers
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool Utf8ByteToUInt32(byte inByte, out uint outUInt32)
            {
                bool success = true;
                outUInt32 = inByte - 48u; // '0'
                success &= (outUInt32 <= 9);
                return success;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool Utf8ByteToUInt32_Branches(byte inByte, out uint outUInt32)
            {
                outUInt32 = inByte - 48u; // '0'
                if (outUInt32 > 9)
                {
                    return false;
                }
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool CheckOverflow(uint value, uint nextDigit)
            {
                bool success = true;
                // uint.MaxValue is a constant 4294967295
                // uint.MaxValue / 10 is 429496729 (integer division truncated)
                // (value > uint.MaxValue / 10) implies overflow when appended to, e.g., 4294967300 > 4294967295
                success &= !(value > uint.MaxValue / 10);
                // if value == 429496729, any nextDigit greater than 5 implies overflow
                success &= !(value == uint.MaxValue / 10 && nextDigit > 5);
                return success;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool CheckOverflow_Branches(uint value, uint nextDigit)
            {
                // uint.MaxValue is a constant 4294967295
                // uint.MaxValue / 10 is 429496729 (integer division truncated)
                // (value > uint.MaxValue / 10) implies overflow when appended to, e.g., 4294967300 > 4294967295
                // if value == 429496729, any nextDigit greater than 5 implies overflow
                if (value > uint.MaxValue / 10 || value == uint.MaxValue / 10 && nextDigit > 5)
                {
                    return false;
                }
                return true;
            }
            #endregion

            #region scratch            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static bool TryParseUInt32_Branches(byte* text, int length, out uint value)
            {
                value = default(uint);
                for (int index = 0; index < length; index++)
                {
                    uint nextDigit;
                    if (!Utf8ByteToUInt32(text[index], out nextDigit))
                    {
                        // Next digit does not exist
                        return true;
                    }
                    if (!CheckOverflow(value, nextDigit))
                    {
                        value = default(uint);
                        return false;
                    }
                    value = value * 10 + nextDigit;
                }

                return length > 0 ? true : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static bool TryParseUInt32_OptimizeOverflowCheck_Branches(byte* text, int length, out uint value)
            {
                value = default(uint);
                if (length < 10)
                {
                    for (int index = 0; index < length; index++)
                    {
                        uint nextDigit;
                        if (!Utf8ByteToUInt32(text[index], out nextDigit))
                        {
                            return true;
                        }
                        value = value * 10 + nextDigit;
                    }
                }
                else
                {
                    for (int index = 0; index < 9; index++)
                    {
                        uint nextDigit;
                        if (!Utf8ByteToUInt32(text[index], out nextDigit))
                        {
                            return true;
                        }
                        value = value * 10 + nextDigit;
                    }
                    for (int index = 9; index < length; index++)
                    {
                        uint nextDigit;
                        if (!Utf8ByteToUInt32(text[index], out nextDigit))
                        {
                            return true;
                        }
                        if (!CheckOverflow(value, nextDigit))
                        {
                            value = default(uint);
                            return false;
                        }
                        value = value * 10 + nextDigit;
                    }
                }

                return length > 0 ? true : false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static bool TryParseUInt32_OptimizeOverflowCheck(byte* text, int length, out uint value)
            {
                value = default(uint);

                bool valid = true;
                bool success = true;
                uint nextDigit;

                if (length < 10)
                {
                    for (int i = 0; i < length; i++)
                    {
                        valid &= Utf8ByteToUInt32(text[i], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                    }
                }
                else
                {
                    for (int i = 0; i < 9; i++)
                    {
                        valid &= Utf8ByteToUInt32(text[i], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                    }
                    for (int i = 9; i < length; i++)
                    {
                        valid &= Utf8ByteToUInt32(text[i], out nextDigit);
                        success &= CheckOverflow(value, nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                    }
                }

                value = success ? value : default(uint);
                return success;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static bool TryParseUInt32_LoopUnroll(byte* text, int length, out uint value)
            {
                value = default(uint);
                if (length < 1)
                {
                    return false;
                }

                bool valid = true;
                bool success = true;
                uint nextDigit;

                switch (length)
                {
                    case 1:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 2:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 3:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 4:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 5:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 6:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[5], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 7:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[5], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[6], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 8:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[5], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[6], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[7], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 9:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[5], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[6], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[7], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[8], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    case 10:
                        valid &= Utf8ByteToUInt32(text[0], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[1], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[2], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[3], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[4], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[5], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[6], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[7], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[8], out nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        valid &= Utf8ByteToUInt32(text[9], out nextDigit);
                        success &= CheckOverflow(value, nextDigit);
                        value = valid ? value * 10 + nextDigit : value;
                        break;
                    default:
                        for (int i = 0; i < 9; i++)
                        {
                            valid &= Utf8ByteToUInt32(text[i], out nextDigit);
                            value = valid ? value * 10 + nextDigit : value;
                        }
                        for (int i = 9; i < length; i++)
                        {
                            valid &= Utf8ByteToUInt32(text[i], out nextDigit);
                            success &= CheckOverflow(value, nextDigit);
                            value = valid ? value * 10 + nextDigit : value;
                        }
                        break;
                }

                value = success ? value : default(uint);
                return success;
            }
            #endregion

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static bool TryParseUInt32(byte* text, int length, out uint value)
            {
                return TryParseUInt32_OptimizeOverflowCheck_Branches(text, length, out value);
            }
            public unsafe static bool TryParseUInt32(byte* text, int length, out uint value, out int bytesConsumed)
            {
                var span = new ReadOnlySpan<byte>(text, length);
                return PrimitiveParser.TryParseUInt32(span, out value, out bytesConsumed, EncodingData.InvariantUtf8);
            }
            public static bool TryParseUInt32(ReadOnlySpan<byte> text, out uint value)
            {
                int consumed;
                return PrimitiveParser.TryParseUInt32(text, out value, out consumed, EncodingData.InvariantUtf8);
            }
            public static bool TryParseUInt32(ReadOnlySpan<byte> text, out uint value, out int bytesConsumed)
            {
                return PrimitiveParser.TryParseUInt32(text, out value, out bytesConsumed, EncodingData.InvariantUtf8);
            }

            public static partial class Hex
            {
                public unsafe static bool TryParseUInt32(byte* text, int length, out uint value)
                {
                    int consumed;
                    var span = new ReadOnlySpan<byte>(text, length);
                    return PrimitiveParser.TryParseUInt32(span, out value, out consumed, EncodingData.InvariantUtf8, 'X');
                }
                public unsafe static bool TryParseUInt32(byte* text, int length, out uint value, out int bytesConsumed)
                {
                    var span = new ReadOnlySpan<byte>(text, length);
                    return PrimitiveParser.TryParseUInt32(span, out value, out bytesConsumed, EncodingData.InvariantUtf8, 'X');
                }
                public static bool TryParseUInt32(ReadOnlySpan<byte> text, out uint value)
                {
                    int consumed;
                    return PrimitiveParser.TryParseUInt32(text, out value, out consumed, EncodingData.InvariantUtf8, 'X');
                }
                public static bool TryParseUInt32(ReadOnlySpan<byte> text, out uint value, out int bytesConsumed)
                {
                    return PrimitiveParser.TryParseUInt32(text, out value, out bytesConsumed, EncodingData.InvariantUtf8, 'X');
                }
            }
        }
    }
}