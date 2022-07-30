using revghost.IO.Storage;

namespace PataNext.Game.Modules.Abilities.Storages;

public class AbilityDescriptionStorage : ChildStorage
{
    public AbilityDescriptionStorage(IStorage root, IStorage parent) : base(root, parent)
    {
    }
    
    public AbilityDescriptionStorage(IStorage parent) : this(parent, parent)
    {
    }
}