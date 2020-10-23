using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class AssignmentChoice : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI totalText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Transform onGroup;
        [SerializeField] private Transform offGroup;
        [SerializeField] private GameObject[] togglePrefabs;
        
        private List<AssignmentToggle> toggledOn = new List<AssignmentToggle>();
        private List<AssignmentToggle> toggledOff = new List<AssignmentToggle>();
        private int total;
        private bool isPayment = true;

        public void SwitchAssignment(AssignmentToggle target)
        { // called by the assignment pieces when they toggle on/off
            int multiplier;
            if (target.isAssigned)
            {
                toggledOn.Remove(target);
                toggledOff.Add(target);
                multiplier = -1;
            }
            else
            {
                toggledOff.Remove(target);
                toggledOn.Add(target);
                multiplier = 1;
            }

            if (isPayment)
            {
                switch (target.representative.type)
                {
                    case GameMaster.PieceType.Assassin:
                        if (UIManager.Instance.participant.hasZeal)
                        {
                            total = total +  1*multiplier;
                            break;
                        }
                        total = total +  2*multiplier;
                        break;
                    case GameMaster.PieceType.Thug:
                        if (UIManager.Instance.participant.character == GameMaster.Character.Ruffian)
                        {
                            int thugAmount = 0;
                            for (int i = 0; i < toggledOn.Count; i++)
                            {
                                if (toggledOn[i].representative.type == GameMaster.PieceType.Thug)
                                {
                                    thugAmount++;
                                }
                            }

                            if (thugAmount < 6)
                            {
                                if (multiplier < 0 && thugAmount == 5)
                                {
                                    // exception case
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                        total = total + 1*multiplier;
                        break;
                }
                UpdateTotal();
            }
            AdjustPositions();
            if (!isPayment)
            {
                if (toggledOff.Count != 3 && toggledOn.Count + toggledOff.Count >= 3)
                {
                    confirmButton.enabled = false;
                }
            }
        }

        public void CreateToggles()
        { // called by the UImanager to fill the list with Ui elements upon opening
            if (!isPayment)
            {
                confirmButton.enabled = false;
            }
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (var piece in pieces)
            {
                if (piece.pv.IsMine)
                {
                    GameObject inst = null;
                    switch (piece.type)
                    {
                        case GameMaster.PieceType.Assassin:
                            inst = Instantiate(togglePrefabs[0], transform);
                            break;
                        case GameMaster.PieceType.Thug:
                            inst = Instantiate(togglePrefabs[1], transform);
                            break;
                        case GameMaster.PieceType.Worker:
                            PhotonNetwork.Destroy(piece.pv);
                            break;
                    }
                    if (piece.type != GameMaster.PieceType.Worker)
                    {
                        AssignmentToggle toggle = inst.GetComponent<AssignmentToggle>();
                        toggle.assigner = this;
                        toggle.representative = piece;
                        toggle.isPrivate = piece.isPrivate;
                        toggledOff.Add(toggle);
                    }
                }
            }
            AdjustPositions();
        }

        private void AdjustPositions()
        { // called whenever one piece moves, TODO: further optimize this by only updating the ones after the changed index
            for (int i = 0; i < toggledOn.Count; i++)
            {
                int row = Mathf.FloorToInt(i / 5f);
                int column = i - 5*row;
                toggledOn[i].transform.position = onGroup.position + new Vector3(-160 + 80*column, 230 - 70*row, 0);
            }
            for (int i = 0; i < toggledOff.Count; i++)
            {
                int row1 = Mathf.FloorToInt(i / 5f);
                int column1 = i - 5*row1;
                toggledOff[i].transform.position = offGroup.position + new Vector3(-160 + 80*column1, 230 - 70*row1, 0);
            }
        }

        private void UpdateTotal()
        { // called whenever a value changes for the post turn payment, both handles the display and the possible disabling of the confirm if above max value
            totalText.text = "Total Amount: " + total;
            int totalPlayerCoins = UIManager.Instance.participant.coins;
            if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfCoin).playerNumber ==
                UIManager.Instance.participant.playerNumber)
            {
                totalPlayerCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfCoin].GetComponent<Board>().coins;
            }
            if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfGoods).playerNumber ==
                UIManager.Instance.participant.playerNumber)
            {
                totalPlayerCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfGoods].GetComponent<Board>().coins;
            }
            if (GameMaster.Instance.FetchPlayerByJob(GameMaster.Job.MasterOfClubs).playerNumber ==
                UIManager.Instance.participant.playerNumber)
            {
                totalPlayerCoins += GameMaster.Instance.jobBoards[(int) GameMaster.Job.MasterOfClubs].GetComponent<Board>().coins;
            }
            if (total > totalPlayerCoins)
            {
                confirmButton.interactable = false;
            }
            else if(confirmButton.interactable == false)
            {
                confirmButton.interactable = true;
            }
        }

        public bool Clean()
        { // called by UImanager for the job assignment and potential others that do not need a detailed tally when cleaning the UI for reuse
            bool returnvalue = toggledOff.Count == 3;
            foreach (var obj in toggledOn)
            {
                Destroy(obj.gameObject);
            }
            foreach (var obj in toggledOff)
            {
                PhotonNetwork.Destroy(obj.representative.gameObject);
                Destroy(obj.gameObject);
            }
            toggledOn = new List<AssignmentToggle>();
            toggledOff = new List<AssignmentToggle>();
            return returnvalue;
        }

        public int TallyAndClean(out int thugAmount)
        { // called by UImanager to get both the total amount owed and the amount payed for thugs for the post turn payment, also cleans for reuse
            thugAmount = 0;
            foreach (var obj in toggledOn)
            {
                if (obj.representative.type == GameMaster.PieceType.Thug)
                {
                    thugAmount++;
                }
                obj.representative.ToggleUse();
                Destroy(obj.gameObject);
            }
            foreach (var obj in toggledOff)
            {
                PhotonNetwork.Destroy(obj.representative.gameObject);
                Destroy(obj.gameObject);
            }
            toggledOn = new List<AssignmentToggle>();
            toggledOff = new List<AssignmentToggle>();
            int totalAmnt = total;
            total = 0;
            UpdateTotal();
            if (UIManager.Instance.participant.character == GameMaster.Character.Ruffian)
            {
                thugAmount -= 5;
                if (thugAmount < 0)
                {
                    thugAmount = 0;
                }
            }
            return totalAmnt;
        }
    }
}
