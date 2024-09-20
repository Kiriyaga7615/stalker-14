﻿using Content.Shared._Stalker.Anomaly.Triggers.Events;
using Robust.Server.GameObjects;

namespace Content.Server._Stalker.Anomaly.Effects.GenericVisualizer;

public sealed class STAnomalyGenericVisualizerEffectSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STAnomalyGenericVisualizerEffectComponent, STAnomalyTriggerEvent>(OnTriggered);
    }

    private void OnTriggered(Entity<STAnomalyGenericVisualizerEffectComponent> effect, ref STAnomalyTriggerEvent args)
    {
        foreach (var group in args.Groups)
        {
            if (!effect.Comp.Options.TryGetValue(group, out var options))
                continue;

            _appearance.SetData(effect, STAnomalyGenericVisualizerEffectVisuals.Layer, options.State);
        }
    }
}
