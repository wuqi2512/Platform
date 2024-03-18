using UnityEngine;

public static class Constant
{
    public static class Layers
    {
        public readonly static int EntityLayer;
        public readonly static int TileLayer;
        public readonly static int BlockLayer;

        static Layers()
        {
            EntityLayer = LayerMask.GetMask("Entity");
            TileLayer = LayerMask.GetMask("Tile");
            BlockLayer = LayerMask.GetMask("Block");
        }
    }

    public static class AnimHash
    {
        public static int Idle_Hash;
        public static int Run_Hash;
        public static int Fall_Hash;
        public static int Jump_Hash;
        public static int WallClimb_Hash;
        public static int WallGrab_Hash;
        public static int DoubleJump_Hash;
        public static int Hit_Hash;

        public const string Idle_AnimName = "Idle";
        public const string Run_AnimName = "Run";
        public const string Fall_AnimName = "Fall";
        public const string Jump_AnimName = "Jump";
        public const string WallClimb_AnimName = "WallClimb";
        public const string WallGrab_AnimName = "WallGrab";
        public const string DoubleJump_AnimName = "DoubleJump";
        public const string Hit_AnimName = "Hit";

        public static int On_Hash;
        public static int Off_Hash;

        public const string On_AnimName = "On";
        public const string Off_AinmName = "Off";

        static AnimHash()
        {
            Idle_Hash = Animator.StringToHash(Idle_AnimName);
            Jump_Hash = Animator.StringToHash(Jump_AnimName);
            WallClimb_Hash = Animator.StringToHash(WallClimb_AnimName);
            WallGrab_Hash = Animator.StringToHash(WallGrab_AnimName);
            DoubleJump_Hash = Animator.StringToHash(DoubleJump_AnimName);
            Fall_Hash = Animator.StringToHash(Fall_AnimName);
            Run_Hash = Animator.StringToHash(Run_AnimName);
            Hit_Hash = Animator.StringToHash(Hit_AnimName);

            On_Hash = Animator.StringToHash(On_AnimName);
            Off_Hash = Animator.StringToHash(Off_AinmName);
        }
    }
}