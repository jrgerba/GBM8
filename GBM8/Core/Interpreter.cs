using System.Runtime.Intrinsics.X86;

namespace GBM8.Core;

public class Interpreter
{
    public FlagModification FlagModification { get; private set; }
    
    #region 8-bit alu
    
    public byte Add(byte paramA, byte paramB, int carry)
    {
        int result = paramA + paramB + carry;
        
        FlagModification.SetFlag(StatusFlag.Z, (result & 0xFF) == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((paramA & 0x0F) + (paramB & 0x0F) + carry) & 0x10) == 0x10);
        FlagModification.SetFlag(StatusFlag.C, (result & 0x100) == 0x100);

        return (byte)result;
    }

    public byte And(byte paramA, byte paramB)
    {
        byte result = (byte)(paramA & paramB);
        
        FlagModification.SetFlag(StatusFlag.Z, result == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, true);
        FlagModification.SetFlag(StatusFlag.C, false);

        return result;
    }

    public void Cp(byte paramA, byte paramB)
    {
        int temp = paramA - paramB;
        
        FlagModification.SetFlag(StatusFlag.Z, (temp & 0xFF) == 0);
        FlagModification.SetFlag(StatusFlag.H, true);
        FlagModification.SetFlag(StatusFlag.H, ((paramA & 0x0F) - (paramB & 0x0F) & 0x10) == 0x10);
        FlagModification.SetFlag(StatusFlag.C, (temp & 0x100) == 0x100);
    }

    public byte Dec(byte param)
    {
        byte result = (byte)(param - 1);
        
        FlagModification.SetFlag(StatusFlag.Z, result == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((param & 0x0F) - 1) & 0x10) == 0x10);

        return result;
    }

    public byte Inc(byte param)
    {
        byte result = (byte)(param + 1);
        
        FlagModification.SetFlag(StatusFlag.Z, result == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((param & 0x0F) + 1) & 0x10) == 0x10);

        return result;
    }

    public byte Or(byte paramA, byte paramB)
    {
        paramA |= paramB;

        FlagModification.SetFlag(StatusFlag.Z, paramA == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        FlagModification.SetFlag(StatusFlag.C, false);

        return paramA;
    }

    protected byte Sub(byte paramA, byte paramB, int carry = 0)
    {
        int result = paramA - paramB - carry;

        FlagModification.SetFlag(StatusFlag.Z, (result & 0xFF) == 0);
        FlagModification.SetFlag(StatusFlag.N, true);
        FlagModification.SetFlag(StatusFlag.H, (((paramA & 0x0F) - (paramB & 0x0F) - carry) & 0x10) == 0x10);
        FlagModification.SetFlag(StatusFlag.C, (result & 0x100) == 0x100);

        return (byte)result;
    }

    protected byte Xor(byte paramA, byte paramB)
    {
        paramA ^= paramB;

        FlagModification.SetFlag(StatusFlag.Z, paramA == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        FlagModification.SetFlag(StatusFlag.C, false);

        return paramA;
    }
    
    #endregion

    #region 16-bit alu

    private ushort Add(ushort paramA, ushort paramB)
    {
        int temp = paramA + paramB;

        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((paramA & 0xFFF) + (paramB & 0xFFF)) & 0x1000) == 0x1000);
        FlagModification.SetFlag(StatusFlag.C, (temp & 0x10000) == 0x10000);

        return (ushort)temp;
    }

    private ushort Add(ushort paramA, sbyte paramB)
    {
        int result = paramA + paramB;

        FlagModification.SetFlag(StatusFlag.Z, false);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((paramA & 0x0F) + (paramB & 0x0F)) & 0x10) == 0x10);
        FlagModification.SetFlag(StatusFlag.C, (((paramA & 0xFF) + (paramB & 0xFF)) & 0x100) == 0x100);

        return (ushort)result;
    }

    private ushort Dec(ushort param) => (ushort)(param - 1);

    private ushort Inc(ushort param) => (ushort)(param + 1);
    
    

    #endregion

    #region Bit Ops

    private void Bit(byte param, int bit)
    {
        FlagModification.SetFlag(StatusFlag.Z, (param & (1 << bit)) == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, true);
    }

    private byte Res(byte param, int bit) => (byte)(param & ~(1 << bit));

    private byte Set(byte param, int bit) => (byte)(param | (1 << bit));

    private byte Swap(byte param)
    {
        param = (byte)((param << 4) | (param >> 4));

        FlagModification.SetFlag(StatusFlag.Z, param == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        FlagModification.SetFlag(StatusFlag.C, false);

        return param;
    }
    
    #endregion
}