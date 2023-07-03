using MysticsRisky2Utils.BaseAssetTypes;

namespace RisingTides.Buffs
{
    public class WaterBubbleTrapCooldown : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_WaterBubbleTrapCooldown";
			buffDef.isHidden = true;
		}
	}
}
