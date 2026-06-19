using System.Linq;
using Content.Server.Chemistry.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Storage.EntitySystems;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Content.Shared.Labels.Components;
using Content.Shared.Storage;
using Content.Server.Hands.Systems;
using Content.Shared.Chemistry.Components;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for refilling containers.
    /// <seealso cref="RefillingContainerComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed partial class RefillingContainerSystem : EntitySystem
    {
        [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<SolutionFillingComponent, SolutionComponent>();
            while (query.MoveNext(out var uid, out var solutionFilling, out var solution))
            {
                Update(uid, solutionFilling, solution, frameTime);
            }
        }

        private void Update(EntityUid uid, SolutionFillingComponent solutionFilling, SolutionComponent solution, float frameTime)
        {
            // Skip systems that have a refil rate of 0
            if (solutionFilling.RefillRate == 0)
                return;
            if (solution.Solution.AvailableVolume == 0)
                return;
            solutionFilling.SummedFrameTime += frameTime;
            if (solutionFilling.SummedFrameTime < solutionFilling.MaxFrameTime)
                return;
            FixedPoint2 amountAdded = FixedPoint2.Min(solutionFilling.RefillRate * solutionFilling.SummedFrameTime, solution.Solution.AvailableVolume);

            FixedPoint2 baseEnergyCost = amountAdded * solutionFilling.EnergyPerUnit;
            FixedPoint2 spesosCost = amountAdded * solutionFilling.CostPerUnit;

            bool allowFilling = true;
            EntityUid parent = Transform(uid).ParentUid;
            if (baseEnergyCost > 0)
            {
                // Consume energy
                if (TryComp<SolutionFillerComponent>(parent, out var filler))
                {
                    Log.Info($"Consuming energy from {parent} with multiplier {filler.EnergyConsumption} and total charge {filler.TotalCharge}/{filler.MaxCharge}");
                    if (filler.TotalCharge >= baseEnergyCost * filler.EnergyConsumption)
                    {
                        filler.TotalCharge -= baseEnergyCost * filler.EnergyConsumption;
                    }
                    else
                    {
                        Log.Info($"Not enough energy in {parent}. Required: {baseEnergyCost * filler.EnergyConsumption}, Available: {filler.TotalCharge}");
                        allowFilling = false;
                    }
                }
                else
                {
                    allowFilling = false;
                }
            }

            if (allowFilling)
            {
                Log.Info($"Refilling container {uid} with {amountAdded} units of {solutionFilling.Reagent} for {baseEnergyCost} energy and {spesosCost} spesos");
                _solutionContainerSystem.AddSolution((uid, solution), new Solution(solutionFilling.Reagent.ToString(), amountAdded));
                if (TryComp<ReagentDispenserComponent>(parent, out var dispenser))
                {
                    ReagentDispenserUpdateEvent ev = new ReagentDispenserUpdateEvent();
                    RaiseLocalEvent(parent, ev);
                }
            }
            solutionFilling.SummedFrameTime = 0f;
        }
    }
}
