using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BulletPrefabs{
    public BulletShape shape;
    public Bullet prefab;
    public int prewarm;
}
public class BulletPool: MonoBehaviour{
    
    public static BulletPool Instance{get;private set;}
    
    const int MaxPoolSize = 200;//池最大容量
    #if UNITY_EDITOR
    private int deNumber,intNumber,recycleNumber;//调试用，出队、生成、回收次数
    #endif
    private Dictionary<BulletShape,Bullet> bulletMap;//若干形状个字典
    private Dictionary<BulletShape,Queue<Bullet>> pools;//池本体
    [SerializeField] private BulletPrefabs[] bulletPrefabs;

    void Awake(){
        if(Instance==null)Instance=this;
        bulletMap=new Dictionary<BulletShape, Bullet>();
        pools=new Dictionary<BulletShape, Queue<Bullet>>();
        foreach (var prefabs in bulletPrefabs){
            bulletMap[prefabs.shape]=prefabs.prefab;
            pools[prefabs.shape]=new Queue<Bullet>();//初始化四个池
            for (int i = 0; i < prefabs.prewarm; i++) {//预热四个池
                Bullet bullet=Instantiate(bulletMap[prefabs.shape],transform);
                bullet.gameObject.SetActive(false);
                pools[prefabs.shape].Enqueue(bullet);
            }
        }
    }

    void OnDestroy(){if(Instance==this)Instance=null;}
    public Bullet Get(BulletShape shape,Vector3 position,Quaternion rotation){
        if(!pools.TryGetValue(shape,out Queue<Bullet> pool)) {
            Debug.LogError($"没有形状({shape})的池子");
            return null;
        }

        #if UNITY_EDITOR
        if (pools[shape].Count > 0) {deNumber++;}
        else{intNumber++;}
        #endif

        Bullet bullet=pool.Count > 0?pools[shape].Dequeue():Instantiate(bulletMap[shape],transform);
        bullet.transform.SetPositionAndRotation(position,rotation);
        bullet.gameObject.SetActive(true);
        return bullet;
    }
    public void ReCycle(Bullet bullet){
        if(!pools.TryGetValue(bullet.Shape, out var queue))return;//若是未激活子弹直接跳过，防止重复回收
        bullet.gameObject.SetActive(false);
        if (queue.Count > MaxPoolSize) {
            Destroy(bullet.gameObject);
        }else {
            queue.Enqueue(bullet);
            #if UNITY_EDITOR
            recycleNumber++;
            #endif
        }
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
        GUI.Label(new Rect(20, 20, 500, 50), $"当前调用池实例{deNumber}次、创建实例{intNumber}次、回收{recycleNumber}次、池内实例数为{GetTotalPoolCount()}个", style);
    }
    #endif

}
