﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using QuantConnect.Util;

namespace QuantConnect.Archives
{
    /// <summary>
    /// Provides an implementation of <see cref="IArchive"/> that uses <see cref="ZipArchive"/> from the .NET framework
    /// </summary>
    public class DotNetFrameworkZipArchive : IArchive
    {
        private readonly ZipArchive _zipArchive;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFrameworkZipArchive"/> class
        /// </summary>
        /// <param name="zipArchive">The zip archive</param>
        public DotNetFrameworkZipArchive(ZipArchive zipArchive)
        {
            _zipArchive = zipArchive;
        }

        /// <summary>
        /// Creates a new collection containing all of this archive's entries
        /// </summary>
        /// <returns>A new collection containing all of this archive's entries</returns>
        public IReadOnlyCollection<IArchiveEntry> GetEntries()
        {
            return _zipArchive.Entries.Select(entry => new Entry(entry.FullName, _zipArchive, entry)).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the entry by the specified name or null if it does not exist
        /// </summary>
        /// <param name="key">The entry's full name</param>
        /// <returns>The archive entry, or null if not found</returns>
        public IArchiveEntry GetEntry(string key)
        {
            if (_zipArchive.Mode == ZipArchiveMode.Create)
            {
                return new Entry(key, _zipArchive, null);
            }

            var zipArchiveEntry = _zipArchive.GetEntry(key);
            return new Entry(key, _zipArchive, zipArchiveEntry);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _zipArchive.DisposeSafely();
        }

        /// <summary>
        /// Defines an entry in a <see cref="DotNetFrameworkZipArchive"/>
        /// </summary>
        private class Entry : IArchiveEntry
        {
            private ZipArchiveEntry _entry;
            private readonly ZipArchive _zipArchive;

            /// <summary>
            /// Gets the entry's key
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// True if the archive contains and entry with this key, false if it does not exist
            /// </summary>
            public bool Exists => _entry != null;

            /// <summary>
            /// Initializes a new instance of the <see cref="Entry"/> class
            /// </summary>
            /// <param name="key">The entry key</param>
            /// <param name="zipArchive">The zip archive</param>
            /// <param name="entry">The zip archive entry</param>
            public Entry(string key, ZipArchive zipArchive, ZipArchiveEntry entry)
            {
                Key = key;
                _zipArchive = zipArchive;
                _entry = entry;
            }

            /// <summary>
            /// Opens the entry's stream for reading
            /// </summary>
            /// <returns>The entry's stream</returns>
            public Stream Read()
            {
                return _entry.Open();
            }

            public void Write(Stream stream)
            {
                if (_zipArchive.Mode == ZipArchiveMode.Update)
                {
                    _entry?.Delete();
                }

                if (_entry == null)
                {
                    _entry = _zipArchive.CreateEntry(Key, CompressionLevel.Fastest);
                }

                using (var entryStream = _entry.Open())
                {
                    stream.CopyTo(entryStream);
                    if (entryStream.Position == 0)
                    {

                    }
                }
            }
        }
    }
}