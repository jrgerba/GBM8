using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace GBM8.Core;

public class Interpreter
{
    private Queue<MemoryOperation> _operationQueue = new();
    private int _tCycleWait = 0;
    private int _mCycleWait = 0;
    private bool _imeWait = false;
    public FlagModification FlagModification { get; }
    public int WaitTime
    {
        get => _mCycleWait;
        private set
        {
            _mCycleWait = value;
            _tCycleWait = value * 4;
        }
    }
    public bool Ready => WaitTime <= 0;
    public bool IME { get; private set; }

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

    #region Bit Shift Ops

    private byte Rl(byte param, int carry, bool setZ = false)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b1000_0000) == 0b1000_0000);

        param = (byte)((param << 1) | carry);

        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        
        FlagModification.SetFlag(StatusFlag.Z, setZ && param == 0);

        return param;
    }

    private byte Rlc(byte param, bool setZ = false)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b1000_0000) == 0b1000_0000);

        param = byte.RotateLeft(param, 1);

        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);

        FlagModification.SetFlag(StatusFlag.Z, setZ && param == 0);

        return param;
    }

    private byte Rr(byte param, int carry, bool setZ)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b0000_0001) == 0b0000_0001);

        param = (byte)((param >> 1) | (carry << 7));

        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        
        FlagModification.SetFlag(StatusFlag.Z, setZ && param == 0);

        return param;
    }

    private byte Rrc(byte param, bool setZ)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b0000_0001) == 0b0000_0001);

        param = byte.RotateRight(param, 1);

        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        
        FlagModification.SetFlag(StatusFlag.Z, setZ && param == 0);

        return param;
    }

    private byte Sla(byte param)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b1000_0000) == 0b1000_0000);

        param <<= 1;

        FlagModification.SetFlag(StatusFlag.Z, param == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);

        return param;
    }

    private byte Sra(byte param)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b0000_0001) == 0b0000_0001);

        param = (byte)((param >> 1) | (param & 0b1000_0000));
        
        FlagModification.SetFlag(StatusFlag.Z, param == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);

        return param;
    }

    private byte Srl(byte param)
    {
        FlagModification.SetFlag(StatusFlag.C, (param & 0b0000_0001) == 0b0000_0001);

        param >>= 1;

        FlagModification.SetFlag(StatusFlag.Z, param == 0);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);

        return param;
    }

    #endregion

    #region Control Flow

    private static bool EvaluateBranchCondition(BranchCondition cc, RegisterPage reg) => cc switch
    {
        BranchCondition.C => reg.FlagC,
        BranchCondition.Nc => !reg.FlagC,
        BranchCondition.Z => reg.FlagZ,
        BranchCondition.Nz => !reg.FlagZ,
        BranchCondition.None or _ => throw new UnreachableException()
    };
    
    private (ushort pc, ushort? push) Call(BranchCondition cc, RegisterPage reg, ushort addr)
    {
        if (cc == BranchCondition.None)
            return (addr, reg.PC);

        if (!EvaluateBranchCondition(cc, reg))
            return (reg.PC, null);

        WaitTime += 3;
        return (addr, reg.PC);

    }

    private ushort Jp(BranchCondition cc, RegisterPage reg, ushort addr)
    {
        if (cc == BranchCondition.None)
            return addr;

        if (!EvaluateBranchCondition(cc, reg))
            return reg.PC;

        WaitTime++;
        return addr;
    }

    private ushort Jr(BranchCondition cc, RegisterPage reg, sbyte offset)
    {
        if (cc == BranchCondition.None)
            return (ushort)(reg.PC + offset);

        if (!EvaluateBranchCondition(cc, reg))
            return reg.PC;

        WaitTime++;
        return (ushort)(reg.PC + offset);
    }

    private bool Ret(BranchCondition cc, RegisterPage reg, bool allowInterrupts)
    {
        if (allowInterrupts)
            IME = true;

        return cc == BranchCondition.None || EvaluateBranchCondition(cc, reg);
    }

    private (ushort addr, ushort? push) Rst(int vec) =>
        Call(BranchCondition.None, new RegisterPage(), (ushort)(vec * 8));

    #endregion

    #region Stack Ops

    private ushort AddSP(ushort paramA, sbyte paramB)
    {
        FlagModification.SetFlag(StatusFlag.Z, false);
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, (((paramA & 0x0F) + (paramB & 0x0F)) & 0x10) == 0x10);
        FlagModification.SetFlag(StatusFlag.C, (((paramA & 0xFF) + (paramB & 0xFF)) & 0x100) == 0x100);

        return (ushort)(paramA + paramB);
    }

    #endregion

    #region Misc

    private void Ccf(bool flagC)
    {
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        FlagModification.SetFlag(StatusFlag.C, !flagC);
    }

    private byte Cpl(byte param)
    {
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);

        return (byte)~param;
    }

    private void Di() => IME = false;

    public void Ei() => _imeWait = true;

    public void Halt()
    {
        // Todo: Implement this
    }

    public void Scf()
    {
        FlagModification.SetFlag(StatusFlag.N, false);
        FlagModification.SetFlag(StatusFlag.H, false);
        FlagModification.SetFlag(StatusFlag.C, true);
    }
    
    #endregion
}