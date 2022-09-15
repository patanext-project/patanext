using revecs.Core;
using revecs.Extensions.Generator.Components;

namespace PataNext.Game.Modules.Abilities.Components;

public partial record struct SetSpawnAbility(ComponentType AbilityType, UEntitySafe Owner, UEntitySafe? Source = null) : ISparseComponent;