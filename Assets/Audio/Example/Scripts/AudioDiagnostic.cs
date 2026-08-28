using System.Text;
using TMPro;
using UnityEngine;

namespace CatCode.Audio
{
    public sealed class AudioDiagnostic : MonoBehaviour
    {
        private StringBuilder _sb = new();
        [SerializeField] private AudioService _service;
        [SerializeField] private TextMeshProUGUI _text;

        private void Update()
        {
            var info = _service.GetInfo();
            _sb.Clear();
            _sb.AppendLine($"Handle: {info.HandleAudioPlayerInfo.ActiveSoundCount} active");
            _sb.AppendLine($"Async: {info.AsyncAudioPlayerInfo.ActiveSoundCount} active");
            _sb.AppendLine($"AudioSource pool: {info.AudioSourcePoolInfo.CountActive}/{info.AudioSourcePoolInfo.CountAll} active");
            _sb.AppendLine($"Runner pool: {info.PlaybackRunnerPoolInfo.CountActive}/{info.PlaybackRunnerPoolInfo.CountAll} active");
            _text.text = _sb.ToString();
        }
    }
}