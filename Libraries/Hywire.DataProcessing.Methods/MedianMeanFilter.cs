using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hywire.DataProcessing
{
    namespace Methods
    {
        static public class MedianMeanFilter
        {
            static public void FilterProcess(ref ushort[] rawArray, int meanSize)
            {
                int totals = 0;
                int halfSize = meanSize / 2;
                for (int i = 0; i < rawArray.Length; i++)
                {
                    if (i < halfSize)
                    {
                        totals = totals + rawArray[i];
                    }
                    else if (i == halfSize)
                    {
                        for (int j = 1; j < halfSize+1; j++)
                        {
                            totals = totals + rawArray[j + halfSize];
                        }

                        rawArray[i] = (ushort)(totals / (meanSize-1));
                    }
                    else if (i < rawArray.Length - halfSize)
                    {
                        totals = totals - rawArray[i] + rawArray[i-1] - rawArray[i - halfSize - 1] + rawArray[i + halfSize];
                        rawArray[i] = (ushort)(totals / (meanSize - 1));
                    }
                }
            }
        }
    }
}
