using System.Numerics;
using GDNative;
using GodotCLR;
using GodotCLR.HighLevel;
using Quadrum.Game.Utilities;

namespace PataNext.Export.Godot.GodotComponents;

public struct SplineModel
{
    public static void Load()
    {
        GD.RegisterClass<SplineModel>(nameof(SplineModel), "Node3D");
        GD.AddMethod(nameof(SplineModel), "init",
            new GD.OnMethodCall<SplineModel>(OnInit),
            new[]
            {
                (Variant.EType.OBJECT, "PointA"),
                (Variant.EType.OBJECT, "PointB"),
                (Variant.EType.OBJECT, "PointC"),
                (Variant.EType.OBJECT, "Spline"),
            },
            Variant.EType.NIL,
            GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_NORMAL
        );
        GD.AddMethod(nameof(SplineModel), "update",
            new GD.OnMethodCall<SplineModel>(OnUpdate),
            new[]
            {
                (Variant.EType.BOOL, "ModifyRotation")
            },
            Variant.EType.NIL,
            GDNativeExtensionClassMethodFlags.GDNATIVE_EXTENSION_METHOD_FLAG_NORMAL
        );
    }

    private bool initialized;
    public GD.Node3D PointA;
    public GD.Node3D PointB;
    public GD.Node3D PointC;
    public GD.Node3D Spline;

    private static Variant OnInit(ref byte methodData, ref SplineModel instance, VariantMethodArgs args)
    {
        instance = default;
        
        var i = 0;
        for (; i < args.Length; i++)
        {
            if (args[i].Type == Variant.EType.NIL)
            {
                UtilityFunctions.Print($"SplineModel: Argument {i} Not initialized");
                return default;
            }
        }

        UtilityFunctions.Print("Initialized");
        instance.initialized = true;
        
        i = 0;
        instance.PointA = new GD.Node3D(args[i++].Object);
        instance.PointB = new GD.Node3D(args[i++].Object);
        instance.PointC = new GD.Node3D(args[i++].Object);
        instance.Spline = new GD.Node3D(args[i++].Object);

        _ = i;
        return default;
    }

    private static float angle(Vector2 a, Vector2 b)
    {
        return MathF.Atan2(b.Y - a.Y, b.X - a.X);
    }

    private static Vector2 bzPos(float t, Vector2 a, Vector2 b, Vector2 c)
    {
        var mT = 1 - t;
        return (a * mT * mT +
                b * 2f * t * mT +
                c * t * t);
    }

    private Vector2 oldA;
    private Vector2 oldB;
    private Vector2 oldC;

    private static Variant OnUpdate(ref byte methodData, ref SplineModel instance, VariantMethodArgs args)
    {
        if (!instance.initialized)
            return default;
        
        var posA = instance.PointA.GetPosition().XY();
        var posB = instance.PointB.GetPosition().XY();
        var posC = instance.PointC.GetPosition().XY();

        if (instance.oldA == posA && instance.oldB == posB && instance.oldC == posC)
        {
            return default;
        }
        
        var correctedB = (4f * posB - posA - posC) / 2f;

        // [0] == modify_rotation
        if (args[0].Bool)
        {
            var p0 = bzPos(0.9f, posA, correctedB, posC);
            var p1 = bzPos(1.0f, posA, correctedB, posC);

            instance.PointC.SetProperty("rotation", new Variant
            {
                Type = Variant.EType.VECTOR3,
                Vector3 = new Vector3(0, 0, angle(p1 - p0, Vector2.UnitX))
            });
        }

        if (instance.oldA != posA)
        {
            instance.oldA = posA;
            instance.Spline.SetProperty(nameof(PointA), new Variant {Type = Variant.EType.VECTOR2, Vector2 = posA});
        }
        
        if (instance.oldB != posB)
        {
            instance.oldB = posB;
            instance.Spline.SetProperty(nameof(PointB), new Variant {Type = Variant.EType.VECTOR2, Vector2 = correctedB});
        }
        
        if (instance.oldC != posC)
        {
            instance.oldC = posC;
            instance.Spline.SetProperty(nameof(PointC), new Variant {Type = Variant.EType.VECTOR2, Vector2 = posC});
        }
        return default;
    }
}