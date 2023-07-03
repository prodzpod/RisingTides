using MysticsRisky2Utils.BaseAssetTypes;

namespace RisingTides.Buffs
{
    public class OneTimeMaxBarrierGained : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_OneTimeMaxBarrierGained";
			buffDef.isHidden = true;
		}
	}
}
