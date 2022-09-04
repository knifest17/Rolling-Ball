using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Bonus : MonoBehaviour
    {
        [SerializeField] Transform rotatingPart;
        [SerializeField] float rotationSpeed, movingSpeed;
        [SerializeField] GameObject collectEffects;

        public static event Action SomeBonusCollected;

        float movingDir = 1f;

        void Start()
        {
            rotatingPart.Rotate(Vector3.up, Random.Range(0, 360));
            rotationSpeed += Random.Range(-rotationSpeed * 0.3f, rotationSpeed * 0.3f);
            movingSpeed += Random.Range(-movingSpeed * 0.3f, movingSpeed * 0.3f);
        }

        void Update()
        {
            rotatingPart.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            transform.position += Vector3.up * movingDir * movingSpeed * Time.deltaTime;
            if(transform.position.y < -0.15f || transform.position.y > 0.15f)
            {
                movingDir *= -1f;
                transform.position += Vector3.up * movingDir * movingSpeed * Time.deltaTime;
            }
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