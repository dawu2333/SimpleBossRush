using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour{
    public int damage;
    public float bulletSpeed;
    public bool isObstacle=false;
    void Start(){
        bulletSpeed*=10;
    }

    void Update(){transform.Translate(0,bulletSpeed*Time.deltaTime,0);}
    void OnTriggerEnter2D(Collider2D other){if(other.gameObject.tag == "Bullet"&&isObstacle){Destroy(other.gameObject);}}
}
