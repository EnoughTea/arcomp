using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;

namespace ArchiveCompare {
    /// <summary> Extension methods for <see cref="BinaryReader"/>. </summary>
    internal static class BinaryReaderExtensions {
        /// <summary> Reads the sequence of <see cref="uint"/> values from a stream. </summary>
        /// <param name="reader">Stream reader.</param>
        /// <param name="count">Number of values to read.</param>
        /// <returns>Read values or empty stream.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        public static uint[] ReadUints(this BinaryReader reader, long count) {
            Contract.Requires(reader != null);
            Contract.Requires(count >= 0);

            return ReadSeq(reader, count, binaryReader => binaryReader.ReadUInt32()).ToArray();
        }

        /// <summary> Reads the sequence of <see cref="ulong"/> values from a stream. </summary>
        /// <param name="reader">Stream reader.</param>
        /// <param name="count">Number of values to read.</param>
        /// <returns>Read values or empty stream.</returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        public static ulong[] ReadUlongs(this BinaryReader reader, long count) {
            Contract.Requires(reader != null);
            Contract.Requires(count >= 0);

            return ReadSeq(reader, count, binaryReader => binaryReader.ReadUInt64()).ToArray();
        }

        /// <summary> Reads the next null-terminated string from the current stream and advances the
        /// current position of the stream by string length in bytes (with null-terminator). </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="encoding">Encoding to use. UTF8 is used by default.</param>
        /// <returns></returns>
        /// <exception cref="IOException">An I/O error occurs.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking.</exception>
        public static string ReadNullTerminatedString(this BinaryReader reader, Encoding encoding = null) {
            var stringBytes = new List<byte>();
            bool encounteredZero = false;
            while ((reader.BaseStream.Position < reader.BaseStream.Length) && !encounteredZero) {
                byte value;
                try {
                    value = reader.ReadByte();
                } catch (EndOfStreamException) { break;}

                encounteredZero = (value == 0);
                if (!encounteredZero) { stringBytes.Add(value); }
            }

            if (encoding == null) { encoding = Encoding.UTF8; }

            return encoding.GetString(stringBytes.ToArray(), 0, stringBytes.Count);
        }

        /// <summary> Reads the sequence of values from a stream. </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="reader">Stream reader.</param>
        /// <param name="count">Number of values to read.</param>
        /// <param name="readSingleValue">Function which does reading of a single value from a stream.</param>
        /// <returns>Read values or empty stream.</returns>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking. </exception>
        private static IEnumerable<T> ReadSeq<T>(this BinaryReader reader, long count,
            Func<BinaryReader, T> readSingleValue) {
            Contract.Requires(reader != null);
            Contract.Requires(readSingleValue != null);

            while ((reader.BaseStream.Position < reader.BaseStream.Length) && count > 0) {
                count--;
                T value;
                try {
                    value = readSingleValue(reader);
                } catch (EndOfStreamException) { yield break; }

                yield return value;
            }
        }
    }
}
