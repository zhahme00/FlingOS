﻿#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;

namespace Kernel.Utilities
{
    [Compiler.PluggedClass]
    public static unsafe class MemoryUtils
    {
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void MemCpy_32(byte* dest, byte* src, uint length)
        {
            for (uint i = 0; i < length; i++)
            {
                dest[i] = src[i];
            }
        }
        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void MemCpy(byte* dest, byte* src, ulong length)
        {
            for(ulong i = 0; i < length; i++)
            {
                dest[i] = src[i];
            }
        }

        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static void* ZeroMem(void* ptr, uint size)
        {
            byte* bPtr = (byte*)ptr;
            byte* bEndPtr = ((byte*)ptr) + size;
            while (bPtr < bEndPtr)
            {
                *bPtr++ = 0;
            }
            return ptr;
        }

        [Compiler.NoGC]
        [Compiler.NoDebug]
        public static byte GetField(byte* addr, byte byteNum, byte shift, byte len) 
        {
            return (byte)((addr[byteNum] >> (shift)) & ((1 << len) - 1));
        }

        [Compiler.PluggedMethod(ASMFilePath=@"ASM\MemoryUtils")]
        public static uint htonl(uint aByte)
        {
            return 0;
        }
    }
}
