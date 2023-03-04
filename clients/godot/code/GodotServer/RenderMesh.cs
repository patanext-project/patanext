using DefaultEcs;
using revecs.Extensions.Generator.Components;

namespace PataNext.GodotServer;

public partial struct RenderMesh : ISparseComponent
{
    public Entity Mesh;
}