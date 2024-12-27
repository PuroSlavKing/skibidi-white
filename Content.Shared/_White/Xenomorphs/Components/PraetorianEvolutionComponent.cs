﻿using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PraetorianEvolutionComponent : Component
{
    [DataField]
    public ProtoId<PolymorphPrototype> PraetorianPolymorphPrototype = "AlienEvolutionPraetorian";

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? PraetorianEvolutionAction = "ActionEvolvePraetorian";

    [DataField]
    public EntityUid? PraetorianEvolutionActionEntity;

    [DataField]
    public float PlasmaCost = 490f;
}