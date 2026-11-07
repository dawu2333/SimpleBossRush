using UnityEngine;

//子弹属性配置
public struct BulletConfig{
    public BulletBehavior behavior;
    public int damage;
    public float speed;
    public ObstacleMode mode;
    public float size;
    public Color color;
    public BulletShape shape;
    public BulletConfig(BulletBehavior behavior, int damage, float speed, ObstacleMode mode, float size, Color color,BulletShape shape){
        this.behavior = behavior;
        this.damage = damage;
        this.speed = speed;
        this.mode = mode;
        this.size = size;
        this.color = color;
        this.shape=shape;
    }
}
