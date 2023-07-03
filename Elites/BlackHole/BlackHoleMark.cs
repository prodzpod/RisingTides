using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RisingTides.Buffs
{
    public class BlackHoleMark : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_BlackHoleMark";
			buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Nullifier/texBuffNullifiedIcon.tif").WaitForCompletion();
			buffDef.buffColor = Color.black;
			buffDef.isDebuff = true;
			buffDef.canStack = true;
			refreshable = true;

			for (var i = 1; i <= 6; i++)
            {
				var vfx = PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/Mark/BlackHoleMarkVFX" + i + ".prefab"), "RisingTidesBlackHoleMarkVFX" + i, false);
				var tempVFXComponent = vfx.AddComponent<CustomTempVFXManagement.MysticsRisky2UtilsTempVFX>();
				tempVFXComponent.enterObjects = new GameObject[]
				{
					vfx.transform.Find("Particle").gameObject
				};
				var buffCount = i;
				CustomTempVFXManagement.allVFX.Add(new CustomTempVFXManagement.VFXInfo
				{
					prefab = vfx,
					condition = (x) => x.GetBuffCount(buffDef) >= buffCount,
					radius = CustomTempVFXManagement.DefaultRadiusCall
				});
			}
		}
    }
}
