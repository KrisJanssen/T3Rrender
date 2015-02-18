using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace T3Rrender
{
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();

            watch.Start();

            long ping;
            long pong;

            // Initialize some structs that will hold instrument info.
            // NOTE: during measurement, these values will have to be retrieved from HW itself!
            TxtHdr txtHdr = new TxtHdr();
            BinHdr binHdr = new BinHdr();
            BoardHdr boardHdr = new BoardHdr();
            TTTRHdr tttrHdr = new TTTRHdr();
            TTTRrecord[] records;
            int[] rawRecords;

            // Read the actual data into memory.
            using (FileStream fsSource = new FileStream("test.t3r", FileMode.Open, FileAccess.Read))
            {
                // Reader for our file stream.
                BinaryReader reader = new BinaryReader(fsSource);

                // The text header.
                byte[] readBuffer = new byte[Marshal.SizeOf(txtHdr)];
                readBuffer = reader.ReadBytes(Marshal.SizeOf(txtHdr));

                GCHandle handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
                txtHdr = (TxtHdr)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TxtHdr));
                handle.Free();

                // The binary header.
                readBuffer = new byte[Marshal.SizeOf(binHdr)];
                readBuffer = reader.ReadBytes(Marshal.SizeOf(binHdr));

                handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
                binHdr = (BinHdr)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BinHdr));
                handle.Free();

                // The board header.
                readBuffer = new byte[Marshal.SizeOf(boardHdr)];
                readBuffer = reader.ReadBytes(Marshal.SizeOf(boardHdr));

                handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
                boardHdr = (BoardHdr)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BoardHdr));
                handle.Free();

                readBuffer = new byte[Marshal.SizeOf(tttrHdr)];
                readBuffer = reader.ReadBytes(Marshal.SizeOf(tttrHdr));

                handle = GCHandle.Alloc(readBuffer, GCHandleType.Pinned);
                tttrHdr = (TTTRHdr)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(TTTRHdr));
                handle.Free();

                // Skip over the "special header"
                reader.BaseStream.Position = reader.BaseStream.Position + tttrHdr.SpecialHeaderSize * sizeof(Int32);

                // Read the actual records.
                readBuffer = reader.ReadBytes(Marshal.SizeOf(typeof(TTTRrecord)) * tttrHdr.NoOfRecords);
                records = new TTTRrecord[tttrHdr.NoOfRecords];
                handle = GCHandle.Alloc(records, GCHandleType.Pinned);
                Marshal.Copy(readBuffer, 0, handle.AddrOfPinnedObject(), readBuffer.Length);
                handle.Free();

                rawRecords = new int[tttrHdr.NoOfRecords];
                handle = GCHandle.Alloc(rawRecords, GCHandleType.Pinned);
                Marshal.Copy(readBuffer, 0, handle.AddrOfPinnedObject(), readBuffer.Length);
                handle.Free();

            }

            int[] timeTags = records.Select(x => Convert.ToInt32(x.TimeTag)).ToArray();
            int[] valid = records.Select(x => Convert.ToInt32(x.Valid)).ToArray();

            // Locations of frame markers.
            int[] framemarkers = records.IndexesOf(x => (x.Valid != 1) & (x.Channel == 2)).ToArray();

            // Locations of line markers.
            int[] linemarkers = records.IndexesOf(x => (x.Valid != 1) & (x.Channel == 4)).ToArray();

            // Count overflows on the first line.
            int ofFirst = 0;

            for (int i = framemarkers[0]; i < linemarkers[0]; i++)
            {
                if ((records[i].Channel & 2048) >> 11 == 1)
                {
                    ofFirst += 1;
                }
            }

            // LineTime is assumed constant throughout the acquisition.
            int lineTime = Convert.ToInt32(records[linemarkers[0]].TimeTag) + ofFirst * 65536 - Convert.ToInt32(records[framemarkers[0]].TimeTag);

            // From lineTime, we easily get pixelTime.
            int pixelTime = Convert.ToInt32(Math.Round(Convert.ToDouble(lineTime / linemarkers.Length)));

            // Unroll overflows.
            // TODO: This cumulative sum approach somehow looks ugly.
            uint[] overflow =
                records.Cumsum(0U, (prev, next) => ((prev.Channel & 2048U) >> 11) + next).ToArray();

            int[] test = T3Rrender.Renderline(
                records,
                overflow,
                framemarkers,
                linemarkers,
                1,
                1,
                pixelTime,
                linemarkers.Length);

            ping = watch.ElapsedMilliseconds;

            int[,] im = new int[400, 400]; 

            for (int i = 1; i <= 400; i++)
            {
                test = T3Rrender.Renderline(
                records,
                overflow,
                framemarkers,
                linemarkers,
                i,
                i,
                pixelTime,
                linemarkers.Length);

                Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
            }

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Struct: " + (pong - ping).ToString());

            ping = watch.ElapsedMilliseconds;

            Parallel.For(
                1,
                400,
                i =>
                    {
                        test = T3Rrender.Renderline(
                            records,
                            overflow,
                            framemarkers,
                            linemarkers,
                            i,
                            i,
                            pixelTime,
                            linemarkers.Length);

                        Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
                    });

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Struct P: " + (pong - ping).ToString());

            ping = watch.ElapsedMilliseconds;

            im = new int[400, 400];

            for (int i = 1; i <= 400; i++)
            {
                test = T3Rrender.RenderlineII(
                rawRecords,
                valid,
                overflow,
                framemarkers,
                linemarkers,
                i,
                i,
                pixelTime,
                linemarkers.Length);

                Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
            }

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Array: " + (pong - ping).ToString());

            ping = watch.ElapsedMilliseconds;

            Parallel.For(
                1,
                400,
                i =>
                {
                    test = T3Rrender.RenderlineII(
                        rawRecords,
                        valid,
                        overflow,
                        framemarkers,
                        linemarkers,
                        i,
                        i,
                        pixelTime,
                        linemarkers.Length);

                    Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
                });

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Array P: " + (pong - ping).ToString());

            ping = watch.ElapsedMilliseconds;

            im = new int[400, 400];

            for (int i = 1; i <= 400; i++)
            {
                test = T3Rrender.RenderlineIII(
                rawRecords,
                valid,
                overflow,
                framemarkers,
                linemarkers,
                i,
                i,
                pixelTime,
                linemarkers.Length);

                Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
            }

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Array switch: " + (pong - ping).ToString());

            ping = watch.ElapsedMilliseconds;

            im = new int[400, 400];

            int lineStartIdx;
            int lineEndIdx;
            int lineRecCount;

            int[] lineRecs;
            int[] lineValid;
            uint[] lineOverflow;

            for (int i = 1; i <= 400; i++)
            {
                if (i > 1)
                {
                    lineStartIdx = linemarkers[i - 2];
                    lineEndIdx = linemarkers[i - 1];
                }
                else
                {
                    lineStartIdx = framemarkers[i - 1];
                    lineEndIdx = linemarkers[i];
                
                }

                lineRecCount = lineEndIdx - lineStartIdx;

                lineRecs = new int[lineRecCount];
                lineValid = new int[lineRecCount];
                lineOverflow = new uint[lineRecCount];

                Buffer.BlockCopy(rawRecords, lineStartIdx, lineRecs,0,lineRecCount);
                Buffer.BlockCopy(valid, lineStartIdx, lineValid, 0, lineRecCount);
                Buffer.BlockCopy(overflow, lineStartIdx - 1, lineOverflow, 0, lineRecCount);

                test = T3Rrender.RenderlineIV(
                lineRecs,
                lineValid,
                lineOverflow,
                framemarkers,
                linemarkers,
                i,
                i,
                pixelTime,
                linemarkers.Length);

                Buffer.BlockCopy(test, 0, im, 4 * (i - 1) * 400, 4 * 400);
            }

            pong = watch.ElapsedMilliseconds;

            Debug.Print("Partial copy: " + (pong - ping).ToString());

            //string path = "mycsv.txt";


            //string str = "";
            //for (int i = 0; i < 400; i++)
            //{
            //    for (int j = 0; j < 400; j++)
            //    {
            //        str = str + im[j, i].ToString() + "\t";
            //    }

            //    str = str + Environment.NewLine;
            //}
            //using (StreamWriter outfile = new StreamWriter(path))
            //{
            //    outfile.Write(str);
            //}
            
            Console.ReadKey();
        }
    }
}
