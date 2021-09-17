﻿/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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

using System.Collections.Generic;
using Gibbed.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Gibbed.LetUsClingTogether.UnpackFILETABLE
{
    public class FileTableManifest
    {
        public FileTableManifest()
        {
            this.Directories = new List<Directory>();
        }

        [JsonProperty("endian", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Endian Endian { get; set; }

        [JsonProperty("title_id_1", Required = Required.Always)]
        public string TitleId1 { get; set; }

        [JsonProperty("title_id_2", Required = Required.Always)]
        public string TitleId2 { get; set; }

        [JsonProperty("unknown32", Required = Required.Always)]
        public byte Unknown32 { get; set; }

        [JsonProperty("parental_level", Required = Required.Always)]
        public byte ParentalLevel { get; set; }

        [JsonProperty("install_data_crypto_key", Required = Required.Always)]
        public byte[] InstallDataCryptoKey { get; set; }

        [JsonProperty("directories")]
        public List<Directory> Directories { get; }

        public class Directory
        {
            [JsonProperty("id", Required = Required.Always)]
            public ushort Id { get; set; }

            [JsonProperty("data_block_size", Required = Required.Always)]
            public byte DataBlockSize { get; set; }

            [JsonProperty("in_install_data", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool IsInInstallData { get; set; }

            [JsonProperty("file_manifest", Required = Required.Always)]
            public string FileManifest { get; set; }
        }

        public class File
        {
            [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int? Id { get; set; }

            [JsonProperty("name_hash", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public uint? NameHash { get; set; }

            [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("pack_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public PackId? PackId { get; set; }

            [JsonProperty("zip", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool IsZip { get; set; }

            [JsonProperty("zip_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string ZipName { get; set; }

            [JsonProperty("pack", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public bool IsPack { get; set; }

            [JsonProperty("path", Required = Required.Always)]
            public string Path { get; set; }
        }

        public struct PackId
        {
            [JsonProperty("file", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ushort FileId { get; set; }

            [JsonProperty("dir", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public ushort DirectoryId { get; set; }

            public uint RawId { get { return (((uint)this.DirectoryId) << 16) | this.FileId; } }

            public PackId(uint rawId)
            {
                this.FileId = (ushort)(rawId & 0xFFFFu);
                this.DirectoryId = (ushort)((rawId >> 16) & 0xFFFFu);
            }

            public static PackId? Create(uint? rawId)
            {
                return rawId == null ? (PackId?)null : new PackId(rawId.Value);
            }
        }
    }
}
