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
            if (solutionFilling.SummedFrameTime < solutionFilling.MaxFrameTime) // Update every second
                return;
            FixedPoint2 maxAdded = solutionFilling.RefillRate * solutionFilling.SummedFrameTime;
            Log.Info($"Refilling container {uid} with {maxAdded} units of {solutionFilling.Reagent}");
            _solutionContainerSystem.AddSolution((uid, solution), new Solution(solutionFilling.Reagent.ToString(), maxAdded));
            EntityUid parent = Transform(uid).ParentUid;
            if (TryComp<ReagentDispenserComponent>(parent, out var dispenser))
            {
                ReagentDispenserUpdateEvent ev = new ReagentDispenserUpdateEvent();
                RaiseLocalEvent(parent, ev);
            }
            solutionFilling.SummedFrameTime = 0f;
        }
    }
}
