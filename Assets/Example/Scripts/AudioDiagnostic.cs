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
            _sb.AppendLine($"Playbacks: {info.ActivePlaybackCount} active");
            _sb.AppendLine($"Runner pool: {info.PlaybackRunnerPool.CountActive}/{info.PlaybackRunnerPool.CountAll} active");
            _text.text = _sb.ToString();
        }
    }
}