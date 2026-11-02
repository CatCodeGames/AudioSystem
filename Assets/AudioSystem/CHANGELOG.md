# Changelog

## [1.0.0]

First release as a UPM package.

### Added
- `PlayAsync` extension in the separate `CatCode.Audio.UniTask` assembly: returns a task and a handle, supports `CancellationToken`.
- `AudioHandle.Mute` and `TryGetMute`.
- `AudioService.Play(clip, group, options = default)` without a callback.
- Pool prewarm in `Awake`.

### Changed
- `AudioPlayOptions` is a `readonly struct` with a constructor; `default` equals `AudioPlayOptions.Default`.
- `PlayOneshot` renamed to `PlayOneShot`.
- `AudioServiceInfo` contains `ActivePlaybackCount` and `PlaybackRunnerPool`.
- Internal types are no longer public.
- Requires the current `EventPrimitives` (`SlotMap`).

### Removed
- `AudioControl` and the old `AudioService.PlayAsync(clip, group, control, token)`.

### Fixed
- Starting a sound from the completion callback.
- `OutputAudioMixerGroup` change through the handle now reroutes the sound.
- `TryGetOutputAudioMixerGroup` returns the group.
- Unknown `AudioMixerGroup` no longer breaks playback.
- Pooled `AudioSource` state is reset on release.
- `AudioService` stops all sounds on destroy.
