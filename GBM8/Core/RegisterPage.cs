using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GBM8.Core;

public struct RegisterPage
{
    public static bool operator ==(RegisterPage regA, RegisterPage regB) =>
        regB.a == regA.a &&
        regB.b == regA.b &&
        regB.c == regA.c &&
        regB.d == regA.d &&
        regB.e == regA.e &&
        regB.f == regA.f &&
        regB.h == regA.h &&
        regB.l == regA.l;

    public static bool operator !=(RegisterPage regA, RegisterPage regB) => !(regA == regB);

    private byte a, b, c, d, e, f, h, l;

    public byte A
    {
        get => a;
        set => a = value;
    }
    public byte B
    {
        get => b;
        set => b = value;
    }
    public byte C
    {
        get => c;
        set => c = value;
    }
    public byte D
    {
        get => d;
        set => d = value;
    }
    public byte E
    {
        get => e;
        set => e = value;
    }
    public byte F
    {
        get => f;
        // Lower nibble of F is tied to 0
        set => f = value &= 0xF0;
    }
    public byte H
    {
        get => h;
        set => h = value;
    }
    public byte L
    {
        get => l;
        set => l = value;
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
        StatusFlag.C => (f & (1 << (int)flag)) != 0,
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
                    f |= (byte)mask;
                else
                    f &= (byte)~mask;

                break;
            default:
                throw new UnreachableException();
        }
    }
}