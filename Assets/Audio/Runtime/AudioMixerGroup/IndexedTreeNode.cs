namespace CatCode.Audio
{
    public readonly struct IndexedTreeNode
    {
        public readonly int ParentIndex;

        public readonly int ChildIndex;
        public readonly int ChildCount;

        public IndexedTreeNode(
            int parentIndex,
            int childStart,
            int childCount)
        {
            ParentIndex = parentIndex;
            ChildIndex = childStart;
            ChildCount = childCount;
        }
    }
}
