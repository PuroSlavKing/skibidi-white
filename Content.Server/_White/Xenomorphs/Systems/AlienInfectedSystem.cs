﻿using System.Linq;
using System.Threading;
using Content.Server.Aliens.Components;
using Content.Server.Body.Systems;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Jittering;
using Content.Shared.Aliens.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Part;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.Ghost.Roles;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Random;
using Robust.Shared.Player;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using FastAccessors;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using AlienInfectedComponent = Content.Shared.Aliens.Components.AlienInfectedComponent;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienInfectedSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AlienInfectedComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<AlienInfectedComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentInit(EntityUid uid, AlienInfectedComponent component, ComponentInit args)
    {
        // var torsoPart = Comp<BodyComponent>(uid).RootContainer.ContainedEntities[0];
        // _body.TryCreateOrganSlot(torsoPart, "alienLarvaOrgan", out _);
        // _body.InsertOrgan(torsoPart, Spawn(component.OrganProtoId, Transform(uid).Coordinates), "alienLarvaOrgan");
        component.NextGrowRoll = _timing.CurTime + TimeSpan.FromSeconds(component.GrowTime);
        component.Stomach = _container.EnsureContainer<Container>(uid, "stomach");
    }

    private void OnComponentShutdown(EntityUid uid, AlienInfectedComponent component, ComponentShutdown args)
    {

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AlienInfectedComponent>();
        while (query.MoveNext(out var uid, out var infected))
        {
            if (_timing.CurTime < infected.NextGrowRoll)
                continue;

            if (HasComp<InsideAlienLarvaComponent>(infected.SpawnedLarva) &&
                Comp<InsideAlienLarvaComponent>(infected.SpawnedLarva).IsGrown)
            {
                _container.EmptyContainer(infected.Stomach);
                _entityManager.RemoveComponent<AlienInfectedComponent>(uid);
                _mobStateSystem.ChangeMobState(uid, MobState.Dead);
                _damageable.TryChangeDamage(uid, infected.BurstDamage, true, false); // TODO: Only torso damage
                _popup.PopupClient(Loc.GetString("larva-burst-entity"),
                    uid, PopupType.LargeCaution);
                _popup.PopupEntity(Loc.GetString("larva-burst-entity-other"),
                    uid, PopupType.MediumCaution);
            }

            if (infected.GrowthStage == 6)
            {
                var larva = Spawn(infected.Prototype, Transform(uid).Coordinates);
                _container.Insert(larva, infected.Stomach);
                infected.SpawnedLarva = larva;
                infected.GrowthStage++;
                _jittering.DoJitter(uid, TimeSpan.FromSeconds(8), true);
                _popup.PopupClient(Loc.GetString("larva-inside-entity"),
                    uid, PopupType.Medium);
            }

            if (_random.Prob(infected.GrowProb))
            {
                infected.GrowthStage++;
            }
            infected.NextGrowRoll = _timing.CurTime + TimeSpan.FromSeconds(infected.GrowTime);
        }
    }
}
