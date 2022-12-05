using System.Diagnostics;

namespace GBM8.Core;

public struct FlagModification
{
    private byte _flags;
    private byte _mask;

    public bool GetFlag(StatusFlag flag) => flag switch
    {
        StatusFlag.Z or 
            StatusFlag.N or
            StatusFlag.H or
            StatusFlag.C => (_flags & (1 << (int)flag)) != 0,
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
                    _flags |= (byte)mask;
                else
                    _flags &= (byte)~mask;

                _mask = (byte)mask;
                
                break;
            default:
                throw new UnreachableException();
        }
    }

    public void Reset()
    {
        _mask = 0;
        _flags = 0;
    }
    
    public void Apply(RegisterPage reg)
    {
        reg.F &= (byte)~_mask;
        reg.F |= _flags;
    }

    public FlagModification()
    {
        Reset();
    }
}