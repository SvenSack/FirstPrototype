using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class ArchiveItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private TextMeshProUGUI implicationText;
        [SerializeField] private GameObject implicationObject;
        [SerializeField] private GameObject contentBox;
        [SerializeField] private Toggle toggle;

        public bool isOpened { private set; get; }

        private bool impliesSomeone;
        private ArchiveUI archive;

        private void Start()
        {
            contentBox.SetActive(false);
        }

        public void ToggleOpen()
        { // called by the toggle to interface UI and code
            isOpened = !isOpened;
            contentBox.SetActive(!contentBox.activeSelf);
            archive.RearrangeArchive();
        }

        public void GiveSource(InformationPiece source, ArchiveUI _archive)
        { // basically an initializer, setting all the values
            contentText.text = source.content;
            headerText.text = source.header;
            impliesSomeone = source.isEvidence;
            archive = _archive;
            toggle.group = archive.GetComponent<ToggleGroup>();
            if (!impliesSomeone || source.header == "The Noble is suspicious")
            {
                implicationObject.SetActive(false);
            }

            implicationText.text  = UIManager.Instance.CreateCharPlayerString(GameMaster.Instance.FetchPlayerByNumber(source.evidenceTargetIndex));
        }
    }
}
