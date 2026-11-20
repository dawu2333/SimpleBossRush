using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LaserPrefabs{
    public LaserVisual visual;
    public Laser prefab;
    public int prewarm;
}
public class LaserPool: MonoBehaviour{
    
    public static LaserPool Instance{get;private set;}
    
    const int MaxPoolSize = 200;//池最大容量

    #if UNITY_EDITOR
    private int deNumber,intNumber,recycleNumber;//调试用，出队、生成、回收次数
    #endif
    private Dictionary<LaserVisual,Laser> laserMap;//若干形状个字典
    private Dictionary<LaserVisual,Queue<Laser>> pools;//池本体
    [SerializeField] private LaserPrefabs[] laserPrefabs;

    void Awake(){
        if (Instance != null && Instance != this) {
            enabled=false;//若出现第二个实例，则关闭自己
            return;
        }
        Instance = this;
        laserMap=new Dictionary<LaserVisual, Laser>();
        pools=new Dictionary<LaserVisual, Queue<Laser>>();
        foreach (var prefabs in laserPrefabs){
            laserMap[prefabs.visual]=prefabs.prefab;
            pools[prefabs.visual]=new Queue<Laser>();//初始化
            for (int i = 0; i < prefabs.prewarm; i++) {//预热
                Laser laser=Instantiate(laserMap[prefabs.visual],transform);
                laser.gameObject.SetActive(false);
                pools[prefabs.visual].Enqueue(laser);
            }
        }
    }

    void OnDestroy(){if(Instance==this)Instance=null;}
    public Laser Get(LaserType type,Vector3 position,Quaternion rotation,bool isPre=false){
        LaserVisual visual=ToVisual(type,isPre);
        if(!pools.TryGetValue(visual,out Queue<Laser> pool)) {
            Debug.LogError($"没有形状({visual})的池子");
            return null;
        }

        #if UNITY_EDITOR
        if (pools[visual].Count > 0) {deNumber++;}
        else{intNumber++;}
        #endif

        Laser laser=pool.Count > 0?pools[visual].Dequeue():Instantiate(laserMap[visual],transform);
        laser.transform.SetPositionAndRotation(position,rotation);
        laser.gameObject.SetActive(true);
        return laser;
    }
    public void ReCycle(Laser laser){
        if(!laser.gameObject.activeSelf) return;//防止重复回收
        LaserVisual visual=ToVisual(laser.Type,laser.Ispre);
        if(!pools.TryGetValue(visual, out var queue))return;//若是未激活的激光直接跳过，防止重复回收
        laser.gameObject.SetActive(false);
        if (queue.Count > MaxPoolSize) {//数量大于最大池大小，直接销毁
            Destroy(laser.gameObject);
        }else {
            queue.Enqueue(laser);
            #if UNITY_EDITOR
            recycleNumber++;
            #endif
        }
    }
    public static LaserVisual ToVisual(LaserType type, bool isPreLaser){
    return type == LaserType.RotateLaser
        ? (isPreLaser ? LaserVisual.RotatePre : LaserVisual.Rotate)
        : (isPreLaser ? LaserVisual.NormalPre : LaserVisual.Normal);
}

    #if UNITY_EDITOR
    private int GetTotalPoolCount(){
    int total = 0;
    foreach (var pool in pools.Values){
        total += pool.Count;
    }
    return total;
}
    public void OnGUI() {
        GUIStyle style = new GUIStyle {fontSize = 30,};
        style.normal.textColor = Color.white; 
        GUI.Label(new Rect(20, 50, 500, 50), $"当前激光调用池实例{deNumber}次、创建实例{intNumber}次、回收{recycleNumber}次、池内实例数为{GetTotalPoolCount()}个", style);
    }
    #endif

}
