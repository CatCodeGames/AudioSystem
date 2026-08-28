using System.Collections.Generic;
using UnityEngine.Audio;


namespace CatCode.Audio
{
    //public static class AudioMixerTreeBuilder
    //{
    //    private readonly struct BuildContext
    //    {
    //        public readonly int SourceIndex;
    //        public readonly int SourceChildStart;
    //        public readonly int SourceChildCount;

    //        public readonly int NodeCurrentIndex;
    //        public readonly int NodeParentIndex;

    //        public readonly string Path;

    //        public BuildContext(
    //            int sourceIndex,
    //            int sourceChildStart,
    //            int sourceChildCount,
    //            int nodeCurrentIndex,
    //            int nodeParentIndex,
    //            string path)
    //        {
    //            SourceIndex = sourceIndex;

    //            SourceChildStart = sourceChildStart;
    //            SourceChildCount = sourceChildCount;

    //            NodeCurrentIndex = nodeCurrentIndex;
    //            NodeParentIndex = nodeParentIndex;

    //            Path = path;
    //        }
    //    }

    //    public static AudioMixerModel Build(AudioMixer mixer)
    //    {
    //        var allGroups = mixer.FindMatchingGroups("");

    //        var nodes = new IndexedTreeNode[allGroups.Length];
    //        var audioMixerGroups = new AudioMixerGroup[allGroups.Length];

    //        var stack = new Stack<BuildContext>(allGroups.Length);

    //        stack.Push(new BuildContext(
    //            sourceIndex: 0,
    //            sourceChildStart: 1,
    //            sourceChildCount: allGroups.Length - 1,
    //            nodeCurrentIndex: 0,
    //            nodeParentIndex: -1,
    //            path: "Master/"));

    //        var nextNodeIndex = 1;

    //        while (stack.Count > 0)
    //        {
    //            var current = stack.Pop();

    //            int childNodeStart = current.SourceChildCount > 0
    //                ? nextNodeIndex
    //                : -1;

    //            int childCount = 0;

    //            int sourceChildIndex = current.SourceChildStart;
    //            int sourceChildEnd = current.SourceChildStart + current.SourceChildCount;

    //            while (sourceChildIndex < sourceChildEnd)
    //            {
    //                var childGroup = allGroups[sourceChildIndex];
    //                var childPath = $"{current.Path}{childGroup.name}/";

    //                var descendants = mixer.FindMatchingGroups(childPath);

    //                stack.Push(new BuildContext(
    //                    sourceIndex: sourceChildIndex,
    //                    sourceChildStart: descendants.Length > 0 ? sourceChildIndex + 1 : -1,
    //                    sourceChildCount: descendants.Length,
    //                    nodeCurrentIndex: nextNodeIndex++,
    //                    nodeParentIndex: current.NodeCurrentIndex,
    //                    path: childPath));

    //                sourceChildIndex += descendants.Length + 1;
    //                childCount++;
    //            }

    //            var group = allGroups[current.SourceIndex];

    //            nodes[current.NodeCurrentIndex] = new(
    //                parentIndex: current.NodeParentIndex,
    //                childStart: childNodeStart,
    //                childCount: childCount);

    //            audioMixerGroups[current.NodeCurrentIndex] = group;
    //        }

    //        return new AudioMixerModel(nodes, audioMixerGroups);
    //    }
    //}
}
