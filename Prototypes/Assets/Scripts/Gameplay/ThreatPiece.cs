using System;
using Photon.Pun;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class ThreatPiece : MonoBehaviour
    {
        [SerializeField] private Material threatMaterial;

        public int originPlayerNumber;
        public Piece thisPiece { get; private set; }
        public bool isThreatening { get; private set; }
        public int damageValue;
        public bool isVigilantePiece;
        
        private Material defaultMaterial;
        private MeshRenderer meshRen;
        

        private void Start()
        {
            meshRen = GetComponent<MeshRenderer>();
            thisPiece = GetComponent<Piece>();
            originPlayerNumber = GameMaster.Instance.FetchPlayerByPlayer(thisPiece.pv.Owner).playerNumber;
            defaultMaterial = meshRen.material;
        }

        public void ReturnToOwner()
        {
            if (isVigilantePiece)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }
            isThreatening = false;
            transform.position = GameMaster.Instance.FetchPlayerByNumber(originPlayerNumber).mySlot.pieceLocation
                .position + new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
            meshRen.material = defaultMaterial;
        }

        public void ThreatenPlayer(int playerIndexToThreaten)
        {
            if (!isThreatening)
            {
                ToggleThreaten();
            }
            Participant target = GameMaster.Instance.FetchPlayerByNumber(playerIndexToThreaten);
            transform.position = target.mySlot.threateningPiecesLocation.position +
                                 new Vector3(Random.Range(-.5f, .5f), .5f, Random.Range(-.5f, .5f));
        }

        public void DestroySelf()
        {
            thisPiece.pv.RPC("DestroyThis", RpcTarget.Others);
        }

        public void ToggleThreaten()
        {
            isThreatening = !isThreatening;
            if (isThreatening)
            {
                meshRen.material = threatMaterial;
            }
            else
            {
                meshRen.material = defaultMaterial;
            }
        }
    }
}
