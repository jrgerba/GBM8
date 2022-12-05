namespace GBM8.Core;

public class Interpreter
{
    protected (ushort, byte)[] memWrites = new (ushort, byte)[2];

    #region 8-bit alu
    public StateModification ADC(RegisterPage reg, byte param)
    {
        int lo = (reg.A & 0x0F) + (param & 0x0F) + (reg.GetFlag(StatusFlag.C) ? 1 : 0);

        reg.SetFlag(StatusFlag.H, (lo & 0x10) != 0);

        int full = lo + (reg.A & 0xF0) + (param & 0xF0);

        reg.SetFlag(StatusFlag.C, (full & 0x100) != 0);
        
        reg.A = (byte)full;

        reg.FlagN = false;
        reg.FlagZ = reg.A == 0;

        return new StateModification(reg);
    }

    public StateModification ADD8Bit(RegisterPage reg, byte param)
    {
        int lo = (reg.A & 0x0F) + (param & 0x0F);

        reg.FlagH = (lo & 0x10) != 0;

        int full = lo + (reg.A & 0xF0) + (param & 0xF0);

        reg.FlagC = (full & 0x100) != 0;

        reg.A = (byte)full;

        reg.FlagN = false;
        reg.FlagZ = reg.A == 0;

        return new StateModification(reg);
    }

    public StateModification AND(RegisterPage reg, byte param)
    {
        reg.A &= param;

        reg.FlagZ = reg.A == 0;
        reg.FlagN = false;
        reg.FlagH = true;
        reg.FlagC = false;

        return new StateModification(reg);
    }

    public StateModification CP(RegisterPage reg, byte param)
    {
        int lo = (reg.A & 0x0F) - (param & 0x0F);
        int full = reg.A - param;

        reg.FlagZ = (full & 0xFF) == 0;
        reg.FlagN = true;
        reg.FlagH = (lo & 0x10) != 0;
        reg.FlagC = (full & 0x100) != 0;

        return new StateModification(reg);
    }

    public StateModification DEC(RegisterPage reg, Register8 r)
    {
        byte param = reg.GetRegister(r);

        int lo = (param & 0x0F) - 1;
        reg.SetRegister(r, (byte)(param - 1));

        reg.FlagZ = reg.GetRegister(r) == 0;
        reg.FlagN = true;
        reg.FlagH = (lo & 0x10) != 0;

        return new StateModification(reg);
    }

    public StateModification DEC(RegisterPage reg, ushort addr, byte param)
    {
        int lo = (param & 0x0F) - 1;
        param -= 1;

        reg.FlagZ = param == 0;
        reg.FlagN = true;
        reg.FlagH = (lo & 0x10) != 0;

        memWrites[0] = (addr, param);

        return new StateModification
        {
            Registers = reg,
            Memory = new ReadOnlySpan<(ushort, byte)>(memWrites, 0, 1)
        };
    }

    protected StateModification INC(RegisterPage reg, Register8 r)
    {
        byte param = reg.GetRegister(r);

        int lo = (param & 0x0F) + 1;
        reg.SetRegister(r, (byte)(param + 1));

        reg.FlagZ = reg.GetRegister(r) == 0;
        reg.FlagN = false;
        reg.FlagH = (lo & 0x10) != 0;

        return new StateModification(reg);
    }

    protected StateModification INC(RegisterPage reg, ushort addr, byte param)
    {
        int lo = (param & 0x0F) + 1;
        param += 1;

        reg.FlagZ = (param & 0xFF) == 0;
        reg.FlagN = false;
        reg.FlagH = (lo & 0x10) != 0;

        memWrites[0] = (addr, param);
        
        return new StateModification
        {
            Registers = reg,
            Memory = new ReadOnlySpan<(ushort, byte)>(memWrites, 0, 1)
        };
    }

    protected StateModification OR(RegisterPage reg, byte param)
    {
        reg.A |= param;

        reg.FlagZ = reg.A == 0;
        reg.FlagN = false;
        reg.FlagH = false;
        reg.FlagC = false;

        return new StateModification(reg);
    }

    protected StateModification SBC(RegisterPage reg, byte param)
    {
        int carry = reg.GetFlag(StatusFlag.C) ? 1 : 0;
        int lo = (reg.A & 0x0F) - (param & 0x0F) - carry;
        int full = reg.A - param - carry;
        reg.A = (byte)full;

        reg.FlagZ = reg.A == 0;
        reg.FlagN = true;
        reg.FlagH = (lo & 0x10) != 0;
        reg.FlagC = (full & 0x100) != 0;

        return new StateModification(reg);
    }

    protected StateModification SUB(RegisterPage reg, byte param)
    {
        int lo = (reg.A & 0x0F) - (param & 0x0F);
        int full = reg.A - param;
        reg.A = (byte)full;

        reg.FlagZ = reg.A == 0;
        reg.FlagN = true;
        reg.FlagH = (lo & 0x10) != 0;
        reg.FlagC = (full & 0x100) != 0;

        return new StateModification(reg);
    }

    protected StateModification XOR(RegisterPage reg, byte param)
    {
        reg.A ^= param;

        reg.FlagZ = reg.A == 0;
        reg.FlagN = false;
        reg.FlagH = false;
        reg.FlagC = false;

        return new StateModification(reg);
    }
    #endregion

    #region 16-bit alu

    private StateModification ADD16Bit(RegisterPage reg, ushort param)
    {
        reg.FlagN = false;
        reg.FlagH = (((reg.HL & 0xFFF) + (param & 0xFFF)) & 0x1000) == 0x1000;
        reg.FlagC = ((reg.HL + param) & 0x1_0000) == 0x1_0000;

        reg.HL += param;
        
        return new StateModification(reg);
    }

    private StateModification AddSP(RegisterPage reg, sbyte param)
    {
        reg.FlagZ = false;
        reg.FlagN = false;
        reg.FlagH = (((reg.SP & 0x0F) + (param & 0x0F)) & 0x10) == 0x10;
        reg.FlagC = (((reg.SP & 0xFF) + (param & 0xFF)) & 0x100) == 0x100;

        reg.SP = (ushort)(reg.SP + param);

        return new StateModification(reg);
    }

    #endregion
}