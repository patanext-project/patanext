using System.Runtime.CompilerServices;

namespace PataNext;

public static class MathUtils
{
    public static Godot.Vector3 SwapNumericsToGodot(System.Numerics.Vector3 vec)
    {
        return Unsafe.As<System.Numerics.Vector3, Godot.Vector3>(ref vec);
    }
    
    public static System.Numerics.Vector3 SwapGodotToNumerics(Godot.Vector3 vec)
    {
        return Unsafe.As<Godot.Vector3, System.Numerics.Vector3>(ref vec);
    }
}