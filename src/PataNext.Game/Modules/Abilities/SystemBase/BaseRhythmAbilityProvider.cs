using System.Linq;
using Collections.Pooled;
using PataNext.Game.Modules.Abilities.Components;
using PataNext.Game.Modules.Abilities.Storages;
using Quadrum.Game.Modules.Simulation.Abilities.Components;
using Quadrum.Game.Modules.Simulation.Abilities.Components.Conditions;
using Quadrum.Game.Modules.Simulation.Common.SystemBase;
using revecs.Core;
using revecs.Extensions.Generator.Components;
using revecs.Querying;
using revecs.Utility;
using revghost;
using revghost.Injection;
using revghost.Injection.Dependencies;

namespace PataNext.Game.Modules.Abilities.SystemBase;

public record struct CreateAbility(UEntitySafe Target);

public abstract class BaseRhythmAbilityProvider : BaseProvider<CreateAbility>
{
    public virtual bool UseStatsModification => true;

    protected BaseRhythmAbilityProvider(Scope scope) : base(scope)
    {
        if (!scope.Context.TryGet(out abilityStorage))
            throw new InvalidOperationException(
                $"Module has created an ability provider, but do not provide an AbilityDescriptionStorage, this is not allowed."
            );
    }

    protected AbilityDescriptionStorage abilityStorage;
    protected string configuration;

    protected abstract void GetComboCommands<TList>(TList componentTypes) where TList : IList<ComponentType>;

    public string GetConfigurationData()
    {
        return configuration;
    }
}

public abstract class BaseRhythmAbilityProvider<TAbility> : BaseRhythmAbilityProvider
    where TAbility : IRevolutionComponent
{
    protected virtual string FilePathPrefix => string.Empty;

    protected virtual string FolderPath
    {
        get
        {
            string getShortComponentName(ComponentType componentType)
            {
                var name = Simulation.ComponentTypeBoard.Names[componentType.Handle];
                return name.Substring(name.LastIndexOf(':') + 1);
            }

            var folder = "{0}";
            if (!string.IsNullOrEmpty(FilePathPrefix))
                folder = string.Format(folder, FilePathPrefix + "\\{0}");

            using var comboCommands = new PooledList<ComponentType>();
            GetComboCommands(comboCommands);
            folder = string.Format(folder, string.Join("_", comboCommands.Select(getShortComponentName)));

            // kinda useless, but it will automatically create folders that don't exist, so it's a bit useful for lazy persons (eg: guerro)
            try
            {
                abilityStorage.GetSubStorage(folder);
            }
            catch
            {
                // ignored (DllStorage will throw an exception if it does not exist)
            }

            return folder;
        }
    }

    protected virtual string FilePath => $"{FolderPath}\\{typeof(TAbility).Name.Replace("Ability", string.Empty)}";

    private AbilitySpawner _spawner;

    protected BaseRhythmAbilityProvider(Scope scope) : base(scope)
    {
        Dependencies.Add(() => ref _spawner);
    }
    
    private static ReadOnlySpan<byte> Utf8Bom => new byte[] {0xEF, 0xBB, 0xBF};

    private ComponentType[] _comboComponentTypes;
    private ComponentType _abilityType;

    protected override void OnInit()
    {
        using var list = new PooledList<ComponentType>();
        GetComboCommands(list);
        _comboComponentTypes = list.ToArray();

        _abilityType = TAbility.ToComponentType(Simulation);

        _spawner.Register(_abilityType, this);
    }

    public override void SetEntityData(ref UEntityHandle handle, CreateAbility data)
    {
        if (!Simulation.Exists(data.Target))
            throw new InvalidOperationException($"Simulation Entity '{data.Target}' does not exist");
        
        Simulation.AddComponent(handle, AbilityLayout.ToComponentType(Simulation));
        Simulation.AddComponent(handle, _abilityType);
        Simulation.AddAbilityType(handle, new AbilityType(_abilityType));
        // don't pass data.Target directly (since Relative components only accept UEntityHandle and not UEntitySafe)
        // TODO: in future Relative component should also add extension methods to RevolutionWorld to not have this problem
        Simulation.AddComponent(handle, AbilityOwnerDescription.Relative.ToComponentType(Simulation), data.Target.Handle);

        if (_comboComponentTypes.Length > 0)
        {
            Simulation.AddComboAbilityCondition(
                handle,
                _comboComponentTypes.AsSpan().UnsafeCast<ComponentType, ComboAbilityCondition>()
            );
        }

        if (UseStatsModification)
        {
            var component = new AbilityModifyStatsOnChaining();
            
            var stats = new Dictionary<string, StatisticModifier>();
            StatisticModifierJson.FromMap(stats, GetConfigurationData());

            void TryGet(string val, out StatisticModifier modifier)
            {
                if (!stats.TryGetValue(val, out modifier))
                    modifier = StatisticModifier.Default;
            }

            TryGet("active", out component.ActiveModifier);
            TryGet("fever", out component.FeverModifier);
            TryGet("perfect", out component.PerfectModifier);
            TryGet("charge", out component.ChargeModifier);
            
            Simulation.AddComponent(handle, AbilityModifyStatsOnChaining.ToComponentType(Simulation), component);
        }
    }
}