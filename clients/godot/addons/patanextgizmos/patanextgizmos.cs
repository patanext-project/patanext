#if TOOLS
using Godot;
using System;
using PataNext.addons.patanextgizmos;
using PataNext.Presentations;

[Tool]
public partial class patanextgizmos : EditorPlugin
{
    private UberheroModelGizmos _gizmos = new();
    
    public override void _EnterTree()
    {
        base._EnterTree();
        
        AddSpatialGizmoPlugin(_gizmos);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        RemoveSpatialGizmoPlugin(_gizmos);
    }
}
#endif
