using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GBM8.Core;

public struct RegisterPage
{
    private byte _a, _b, _c, _d, _e, _f, _h, _l;

    public byte A
    {
        get => _a;
        set => _a = value;
    }
    public byte B
    {
        get => _b;
        set => _b = value;
    }
    public byte C
    {
        get => _c;
        set => _c = value;
    }
    public byte D
    {
        get => _d;
        set => _d = value;
    }
    public byte E
    {
        get => _e;
        set => _e = value;
    }
    public byte F
    {
        get => _f;
        // Lower nibble of F is tied to 0
        set => _f = value &= 0xF0;
    }
    public byte H
    {
        get => _h;
        set => _h = value;
    }
    public byte L
    {
        get => _l;
        set => _l = value;
    }

    public ushort AF
    {
        get => (ushort)((A << 8) | F);
        set
        {
            A = (byte)(value >> 8);
            F = (byte)value;
        }
    }
    public ushort BC
    {
        get => (ushort)((B << 8) | C);
        set
        {
            B = (byte)(value >> 8);
            C = (byte)value;
        }
    }
    public ushort DE
    {
        get => (ushort)((D << 8) | E);
        set
        {
            D = (byte)(value >> 8);
            E = (byte)value;
        }
    }
    public ushort HL
    {
        get => (ushort)((H << 8) | L);
        set
        {
            H = (byte)(value >> 8);
            L = (byte)value;
        }
    }
    
    public ushort SP { get; set; }
    
    public ushort PC { get; set; }

    public bool FlagZ
    {
        get => GetFlag(StatusFlag.Z);
        set => SetFlag(StatusFlag.Z, value);
    }

    public bool FlagN
    {
        get => GetFlag(StatusFlag.N);
        set => SetFlag(StatusFlag.N, value);
    }

    public bool FlagH
    {
        get => GetFlag(StatusFlag.H);
        set => SetFlag(StatusFlag.H, value);
    }

    public bool FlagC
    {
        get => GetFlag(StatusFlag.C);
        set => SetFlag(StatusFlag.C, value);
    }

    public byte GetRegister(Register8 r) => r switch
    {
        Register8.A => A,
        Register8.B => B,
        Register8.C => C,
        Register8.D => D,
        Register8.E => E,
        Register8.F => F,
        Register8.H => H,
        Register8.L => L,
        _ => throw new UnreachableException()
    };

    public ushort GetRegister(Register16 r) => r switch
    {
        Register16.AF => AF,
        Register16.BC => BC,
        Register16.DE => DE,
        Register16.HL => HL,
        _ => throw new UnreachableException(),
    };

    public void SetRegister(Register8 r, byte value)
    {
        switch (r)
        {
            case Register8.A:
                A = value; 
                break;
            case Register8.B: 
                B = value;
                break;
            case Register8.C:
                C = value;
                break;
            case Register8.D:
                D = value;
                break;
            case Register8.E:
                E = value;
                break;
            case Register8.F:
                F = value;
                break;
            case Register8.H:
                H = value;
                break;
            case Register8.L: 
                L = value;
                break;
            default:
                throw new UnreachableException();
        }
    }

    public void SetRegister(Register16 r, ushort value)
    {
        switch (r)
        {
            case Register16.AF:
                AF = value;
                break;
            case Register16.BC:
                BC = value;
                break;
            case Register16.DE:
                DE = value;
                break;
            case Register16.HL:
                HL = value;
                break;
            default:
                throw new UnreachableException();
        }
    }

    public bool GetFlag(StatusFlag flag) => flag switch
    {
        StatusFlag.Z or 
        StatusFlag.N or
        StatusFlag.H or
        StatusFlag.C => (_f & (1 << (int)flag)) != 0,
        _ => throw new UnreachableException()
    };

    public void SetFlag(StatusFlag flag, bool value)
    {
        switch (flag)
        {
            case StatusFlag.Z:
            case StatusFlag.N:
            case StatusFlag.H:
            case StatusFlag.C:
                int mask = 1 << (int)flag;

                if (value)
                    _f |= (byte)mask;
                else
                    _f &= (byte)~mask;

                break;
            default:
                throw new UnreachableException();
        }
    }
}