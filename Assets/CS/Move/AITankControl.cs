using UnityEngine;
using UnityEngine.Android;

public class AITankControl : MonoBehaviour, ITankControl
{
    [Header("Patrol Area")] // Movement Patrol, Spawn Point 기준
    //public Transform[] PatrolPoints;
    public Vector2 PatrolAreaSize = new Vector2(12f, 12f);
    public float FixedY = 0f;

    [Header("Move")]
    public float ArriveDistance = 2f;
    public float pointWaitTime = 1f;

    [Header("Scout")] // Turret Scouting
    public float scanAngleSpeed = 45f;
    public float scanAngleRange = 70f;
    int scanDir = 1;
    float scanYaw = 0f;


    [Header("Target")] // Targeting
    public float chaseSpeed = 1.5f;

    public Vector2 Move { get; private set; }
    public Vector3 AimDir { get; private set; }
    public bool isAim { get; private set; }
    public bool isFire { get; private set; }

    Vector3 currentGoal;
    Vector3 spawnPos;
    float waitTimer = 0f;
    Vector3 lastMoveDir = Vector3.forward;


    private void Start()
    {
        isAim = true;
        isFire = false;
        spawnPos = transform.position;
        lastMoveDir = transform.forward;
        GeneratePatrolPoint();
    }

    private void Update()
    {
        //Find Target
        Transform targetPoint = FindTarget(); // TODO: Find Player Tank and Change State

        // Patrol Logic
        bool isWaiting = PatrolMove();
        // Turret Logic
        if(isWaiting)
        {
            PatrolTurret_Waiting();
        }
        else
        {
            PatrolTurret_Moving();
        }
    }

    Transform FindTarget()
    {
        //player 탐색 로직 작성

        return null;
    }

    bool PatrolMove()
    {
        Vector3 dir = currentGoal - transform.position;
        dir.y = 0f;
        if(dir.magnitude <= ArriveDistance)
        {
            Move = Vector2.zero;
            waitTimer += Time.deltaTime;

            if(waitTimer >= pointWaitTime)
            {
                GeneratePatrolPoint();
                waitTimer = 0f;

                scanYaw = 0f;
                scanDir = 1;
            }
            return true;
        }


        Vector3 dirW = dir.normalized;
        lastMoveDir = dirW;
        Vector3 dirL = transform.InverseTransformDirection(dirW);

        float turn = Mathf.Clamp(dirL.x, -1f, 1f);
        float forward = Mathf.Clamp(dirL.z, 0f, 1f);

        Move = new Vector2(turn, forward);

        return false;
    }

    void PatrolTurret_Waiting()
    {
        scanYaw += scanDir * scanAngleSpeed * Time.deltaTime;

        if(scanYaw > scanAngleRange)
        {
            scanYaw = scanAngleRange;
            scanDir = -1;
        }
        if(scanYaw < -scanAngleRange)
        {
            scanYaw = -scanAngleRange;
            scanDir = 1;
        }

        Quaternion baseRot = Quaternion.LookRotation(lastMoveDir.sqrMagnitude < 0.0001f ? transform.forward : lastMoveDir, Vector3.up);
        Vector3 dir = (baseRot * Quaternion.Euler(0f, scanYaw, 0f)) * Vector3.forward;

        dir.y = 0f;
        if (dir.sqrMagnitude < 0.00001f)
            return;


        AimDir = dir.normalized;
        isAim = true;
    }

    void PatrolTurret_Moving()
    {
        Vector3 dir = lastMoveDir;
        dir.y = 0f;
        if(dir.sqrMagnitude < 0.001f)
        {
            return;
        }
        AimDir = dir.normalized;
        isAim = true;
    }

    void GeneratePatrolPoint()
    {
        float randX = Random.Range(-PatrolAreaSize.x, PatrolAreaSize.x);
        float randZ = Random.Range(-PatrolAreaSize.y, PatrolAreaSize.y);

        currentGoal = new Vector3(spawnPos.x + randX, FixedY, spawnPos.z + randZ);
    }


    private void OnDrawGizmosSelected() //디버깅용
    {
        Vector3 center = Application.isPlaying ? spawnPos : transform.position;

        center.y = FixedY;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(PatrolAreaSize.x * 2f, 0.1f, PatrolAreaSize.y * 2f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentGoal, 0.5f);
        Gizmos.DrawLine(new Vector3(transform.position.x, FixedY, transform.position.z), currentGoal);
    }
}
