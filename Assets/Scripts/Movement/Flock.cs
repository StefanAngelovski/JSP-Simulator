using UnityEngine;

public class Flock : MonoBehaviour
{
    float speed;
    bool turning = false;
    Vector3 targetPosition;

    void Start()
    {
        speed = Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
        SetNewTargetPosition(); // Initial target position
    }

    void Update()
    {
        Bounds b = new Bounds(FlockManager.FM.spawnPlatform.transform.position, FlockManager.FM.spawnPlatform.GetComponent<Collider>().bounds.size);

        if (!b.Contains(transform.position))
        {
            turning = true;
            SetNewTargetPosition(); // Choose a new target position within bounds
        }
        else
        {
            turning = false;
        }

        if (turning)
        {
            Vector3 direction = targetPosition - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), FlockManager.FM.rotationSpeed * Time.deltaTime);
        }
        else
        {
            if (Random.Range(0, 100) < 10)
            {
                speed = Random.Range(FlockManager.FM.minSpeed, FlockManager.FM.maxSpeed);
            }
            if (Random.Range(0, 100) < 10)
            {
                ApplyRules();
            }
            MoveTowardsTarget();
        }
        this.transform.Translate(0, 0, speed * Time.deltaTime); // Ensure continuous movement
    }

    void ApplyRules()
    {
        GameObject[] gos = FlockManager.FM.allFish;
        Vector3 vCenter = Vector3.zero;
        Vector3 vAvoid = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        int groupSize = 0;

        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {
                nDistance = Vector3.Distance(go.transform.position, this.transform.position);
                if (nDistance <= FlockManager.FM.neighbourDistance)
                {
                    vCenter += go.transform.position;
                    groupSize++;
                    if (nDistance < 1.0f)
                    {
                        vAvoid = vAvoid + (this.transform.position - go.transform.position);
                    }
                    Flock anotherFlock = go.GetComponent<Flock>();
                    gSpeed = gSpeed + anotherFlock.speed;
                }
            }
        }

        if (groupSize > 0)
        {
            vCenter = vCenter / groupSize + (FlockManager.FM.goalPos - this.transform.position);
            speed = gSpeed / groupSize;
            if (speed > FlockManager.FM.maxSpeed)
            {
                speed = FlockManager.FM.maxSpeed;
            }
            Vector3 direction = (vCenter + vAvoid) - transform.position;
            direction.y = 0; // Ensure no movement on the y-axis
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), FlockManager.FM.rotationSpeed * Time.deltaTime);
            }
        }
    }

    void SetNewTargetPosition()
    {
        targetPosition = FlockManager.FM.GetRandomPositionOnPlatform();
    }

    void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Ensure no movement on the y-axis
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            SetNewTargetPosition();
        }
    }
}
