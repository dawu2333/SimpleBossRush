using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Laser : MonoBehaviour{
    public int damage;
    public float width;
    public float rotateSpeed;
    void Start(){
        gameObject.transform.localScale=new Vector3(width!=0?width:1,transform.localScale.y,transform.localScale.z);
        gameObject.transform.localPosition=new Vector3(transform.localPosition.x,transform.localPosition.y,-1);
    }
    void Update(){
        if(gameObject.tag=="RotateLaser"){
            //Debug.Log("Rotating...");
            gameObject.transform.Rotate(new Vector3(0,0,rotateSpeed==0?-20*Time.deltaTime:rotateSpeed*Time.deltaTime));
        }
    }
}
