namespace GBM8.Core;

public ref struct StateModification
{
    public static bool operator ==(StateModification smA, StateModification smB)
    {
        if (smA.Registers != smB.Registers) 
            return false;

        if (smA.Memory.Length != smB.Memory.Length)
            return false;

        for (int i = 0; i < smA.Memory.Length; i++)
            if (smA.Memory[i].addr != smB.Memory[i].addr && smA.Memory[i].val != smB.Memory[i].val)
                return false;

        return true;
    }

    public static bool operator !=(StateModification smA, StateModification smB) => !(smA == smB); 

    public RegisterPage Registers;
    public ReadOnlySpan<(ushort addr, byte val)> Memory;

    public StateModification(RegisterPage reg)
    {
        Registers = reg;
        Memory = ReadOnlySpan<(ushort, byte)>.Empty;
    }
}