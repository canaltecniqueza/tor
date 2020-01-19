﻿/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.IO;

namespace Gibbed.TacticsOgre.FileFormats
{
    public class TableFile
    {
        public List<Table.DirectoryEntry> Directories =
            new List<Table.DirectoryEntry>();

        public void Deserialize(Stream input)
        {
            var unknown00 = input.ReadValueU16();
            var unknown02 = input.ReadValueU16();
            var dataSourceCount = input.ReadValueU16();
            var unknown06 = input.ReadValueU16();
            var fileTableOffset = input.ReadValueU32();
            var tableSize = input.ReadValueU32();
            var titleId1 = input.ReadString(16, Encoding.ASCII);
            var titleId2 = input.ReadString(16, Encoding.ASCII);

            var unknown30 = input.ReadValueU32(Endian.Big);
            var unknown34 = new byte[16];
            input.Read(unknown34, 0, unknown34.Length);

            // skip weird data
            input.Seek(unknown02 * 8, SeekOrigin.Current);

            var sources = new Table.SourceHeader[dataSourceCount];
            for (ushort i = 0; i < dataSourceCount; i++)
            {
                sources[i] = new Table.SourceHeader();
                sources[i].Deserialize(input);
            }

            var totalDirCount = sources.Sum(s => s.DirectoryCount);
            if (input.Position + (totalDirCount * 8) != fileTableOffset)
            {
                throw new InvalidOperationException();
            }

            var groups = new Table.GroupHeader[totalDirCount];
            for (int i = 0; i < totalDirCount; i++)
            {
                groups[i] = new Table.GroupHeader();
                groups[i].Deserialize(input);
            }

            var totalFileCount = groups.Sum(d => d.FileCount);

            long fileTableSize = 0;
            foreach (var dir in groups)
            {
                var fileSizeSize =
                    dir.Flags.HasFlag(Table.GroupFlags.ExtendedSizes) ?
                    4 : 2;
                fileTableSize += dir.FileCount * (2 + fileSizeSize);
            }

            if (tableSize - fileTableOffset != fileTableSize)
            {
                throw new InvalidOperationException();
            }

            var fileTable = input.ReadToMemoryStream((int)fileTableSize);

            this.Directories.Clear();
            for (int i = 0; i < sources.Length; i++)
            {
                var source = sources[i];
                var dir = new Table.DirectoryEntry();
                dir.Id = source.Id;

                for (ushort j = 0; j < source.DirectoryCount; j++)
                {
                    var gindex = source.DirectoryTableOffset / 8;
                    var group = groups[gindex + j];
                    
                    bool extendedSize =
                        group.Flags.HasFlag(Table.GroupFlags.ExtendedSizes);

                    fileTable.Seek(group.FileTableOffset, SeekOrigin.Begin);
                    for (ushort k = 0; k < group.FileCount; k++)
                    {
                        var file = new Table.FileEntry();
                        file.Group = j;
                        file.Id = (uint)group.Id + k;
                        file.Offset = (uint)fileTable.ReadValueU16() * 0x8000;
                        file.Size = extendedSize == true ?
                            fileTable.ReadValueU32() :
                            (uint)fileTable.ReadValueU16();

                        var test = dir.Files.FirstOrDefault(
                            f =>
                                f.Group == file.Group &&
                                f.Id == file.Id);
                        if (test != null)
                        {
                            throw new InvalidOperationException();
                        }

                        dir.Files.Add(file);
                    }
                }

                this.Directories.Add(dir);
            }
        }
    }
}
