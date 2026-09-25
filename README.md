[RU README](https://github.com/CatCodeGames/AudioSystem/blob/main/README_RU.md)  
[EN README](https://github.com/CatCodeGames/AudioSystem/blob/main/README.md)

## Audio System

An audio system for Unity designed to manage 2D audio playback.

Key features:
- a simple API for controlling playback through a safe and lightweight `AudioHandle`;
- process management without relying on `MonoBehaviour` or `Update`;
- reduced allocations through pooling and the use of structs;
- `Mute` and `Pause` states for `AudioMixerGroup`, with hierarchy-aware state handling.

The system currently supports 2D audio only.

Some of the implementation techniques are not specific to audio and can also be applied to other systems that need to run processes and manage their state and lifetime.

The system uses several external libraries. Install the required dependencies before using it:
- [UniLoop](https://github.com/CatCodeGames/UniLoop)
- [EventPrimitives](https://github.com/CatCodeGames/EventPrimitives)
- [UniTask](https://github.com/Cysharp/UniTask)

## Usage

To use the system, create and configure an AudioService instance in the scene.

Starting and controlling playback:

```csharp
AudioPlayOptions options = AudioPlayOptions.Default;
AudioHandle handle = audioService.Play(audioClip, audioMixerGroup, options, OnFinished);

// Control playback
handle.Volume = 0.5f;
handle.Loop = true;

handle.Pause();
handle.Resume();
handle.Stop();

// Read current values
if (handle.TryGetVolume(out var volume))
    Debug.Log($"Volume : {volume}");

void OnFinished(PlaybackResult result)
    => Debug.Log($"Result : {result}");
``` 
## API

### `AudioPlayOptions`

A struct containing the initial playback parameters: `volume`, `pitch`, and `loop`.

### `AudioHandle`

A lightweight struct for controlling active playback.

`AudioHandle` does not own the audio playback process; it only provides access to it. Once playback finishes, the `AudioHandle` and all of its copies become invalid, while operations performed through them remain safe.

- `IsValid` — checks whether the `AudioHandle` is associated with an active playback. Returns false for a default value and after playback has finished.
- `Volume`, `Pitch`, `Loop`, `OutputAudioMixerGroup` — playback parameters that can be modified through the `AudioHandle`. Accessing them after playback has finished is safe and does not throw exceptions.
- `TryGetVolume`, `TryGetPitch`, `TryGetLoop`, `TryGetOutputAudioMixerGroup` — used to read playback parameters. Return false when the playback process is no longer active.
- `Pause`, `Resume`, `Stop` — methods for controlling playback.
- `PlaybackResult` — indicates how playback finished: Completed or Interrupted.

An `AudioHandle` can also be created in an invalid state using `None. Unlike default, this `AudioHandle` has a valid internal state and is safe to use afterwards.

### Passing State to a Callback

In addition to the callback delegate, `Play()` can accept a state object.

The state object is passed to the callback when playback finishes. This allows data to be passed to the callback without capturing external variables in a closure.

```csharp
MyClass obj = new ();
AudioHandle handle = audioService.Play(
    audioClip,
    audioMixerGroup,
    options,
    obj, OnFinished);

void OnFinished(MyClass obj, PlaybackResult result)
{
    ...
}
```

### `AudioMixerGroupMirror`

`AudioMixerGroup` allows you to control the parameters of all sounds routed through it. For example, you can adjust the volume of effects, dialogue, or music separately.

```csharp
audioMixerGroup.audioMixer.SetFloat("EffectsVolume", 0.5f);
``` 

In addition to the standard parameter controls, the system adds support for pausing a group and muting its audio.

To achieve this, the system creates a mirror structure for the `AudioMixer` that preserves the original group hierarchy. Changes to a parent group's state are automatically taken into account by all of its child groups.

```csharp
AudioMixerGroupMirror mirror = audioService.GetAudioMixerGroupMirror(audioMixerGroup);

// Pause all AudioSources started through AudioService
// and routed through this group
mirror.Pause.SetValue(true);

// Local state of the group
var localValue = mirror.Pause.LocalValue;

// Resulting state, including parent groups
var totalValue = mirror.Pause.TotalValue;

// The same applies to Mute
mirror.Mute.SetValue(false);
```

- `LocalValue` — the state set directly for this group through `SetValue()`.

- `TotalValue` — the resulting state after taking the group's position in the hierarchy into account.

Group state (`Mute` and `Pause`) changes can be observed through `Subscribe()`:

```csharp
// Subscribe to changes of the resulting value
var subscription = mirror.Mute.Subscribe(value =>
{
});

subscription.Unsubscribe();
```

## Async

There are two ways to asynchronously wait for playback to finish.

### Using an External Completion Source

You can create a `UniTaskCompletionSource`, `TaskCompletionSource`, or another completion source yourself and pass it to `Play()` together with the callback.

The passed state object is then available in the callback:

``` csharp
var tcs = AutoResetUniTaskCompletionSource<PlaybackResult>.Create();
var handle = audioService.Play(audioClip, audioMixerGroup, options, tcs, OnFinished);

var result = await tcs.Task;

void OnFinished(
    AutoResetUniTaskCompletionSource<PlaybackResult> tcs,
    PlaybackResult result)
{
    tcs.TrySetResult(result);

    // Or treat Interrupted as cancellation:
    // if (result != PlaybackResult.Completed)
    //     tcs.TrySetCanceled();
}
```

This approach is useful when the completion source needs to be fully controlled by the calling code.

### Using PlayAsync

`AudioService` also provides a ready-to-use asynchronous `PlayAsync()` method that returns a `UniTask`. No completion source needs to be created manually.

```csharp
var control = new AudioControl();
var cts = new CancellationTokenSource();

await audioService.PlayAsync(
    audioClip,
    audioMixerGroup,
    control,
    cts.Token);

control.Volume = 1f;
control.Pause();
```

`AudioControl` can be used to control one or multiple active playbacks. The same object can be passed to several processes, allowing their parameters to be changed simultaneously.

## Architecture

### `AudioPlaybackRunner`

When playback starts, `AudioService` passes the `AudioSource` to `AudioPlaybackRunner`. It manages the playback, tracks its completion, and reports the result.

`AudioPlaybackRunner` uses the `UniLoop` library to track playback. The process runs directly in the `PlayerLoop`, without relying on `MonoBehaviour `and with minimal overhead.

All active playbacks are registered in AudioService, which allows them to be accessed through AudioHandle.


### `AudioHandle`

`AudioHandle` is a lightweight struct for controlling active playback.

All copies of an `AudioHandle` contain the same identifier and control the same registry entry.

When playback finishes, the entry is removed from the registry and its slot generation is changed. This prevents an old identifier from matching the identifier of a later process that reuses the same slot.


### `AudioMixerMirror`

`AudioMixerMirror` extends the functionality available for `AudioMixerGroup`, allowing the system to work with more than just the parameters provided by `AudioMixer` itself.

AudioMixerMirror creates a mirror representation of the `AudioMixer` while preserving its `AudioMixerGroup` hierarchy. The internal `AudioMixerGroupMirror` tree is stored as a flat array. Each group has its own `Mute` and `Pause` state, which takes its position in the hierarchy into account.

`AudioPlaybackRunner` subscribes to changes in the resulting `Mute` and `Pause` states of its `AudioMixerGroup`. As a result, changing the state of a parent group is automatically applied to all active playbacks in its subtree.
