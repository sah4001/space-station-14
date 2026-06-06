using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Shared.Chemistry.Components;

namespace Content.Server.Atmos.Piping.Unary.Components;

/// <summary>
/// Used for an entity that converts moles of gas into spesos.
/// </summary>
[RegisterComponent]
[Access(typeof(GasSellerSystem))]
public sealed partial class GasSellerComponent : Component
{
    /// <summary>
    /// The ID for the pipe node.
    /// </summary>
    [DataField]
    public string Inlet = "pipe";

    /// <summary>
    /// For a seller, multiplier for how many spesos are given per each mole of gas.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MolesToSpesoMultiplier = 1.0f;
}
