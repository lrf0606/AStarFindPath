using UnityEngine;


public class AI : MonoBehaviour
{
    public GameObject Monster;
    private AStarFindPath m_FindPath;
    // Start is called before the first frame update

    private void Awake()
    {
        m_FindPath = new AStarFindPath();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    public void MonsterFindPath(Vector3 targetPos)
    {
        if (Monster == null)
        {
            return;
        }
        var monsterComp = Monster.GetComponent<Monster>();
        var startGrid = MapUtil.WorldPos2Grid(Monster.transform.position);
        var targetGrid = MapUtil.WorldPos2Grid(targetPos);
        float startTime = Time.realtimeSinceStartup;
        var pathList = m_FindPath.FindPath(startGrid, targetGrid, monsterComp.Walkable);
        if (pathList == null)
        {
            return;
        }
        Debug.Log($"FindPath结束 耗时{(Time.realtimeSinceStartup - startTime) * 1000:F2}ms");
        monsterComp.SetPath(pathList);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            var mask = LayerMask.GetMask("Ground");
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                var targetPos = hit.point;
                MonsterFindPath(targetPos);
            }
        }*/

        if (Input.GetKeyUp(KeyCode.Space))
        {
            TestFindPath();
        }
    }

    public void TestFindPath()
    {
        if (Monster == null)
        {
            return;
        }
        var startPos = new Vector3(40, 1, 0);
        var targetPos = new Vector3(-40, 1, 0);
        Monster.transform.position = startPos;
        var monsterComp = Monster.GetComponent<Monster>();
        float startTime = Time.realtimeSinceStartup;
        var pathList = m_FindPath.FindPath(MapUtil.WorldPos2Grid(startPos), MapUtil.WorldPos2Grid(targetPos), monsterComp.Walkable);
        if (pathList == null)
        {
            return;
        }
        Debug.Log($"FindPath结束 起点{startPos}终点{targetPos}耗时{(Time.realtimeSinceStartup - startTime) * 1000:F3}ms"); // 10-11ms
        monsterComp.SetPath(pathList);
    }
}
