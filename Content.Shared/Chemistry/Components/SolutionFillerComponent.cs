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
public sealed partial class SolutionFillerComponent : Component
{
    [DataField]
    public FixedPoint2 EnergyConsumption = 1f;
    [DataField]
    public FixedPoint2 TotalCharge = 0f;
    [DataField]
    public FixedPoint2 MaxCharge = 100f;
    [DataField]
    public FixedPoint2 EnergyGain = 2.0f;

    [DataField]
    public float MaxFrameTime = 2f;

    public float SummedFrameTime = 0f;
}

