using UnityEngine;

namespace Gameplay
{
    public class CursorFollower : MonoBehaviour
    {
        public static CursorFollower Instance;
        public bool IsHovering { get; private set; }
        public Camera playerCam;
        private LayerMask tableMask;
    
    
        // Start is called before the first frame update
        void Start()
        {
            tableMask = LayerMask.GetMask("Table");
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            AdjustPosition();
        }

        private void AdjustPosition()
        {
            if (Physics.Raycast(playerCam.ScreenPointToRay(Input.mousePosition), out RaycastHit tableHit, 100f, tableMask))
            {
                Vector3 cursorTarget = tableHit.point;
                transform.position = Vector3.Lerp(transform.position, cursorTarget, .5f);
            }
            else
            {
                if (IsHovering)
                {
                    IsHovering = false;
                }
            }
        }

        public void ToggleHover()
        {
            IsHovering = !IsHovering;
        }
    }
}
