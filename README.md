[RU README](https://github.com/CatCodeGames/AudioSystem/blob/main/README_RU.md)  
[EN README](https://github.com/CatCodeGames/AudioSystem/blob/main/README.md)

# Audio System

An audio system for Unity for 2D audio playback.

- **Pause and Mute for mixer groups.** In Unity an `AudioMixerGroup` only has parameters. Here a group can be paused or muted with one line, and it affects its whole subtree: pausing `SFX` also pauses `SFX/UI` and `SFX/Steps`.
- **Safe `AudioHandle`.** The handle is an allocation-free struct. Once the sound ends, it becomes invalid and every call through it is safe: an old handle can never stop or change another sound.
- **The result is guaranteed to arrive.** The callback or task is passed at start, so completion is never missed, even if the sound ends immediately. A new sound can be started right from the callback.
- **No `MonoBehaviour` or `Update` per sound.** Sounds are tracked directly in the `PlayerLoop`, and runners with their `AudioSource`s are reused from a pool.
- **Garbage-free `async`/`await`.** `PlayAsync` returns both a task and a handle, supports token cancellation, and does not allocate after warm-up.

## Installation

Install through the Package Manager: **Add package from git URL**. Unity does not resolve git dependencies on its own, so install the dependencies first, then the package:

```
https://github.com/CatCodeGames/UniLoop.git?path=Assets/UniLoop
https://github.com/CatCodeGames/EventPrimitives.git?path=Assets/EventPrimitives
https://github.com/CatCodeGames/AudioSystem.git?path=Assets/AudioSystem
```

[UniTask](https://github.com/Cysharp/UniTask) is optional. When it is installed, `PlayAsync` becomes available.

An example scene lives in the repository under `Assets/Example` and is not part of the package.

## Setup

Add an `AudioService` to the scene and list every `AudioMixer` whose groups will be used.

## Playback

```csharp
// Play without a callback
audioService.Play(clip, group);

// Play with options and a completion callback
var options = new AudioPlayOptions(volume: 0.8f, loop: true);
AudioHandle handle = audioService.Play(clip, group, options, result => Debug.Log(result));

// Control through the handle
handle.Volume = 0.5f;
handle.Pitch = 1.2f;
handle.Loop = false;
handle.Mute = true;
handle.OutputAudioMixerGroup = otherGroup;

handle.Pause();
handle.Resume();
handle.Stop();          // the callback receives PlaybackResult.Interrupted

// Read values
if (handle.TryGetVolume(out var volume))
    Debug.Log(volume);

// After playback ends the handle becomes invalid; calls through it are safe
bool isActive = handle.IsValid;

// Callback without a closure: the state is passed separately
audioService.Play(clip, group, options, this, static (self, result) => self.OnFinished(result));

// One-shot
audioService.PlayOneShot(clip, group, volume: 0.7f);
```

A new sound can be started directly from the completion callback.

## Group Pause and Mute

`AudioService` builds a mirror of every `AudioMixer` with the same group hierarchy. Each group has two states, `Pause` and `Mute`:
- `LocalValue` — the value set for the group itself;
- `TotalValue` — the resulting value, including all parents.

Sounds follow the resulting value of their group. So a pause menu, silence during a cutscene, or turning music off is a single call, with no manual loop over sounds.

```csharp
AudioMixerGroupMirror mirror = audioService.GetAudioMixerGroupMirror(group);

// Pauses every sound in the group and its child groups, one-shots included
mirror.Pause.SetValue(true);
mirror.Mute.SetValue(true);

bool local = mirror.Pause.LocalValue;  // value of the group itself
bool total = mirror.Pause.TotalValue;  // including parent groups

// Subscribe to the resulting value, for example for a UI icon
var subscription = mirror.Mute.Subscribe(muted => Debug.Log(muted));
subscription.Unsubscribe();
```

Volume and other group parameters are still set through the `AudioMixer` itself, for example `audioMixer.SetFloat("SfxVolume", -10f)`.

## Async

```csharp
var task = audioService.PlayAsync(clip, group, options, out var handle, cancellationToken);

handle.Volume = 0.5f;   // the sound can be controlled while it plays

PlaybackResult result = await task;   // Completed or Interrupted
```

Cancelling the token stops the sound, and the task completes with `Interrupted` without an exception. One token can be passed to several calls to stop all of them at once.

## How It Works

- Each sound is played by a pooled `AudioPlaybackRunner` with its own `AudioSource`. Completion is tracked through `UniLoop` directly in the `PlayerLoop`.
- `AudioHandle` stores a slot id with a generation, so an old handle cannot control a new sound that reuses the same slot.
- `AudioService` builds a mirror of the `AudioMixer` hierarchy in a flat array. Each sound subscribes to the resulting `Pause` and `Mute` of its group.
