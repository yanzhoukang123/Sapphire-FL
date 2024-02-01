using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Hats.APDCom
{
    /// <summary>
    /// Struct to byte[]
    /// </summary>
    public static class StructConverter
    {
        public static Byte[] StructToBytes(Object structure)
        {
            Int32 size = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(size);

            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                Byte[] bytes = new Byte[size];
                Marshal.Copy(buffer, bytes, 0, size);

                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

        }

        //2、byte[] to Struct
        public static Object BytesToStruct(Byte[] bytes, Type strcutType)
        {
            Int32 size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Object result = new object() ;

            try
            {
                Marshal.Copy(bytes, 0, buffer, size);

                result=Marshal.PtrToStructure(buffer, strcutType);
            }
            catch
            {
                
             }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return result;

        }

    }
}
