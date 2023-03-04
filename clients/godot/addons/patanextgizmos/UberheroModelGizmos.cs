#if TOOLS
using Godot;
using PataNext.Presentations;

namespace PataNext.addons.patanextgizmos;

[Tool]
public partial class UberheroModelGizmos : EditorNode3DGizmoPlugin
{
    public override bool _HasGizmo(Node3D forNode3d)
    {
        GD.Print($"Has {forNode3d.GetType()}");
        return forNode3d is UberheroModel;
    }

    public override EditorNode3DGizmo _CreateGizmo(Node3D forNode3d)
    {
        CSharpScript ok;
        GD.Print($" ? {forNode3d.GetScript()}");
        if (forNode3d.GetScript().Obj is CSharpScript script)
        {
            GD.Print($"found model! {script.New()}");
        }

        return base._CreateGizmo(forNode3d);
    }

    public override string _GetGizmoName()
    {
        return "UberheroModel";
    }
}
#endif