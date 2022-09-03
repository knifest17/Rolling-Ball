using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{ 
    public class Ball : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] BallSettings ballSettings;
        [SerializeField] Transform visibleObject, particles;
        [SerializeField] ParticleSystem[] smokeParticles;
        [SerializeField] Cinemachine.CinemachineVirtualCamera virtualCum;
        [SerializeField] InputManager inputManager;

        Transform t;
        Rigidbody rb;
        Material material;

        float speed = 0f;
        Vector3 direction;

        Coroutine rollingCoroutine = null;
        int lastWallId = 0;

        float rollingSpeedMult = 1f;
        float cameraFOVIncr = 0f;
        bool inCollision = false;
        bool inputPresent = false;

        void Awake()
        {
            t = transform;
            rb = GetComponent<Rigidbody>();
            material = visibleObject.GetComponent<MeshRenderer>().material;
        }

        void OnEnable()
        {
            inputManager.InputEnded += OnInputEnded;
            inputManager.InputBegined += OnInputBegined;
        }

        void OnDisable()
        {
            inputManager.InputEnded -= OnInputEnded;
            inputManager.InputBegined -= OnInputBegined;
        }

        void OnInputBegined()
        {
            inputPresent = true;
            lineRenderer.gameObject.SetActive(true);
        }

        void OnInputEnded(Vector3 input)
        {
            inputPresent = false;
            lineRenderer.gameObject.SetActive(false);
            if (inCollision) return;
            direction = input.normalized;
            speed = Mathf.Clamp(input.magnitude, 0, ballSettings.MaxSpeed);
            StartRolling();
            lastWallId = 0;
        }

        void Update()
        {
            virtualCum.m_Lens.FieldOfView = 60 + cameraFOVIncr;
            if (cameraFOVIncr != speed)
            {
                cameraFOVIncr += Time.deltaTime * (cameraFOVIncr < speed ? 1 : -1) * speed;
                if (Mathf.Abs(speed - cameraFOVIncr) < Time.deltaTime) cameraFOVIncr = speed;
            }
            if (inputPresent)
            {
                lineRenderer.SetPosition(0, t.position);
                lineRenderer.SetPosition(1, t.position + inputManager.Input);
            }
        }

        IEnumerator RollingRoutine()
        {
            while (speed > 0)
            {
                t.position += direction * speed * rollingSpeedMult * Time.deltaTime;
                visibleObject.RotateAround(
                    visibleObject.position,
                    new Vector3(direction.z, 0, -direction.x),
                    speed * 0.35f * rollingSpeedMult);
                speed -= ballSettings.Friction * rollingSpeedMult * Time.deltaTime;
                yield return null;
            }
            rollingCoroutine = null;
        }

        void StartRolling()
        {
            if (rollingCoroutine == null)
                rollingCoroutine = StartCoroutine(RollingRoutine());
        }

        void StopRolling()
        {
            if (rollingCoroutine == null) return;
            StopCoroutine(rollingCoroutine);
            rollingCoroutine = null;
        }

        void PlaySmoke()
        {
            foreach (var s in smokeParticles)
            {
                s.Play();
                var burst = s.emission.GetBurst(0);
                var count = burst.count;
                count.constant = speed * 2;
                burst.count = count;
                s.emission.SetBurst(0, burst);
            }
        }

        void SetInCollision(bool value)
        {
            rollingSpeedMult = value ? ballSettings.SquashRollingMult : 1f;
            rb.isKinematic = value;
            inCollision = value;
        }

        void LerpToRedColor(float value)
        {
            material.color = Color.Lerp(Color.white, Color.red, value);
        }

        IEnumerator CollisionRoutine(Vector3 normal, Vector3 newDir)
        {
            SetInCollision(true);

            var lookRot = Quaternion.LookRotation(normal, Vector3.up);
            t.localRotation = lookRot;
            visibleObject.Rotate(Vector3.up, -lookRot.eulerAngles.y);

            float compression = 0;
            float sign = 1f;
            bool squash = true;
            float angle = Vector3.Angle(newDir, normal);
            float compSpeed = ballSettings.SquashSpeed * (Mathf.Pow(angle, 1f / 2f) + 10f) * 0.1f;
            float targetCompression = Mathf.Clamp(0.25f * speed * 0.1f, 0, ballSettings.MaxSquash);

            while (squash && speed > 0)
            {
                compression += sign * compSpeed * Time.deltaTime;
                float delta = compression * 2;
                t.localScale = new Vector3(1f + delta, 1f + delta, 1f - delta);
                LerpToRedColor(compression / ballSettings.MaxSquash * 0.5f);

                if (compression >= targetCompression)
                {
                    direction = newDir;
                    sign = -1f;
                }

                if (sign == -1f && compression < 0f) squash = false;
                yield return null;
            }

            t.localScale = Vector3.one;
            visibleObject.Rotate(Vector3.up, lookRot.eulerAngles.y);
            t.localRotation = Quaternion.identity;

            SetInCollision(false);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetInstanceID() == lastWallId) return;
            if (collision.contactCount == 0) return;
            if (inCollision) return;

            lastWallId = collision.gameObject.GetInstanceID();
            var contact = collision.GetContact(0);
            var newDirection = Vector3.Reflect(direction, contact.normal);

            if (speed > ballSettings.MinSquashSpeed)
                StartCoroutine(CollisionRoutine(contact.normal, newDirection));
            else
                direction = newDirection;

            particles.position = contact.point;
            particles.rotation = Quaternion.LookRotation(contact.normal, Vector3.up);
            PlaySmoke();
        }
    }
}