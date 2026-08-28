using CatCode.EventPrimitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CatCode.Audio
{
    public sealed class AudioMixerGroupPropertyStore
    {
        private readonly IndexedTreeNode[] _nodes;
        private readonly BoolStateData[] _states;

        public AudioMixerGroupPropertyStore(IndexedTreeNode[] nodes, BoolStateData[] states)
        {
            _nodes = nodes;
            _states = states;
        }

        internal bool GetLocalValue(int index)
            => _states[index].Local;

        internal bool GetTotalValue(int index)
            => _states[index].Total;

        internal void SetValue(int index, bool value)
        {
            ref readonly var node = ref _nodes[index];
            ref var state = ref _states[index];

            if (state.Local == value)
                return;

            state.Local = value;
            var newTotal = value;

            var parentIndex = node.ParentIndex;
            if (parentIndex >= 0)
                newTotal |= _states[parentIndex].Total;

            if (!UpdateState(ref state, newTotal))
                return;

            using var stackLease = UnityEngine.Pool.UnsafeGenericPool<Stack<int>>.Get(out var stack);

            stack.Clear();
            stack.Push(index);

            while (stack.Count > 0)
            {
                var nodeIndex = stack.Pop();

                ref readonly var currentNode = ref _nodes[nodeIndex];
                var parentState = _states[nodeIndex];

                var childStart = currentNode.ChildIndex;
                var childEnd = childStart + currentNode.ChildCount;

                for (int i = childStart; i < childEnd; i++)
                {
                    ref var childState = ref _states[i];
                    var childTotal = parentState.Total || childState.Local;

                    if (!UpdateState(ref childState, childTotal))
                        continue;

                    stack.Push(i);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool UpdateState(ref BoolStateData state, bool totalValue)
            {
                if (state.Total == totalValue)
                    return false;

                state.Total = totalValue;
                state.OnChanged.Publish(state.Total);

                return true;
            }
        }

        internal ObservableSubscription Subscribe(int index, Action<bool> callback)
            => _states[index].OnChanged.Subscribe(callback);
    }
}
