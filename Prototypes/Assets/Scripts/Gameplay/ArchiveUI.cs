using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class ArchiveUI : MonoBehaviour
    {
        [SerializeField] private Vector2 firstItemPosition;
        [SerializeField] private float distanceBetweenItems;
        [SerializeField] private Transform archiveParent;
        [SerializeField] private GameObject informationPieceUIPrefab;
        
        public List<ArchiveItem> archive = new List<ArchiveItem>();

        public void PopulateArchive(List<InformationPiece> informationPieces)
        { // the function used by the UImanager when the archive is opened
            for (var i = 0; i < informationPieces.Count; i++)
            {
                ArchiveItem newItem = Instantiate(informationPieceUIPrefab, archiveParent).GetComponent<ArchiveItem>();
                newItem.GiveSource(informationPieces[i], this);
                archive.Add(newItem);
            }
            RearrangeArchive();
        }

        public void DropArchive()
        { // the function used by the UImanager when the archive is closed
            foreach (var item in archive)
            {
                Destroy(item.gameObject);
            }
            archive = new List<ArchiveItem>();
        }

        public void RearrangeArchive()
        { // basic reordering to make everything fit, gets called on each opening from the archive items
            bool isAfterOpened = false;
            for (int i = 0; i < archive.Count; i++)
            {
                if (!isAfterOpened)
                {
                    archive[i].transform.position = archiveParent.position + (Vector3)firstItemPosition+ new Vector3(0, -distanceBetweenItems*i, 0);
                }
                else
                {
                    archive[i].transform.position = archiveParent.position + (Vector3)firstItemPosition+ new Vector3(0, 200-distanceBetweenItems*i, 0);
                }
                if (archive[i].isOpened)
                {
                    isAfterOpened = true;
                }
            }
        }
    }
}
