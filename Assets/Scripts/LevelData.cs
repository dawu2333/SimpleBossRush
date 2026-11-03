using Unity.Mathematics;

public static class LevelData{
    public class Level{
        public int playerHP, bossHP,bossNum;
        public int[] bossAttackPattern;
        public float bossSpeed;
        public float bossScale,playerScale;
        public float playerAttackCoef;
        public float playerSpeed;
    }
    public static readonly Level[] levels = new Level[]{
        null,
        new(){playerHP=10,bossHP=200,bossNum=1,bossAttackPattern=new int[]{1,2,3,4,5},bossSpeed=5f,bossScale=5f,playerScale=1f,playerAttackCoef=1f,playerSpeed=10f},
        new(){playerHP=10,bossHP=250,bossNum=1,bossAttackPattern=new int[]{1,2,3,4,5,8},bossSpeed=5f,bossScale=5f,playerScale=1f,playerAttackCoef=1f,playerSpeed=10f},
        new(){playerHP=15,bossHP=400,bossNum=1,bossAttackPattern=new int[]{1,2,3,4,5,6,7,8,9,10},bossSpeed=5f,bossScale=5f,playerScale=0.8f,playerAttackCoef=1.2f,playerSpeed=10f},
        new(){playerHP=25,bossHP=150,bossNum=2,bossAttackPattern=new int[]{1,2,3,4,5,6,7,8,9},bossSpeed=7f,bossScale=3f,playerScale=0.6f,playerAttackCoef=1.5f,playerSpeed=10f},
        new(){playerHP=20,bossHP=800,bossNum=1,bossAttackPattern=new int[]{1,2,3,4,5,6,7,8,9,10},bossSpeed=3f,bossScale=9f,playerScale=0.4f,playerAttackCoef=2f,playerSpeed=10f}
    };

}