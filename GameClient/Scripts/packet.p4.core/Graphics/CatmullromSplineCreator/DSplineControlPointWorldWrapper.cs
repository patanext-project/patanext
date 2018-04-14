using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace P4.Core.Graphics
{
    public struct DSplineControlPointWorldData : IComponentData
    {
    }

    public class DSplineControlPointWrapper : ComponentDataWrapper<DSplineControlPointWorldData>
    {

    }
}
