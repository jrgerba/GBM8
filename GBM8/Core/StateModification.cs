namespace GBM8.Core;

public ref struct StateModification
{ 
    public RegisterPage Registers;
    public ReadOnlySpan<(ushort addr, byte val)> Memory;

    public StateModification(RegisterPage reg)
    {
        Registers = reg;
        Memory = ReadOnlySpan<(ushort, byte)>.Empty;
    }
}