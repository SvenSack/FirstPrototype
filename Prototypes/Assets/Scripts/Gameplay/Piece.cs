using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Gameplay
{
    public class Piece : MonoBehaviour
    {
        public bool isUsed;
        public bool isPickedUp;
        public PhotonView pv;
        public Rigidbody rb;
        public Camera cam;
        private LayerMask tableMask;
        private int pieceLayer;
        public GameMaster.PieceType type;
    
        void Start()
        {
            pv = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();
            tableMask = LayerMask.GetMask("Table");
            pieceLayer = LayerMask.NameToLayer("Pieces");
        }

        void Update()
        {
            
            if (isPickedUp)
            {
                if (Input.GetMouseButton(0))
                {
                    if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out RaycastHit tableHit, 100f, tableMask))
                    {
                        Vector3 cursorTarget = tableHit.point + Vector3.up*.5f;
                        transform.position = Vector3.Lerp(transform.position, cursorTarget, .5f);
                    }
                    else
                    {
                        Debug.Log("dropped off table");
                        ToggleSelfPickup();
                    }
                }
                else
                {
                    Debug.Log("stopped holding");
                    ToggleSelfPickup();
                } 
            }
            
        }

        private void ToggleSelfPickup()
        {
            rb.isKinematic = !rb.isKinematic;
            isPickedUp = !isPickedUp;
            UIManager.Instance.isGrabbingPiece = !UIManager.Instance.isGrabbingPiece;
        }

        public void ToggleUse()
        {
            if (!isUsed)
            {
                isUsed = true;
                gameObject.layer = 0;
            }
            else
            {
                isUsed = false;
                gameObject.layer = pieceLayer;
            }
        }

        public bool TryPickup(Player owner)
        {
            if (Equals(owner, pv.Controller))
            {
                ToggleSelfPickup();
                return true;
            }

            return false;
        }
    }
}
