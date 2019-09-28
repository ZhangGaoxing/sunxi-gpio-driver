// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    [DllImport(LibcLibrary, SetLastError = true)]
    internal static extern IntPtr mmap(IntPtr addr, int length, MemoryMappedProtections prot, MemoryMappedFlags flags, int fd, int offset);
}

[Flags]
internal enum MemoryMappedProtections
{
    PROT_NONE = 0x0,
    PROT_READ = 0x1,
    PROT_WRITE = 0x2,
    PROT_EXEC = 0x4
}

[Flags]
internal enum MemoryMappedFlags
{
    MAP_SHARED = 0x01,
    MAP_PRIVATE = 0x02,
    MAP_FIXED = 0x10
}

internal enum AllwinnerRegister
{
    /// <summary>
    /// Port A Configure Register 0(PA0 - PA7, 32bit). Default Value 0x77777777.
    /// </summary>
    PA_CFG0_REG = 0x00,
    /// <summary>
    /// Port A Configure Register 1(PA8 - PA15, 32bit)
    /// </summary>
    PA_CFG1_REG = 0x04,
    /// <summary>
    /// Port A Configure Register 2(PA16 - PA21, 32bit). Default Value 0x00777777.
    /// </summary>
    PA_CFG2_REG = 0x08,
    /// <summary>
    /// Port A PULL Register 0(PA0 - PA15, 32bit). Default Value 0x00000000.
    /// </summary>
    PA_PULL0_REG = 0x1C,
    /// <summary>
    /// Port A PULL Register 1(PA16 - PA21, 32bit). Default Value 0x00000000.
    /// </summary>
    PA_PULL1_REG = 0x20,

    /// <summary>
    /// Port C Configure Register 0(PC0 - PC7, 32bit). Default Value 0x77777777.
    /// </summary>
    PC_CFG0_REG = 0x48,
    /// <summary>
    /// Port C Configure Register 1(PC8 - PC15, 32bit)
    /// </summary>
    PC_CFG1_REG = 0x4C,
    /// <summary>
    /// Port C Configure Register 2(PC16, 32bit). Default Value 0x00000777.
    /// </summary>
    PC_CFG2_REG = 0x50,
    /// <summary>
    /// Port C PULL Register 0(PC0 - PC15, 32bit). Default Value 0x00005140.
    /// </summary>
    PC_PULL0_REG = 0x1C,
    /// <summary>
    /// Port C PULL Register 1(PC16 - PC18, 32bit). Default Value 0x00000014.
    /// </summary>
    PC_PULL1_REG = 0x20,

    /// <summary>
    /// Port D Configure Register 0(PD0 - PD7, 32bit). Default Value 0x77777777.
    /// </summary>
    PD_CFG0_REG = 0x6C,
    /// <summary>
    /// Port D Configure Register 1(PD8 - PD15, 32bit)
    /// </summary>
    PD_CFG1_REG = 0x70,
    /// <summary>
    /// Port D Configure Register 2(PD16 - PD17, 32bit). Default Value 0x00000077.
    /// </summary>
    PD_CFG2_REG = 0x74,
    /// <summary>
    /// Port D PULL Register 0(PD0 - PD15, 32bit). Default Value 0x00000000.
    /// </summary>
    PD_PULL0_REG = 0x88,
    /// <summary>
    /// Port D PULL Register 1(PD16 - PD17, 32bit). Default Value 0x00000000.
    /// </summary>
    PD_PULL1_REG = 0x8C,

    /// <summary>
    /// Port E Configure Register 0(PE0 - PE7, 32bit). Default Value 0x77777777.
    /// </summary>
    PE_CFG0_REG = 0x90,
    /// <summary>
    /// Port E Configure Register 1(PE8 - PE15, 32bit)
    /// </summary>
    PE_CFG1_REG = 0x94,
    /// <summary>
    /// Port E PULL Register 0(PE0 - PE15, 32bit). Default Value 0x00000000.
    /// </summary>
    PE_PULL0_REG = 0xAC,

    /// <summary>
    /// Port F Configure Register 0(PF0 - PF6, 32bit). Default Value 0x07373733.
    /// </summary>
    PF_CFG0_REG = 0xB4,
    /// <summary>
    /// Port F PULL Register 0(PF0 - PF6, 32bit). Default Value 0x00000000.
    /// </summary>
    PF_PULL0_REG = 0xD0,

    /// <summary>
    /// Port G Configure Register 0(PG0 - PG7, 32bit). Default Value 0x77777777.
    /// </summary>
    PG_CFG0_REG = 0xD8,
    /// <summary>
    /// Port G Configure Register 1(PG8 - PG13, 32bit)
    /// </summary>
    PG_CFG1_REG = 0xDC,
    /// <summary>
    /// Port G PULL Register 0(PG0 - PG13, 32bit). Default Value 0x00000000.
    /// </summary>
    PG_PULL0_REG = 0xF4,

    /// <summary>
    /// Port L Configure Register 0(PL0 - PL7, 32bit). Default Value 0x77777777.
    /// </summary>
    PL_CFG0_REG = 0x00,
    /// <summary>
    /// Port L Configure Register 1(PL8 - PL11, 32bit)
    /// </summary>
    PL_CFG1_REG = 0x04,
    /// <summary>
    /// Port L PULL Register 0(PL0 - PL11, 32bit). Default Value 0x00000000.
    /// </summary>
    PL_PULL0_REG = 0x1C,
}

/// <summary>
/// The BCM GPIO registers expose the data/direction/interrupt/etc functionality of pins.
/// Each register is 64 bits, where each bit represents a logical register number.
/// 
/// For example, writing HIGH to register 20 would translate to (registerViewPointer).GPSET[0] | (1U &lt;&lt; 20).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal unsafe struct RegisterView
{
    ///<summary>GPIO Function Select, 6x32 bits, R/W.</summary>
    [FieldOffset(0x00)]
    public fixed uint GPFSEL[6];

    ///<summary>GPIO Pin Output Set, 2x32 bits, W.</summary>
    [FieldOffset(0x1C)]
    public fixed uint GPSET[2];

    ///<summary>GPIO Pin Output Clear, 2x32 bits, W.</summary>
    [FieldOffset(0x28)]
    public fixed uint GPCLR[2];

    ///<summary>GPIO Pin Level, 2x32 bits, R.</summary>
    [FieldOffset(0x34)]
    public fixed uint GPLEV[2];

    ///<summary>GPIO Pin Event Detect Status, 2x32 bits, R/W.</summary>
    [FieldOffset(0x40)]
    public fixed uint GPEDS[2];

    ///<summary>GPIO Pin Rising Edge Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x4C)]
    public fixed uint GPREN[2];

    ///<summary>GPIO Pin Falling Edge Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x58)]
    public fixed uint GPFEN[2];

    ///<summary>GPIO Pin High Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x64)]
    public fixed uint GPHEN[2];

    ///<summary>GPIO Pin Low Detect Enable, 2x32 bits, R/W.</summary>
    [FieldOffset(0x70)]
    public fixed uint GPLEN[2];

    ///<summary>GPIO Pin Async. Rising Edge Detect, 2x32 bits, R/W.</summary>
    [FieldOffset(0x7C)]
    public fixed uint GPAREN[2];

    ///<summary>GPIO Pin Async. Falling Edge Detect, 2x32 bits, R/W.</summary>
    [FieldOffset(0x88)]
    public fixed uint GPAFEN[2];

    ///<summary>GPIO Pin Pull-up/down Enable, 32 bits, R/W.</summary>
    [FieldOffset(0x94)]
    public uint GPPUD;

    ///<summary>GPIO Pin Pull-up/down Enable Clock, 2x32 bits, R/W.</summary>
    [FieldOffset(0x98)]
    public fixed uint GPPUDCLK[2];
}
