using MysticsRisky2Utils.BaseAssetTypes;

namespace RisingTides.Buffs
{
    public class AffectedByVoidFreeze : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffectedByVoidFreeze";
			buffDef.isHidden = true;
		}
	}
}
