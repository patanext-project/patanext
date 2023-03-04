using Godot;
using revecs.Extensions.Generator.Components;

namespace PataNext.GodotServer;

public partial record struct GodotInstanceId(RID Value) : ISparseComponent;
public partial record struct GodotInstanceMeshId(RID Value) : ISparseComponent;