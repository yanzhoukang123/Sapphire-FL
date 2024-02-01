using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Hywire.DataProcessing
{
    namespace Methods
    {
        static public class FindPosOfMaxValue
        {
            static public void ThresholdProcess(Point[] rawArray, int thresholdPercent, out double positionOfMaxValue, out double valueMax)
            {
                double posOfThresholdLeft = 0;
                double posOfThresholdRight = 0;
                double maxValue = 0;
                double minValue = 1e8;
                double thresholdValue = 0;
                int indexMaxValue = 0;
                int indexLeft = 0;
                int indexRight = 0;
                // find index of max, min values and index of max value
                for (int i = 0; i < rawArray.Length; i++)
                {
                    if (rawArray[i].Y > maxValue)
                    {
                        maxValue = rawArray[i].Y;
                        indexMaxValue = i;
                    }
                    if (rawArray[i].Y < minValue)
                    {
                        minValue = rawArray[i].Y;
                    }
                }
                // calculate threshold value
                thresholdValue = (maxValue - minValue) * thresholdPercent * 0.01 + minValue;
                // find left position of threshold value
                for(int i = indexMaxValue-1; i >= 0; i--)
                {
                    if (rawArray[i].Y <= thresholdValue)
                    {
                        if (rawArray[i].Y == rawArray[i + 1].Y)
                        {
                            posOfThresholdLeft = rawArray[i].X;
                        }
                        else
                        {
                            posOfThresholdLeft = (thresholdValue - rawArray[i].Y) / (rawArray[i + 1].Y - rawArray[i].Y) * (rawArray[i + 1].X - rawArray[i].X) + rawArray[i].X;
                        }
                        indexLeft = i;
                        break;
                    }
                }
                // find right position of threshold value
                for (int i = indexMaxValue+1; i < rawArray.Length; i++)
                {
                    if (rawArray[i].Y <= thresholdValue)
                    {
                        if (rawArray[i].Y == rawArray[i - 1].Y)
                        {
                            posOfThresholdRight = rawArray[i].X;
                        }
                        else
                        {
                            posOfThresholdRight = (thresholdValue - rawArray[i].Y) / (rawArray[i - 1].Y - rawArray[i].Y) * (rawArray[i - 1].X - rawArray[i].X) + rawArray[i].X;
                        }
                        indexRight = i;
                        break;
                    }
                }
                // calculate the real position of max value
                positionOfMaxValue = (posOfThresholdLeft + posOfThresholdRight) / 2;
                valueMax = 0;
                // find the value of the real position
                for (int i = indexLeft+1; i < indexRight; i++)
                {
                    if (positionOfMaxValue <= rawArray[i].X)
                    {
                        valueMax = (positionOfMaxValue - rawArray[i - 1].X) / (rawArray[i].X - rawArray[i - 1].X) * (rawArray[i].Y - rawArray[i - 1].Y) + rawArray[i - 1].Y;
                        break;
                    }
                }
            }
        }
    }
}
