﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Konata.AvCodec
{
    public static class SilkCodec
    {
        private delegate void CodecCallback(IntPtr p, int len);

        [DllImport("SilkCodec")]
        private static extern bool silkDecode(IntPtr silkData, int dataLen, int sampleRate, CodecCallback cb);

        [DllImport("SilkCodec")]
        private static extern bool silkEncode(IntPtr pcmData, int dataLen, int sampleRate, CodecCallback cb);

        /// <summary>
        /// Decode silk
        /// </summary>
        /// <param name="silkData">Silk data to decode</param>
        /// <param name="sampleRate"> Sample rate. default 24000Hz</param>
        /// <param name="pcmData">output</param>
        /// <returns></returns>
        public static Task<bool> Decode(byte[] silkData, int sampleRate, out byte[] pcmData)
        {
            pcmData = null;

            // Duplicate the data
            var lpSilkData = Marshal.AllocHGlobal(silkData.Length);
            Marshal.Copy(silkData, 0, lpSilkData, silkData.Length);

            try
            {
                // Prepare the stream
                using var memStream = new MemoryStream();
                using var binaryWriter = new BinaryWriter(memStream);

                // Decode the silk data
                var result = silkDecode(lpSilkData, silkData.Length, sampleRate, (data, length) =>
                {
                    // Copy the part
                    var buffer = new byte[length];
                    Marshal.Copy(data, buffer, 0, length);

                    // Write to stream
                    binaryWriter.Write(buffer);
                });

                // Okay
                pcmData = memStream.ToArray();
                return Task.FromResult(result);
            }

            // Catch native exceptions
            catch
            {
                return Task.FromResult(false);
            }

            // Cleanup
            finally
            {
                Marshal.FreeHGlobal(lpSilkData);
            }
        }

        /// <summary>
        /// Decode silk from file
        /// </summary>
        /// <param name="silkFile"></param>
        /// <param name="sampleRate"></param>
        /// <param name="pcmData"></param>
        /// <returns></returns>
        public static Task<bool> Decode(string silkFile, int sampleRate, out byte[] pcmData)
            => Decode(File.ReadAllBytes(silkFile), sampleRate, out pcmData);

        /// <summary>
        /// Encode silk from pcm
        /// </summary>
        /// <param name="pcmData"></param>
        /// <param name="sampleRate"></param>
        /// <param name="silkData"></param>
        /// <returns></returns>
        public static Task<bool> Encode(byte[] pcmData, int sampleRate, out byte[] silkData)
        {
            silkData = null;

            // Duplicate the data
            var lpPcmData = Marshal.AllocHGlobal(pcmData.Length);
            Marshal.Copy(pcmData, 0, lpPcmData, pcmData.Length);

            try
            {
                // Prepare the stream
                using var memStream = new MemoryStream();
                using var binaryWriter = new BinaryWriter(memStream);

                // Encode the pcm data
                var result = silkEncode(lpPcmData, pcmData.Length, sampleRate, (data, length) =>
                {
                    // Copy the part
                    var buffer = new byte[length];
                    Marshal.Copy(data, buffer, 0, length);

                    // Write to stream
                    binaryWriter.Write(buffer);
                });

                // Okay
                silkData = memStream.ToArray();
                return Task.FromResult(result);
            }

            // Catch native exceptions
            catch
            {
                return Task.FromResult(false);
            }

            // Cleanup
            finally
            {
                Marshal.FreeHGlobal(lpPcmData);
            }
        }

        /// <summary>
        /// Encode silk from file
        /// </summary>
        /// <param name="pcmFile"></param>
        /// <param name="sampleRate"></param>
        /// <param name="silkData"></param>
        /// <returns></returns>
        public static Task<bool> Encode(string pcmFile, int sampleRate, out byte[] silkData)
            => Encode(File.ReadAllBytes(pcmFile), sampleRate, out silkData);
    }
}
