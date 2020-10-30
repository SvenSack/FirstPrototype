using System;
using UnityEngine;

namespace Gameplay
{
    public class HoverToolTip : MonoBehaviour
    {
        [SerializeField] private GameObject hoverObject;

        public bool canHover;
        public bool follow = true;
    
        void Start()
        {
            hoverObject.SetActive(false);
        }

        private void Update()
        {
            if (hoverObject.activeSelf && follow)
            {
                hoverObject.transform.position = Input.mousePosition;
            }
        }

        public void ToggleHover()
        {
            if (canHover)
            {
                hoverObject.SetActive(!hoverObject.activeSelf);
            }
        }
    }
}
