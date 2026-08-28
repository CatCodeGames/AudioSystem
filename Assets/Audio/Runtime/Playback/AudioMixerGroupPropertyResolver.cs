using CatCode.EventPrimitives;
using System;

namespace CatCode.Audio
{
    public sealed class AudioMixerGroupPropertyResolver
    {
        private bool _userValue;
        private bool _groupValue;

        private bool _resultValue;

        private readonly Action<bool> _onGroupValueChanged;
        private readonly Action<bool> _onResultValueChanged;

        private IAudioMixerGroupBoolProperty _state;
        private ObservableSubscription _subscription;

        public bool LocalValue
        {
            get => _userValue;
            set
            {
                _userValue = value;
                RecalculateResultValue();
            }
        }

        public bool ResultValue => _resultValue;

        public void Bind(IAudioMixerGroupBoolProperty state)
        {
            _subscription.Dispose();
            _state = state;

            if (_state == null)
                return;

            _subscription = _state.Subscribe(_onGroupValueChanged);
            _groupValue = _state.TotalValue;
            _resultValue = _userValue || _groupValue;
        }

        public void UnBind()
            => Bind(null);


        public AudioMixerGroupPropertyResolver(Action<bool> onValueChanged)
        {
            _onResultValueChanged = onValueChanged;
            _onGroupValueChanged = OnGroupValueChanged;
        }

        private void OnGroupValueChanged(bool value)
        {
            _groupValue = value;
            RecalculateResultValue();
        }

        private void RecalculateResultValue()
        {
            var newResultValue = _userValue || _groupValue;

            if (_resultValue == newResultValue)
                return;

            _resultValue = newResultValue;
            _onResultValueChanged(_resultValue);
        }
    }
}