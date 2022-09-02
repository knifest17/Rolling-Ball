using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] LayerMask groundLayer;
    [SerializeField]
    float
        speedMultiplier,
        friction,
        squashSpeed,
        squashSpeedMult,
        minSquashSpeed,
        maxSquash;
    [SerializeField] Transform visibleObject;

    Camera cam;
    float speed;
    Vector3 direction;
    Coroutine movingCoroutine;
    Rigidbody rb;
    int lastWallId = 0;
    float rollingSpeedMult = 1f;
    

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
            transform.position += direction * speed * rollingSpeedMult * Time.deltaTime;
            visibleObject.RotateAround(
                visibleObject.position,
                new Vector3(direction.z, 0, -direction.x),
                speed * 0.5f * rollingSpeedMult);
            speed -= friction * rollingSpeedMult * Time.deltaTime;
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
        var newDirection = Vector3.Reflect(direction, contact.normal);
        if (speed > minSquashSpeed) StartCoroutine(CollisionRoutine());
        else direction = newDirection;

        IEnumerator CollisionRoutine()
        {
            rollingSpeedMult = 0.5f;
            rb.isKinematic = true;

            var lookRot = Quaternion.LookRotation(contact.normal, Vector3.up);
            transform.rotation = lookRot;
            visibleObject.Rotate(Vector3.up, -lookRot.eulerAngles.y);

            float compression = 0;
            float sign = 1f;
            bool squash = true;
            float angle = Vector3.Angle(newDirection, contact.normal);
            float compSpeed = squashSpeed * (Mathf.Pow(angle, 1f / 2f) + 10f) * 0.1f;
            print(speed);
            float targetCompression = Mathf.Clamp(0.25f * speed * 0.1f, 0, maxSquash);
            while (squash && speed > 0)
            {
                transform.position += direction * compSpeed * Time.deltaTime;
                compression += sign * compSpeed * Time.deltaTime;
                float delta = compression * 2;
                transform.localScale = new Vector3(1f + delta, 1f + delta, 1f - delta);
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

    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.GetInstanceID() == lastWallId) return;
    //    if (collision.contactCount == 0) return;
    //    lastWallId = collision.gameObject.GetInstanceID();
    //    var contact = collision.GetContact(0);
    //    var newDirection = Vector3.Reflect(direction, contact.normal);
    //    if (speed > minSquashSpeed) StartCoroutine(CollisionRoutine());
    //    else direction = newDirection;

    //    IEnumerator CollisionRoutine()
    //    {
    //        if (movingCoroutine != null) StopMoving();
    //        rb.isKinematic = true;

    //        var lookRot = Quaternion.LookRotation(contact.normal, Vector3.up);
    //        transform.rotation = lookRot;
    //        visibleObject.Rotate(Vector3.up, -lookRot.eulerAngles.y);

    //        float compression = 0;
    //        float sign = 1f;
    //        bool squash = true;
    //        float angle = Vector3.Angle(newDirection, contact.normal);
    //        float squashSpeed = (1 / speed * squashSpeedMult) * (angle + 10f) * 0.1f;
    //        print(speed);
    //        float maxCompression = 0.25f * speed * 0.1f;
    //        while (squash)
    //        {
    //            transform.position += direction * squashSpeed * Time.deltaTime;
    //            float delta = compression * 2;
    //            transform.localScale = new Vector3(1f + delta, 1f + delta, 1f - delta);
    //            compression += sign * squashSpeed * Time.deltaTime;
    //            if (compression >= maxCompression)
    //            {
    //                direction = newDirection;
    //                sign = -1f;
    //            }
    //            if (sign == -1f && compression <= 0f) squash = false;
    //            yield return null;
    //        }

    //        transform.localScale = Vector3.one;
    //        visibleObject.Rotate(Vector3.up, lookRot.eulerAngles.y);
    //        transform.localRotation = Quaternion.identity;

    //        rb.isKinematic = false;
    //        movingCoroutine = StartCoroutine(MovingRoutine());
    //    }
    //}
}
