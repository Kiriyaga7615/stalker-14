﻿using Content.Shared._Stalker.Anomaly.Triggers.Events;
using Content.Shared.Damage;

namespace Content.Server._Stalker.Anomaly.Effects.Damage;

public sealed class STAnomalyDamageEffectSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STAnomalyDamageEffectComponent, STAnomalyTriggerEvent>(OnTriggered);
    }

    private void OnTriggered(Entity<STAnomalyDamageEffectComponent> effect, ref STAnomalyTriggerEvent args)
    {
        foreach (var group in args.Groups)
        {
            if (!effect.Comp.Options.TryGetValue(group, out var options))
                continue;

            var entities =
                _entityLookup.GetEntitiesInRange<DamageableComponent>(Transform(effect).Coordinates, options.Range);

            foreach (var entity in entities)
            {
                _damageable.TryChangeDamage(entity, options.Damage, damageable: entity.Comp);
            }
        }
    }
}
