using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text moneyText;
        [SerializeField] float moneyIncrementSpeed;

        public void SetMoney(int value)
        {

            StartCoroutine(MoneyIncrementRoutine());

            IEnumerator MoneyIncrementRoutine()
            {
                int money = int.Parse(moneyText.text);
                while (money != value)
                {
                    money += value > money ? 1 : -1;
                    moneyText.text = money.ToString();
                    moneyText.transform.localScale = Vector3.one * (money % 5 == 0 ? 1.3f : 1f);
                    yield return new WaitForSeconds(1f / moneyIncrementSpeed);
                }
                moneyText.text = value.ToString();
                moneyText.transform.localScale = Vector3.one;
            }
        }
    }
}