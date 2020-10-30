using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Gameplay
{
    public class InformationSelection : MonoBehaviour
    {
        [SerializeField] private GameObject infoSelectItemPrefab;
        [SerializeField] private float rowSize;
        [SerializeField] private float columnSize;
        [SerializeField] private int columns;
        [SerializeField] private Vector2 firstPostion;

        public TextMeshProUGUI headLine;
    
        private List<InformationSelectItem> content = new List<InformationSelectItem>();

        public void CreateButtons()
        {
            foreach (var info in UIManager.Instance.participant.informationHand)
            {
                GameObject inst = Instantiate(infoSelectItemPrefab, transform);
                InformationSelectItem isi = inst.GetComponent<InformationSelectItem>();
                isi.represents = info;
                inst.GetComponentInChildren<TextMeshProUGUI>().text = info.header;
                content.Add(isi);
            }
            AdjustPositions();
        }

        public void DropAll()
        {
            foreach (var item in content)
            {
                Destroy(item);
            }
            content = new List<InformationSelectItem>();
        }
    
        private void AdjustPositions()
        { // this places objects correctly to avoid gaps
            for (int i = 0; i < content.Count; i++)
            {
                int row = Mathf.FloorToInt(i / (float)columns);
                int column = i - columns*row;
                content[i].transform.position = transform.position + new Vector3(firstPostion.x + columnSize*column, firstPostion.y - rowSize*row, -.1f);
            }
        }
    }
}
