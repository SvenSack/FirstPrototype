using UnityEngine;

namespace Gameplay
{
    public class InformationSelectItem : MonoBehaviour
    {
        public InformationPiece represents;

        public void SelectInformation()
        {
            UIManager.Instance.ConfirmInformationSelection(represents);
        }
    }
}
