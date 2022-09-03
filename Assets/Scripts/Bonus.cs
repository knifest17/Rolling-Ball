using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Bonus : MonoBehaviour
    {
        [SerializeField] Transform rotatingPart;
        [SerializeField] float rotationSpeed;
        [SerializeField] GameObject collectEffects;

        public static event Action SomeBonusCollected;

        void Start()
        {
            rotatingPart.Rotate(Vector3.up, UnityEngine.Random.Range(0, 360));
        }

        void Update()
        {
            rotatingPart.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);    
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Ball>() != null)
            {
                var effects = Instantiate(collectEffects, transform.position, transform.rotation);
                Destroy(effects, 5f);
                SomeBonusCollected?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}