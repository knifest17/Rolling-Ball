using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] LineRenderer lineRenderer;
        [SerializeField] LayerMask groundLayer;
        [SerializeField] BallSettings ballSettings;
        [SerializeField] Transform visibleObject, particles;
        [SerializeField] ParticleSystem[] smokeParticles;
        [SerializeField] Cinemachine.CinemachineVirtualCamera virtualCum;

        Camera cam;
        float speed;
        Vector3 direction;
        Coroutine movingCoroutine;
        Rigidbody rb;
        int lastWallId = 0;
        float rollingSpeedMult = 1f;
        float cameraFOVIncr = 0f;

        void Awake()
        {
            cam = Camera.main;
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            virtualCum.m_Lens.FieldOfView = 60 + cameraFOVIncr;
            if (cameraFOVIncr != speed)
            {
                cameraFOVIncr += Time.deltaTime * (cameraFOVIncr < speed ? 1 : -1) * speed;
                if (Mathf.Abs(speed - cameraFOVIncr) < Time.deltaTime) cameraFOVIncr = speed;
            }
            if (Input.GetMouseButtonDown(0)) OnTouchBegin();
            if (!Input.GetMouseButton(0) && !Input.GetMouseButtonUp(0)) return;
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, groundLayer))
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hit.point);
                if (Input.GetMouseButtonUp(0))
                {
                    speed = Mathf.Sqrt(Vector3.Distance(transform.position, hit.point)) * ballSettings.SpeedMultiplier;
                    direction = (hit.point - transform.position).normalized;
                    direction.y = 0;
                    OnTouchEnd();
                }
            }
        }

        void OnTouchBegin()
        {
            lineRenderer.gameObject.SetActive(true);
        }
        IEnumerator RollingRoutine()
        {
            while (speed > 0)
            {
                transform.position += direction * speed * rollingSpeedMult * Time.deltaTime;
                visibleObject.RotateAround(
                    visibleObject.position,
                    new Vector3(direction.z, 0, -direction.x),
                    speed * 0.35f * rollingSpeedMult);
                speed -= ballSettings.Friction * rollingSpeedMult * Time.deltaTime;
                yield return null;
            }
        }

        void OnTouchEnd()
        {
            if (movingCoroutine is not null) StopCoroutine(movingCoroutine);
            lineRenderer.gameObject.SetActive(false);
            movingCoroutine = StartCoroutine(RollingRoutine());
            lastWallId = 0;
        }

        void StopMoving()
        {
            StopCoroutine(movingCoroutine);
            movingCoroutine = null;
        }

        void PlaySmoke()
        {
            foreach (var s in smokeParticles)
            {
                s.Play();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetInstanceID() == lastWallId) return;
            if (collision.contactCount == 0) return;
            lastWallId = collision.gameObject.GetInstanceID();
            var contact = collision.GetContact(0);
            var newDirection = Vector3.Reflect(direction, contact.normal);
            if (speed > ballSettings.MinSquashSpeed) StartCoroutine(CollisionRoutine());
            else direction = newDirection;
            particles.position = contact.point;
            particles.rotation = Quaternion.LookRotation(contact.normal, Vector3.up);
            PlaySmoke();

            IEnumerator CollisionRoutine()
            {
                rollingSpeedMult = ballSettings.SquashRollingMult;
                rb.isKinematic = true;

                var lookRot = Quaternion.LookRotation(contact.normal, Vector3.up);
                transform.rotation = lookRot;
                visibleObject.Rotate(Vector3.up, -lookRot.eulerAngles.y);

                float compression = 0;
                float sign = 1f;
                bool squash = true;
                float angle = Vector3.Angle(newDirection, contact.normal);
                float compSpeed = ballSettings.SquashSpeed * (Mathf.Pow(angle, 1f / 2f) + 10f) * 0.1f;
                print(compSpeed);
                float targetCompression = Mathf.Clamp(0.25f * speed * 0.1f, 0, ballSettings.MaxSquash);
                var material = visibleObject.GetComponent<MeshRenderer>().material;
                while (squash && speed > 0)
                {
                    transform.position += direction * compSpeed * Time.deltaTime;
                    compression += sign * compSpeed * Time.deltaTime;
                    float delta = compression * 2;
                    transform.localScale = new Vector3(1f + delta, 1f + delta, 1f - delta);
                    material.color = Color.Lerp(Color.white, Color.red, compression / ballSettings.MaxSquash * 0.5f);
                    if (compression >= targetCompression)
                    {
                        direction = newDirection;
                        sign = -1f;
                    }
                    if (sign == -1f && compression <= 0f) squash = false;
                    yield return null;
                }

                transform.localScale = Vector3.one;
                visibleObject.Rotate(Vector3.up, lookRot.eulerAngles.y);
                transform.localRotation = Quaternion.identity;

                rb.isKinematic = false;
                rollingSpeedMult = 1f;
            }
        }
    }
}