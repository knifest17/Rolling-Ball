using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerStats : MonoBehaviour
    {
        int money;

        public int Money => money;

        void Start()
        {
            money = 0;
            Bonus.SomeBonusCollected += OnSomeBonusCollected;
        }

        void OnSomeBonusCollected()
        {
            money += 50;
            print(money);
        }
    }
}