using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using JetBrains.Annotations;
using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Components;
using Content.Server.Station.Systems;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems;

[UsedImplicitly]
public sealed partial class GasSellerSystem : EntitySystem
{
    [Dependency] private AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private PowerReceiverSystem _power = default!;
    [Dependency] private NodeContainerSystem _nodeContainer = default!;
    [Dependency] private CargoSystem _cargo = default!;
    [Dependency] private StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasSellerComponent, AtmosDeviceUpdateEvent>(OnSellerUpdated);
    }

    private void OnSellerUpdated(Entity<GasSellerComponent> entity, ref AtmosDeviceUpdateEvent args)
    {
        if (!(TryComp<ApcPowerReceiverComponent>(entity, out var receiver) && _power.IsPowered(entity, receiver))
            || !_nodeContainer.TryGetNode(entity.Owner, entity.Comp.Inlet, out PipeNode? inlet))
        {
            return;
        }

        if (inlet.Air.TotalMoles < 100)
            return;

        if (_station.GetOwningStation(entity.Owner) is not { } station ||
            !TryComp<StationBankAccountComponent>(station, out var bankAccount))
        {
            return;
        }

        var molesToConvert = inlet.Air.TotalMoles;
        var removed = inlet.Air.Remove(molesToConvert);
        float value = 0;
        for (var i = 0; i < Atmospherics.TotalNumberOfGases; i++)
        {
            var moles = removed[i];
            if (moles <= 0)
                continue;

            var moleToSpesoMultiplier = entity.Comp.MolesToSpesoMultiplier;
            var amount = moles * moleToSpesoMultiplier * _atmosphereSystem.GetGas(i).PricePerMole;
            if (amount <= 0)
                continue;
            value += amount;
        }
        if (value > 0)
        {
            var baseDistribution = _cargo.CreateAccountDistribution((station, bankAccount));
            _cargo.UpdateBankAccount((station, bankAccount), (int) Math.Round(value), baseDistribution, false);
            Dirty(station, bankAccount);
        }
    }
}
