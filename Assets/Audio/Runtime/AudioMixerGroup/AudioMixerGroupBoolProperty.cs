using CatCode.EventPrimitives;
using System;

namespace CatCode.Audio
{
    public interface IAudioMixerGroupBoolProperty 
    {
        bool LocalValue { get; }
        bool TotalValue { get; }
        void SetValue(bool value); 
        ObservableSubscription Subscribe(Action<bool> callback);
    }

    public sealed class EmptyBoolProperty : IAudioMixerGroupBoolProperty
    {
        private bool _value;
        private readonly ObservableSource<bool> _onChanged = ObservableSource<bool>.CreateDefault();

        public bool LocalValue => _value;
        public bool TotalValue => _value;

        public EmptyBoolProperty(bool value)
        {
            _value = value;
        }

        public void SetValue(bool value)
        {
            if (_value == value)
                return;
            _value = value;
            _onChanged.Publish(_value);
        }

        public ObservableSubscription Subscribe(Action<bool> callback)
            => _onChanged.Subscribe(callback);
    }

    public sealed class AudioMixerGroupBoolProperty : IAudioMixerGroupBoolProperty
    {
        private readonly int _index;
        private readonly AudioMixerGroupPropertyStore _groupPropertyStore;

        public AudioMixerGroupBoolProperty(int index, AudioMixerGroupPropertyStore groupPropertyStore)
        {
            _index = index;
            _groupPropertyStore = groupPropertyStore;
        }

        public bool LocalValue
            => _groupPropertyStore.GetLocalValue(_index);

        public bool TotalValue
            => _groupPropertyStore.GetTotalValue(_index);

        public void SetValue(bool value)
            => _groupPropertyStore.SetValue(_index, value);

        public ObservableSubscription Subscribe(Action<bool> callback)
            => _groupPropertyStore.Subscribe(_index, callback);
    }
}
