using UnityEngine;

public struct BulletConfig{
    public BulletType type;
    public int damage;
    public float speed;
    public ObstacleMode mode;
    public float size;
    public Color color;
    public BulletConfig(BulletType type, int damage, float speed, ObstacleMode mode, float size, Color color){
        this.type = type;
        this.damage = damage;
        this.speed = speed;
        this.mode = mode;
        this.size = size;
        this.color = color;
    }
}
