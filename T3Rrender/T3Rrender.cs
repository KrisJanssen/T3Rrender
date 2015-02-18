using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T3Rrender
{
    public static class T3Rrender
    {
        #region OLD attempts

        public static int[] Renderline(TTTRrecord[] allRecords, uint[] absTimes, int[] frameMarkers, int[] lineMarkers, int frame, int line, int pixelduration, int pixelCount)
        {
            int[] linePixels = new int[pixelCount];

            // TODO: handle the no lines case.
            // TODO: handle the multi fram case.
            int lineStartIdx;
            int lineEndIdx;

            if (line > 1)
            {
                lineStartIdx = lineMarkers[line - 2];
                lineEndIdx = lineMarkers[line - 1];
            }
            else
            {
                lineStartIdx = frameMarkers[line - 1];
                lineEndIdx = lineMarkers[line];
            }

            int lineRecordCount = lineEndIdx - lineStartIdx;

            uint lineStartTime = allRecords[lineStartIdx].TimeTag + absTimes[lineStartIdx - 1] * 65536;

            int tempIdx = lineStartIdx;

            for (int j = 0; j < linePixels.Length; j++)
            {
                int count = 0;

                for (int i = tempIdx; i < lineEndIdx; i++)
                {
                    uint current = allRecords[i].TimeTag + absTimes[i - 1] * 65536;
                    uint valid = allRecords[i].Valid;

                    //if (valid == 1 && lineStartTime + j * pixelduration <= current && lineStartTime + (j + 1) * pixelduration >= current)
                    if (valid == 1 && lineStartTime + (j + 1) * pixelduration >= current)
                    {
                        count++;
                    }

                }

                linePixels[j] = count;
                tempIdx += count;
            }

            return linePixels;
        }

        public static int[] RenderlineII(int[] allRecords, int[] valid, uint[] absTimes, int[] frameMarkers, int[] lineMarkers, int frame, int line, int pixelduration, int pixelCount)
        {
            int[] linePixels = new int[pixelCount];

            // TODO: handle the no lines case.
            // TODO: handle the multi fram case.
            int lineStartIdx;
            int lineEndIdx;

            if (line > 1)
            {
                lineStartIdx = lineMarkers[line - 2];
                lineEndIdx = lineMarkers[line - 1];
            }
            else
            {
                lineStartIdx = frameMarkers[line - 1];
                lineEndIdx = lineMarkers[line];
            }

            int lineRecordCount = lineEndIdx - lineStartIdx;

            long lineStartTime = allRecords[lineStartIdx] + absTimes[lineStartIdx - 1] * 65536;

            int tempIdx = lineStartIdx;

            for (int j = 0; j < linePixels.Length; j++)
            {
                int count = 0;

                for (int i = tempIdx; i < lineEndIdx; i++)
                {
                    long current = allRecords[i] + absTimes[i - 1] * 65536;
                    long isValid = valid[i];

                    if (isValid == 1 && lineStartTime + (j + 1) * pixelduration >= current)
                    {
                        count++;
                    }

                }

                linePixels[j] = count;
                tempIdx += count;
            }

            return linePixels;
        }

        public static int[] RenderlineIII(int[] allRecords, int[] valid, uint[] absTimes, int[] frameMarkers, int[] lineMarkers, int frame, int line, int pixelduration, int pixelCount)
        {
            int[] linePixels = new int[pixelCount];

            // TODO: handle the no lines case.
            // TODO: handle the multi fram case.
            int lineStartIdx;
            int lineEndIdx;

            if (line > 1)
            {
                lineStartIdx = lineMarkers[line - 2];
                lineEndIdx = lineMarkers[line - 1];
            }
            else
            {
                lineStartIdx = frameMarkers[line - 1];
                lineEndIdx = lineMarkers[line];
            }

            int lineRecordCount = lineEndIdx - lineStartIdx;

            long lineStartTime = allRecords[lineStartIdx] + absTimes[lineStartIdx - 1] * 65536;

            int tempIdx = lineStartIdx;

            for (int j = 0; j < linePixels.Length; j++)
            {
                int count = 0;

                for (int i = tempIdx; i < lineEndIdx; i++)
                {
                    long current = allRecords[i] + absTimes[i - 1] * 65536;
                    long isValid = valid[i];

                    switch (isValid)
                    {
                        case 1:
                            if (lineStartTime + (j + 1) * pixelduration >= current)
                            {
                                count++;
                            }
                            break;
                        default:
                            break;
                    }

                }

                linePixels[j] = count;
                tempIdx += count;
            }

            return linePixels;
        }

        public static int[] RenderlineIV(int[] someRecords, int[] valid, uint[] absTimes, int[] frameMarkers, int[] lineMarkers, int frame, int line, int pixelduration, int pixelCount)
        {
            int[] linePixels = new int[pixelCount];

            // TODO: handle the no lines case.
            // TODO: handle the multi fram case.
            

            long lineStartTime = someRecords[0] + absTimes[0] * 65536;

            int tempIdx = 0;

            for (int j = 0; j < linePixels.Length; j++)
            {
                int count = 0;

                for (int i = tempIdx; i < someRecords.Length; i++)
                {
                    long current = someRecords[i] + absTimes[i] * 65536;
                    long isValid = valid[i];

                    if (isValid == 1 && lineStartTime + (j + 1) * pixelduration >= current)
                    {
                        count++;
                    }

                }

                linePixels[j] = count;
                tempIdx += count;
            }

            return linePixels;
        }

        #endregion

        public static int[] RenderlineV(int[] someRecords, int pixelduration, int pixelCount)
        {
            int[] linePixels = new int[pixelCount];

            int[] timeTag = someRecords.Select(x => Convert.ToInt32(x & 65535)).ToArray();
            int[] channel = someRecords.Select(x => Convert.ToInt32((x >> 16) & 4095)).ToArray();
            int[] valid = someRecords.Select(x => Convert.ToInt32((x >> 30) & 1)).ToArray();
            int[] overflow = channel.Select(x => (x & 2048) >> 11).ToArray();
            int[] absTime = new int[overflow.Length];
            absTime[0] = 0;

            Buffer.BlockCopy(overflow, 0, absTime, 4, (overflow.Length - 1) * 4);

            absTime = absTime.Cumsum(0, (prev, next) => prev * 65536 + next).Zip(timeTag, (o, tt) => o + tt).ToArray();

            long lineStartTime = absTime[0];

            int tempIdx = 0;

            for (int j = 0; j < linePixels.Length; j++)
            {
                int count = 0;
                lineStartTime += pixelduration;

                for (int i = tempIdx; i < someRecords.Length; i++)
                {
                    //if (lineStartTime + (j + 1) * pixelduration >= absTime[i] && valid[i] == 1)
                    // Seems quicker to calculate the boundary before...
                    //if (valid[i] == 1 && lineStartTime >= absTime[i] )
                    // Quicker still...
                    if (lineStartTime >= absTime[i] && valid[i] == 1)
                    {
                        count++;
                    }
                }

                linePixels[j] = count;
                tempIdx += count;
            }

            return linePixels;
        }
    }
}
