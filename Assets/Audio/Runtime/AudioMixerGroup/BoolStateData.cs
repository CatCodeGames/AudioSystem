using CatCode.EventPrimitives;

namespace CatCode.Audio
{
    public struct BoolStateData
    {
        public bool Local;
        public bool Total;

        public ObservableSource<bool> OnChanged;

        public BoolStateData(bool local, bool total, ObservableSource<bool> observableSource)
        {
            Local = local;
            Total = total;
            OnChanged = observableSource;
        }
    }
}
