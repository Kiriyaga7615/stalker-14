using Content.Server.Lightning;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared._Stalker.ZoneAnomaly.Components;
using Content.Shared._Stalker.ZoneAnomaly.Effects.Components;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Map.Components;

namespace Content.Server._Stalker.ZoneAnomaly.Effects.Systems;

public sealed class ZoneAnomalyEffectLightArcSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ZoneAnomalyEffectLightArcComponent, ZoneAnomalyActivateEvent>(OnActivate);
    }

    private void OnActivate(Entity<ZoneAnomalyEffectLightArcComponent> effect, ref ZoneAnomalyActivateEvent args)
    {
        var entities = _lookup.GetEntitiesInRange(Transform(effect).Coordinates, effect.Comp.Distance);
        foreach (var entity in entities)
        {
            // We don't need to shoot all the entities
            if(_whitelistSystem.IsWhitelistPass(effect.Comp.Whitelist, entity))
                continue;

            // Fixes 10 million shots being fired at one entity due to it containing targets
            if (IsValidRecursively(effect, entity))
                continue;

            TryRecharge(effect, entity);
            TryChangeState(effect, entity);

            _lightning.ShootLightning(effect, entity, effect.Comp.Lighting);
        }
    }

    private void TryRecharge(Entity<ZoneAnomalyEffectLightArcComponent> effect, EntityUid target)
    {
        if (!TryComp<BatteryComponent>(target, out var battery))
            return;

        _battery.SetCharge(target, battery.CurrentCharge + battery.MaxCharge * effect.Comp.ChargePercent, battery);
    }

    private void TryChangeState(Entity<ZoneAnomalyEffectLightArcComponent> effect, EntityUid target)
    {
        if (!TryComp<DamageableComponent>(target, out var damageable))
            return;

        if (!_mobThreshold.TryGetThresholdForState(target, MobState.Dead, out var threshold))
            return;

        if (damageable.TotalDamage >= threshold)
            return;

        _mobState.ChangeMobState(target, MobState.Critical);
    }

    private bool IsValidRecursively(Entity<ZoneAnomalyEffectLightArcComponent> effect, EntityUid uid)
    {
        var parent = Transform(uid).ParentUid;
        if (HasComp<MapComponent>(parent) || HasComp<MapGridComponent>(parent))
            return false;

        if (_whitelistSystem.IsWhitelistPass(effect.Comp.Whitelist, parent))
            return true;

        return IsValidRecursively(effect, parent);
    }
}
