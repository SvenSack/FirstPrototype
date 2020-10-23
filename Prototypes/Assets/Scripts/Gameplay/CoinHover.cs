using System;
using UnityEngine;

namespace Gameplay
{
    public class CoinHover : MonoBehaviour
    {
        [SerializeField] private float hoverTimer = 2f;
        [SerializeField] private GameObject hoverText;
        
        private float hoverTime;
        private bool showing;


        private void Start()
        {
            hoverText.SetActive(false);
        }

        private void OnTriggerStay(Collider other)
        { // this and the following methods are just for my worldspace UI where I do hover toggles based on the CursorFollower
            if (other.CompareTag("CursorFollower"))
            {
                hoverTime += Time.deltaTime;
                if (hoverTime >= hoverTimer && !showing)
                {
                    ToggleHover();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("CursorFollower"))
            {
                if (CursorFollower.Instance.IsHovering && showing)
                {
                    ToggleHover();
                }

                hoverTime = 0;
            }
        }

        private void ToggleHover()
        {
            showing = !showing;
            CursorFollower.Instance.ToggleHover();
            hoverText.SetActive(CursorFollower.Instance.IsHovering);
        }
    }
}
