using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Materials;
using Content.Shared.Temperature.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry.Components;


[RegisterComponent, NetworkedComponent]
public sealed partial class SolutionFillingComponent : Component
{
    /// <summary>
    /// What reagent is being filled.
    /// </summary>
    [IncludeDataField]
    [ViewVariables]
    public ReagentId Reagent { get; private set; }

    /// <summary>
    /// The rate at which the solution is being filled.
    /// </summary>
    [DataField("RefillRate")]
    public FixedPoint2 RefillRate = 0;

    /// <summary>
    /// The energy cost per unit of the reagent.
    /// </summary>
    [DataField]
    public FixedPoint2 EnergyPerUnit = 0;

    /// <summary>
    /// The spesos cost per unit of the reagent.
    /// </summary>
    [DataField]
    public FixedPoint2 CostPerUnit = 0;

    [DataField]
    public float MaxFrameTime = 1f;

    public float SummedFrameTime = 0f;
}

