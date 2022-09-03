using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] PlayerStats playerStats;
        [SerializeField] UIManager uiManager;

        void Start()
        {
            playerStats.MoneyChanged += OnMoneyChanged;
        }

        private void OnMoneyChanged(int value)
        {
            uiManager.SetMoney(value);
        }
    }
}