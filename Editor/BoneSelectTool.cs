using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Narazaka.Unity.BoneTools
{
    public class BoneSelectTool : EditorWindow
    {
        [MenuItem("Window/BoneSelectTool")]
        public static void Open()
        {
            GetWindow<BoneSelectTool>(nameof(BoneSelectTool));
        }

        Transform HumanoidRoot;
        BoneSelector BoneSelector;
        HashSet<Transform> Includes;
        HashSet<Transform> Excludes;
        Vector2 Scroll;

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = 40;
            var newHumanoidRoot = EditorGUILayout.ObjectField("Avatar", HumanoidRoot, typeof(Transform), true) as Transform;
            EditorGUIUtility.labelWidth = 0;
            if (newHumanoidRoot != HumanoidRoot)
            {
                HumanoidRoot = newHumanoidRoot;
                BoneSelector = null;
            }

            if (HumanoidRoot == null) return;
            if (BoneSelector == null)
            {
                BoneSelector = new BoneSelector(BoneReference.Make(HumanoidRoot));
            }
            if (Includes == null)
            {
                Includes = new HashSet<Transform>();
            }
            if (Excludes == null)
            {
                Excludes = new HashSet<Transform>();
            }

            if (GUILayout.Button("Select Bones"))
            {
                SelectBones();
            }
#if VRC_SDK_VRCSDK3
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select PhysBones"))
                {
                    SelectPhysBones();
                }
                if (GUILayout.Button("GameObjects", GUILayout.Width(90)))
                {
                    SelectPhysBoneGameObjects();
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select PhysBone Related Colliders"))
                {
                    SelectPhysBoneRelatedColliders();
                }
                if (GUILayout.Button("GameObjects", GUILayout.Width(90)))
                {
                    SelectPhysBoneRelatedColliderGameObjects();
                }
            }
#endif

            EditorGUILayout.LabelField("SkinnedMeshが使っているボーンを...");
            EditorGUILayout.LabelField("含む 除く");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("○", GUILayout.Width(24)))
                {
                    Includes = new HashSet<Transform>(BoneSelector.AllReferences);
                }
                if (GUILayout.Button("○", GUILayout.Width(24)))
                {
                    Excludes = new HashSet<Transform>(BoneSelector.AllReferences);
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("×", GUILayout.Width(24)))
                {
                    Includes = new HashSet<Transform>();
                }
                if (GUILayout.Button("×", GUILayout.Width(24)))
                {
                    Excludes = new HashSet<Transform>();
                }
            }
            using (var scrollView = new EditorGUILayout.ScrollViewScope(Scroll))
            {
                Scroll = scrollView.scrollPosition;
                foreach (var tr in BoneSelector.AllReferences.OrderBy(r => r.name))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var include = Includes.Contains(tr);
                        var newInclude = EditorGUILayout.Toggle(include, GUILayout.Width(24));
                        if (newInclude != include)
                        {
                            if (newInclude)
                            {
                                Includes.Add(tr);
                            }
                            else
                            {
                                Includes.Remove(tr);
                            }
                        }
                        var exclude = Excludes.Contains(tr);
                        var newExclude = EditorGUILayout.Toggle(exclude, GUILayout.Width(24));
                        if (newExclude != exclude)
                        {
                            if (newExclude)
                            {
                                Excludes.Add(tr);
                            }
                            else
                            {
                                Excludes.Remove(tr);
                            }
                        }
                        EditorGUILayout.LabelField(tr.name);
                    }
                }
            }
        }

        IEnumerable<BoneReference> FilteredBoneReferences()
        {
            if (Includes.Count == 0 && Excludes.Count == 0)
            {
                return Enumerable.Empty<BoneReference>();
            }
            else if (Includes.Count == 0 && Excludes.Count > 0)
            {
                return BoneSelector.FilteredBoneReferences(null, Excludes);
            }
            else
            {
                return BoneSelector.FilteredBoneReferences(Includes, Excludes);
            }
        }

        void SelectBones()
        {
            Selection.objects = FilteredBoneReferences().Select(r => r.Bone.gameObject).ToArray();
        }
#if VRC_SDK_VRCSDK3
        void SelectPhysBoneGameObjects()
        {
            Selection.objects = new PhysBoneSelector(HumanoidRoot).GetRelatedPhysBones(FilteredBoneReferences()).Select(pb => pb.gameObject).ToArray();
        }

        void SelectPhysBones()
        {
            Selection.objects = new PhysBoneSelector(HumanoidRoot).GetRelatedPhysBones(FilteredBoneReferences()).ToArray();
        }

        void SelectPhysBoneRelatedColliderGameObjects()
        {
            var selector = new PhysBoneSelector(HumanoidRoot);
            Selection.objects = selector.GetPhysBoneRelatedColliders(selector.GetRelatedPhysBones(FilteredBoneReferences())).Select(pb => pb.gameObject).ToArray();
        }

        void SelectPhysBoneRelatedColliders()
        {
            var selector = new PhysBoneSelector(HumanoidRoot);
            Selection.objects = selector.GetPhysBoneRelatedColliders(selector.GetRelatedPhysBones(FilteredBoneReferences())).ToArray();
        }
#endif
    }
}
