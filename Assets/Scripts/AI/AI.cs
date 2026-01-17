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
        Debug.Log($"FindPath½áÊø ºÄÊ±{(Time.realtimeSinceStartup - startTime) * 1000:F2}ms");
        monsterComp.SetPath(pathList);
    }

    // Update is called once per frame
    void Update()
    {
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
        }
    }
}
