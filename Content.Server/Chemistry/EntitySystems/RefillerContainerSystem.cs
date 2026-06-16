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
using Content.Shared.Power.Components;
using Content.Server.Power.Components;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for refilling containers.
    /// <seealso cref="RefillingContainerComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed partial class RefillerContainerSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<SolutionFillerComponent>();
            while (query.MoveNext(out var uid, out var solutionFiller))
            {
                Update(uid, solutionFiller, frameTime);
            }
        }

        private void Update(EntityUid uid, SolutionFillerComponent solutionFiller, float frameTime)
        {
            // If full skip
            FixedPoint2 missingCharge = solutionFiller.MaxCharge - solutionFiller.TotalCharge;
            if (missingCharge <= 0)
            {
                if (TryComp<ApcPowerReceiverComponent>(uid, out var receiver))
                {
                    receiver.Load = 1;
                }
                return;
            }
            // If not a frame cycle wait
            solutionFiller.SummedFrameTime += frameTime;
            if (solutionFiller.SummedFrameTime < solutionFiller.MaxFrameTime)
                return;

            FixedPoint2 addingCharge = FixedPoint2.Min(missingCharge, solutionFiller.EnergyGain * solutionFiller.SummedFrameTime);
            solutionFiller.TotalCharge += addingCharge;
            Log.Info($"Adding charge to {uid}. New charge: {solutionFiller.TotalCharge}/{solutionFiller.MaxCharge}");
            if (TryComp<ApcPowerReceiverComponent>(uid, out var receiver2))
            {
                receiver2.Load = (int)(addingCharge / solutionFiller.SummedFrameTime);
            }

            solutionFiller.SummedFrameTime = 0f;
        }
    }
}
