using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class DialUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button[] buttons = new Button[2];
        
        public int amount;
        public int maxAmount;

    
    
        public void Change(bool isIncrease)
        { // this class is just to create a UI dial (two buttons which affect the same value with + & - including visualization)
            if (isIncrease)
            {
                amount++;
                text.text = amount.ToString();
            }
            else
            {
                amount--;
                text.text = amount.ToString();
            }

            if (amount == 0)
            {
                buttons[0].enabled = false;
            }
            else
            {
                if (!buttons[0].enabled)
                {
                    buttons[0].enabled = true;
                }
            }

            if (amount == maxAmount)
            {
                buttons[1].enabled = false;
            }
            else
            {
                if (!buttons[1].enabled)
                {
                    buttons[1].enabled = true;
                }
            }
        }


        public void Reset()
        {
            amount = 0;
            text.text = amount.ToString();
            buttons[0].enabled = false;
            buttons[1].enabled = true;
        }
    }
}
