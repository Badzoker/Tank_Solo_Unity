using UnityEngine;

public class AITankControl : MonoBehaviour, ITankControl
{
    [SerializeField] private AITankState State;
    public AITankState curState => State;

    [Header("Patrol Area")] // Movement Patrol, Spawn Point 기준
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
    [SerializeField] private Transform turret;
    [SerializeField] private float detectRange = 15f;
    [SerializeField] private float detectAngle = 45f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Combat Fire")]
    [SerializeField] private float fireDelay = 5f;
    [SerializeField] private float fireDuration = 10f;
    [SerializeField] private float fireInterval = 1f;

    [Header("Combat Move")]
    [SerializeField] private float orbitRadiusFactor = 0.5f;
    [SerializeField] private float orbitRadialGain = 2.0f;
    [SerializeField] private float orbitExitFactor = 1.2f;   // 공전 탈출 완충(0.5R -> 0.6R)
    [SerializeField] private float orbitForwardMin = 0.2f;   // 공전 중 최소 전진



    public Vector2 Move { get; private set; }
    public Vector3 AimDir { get; private set; }
    public bool isAim { get; private set; }
    public bool isFire { get; private set; }

    private Vector3 currentGoal;
    private Vector3 spawnPos;
    private float waitTimer = 0f;
    private Vector3 lastMoveDir = Vector3.forward;
    private Transform curTarget;
    private bool isChasing = false;
    private float combatStartTime = 0f;
    private float nextFireTime = 0f;
    private bool orbitLeft = false;
    private bool isOrbiting = false;


    private void Start()
    {
        isAim = true;
        isFire = false;
        spawnPos = transform.position;
        lastMoveDir = transform.forward;
        State = AITankState.Patrol;
        GeneratePatrolPoint();
    }

    private void Update()
    {
        //Find Target
        curTarget = FindTarget(); // TODO: Find Player Tank and Change State

        UpdateState(curTarget);
        TickState(curTarget);
    }

    private void UpdateState(Transform _target)
    {
        if(isChasing)
        {
            if(State != AITankState.Combat)
            {
                ChangeState(AITankState.Combat);
            }
            return;

        }
        if(State == AITankState.Patrol && _target != null)
        {
            isChasing = true;
            ChangeState(AITankState.Combat);
        }
    }

    private void TickState(Transform _target)
    {
        switch(State)
        {
            case AITankState.Patrol:
                DoPatrol();
            break;
            case AITankState.Combat:
                DoCombat(_target);
                break;
        }
    }

    private void ChangeState(AITankState _Estate)
    {
        if (State == _Estate)
            return;

        OnExitState(State);
        State = _Estate;
        OnEnterState(State);
    }

    private void OnEnterState(AITankState _Estate)
    {
        if(_Estate == AITankState.Combat)
        {
            Debug.Log($"[AI] {name} → COMBAT 진입! Target 발견", this);
            combatStartTime = Time.time;
            nextFireTime = Time.time + fireDelay;
            orbitLeft = Random.value > 0.5f;
            isFire = false;
        }
    }

    private void OnExitState(AITankState _Estate)
    {
        // 필요하면 종료 처리
    }

    Transform FindTarget()
    {
        if (turret == null) return null;

        // 1) 거리 내 후보 찾기
        Collider[] hits = Physics.OverlapSphere(turret.position, detectRange, targetMask);
        if (hits == null || hits.Length == 0) return null;

        // 2) 후보 중 "가장 적합한" 타겟 선택(가까운 순 / 시야 중심에 가까운 순 등)
        Transform best = null;
        float bestScore = float.NegativeInfinity;

        Vector3 origin = turret.position;
        Vector3 forward = turret.forward;

        float cosHalfFov = Mathf.Cos(detectAngle * Mathf.Deg2Rad); // dot 비교용(빠르고 깔끔)

        for (int i = 0; i < hits.Length; i++)
        {
            Transform t = hits[i].transform;
            Vector3 targetPoint = t.position + Vector3.up * 0.8f;

            Vector3 to = targetPoint - origin;
            float dist = to.magnitude;
            if (dist <= 0.0001f) continue;
            Vector3 dir = to / dist;

            float dot = Vector3.Dot(forward, dir);
            if (dot < cosHalfFov) continue; // 45도 밖이면 탈락


            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, obstacleMask))
            {
                continue; // 중간에 장애물 있으면 가림
            }

            float score = dot * 2.0f - (dist / detectRange); // 가중치는 취향대로
            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    void DoPatrol()
    {
        // Patrol Logic
        bool isWaiting = PatrolMove();
        // Turret Logic
        if (isWaiting)
        {
            PatrolTurret_Waiting();
        }
        else
        {
            PatrolTurret_Moving();
        }
    }
    void DoCombat(Transform _target)
    {
        isAim = true;
        isFire = false;

        if(_target == null)
        {
            Move = Vector2.zero;
            return;
        }

        Vector3 toT = (_target.position - turret.position);
        toT.y = 0f;
        if (toT.sqrMagnitude > 0.0001f)
            AimDir = toT.normalized;

        // --- 발사 타이머: (진입 후 5초 대기) + (그 다음 10초 동안 1초마다) ---
        float elapsed = Time.time - combatStartTime;

        float cycle = fireDelay + fireDuration;
        float tInCycle = elapsed % cycle; // 0~cycle 반복

        bool inFireWindow = (tInCycle >= fireDelay); // delay 지나면 발사 구간(나머지 10초)

        if (inFireWindow && Time.time >= nextFireTime)
        {
            isFire = true;
            Debug.Log($"[AI] {name} → isFire!", this);
            nextFireTime += fireInterval;

            // (선택) 혹시 프레임 드랍으로 nextFireTime이 너무 뒤처졌으면 따라잡기
            if (nextFireTime < Time.time) nextFireTime = Time.time + fireInterval;
        }

        // (delay 구간에서 쏘지 않게 보장)
        if (tInCycle < Time.deltaTime) // 새 사이클 시작 프레임 감지
        {
            nextFireTime = Time.time + fireDelay; // 다음 사격 시작 시각
        }

        float orbitRadius = detectRange * orbitRadiusFactor;
        float orbitExitRadius = orbitRadius * orbitExitFactor; // 완충 구간

        Vector3 toTank = (_target.position - transform.position);
        toTank.y = 0f;
        float dist = toTank.magnitude;

        if (!isOrbiting && dist <= orbitRadius) isOrbiting = true;
        else if (isOrbiting && dist >= orbitExitRadius) isOrbiting = false;

        Vector3 desiredWorldDir;

        if (!isOrbiting)
        {
            // 추격
            desiredWorldDir = (dist > 0.0001f) ? (toTank / dist) : transform.forward;
        }
        else
        {
            // 공전
            Vector3 radial = (dist > 0.0001f) ? (toTank / dist) : transform.forward;
            Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;
            if (!orbitLeft) tangent = -tangent;

            // 반경 보정(너무 세면 움찔하니 일단 약하게/클램프)
            float radialError = orbitRadius - dist;
            radialError = Mathf.Clamp(radialError, -1.0f, 1.0f); //  보정 폭 제한
            Vector3 correction = -radial * radialError * orbitRadialGain;

            desiredWorldDir = (tangent + correction).normalized;
        }

        ApplyMoveFromWorldDir(desiredWorldDir, isOrbiting ? orbitForwardMin : 0f);
    }

    void ApplyMoveFromWorldDir(Vector3 _vec, float minForward)
    {
        _vec.y = 0f;
        if (_vec.sqrMagnitude < 0.0001f)
        {
            Move = Vector2.zero;
            return;
        }

        _vec.Normalize();
        Vector3 local = transform.InverseTransformDirection(_vec);

        float turn = Mathf.Clamp(local.x, -1f, 1f);
        float forward = Mathf.Clamp(local.z, -0.3f, 1f);     
        forward = Mathf.Max(forward, minForward);           

        Move = new Vector2(turn, forward);
        lastMoveDir = _vec;
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

        if (turret == null) return;

        Vector3 origin = turret.position;

        // 1) 탐지 거리 원 (OverlapSphere 반경)
        Gizmos.color = new Color(1f, 0.6f, 0f, 1f); // 주황
        Gizmos.DrawWireSphere(origin, detectRange);

        // 2) 시야각 경계선 (±detectAngle)
        // detectAngle은 "한쪽 각도"로 사용 중(±45도)
        Vector3 fwd = turret.forward;
        Vector3 leftDir = Quaternion.Euler(0f, -detectAngle, 0f) * fwd;
        Vector3 rightDir = Quaternion.Euler(0f, detectAngle, 0f) * fwd;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftDir.normalized * detectRange);
        Gizmos.DrawLine(origin, origin + rightDir.normalized * detectRange);

        // 3) 부채꼴 호(arc) - 보기 좋게 여러 선으로 근사
        int steps = 24; // 클수록 부드러움
        Vector3 prev = origin + leftDir.normalized * detectRange;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float ang = Mathf.Lerp(-detectAngle, detectAngle, t);
            Vector3 dir = Quaternion.Euler(0f, ang, 0f) * fwd;
            Vector3 next = origin + dir.normalized * detectRange;

            Gizmos.DrawLine(prev, next);
            prev = next;
        }

        // 4) 터렛 forward도 같이 표시(정면 확인용)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, origin + fwd.normalized * Mathf.Min(3f, detectRange));
    }
}
