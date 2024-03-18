using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class BuildAnimation : Editor
{
    private static float FrameRate = 20f;
    private static float FrameTime = 0.05f;
    private static string Idle_AnimName = "Idle";
    private static string[] LoopAnimNames = new string[]
    {
        "Idle",
        "On",
    };
    //生成出的AnimationController的路径
    private static string AnimationControllerPath = "Assets/AnimationController";
    //生成出的Animation的路径
    private static string AnimationPath = "Assets/Animation";
    //原始图片路径
    private static string ImagePath = "Assets/Raw";

    [MenuItem("Build/BuildAnimaiton")]
    static void BuildAniamtion()
    {
        DirectoryInfo raw = new DirectoryInfo(ImagePath);
        foreach (DirectoryInfo dictorys in raw.GetDirectories())
        {
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (DirectoryInfo dictoryAnimations in dictorys.GetDirectories())
            {
                clips.Add(BuildAnimationClip(dictoryAnimations));
            }
            BuildAnimationController(clips, dictorys.Name);
        }
    }


    static AnimationClip BuildAnimationClip(DirectoryInfo dictorys)
    {
        string animationName = dictorys.Name;
        FileInfo[] images = dictorys.GetFiles("*.png");
        Array.Sort(images, (x1, x2) => int.Parse(Regex.Match(x1.Name, @"\d+").Value).CompareTo(int.Parse(Regex.Match(x2.Name, @"\d+").Value)));
        AnimationClip clip = new AnimationClip();
        EditorCurveBinding curveBinding = new EditorCurveBinding();
        curveBinding.type = typeof(SpriteRenderer);
        curveBinding.path = "";
        curveBinding.propertyName = "m_Sprite";
        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(DataPathToAssetPath(images[i].FullName));
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = FrameTime * i,
                value = sprite
            };
        }
        clip.frameRate = FrameRate;

        // 设置为循环动画
        for (int i = 0; i < LoopAnimNames.Length; i++)
        {
            if (animationName.IndexOf(LoopAnimNames[i]) >= 0)
            {
                SerializedObject serializedClip = new SerializedObject(clip);
                AnimationClipSettings clipSettings = new AnimationClipSettings(serializedClip.FindProperty("m_AnimationClipSettings"));
                clipSettings.loopTime = true;
                serializedClip.ApplyModifiedProperties();
                break;
            }
        }

        string parentName = System.IO.Directory.GetParent(dictorys.FullName).Name;
        System.IO.Directory.CreateDirectory(Path.Combine(AnimationPath, parentName));
        AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);
        AssetDatabase.CreateAsset(clip, Path.Combine(AnimationPath, parentName, animationName + ".anim"));
        AssetDatabase.SaveAssets();
        return clip;
    }

    static AnimatorController BuildAnimationController(List<AnimationClip> clips, string name)
    {
        string controllerPath = Path.Combine(AnimationControllerPath, name + ".controller");
        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (animatorController == null)
        {
            animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        animatorController.AddLayer("New Layer");
        animatorController.RemoveLayer(0);
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        foreach (AnimationClip newClip in clips)
        {
            AnimatorState state = stateMachine.AddState(newClip.name);
            state.motion = newClip;
            // 设置默认动画
            if (newClip.name == Idle_AnimName)
            {
                stateMachine.defaultState = state;
            }
        }

        AssetDatabase.SaveAssets();
        return animatorController;
    }

    public static string DataPathToAssetPath(string path)
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
            return path.Substring(path.IndexOf("Assets\\"));
        else
            return path.Substring(path.IndexOf("Assets/"));
    }

    [MenuItem("CONTEXT/AnimatorController/BuildAnimInfo")]
    public static void BuildAnimInfo(MenuCommand menuCommand)
    {
        AnimatorController animatorController = (AnimatorController)menuCommand.context;
        Dictionary<string, AnimInfo> anims = new Dictionary<string, AnimInfo>();
        foreach (var s in animatorController.animationClips)
        {
            anims.Add(s.name, new AnimInfo(0, s.length, Animator.StringToHash(s.name)));
        }
        using (FileStream fileStream = new FileStream(Path.Combine(AnimationControllerPath, animatorController.name + ".json"), FileMode.Create, FileAccess.Write))
        {
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(JsonConvert.SerializeObject(anims, Formatting.Indented));
            }
        }

        AssetDatabase.Refresh();
    }


    class AnimationClipSettings
    {
        SerializedProperty m_Property;

        private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

        public AnimationClipSettings(SerializedProperty prop) { m_Property = prop; }

        public float startTime { get { return Get("m_StartTime").floatValue; } set { Get("m_StartTime").floatValue = value; } }
        public float stopTime { get { return Get("m_StopTime").floatValue; } set { Get("m_StopTime").floatValue = value; } }
        public float orientationOffsetY { get { return Get("m_OrientationOffsetY").floatValue; } set { Get("m_OrientationOffsetY").floatValue = value; } }
        public float level { get { return Get("m_Level").floatValue; } set { Get("m_Level").floatValue = value; } }
        public float cycleOffset { get { return Get("m_CycleOffset").floatValue; } set { Get("m_CycleOffset").floatValue = value; } }

        public bool loopTime { get { return Get("m_LoopTime").boolValue; } set { Get("m_LoopTime").boolValue = value; } }
        public bool loopBlend { get { return Get("m_LoopBlend").boolValue; } set { Get("m_LoopBlend").boolValue = value; } }
        public bool loopBlendOrientation { get { return Get("m_LoopBlendOrientation").boolValue; } set { Get("m_LoopBlendOrientation").boolValue = value; } }
        public bool loopBlendPositionY { get { return Get("m_LoopBlendPositionY").boolValue; } set { Get("m_LoopBlendPositionY").boolValue = value; } }
        public bool loopBlendPositionXZ { get { return Get("m_LoopBlendPositionXZ").boolValue; } set { Get("m_LoopBlendPositionXZ").boolValue = value; } }
        public bool keepOriginalOrientation { get { return Get("m_KeepOriginalOrientation").boolValue; } set { Get("m_KeepOriginalOrientation").boolValue = value; } }
        public bool keepOriginalPositionY { get { return Get("m_KeepOriginalPositionY").boolValue; } set { Get("m_KeepOriginalPositionY").boolValue = value; } }
        public bool keepOriginalPositionXZ { get { return Get("m_KeepOriginalPositionXZ").boolValue; } set { Get("m_KeepOriginalPositionXZ").boolValue = value; } }
        public bool heightFromFeet { get { return Get("m_HeightFromFeet").boolValue; } set { Get("m_HeightFromFeet").boolValue = value; } }
        public bool mirror { get { return Get("m_Mirror").boolValue; } set { Get("m_Mirror").boolValue = value; } }
    }

}