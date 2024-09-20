using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Stalker.WeaponModule;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server._Stalker.WeaponModule;

public sealed partial class STWeaponModuleSystem : STSharedWeaponModuleSystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;

    private EntityQuery<ContainerManagerComponent> _containerMangerQuery;
    private EntityQuery<STWeaponModuleContainerComponent> _containerModuleQuery;

    public override void Initialize()
    {
        base.Initialize();

        _containerMangerQuery = GetEntityQuery<ContainerManagerComponent>();
        _containerModuleQuery = GetEntityQuery<STWeaponModuleContainerComponent>();

        SubscribeLocalEvent<STWeaponModuleComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<STWeaponModuleComponent, EntGotRemovedFromContainerMessage>(OnRemoved);

        SubscribeLocalEvent<STWeaponModuleContainerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<STWeaponModuleContainerComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnInserted(Entity<STWeaponModuleComponent> entity, ref EntGotInsertedIntoContainerMessage args)
    {
        if (entity.Comp.ScopeEffect != null && HasComp<GunComponent>(args.Container.Owner))
            SetGunScope(entity, args.Container);
        UpdateContainerEffect(args.Container);
    }

    private void OnRemoved(Entity<STWeaponModuleComponent> entity, ref EntGotRemovedFromContainerMessage args)
    {
        if (entity.Comp.ScopeEffect != null && HasComp<GunComponent>(args.Container.Owner))
            DelGunScope(entity, args.Container);
        UpdateContainerEffect(args.Container);
    }

    private void OnInit(Entity<STWeaponModuleContainerComponent> entity, ref ComponentInit args)
    {
        entity.Comp.HashedEffect = new STWeaponModuleEffect();
        entity.Comp.HashedScopeEffect = null;

        if (!_containerMangerQuery.TryGetComponent(entity, out var containerComponent))
            return;

        foreach (var (_, container) in containerComponent.Containers)
        {
            UpdateContainerEffect(entity, container);
        }
    }

    private void OnGunRefreshModifiers(Entity<STWeaponModuleContainerComponent> entity, ref GunRefreshModifiersEvent args)
    {
        var effect = entity.Comp.HashedEffect;

        args.FireRate *= effect.FireRateModifier;
        args.AngleDecay *= effect.AngleDecayModifier;
        args.AngleIncrease *= effect.AngleIncreaseModifier;
        args.MinAngle *= effect.MinAngleModifier;
        args.MaxAngle *= effect.MaxAngleModifier;
        args.ProjectileSpeed *= effect.ProjectileSpeedModifier;

        if (args.SoundGunshot is null)
            return;

        var audioParams = args.SoundGunshot?.Params ?? AudioParams.Default;

        // Hotfix how to handle super silent silencers happening because volume additions
        // pile up. We need to find something else, because a user in the future might have
        // not only one volume reducing module
        audioParams.Volume = effect.SoundGunshotVolumeAddition;
        args.SoundGunshot!.Params = audioParams;
    }

    private void UpdateContainerEffect(BaseContainer container)
    {
        UpdateContainerEffect(container.Owner, container);
    }

    private void UpdateContainerEffect(EntityUid entityUid, BaseContainer container)
    {
        if (!_containerModuleQuery.TryGetComponent(entityUid, out var containerComponent))
            return;

        UpdateContainerEffect((entityUid, containerComponent), container);
    }

    private void UpdateContainerEffect(Entity<STWeaponModuleContainerComponent> entity, BaseContainer container)
    {
        var effect = new STWeaponModuleEffect();
        STWeaponModuleScopeEffect? scopeEffect = null;

        foreach (var containedEntity in container.ContainedEntities)
        {
            if (!TryComp<STWeaponModuleComponent>(containedEntity, out var moduleComponent))
                continue;

            effect = STWeaponModuleEffect.Merge(effect, moduleComponent.Effect);
            scopeEffect = STWeaponModuleScopeEffect.Merge(scopeEffect, moduleComponent.ScopeEffect);
        }

        if (!TryComp<GunComponent>(entity, out var gun))
            return;

        var differ = effect.AdditionalAvailableModes ^ entity.Comp.HashedEffect.AdditionalAvailableModes;
        _gun.SetAvailableModes(entity, gun.AvailableModes ^ differ, gun); // here bit mask works like switch and switch modes that changed
        entity.Comp.HashedEffect = effect;
        Dirty(entity);

        _gun.RefreshModifiers((entity, null));
    }
}
