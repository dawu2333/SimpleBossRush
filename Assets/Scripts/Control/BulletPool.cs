using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct BulletPrefabs{
    public BulletType bulletType;
    public GameObject prefab;
}



public class BulletPool: MonoBehaviour{
    [SerializeField] private BulletPrefabs[] bulletPrefabs;

    private Dictionary<BulletType ,GameObject>[] BulletInstances;
    private Queue<Dictionary<BulletType ,GameObject>> CircleBulletPool;
    private Queue<Dictionary<BulletType ,GameObject>> SquareBulletPool;
    private Queue<Dictionary<BulletType ,GameObject>> FragmentedBulletPool;
    private Queue<Dictionary<BulletType ,GameObject>> ExplosiveBulletPool;


    void Awake(){
        
    }
    void Start(){
        
    }
    public GameObject CreateBullet(){
        return null;
    }

}
