using TMPro;
using UnityEngine;

namespace GlobalUtil.UI
{
    /// <summary>
    ///     Credit: Aki
    /// </summary>
    // There is an issue with how TMP imports itself and alignment has to be reapplied
    internal class TMPFixer : KMonoBehaviour {
        [SerializeField]
        public TextAlignmentOptions alignment;

        [MyCmpReq] private LocText text;

        protected override void OnSpawn() {
            base.OnSpawn();
            text.alignment = alignment;
            Destroy(this);
        }
    }
}