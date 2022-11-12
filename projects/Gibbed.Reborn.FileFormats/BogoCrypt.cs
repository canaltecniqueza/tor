﻿/* Copyright (c) 2022 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

namespace Gibbed.Reborn.FileFormats
{
    public static class BogoCrypt
    {
        private static readonly byte[] Table2;
        private static readonly byte[] Table3;

        static BogoCrypt()
        {
            Table2 = new byte[]
            {
                0xB0, 0xBC, 0x6D, 0x19, 0xE5, 0x14, 0xB6, 0xEA,
                0xF4, 0xCB, 0x16, 0xB1, 0x7D, 0xE5, 0x7F, 0xB5,
                0x0B, 0x7C, 0x5E, 0xC4, 0x2D, 0x4F, 0x31, 0x99,
                0x67, 0x98, 0x19, 0xE8, 0x28, 0x58, 0xDE, 0xC9,
                0xC1, 0x9B, 0xC9, 0x83, 0x34, 0xC4, 0x64, 0x85,
                0x36, 0xFA, 0x0E, 0xDE, 0xFB, 0xE7, 0x68, 0x99,
                0x71, 0x32, 0x34, 0x36, 0xE5, 0x01, 0x7D, 0x63,
                0x5C, 0x81, 0x77, 0x7C,
            };
            Table3 = new byte[]
            {
                0xC6, 0x53, 0x98, 0xED, 0xD0, 0xAA, 0xE0, 0x1B,
                0x43, 0x31, 0x16, 0x32, 0xCE, 0x0D, 0x52, 0x57,
                0x7B, 0x72, 0x0F, 0xFB, 0xB7, 0xA9, 0xC6, 0xF1,
                0xF1, 0x72, 0x2C, 0x89, 0x5F, 0xB2, 0x1B, 0xFA,
                0xA8, 0xDB, 0xE3, 0xE2, 0x41, 0xB5, 0xBE, 0xC9,
                0x5D, 0xAE, 0xC6, 0x21, 0xE4, 0xD2, 0x88, 0x74,
                0x97, 0x26, 0x4F, 0x87, 0x7A, 0xF0, 0x69, 0x7A,
                0xB7, 0x7E, 0x15, 0x09, 0xBA, 0x55, 0xAD, 0xCA,
                0xB1, 0x60, 0x34, 0x76,
            };
        }

        public static void Deobfuscate(byte[] bytes, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }
            Pass1(bytes, offset, count, 3);
            Pass2(bytes, offset, count, 8, Table2);
            Pass2(bytes, offset + 16, count - 16, bytes[offset + 15] & 0x7F, Table3);
        }

        private static void Pass1(byte[] bytes, int offset, int count, int shift)
        {
            if (count <= 0)
            {
                return;
            }
            var firstByte = bytes[offset];
            int rightShift = 8 - shift;
            int o = offset;
            for (int i = 0; i < count - 1; i++, o++)
            {
                bytes[o] = (byte)((bytes[o] << shift) | (bytes[o + 1] >> rightShift));
            }
            bytes[o] = (byte)((bytes[o] << shift) | (firstByte >> rightShift));
        }

        private static void Pass2(byte[] bytes, int offset, int count, int seed, byte[] table)
        {
            if (count <= 0)
            {
                return;
            }
            int tableLength = table.Length >> 2;
            int tableIndex = seed;
            for (int i = 0, o = offset; i + 4 <= count; i += 4, o += 4)
            {
                tableIndex %= tableLength;
                var tableOffset = tableIndex * 4;
                tableIndex++;
                bytes[o + 0] ^= table[tableOffset + 0];
                bytes[o + 1] ^= table[tableOffset + 1];
                bytes[o + 2] ^= table[tableOffset + 2];
                bytes[o + 3] ^= table[tableOffset + 3];
            }
        }
    }
}