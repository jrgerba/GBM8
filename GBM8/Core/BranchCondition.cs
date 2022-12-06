namespace GBM8.Core;

public enum BranchCondition
{
    /// <summary>
    /// Execute if Z flag is set
    /// </summary>
    Z,
    /// <summary>
    /// Execute if Z flag is reset
    /// </summary>
    Nz,
    /// <summary>
    /// Execute if C flag is set
    /// </summary>
    C,
    /// <summary>
    /// Execute if C flag is reset
    /// </summary>
    Nc,
    
    None
}