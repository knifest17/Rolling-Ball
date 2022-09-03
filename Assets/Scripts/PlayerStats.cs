using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerStats : MonoBehaviour
    {
        int money;

        public int Money
        {
            get => money;
            private set
            {
                money = value;
                MoneyChanged?.Invoke(money);
            }
        }

        public event Action<int> MoneyChanged;

        void Start()
        {
            Money = 0;
            Bonus.SomeBonusCollected += OnSomeBonusCollected;
        }

        void OnSomeBonusCollected()
        {
            Money += 50;
        }
    }
}