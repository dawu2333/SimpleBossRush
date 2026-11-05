using UnityEngine;
public static class InitInstanceData{
    public static BulletConfig PlayerBulletConfig(){return new BulletConfig(BulletType.SquareBullet,1,50f,ObstacleMode.None,1f,Color.white);}
    public static BulletConfig BossBulletConfig(int i){
        switch (i){
            case 1:return new BulletConfig(BulletType.CircleBullet,1,30,ObstacleMode.None,1f,Color.white);
            case 2:return new BulletConfig(BulletType.CircleBullet,1,65f,ObstacleMode.None,1.4f,Color.yellow);
            case 3:return new BulletConfig(BulletType.CircleBullet,2,10,ObstacleMode.Persistent,10f,Color.red);
            case 4:return new BulletConfig(BulletType.CircleBullet,1,30,ObstacleMode.None,1.4f,Color.blue);
            case 5:return new BulletConfig(BulletType.CircleBullet,1,10,ObstacleMode.None,1.3f,Color.green);
            case 6:return new BulletConfig(BulletType.CircleBullet,1,35f,ObstacleMode.None,1.5f,Color.cyan);
            case 7:return new BulletConfig(BulletType.CircleBullet,1,8f,ObstacleMode.None,1.3f,new Color(1.0f, 0.753f, 0.796f));
            default:return new BulletConfig(BulletType.CircleBullet,1,30f,ObstacleMode.None,1f,Color.white);
        }
    }
    public static LaserConfig BossLaserConfig(int i){
        switch (i){
            case 1:return new LaserConfig(LaserType.NormalLaser,3,1f,0);
            case 2:return new LaserConfig(LaserType.NormalLaser,3,.8f,0);
            case 3:return new LaserConfig(LaserType.RotateLaser,3,1f,-25);
            default:return new LaserConfig(LaserType.NormalLaser,3,1f,0);
        }
    }
}
