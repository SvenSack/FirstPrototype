using System;
using UnityEngine;

namespace Gameplay
{
    public class CoinHover : MonoBehaviour
    {

        [SerializeField] private GameObject hoverText;
        private float hoverTime;
        [SerializeField] private float hoverTimer = 2f;
        private bool showing;


        private void Start()
        {
            hoverText.SetActive(false);
        }

        private void OnTriggerStay(Collider other)
        {
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
