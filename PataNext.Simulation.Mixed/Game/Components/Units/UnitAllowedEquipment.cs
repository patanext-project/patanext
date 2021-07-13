using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Simulation.Utility.Resource;
using PataNext.Module.Simulation.Resources;

namespace PataNext.Module.Simulation.Components.Units
{
	/// <summary>
	/// Get the allowed equipments of an unit
	/// </summary>
	/// <example>
	///	Taterazay:
	///		equip_root/helm -> helms
	///		equip_root/l_eq -> shields
	///		equip_root/r_eq -> swords, spears
	///
	/// pseudo code:
	///		add(equip_root/helm, helm_type)
	///		add(equip_root/l_eq, shield_type)
	///		add(equip_root/r_eq, sword_type)
	///		add(equip_root/r_eq, spear_type)
	/// </example>
	public struct UnitAllowedEquipment : IComponentBuffer
	{
		public GameResource<UnitAttachmentResource> Attachment;
		// Fill an "equipment type" string, not an "equipment" string
		// The design choice is by reason
		public GameResource<EquipmentResource>      EquipmentType;
	}
}