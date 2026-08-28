using CatCode.Collections;
using System.Collections.Generic;

namespace CatCode.Audio
{
    public sealed class EmptySlotStorage<T> : ISlotStorage<T>
    {
        public SlotId Add(T item)
            => new(-1, -1);

        public T Get(SlotId slotId)
            => throw new KeyNotFoundException();

        public bool IsValid(SlotId itemId)
            => false;

        public bool Remove(SlotId itemId)
            => false;

        public bool TryGet(SlotId itemId, out T item)
        {
            item = default;
            return false;
        }
    }
}