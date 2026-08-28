using CatCode.EventPrimitives;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace CatCode.Audio
{
    public sealed class AudioMixerMirror
    {
        private readonly struct BuildData
        {
            public readonly int SourceIndex;
            public readonly int SourceChildStart;
            public readonly int SourceChildCount;

            public readonly int NodeCurrentIndex;
            public readonly int NodeParentIndex;

            public readonly string Path;

            public BuildData(
                int sourceIndex,
                int sourceChildStart,
                int sourceChildCount,
                int nodeCurrentIndex,
                int nodeParentIndex,
                string path)
            {
                SourceIndex = sourceIndex;

                SourceChildStart = sourceChildStart;
                SourceChildCount = sourceChildCount;

                NodeCurrentIndex = nodeCurrentIndex;
                NodeParentIndex = nodeParentIndex;

                Path = path;
            }
        }

        private readonly AudioMixerGroupPropertyStore _pauseStateStore;
        private readonly AudioMixerGroupPropertyStore _muteStateStore;

        private readonly Dictionary<AudioMixerGroup, AudioMixerGroupMirror> _states;
        private readonly AudioMixerGroupMirror _empty;

        public IReadOnlyDictionary<AudioMixerGroup, AudioMixerGroupMirror> GroupStates => _states;

        public AudioMixerMirror(
            AudioMixerGroupPropertyStore pauseStateStore,
            AudioMixerGroupPropertyStore muteStateStore,
            Dictionary<AudioMixerGroup, AudioMixerGroupMirror> states,
            AudioMixerGroupMirror empty)
        {
            _pauseStateStore = pauseStateStore;
            _muteStateStore = muteStateStore;
            _states = states;
            _empty = empty;
        }

        public AudioMixerGroupMirror GetMirror(AudioMixerGroup group)
        {
            if (group == null)
                return _empty;

            return _states[group];
        }

        public static AudioMixerMirror Build(AudioMixer[] audioMixers)
        {
            var mixerGroups = new AudioMixerGroup[audioMixers.Length][];
            var totalCount = 0;

            for (var i = 0; i < audioMixers.Length; i++)
            {
                var groups = audioMixers[i].FindMatchingGroups("");
                mixerGroups[i] = groups;
                totalCount += groups.Length;
            }

            var nodes = new IndexedTreeNode[totalCount];
            var stack = new Stack<BuildData>(totalCount);
            var mirrors = new Dictionary<AudioMixerGroup, AudioMixerGroupMirror>(totalCount);

            var pauseStates = new BoolStateData[totalCount];
            var muteStates = new BoolStateData[totalCount];

            for (var i = 0; i < totalCount; i++)
            {
                pauseStates[i] = new BoolStateData(false, false, ObservableSource<bool>.CreateDefault());
                muteStates[i] = new BoolStateData(false, false, ObservableSource<bool>.CreateDefault());
            }

            var pauseStore = new AudioMixerGroupPropertyStore(nodes, pauseStates);
            var muteStore = new AudioMixerGroupPropertyStore(nodes, muteStates);

            var nextNodeIndex = 0;

            for (var mixerIndex = 0; mixerIndex < mixerGroups.Length; mixerIndex++)
            {
                var audioMixer = audioMixers[mixerIndex];
                var allGroups = mixerGroups[mixerIndex];

                var count = allGroups.Length;

                if (count == 0)
                    continue;

                var rootIndex = nextNodeIndex;

                stack.Push(new BuildData(
                    sourceIndex: 0,
                    sourceChildStart: 1,
                    sourceChildCount: count - 1,
                    nodeCurrentIndex: rootIndex,
                    nodeParentIndex: -1,
                    path: "Master/"));

                nextNodeIndex++;

                while (stack.Count > 0)
                {
                    var current = stack.Pop();

                    int childNodeStart = current.SourceChildCount > 0
                        ? nextNodeIndex
                        : -1;

                    int childCount = 0;

                    int sourceChildIndex = current.SourceChildStart;
                    int sourceChildEnd = current.SourceChildStart + current.SourceChildCount;

                    while (sourceChildIndex < sourceChildEnd)
                    {
                        var childGroup = allGroups[sourceChildIndex];
                        var childPath = $"{current.Path}{childGroup.name}/";

                        var descendants = audioMixer.FindMatchingGroups(childPath);

                        stack.Push(new BuildData(
                            sourceIndex: sourceChildIndex,
                            sourceChildStart: descendants.Length > 0 ? sourceChildIndex + 1 : -1,
                            sourceChildCount: descendants.Length,
                            nodeCurrentIndex: nextNodeIndex,
                            nodeParentIndex: current.NodeCurrentIndex,
                            path: childPath));

                        nextNodeIndex++;
                        sourceChildIndex += descendants.Length + 1;
                        childCount++;
                    }

                    nodes[current.NodeCurrentIndex] = new(
                        parentIndex: current.NodeParentIndex,
                        childStart: childNodeStart,
                        childCount: childCount);

                    var pauseState = new AudioMixerGroupBoolProperty(current.NodeCurrentIndex, pauseStore);
                    var muteState = new AudioMixerGroupBoolProperty(current.NodeCurrentIndex, muteStore);

                    var group = allGroups[current.SourceIndex];
                    var mirror = new AudioMixerGroupMirror(pauseState, muteState);

                    mirrors.Add(group, mirror);
                }
            }
            var empty = new AudioMixerGroupMirror(new EmptyBoolProperty(false), new EmptyBoolProperty(false));

            return new AudioMixerMirror(pauseStore, muteStore, mirrors, empty);
        }
    }
}
