using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace T3Rrender
{
    /*
The following structures are used to hold the TimeHarp file data
They reflect the file structure.
The data types used here to match the file structure are correct
for MSVC++. They may have to be changed for other compilers.
*/

    [StructLayout(LayoutKind.Sequential)]
    public struct tParamStruct
    {
        public float Start;

        public float Step;

        public float End;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tCurveMapping
    {
        public Int32 MapTo;

        public Int32 Show;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TxtHdr
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string Ident;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string FormatVersion;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string CreatorName;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string CreatorVersion;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string FileTime;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 2)]
        public string CRLF;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string CommentField;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BinHdr
    {
        public Int32 Channels;

        public Int32 Curves;

        public Int32 BitsPerChannel;

        public Int32 RoutingChannels;

        public Int32 NumberOfBoards;

        public Int32 ActiveCurve;

        public Int32 MeasMode;

        public Int32 SubMode;

        public Int32 RangeNo;

        public Int32 Offset; /* in ns */

        public Int32 Tacq; /* in ms */

        public Int32 StopAt;

        public Int32 StopOnOvfl;

        public Int32 Restart;

        public Int32 DispLinLog;

        public Int32 DispTimeFrom;

        public Int32 DispTimeTo;

        public Int32 DispCountsFrom;

        public Int32 DispCountsTo;

        [MarshalAsAttribute(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 8)]
        public tCurveMapping[] DispCurves;

        [MarshalAsAttribute(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 3)]
        public tParamStruct[] Params;

        public Int32 RepeatMode;

        public Int32 RepeatsPerCurve;

        public Int32 RepeatTime;

        public Int32 RepeatWaitTime;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string ScriptName;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BoardHdr
    {
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string HardwareIdent;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string HardwareVersion;

        public Int32 BoardSerial;

        public Int32 CFDZeroCross;

        public Int32 CFDDiscrMin;

        public Int32 SyncLevel;

        public Int32 CurveOffset;

        public float Resolution;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TTTRHdr
    {
        public Int32 Globclock;

        public Int32 ExtDevices;

        public Int32 Reserved1;

        public Int32 Reserved2;

        public Int32 Reserved3;

        public Int32 Reserved4;

        public Int32 Reserved5;

        public Int32 SyncRate;

        public Int32 TTTRCFDRate;

        public Int32 TTTRStopAfter;

        public Int32 TTTRStopReason;

        public Int32 NoOfRecords;

        public Int32 SpecialHeaderSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TTTRrecord
    {
        private Int32 data;

        public uint TimeTag
        {
            get
            {
                return (uint)(data & 65535);
            }
            set
            {
                data = Convert.ToInt32((data & ~65535u) | (value & 65535));
            }
        }

        public uint Channel
        {
            get
            {
                return (uint)((data >> 16) & 4095);
            }
            set
            {
                data = Convert.ToInt32((data & ~(4095u << 16)) | (value & 4095) << 16);
            }
        }

        public uint Route
        {
            get
            {
                return (uint)((data >> 28) & 3);
            }
            set
            {
                data = Convert.ToInt32((data & ~(3u << 28)) | (value & 3) << 28);
            }
        }

        public uint Valid 
        {
            get
            {
                return (uint)((data >> 30) & 1);
            }
            set
            {
                data = Convert.ToInt32((data & ~(1u << 30)) | (value & 1) << 30);
            }
        }

        public uint Reserved
        {
            get
            {
                return (uint)((data >> 31) & 1);
            }
            set
            {
                data = Convert.ToInt32((data & ~(1u << 31)) | (value & 1) << 31);
            }
        }
    }
}