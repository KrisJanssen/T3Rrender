using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T3Rrender
{
    public static class T3Rrender
    {
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
    }
}
