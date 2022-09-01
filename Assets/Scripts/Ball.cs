using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Ball : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float speedMultiplier, friction, squashTimeMult, squashScaleMult;

    Camera cam;
    float speed;
    Vector3 direction;
    Coroutine movingCoroutine;
    Rigidbody rb;
    int lastWallId = 0;

    void Awake()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
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
                speed = Mathf.Sqrt(Vector3.Distance(transform.position, hit.point)) * speedMultiplier;
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
    IEnumerator MovingRoutine()
    {
        while (speed > 0)
        {
            transform.position += direction * speed * Time.deltaTime;
            transform.RotateAround(
                transform.position,
                new Vector3(direction.z, 0, -direction.x),
                speed);
            speed -= friction * Time.deltaTime;
            yield return null;
        }
    }

    void OnTouchEnd()
    {
        if (movingCoroutine is not null) StopCoroutine(movingCoroutine);
        lineRenderer.gameObject.SetActive(false);
        movingCoroutine = StartCoroutine(MovingRoutine());
        lastWallId = 0;
    }

    void StopMoving()
    {
        StopCoroutine(movingCoroutine);
        movingCoroutine = null;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetInstanceID() == lastWallId) return;
        if (collision.contactCount == 0) return;
        lastWallId = collision.gameObject.GetInstanceID();
        var contact = collision.GetContact(0);
        direction = Vector3.Reflect(direction, contact.normal);
        StartCoroutine(CollisionRoutine());

        IEnumerator CollisionRoutine()
        {
            if (movingCoroutine != null) StopMoving();
            float angle = Vector3.Angle(contact.normal, direction);
            print(angle);
            print(speed);
            if (angle > 60 || speed < 1.5f) angle = 90;
            float squash = Mathf.Sqrt(speed) * Mathf.Abs(90 - angle) * 0.01f;
            if (squash > 0)
            {
                rb.isKinematic = true;
                float scaleDelta = squash * squashScaleMult;
                var targetScale = new Vector3(1f + scaleDelta, 1f + scaleDelta, 1f - scaleDelta);
                float duration = squash * squashTimeMult;
                transform.localRotation = Quaternion.LookRotation(contact.normal, Vector3.up);
                transform.DOLocalMove(transform.localPosition - contact.normal * 0.25f, duration);
                transform.DOScale(targetScale, duration);
                GetComponent<MeshRenderer>().material.DOColor(Color.Lerp(Color.white, Color.red, duration * 2f), duration);
                yield return new WaitForSeconds(duration);
                transform.DOScale(Vector3.one, duration);
                transform.DOLocalMove(transform.localPosition + contact.normal * 0.25f, duration);
                GetComponent<MeshRenderer>().material.DOColor(Color.white, duration);
                yield return new WaitForSeconds(duration);
                transform.localRotation = Quaternion.identity;
                rb.isKinematic = false;
            }
            movingCoroutine = StartCoroutine(MovingRoutine());
        }
    }
}
