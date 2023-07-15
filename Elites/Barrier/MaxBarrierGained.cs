using MysticsRisky2Utils.BaseAssetTypes;

namespace RisingTides.Buffs
{
    public class MaxBarrierGained : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_MaxBarrierGained";
			buffDef.isHidden = true;
		}
	}
}
