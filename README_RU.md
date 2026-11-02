[RU README](https://github.com/CatCodeGames/AudioSystem/blob/main/README_RU.md)  
[EN README](https://github.com/CatCodeGames/AudioSystem/blob/main/README.md)

# Audio System

Аудиосистема для Unity для воспроизведения 2D-звуков.

- **Пауза и Mute для групп микшера.** В Unity у `AudioMixerGroup` есть только параметры. Здесь группу можно поставить на паузу или заглушить одной строкой, и это действует на всё её поддерево: пауза группы `SFX` ставит на паузу и `SFX/UI`, и `SFX/Steps`.
- **Безопасный `AudioHandle`.** Хэндл — структура без аллокаций. После завершения звука он становится невалидным, и любые вызовы через него безопасны: остановить или изменить чужой звук через старый хэндл невозможно.
- **Результат приходит гарантированно.** Колбэк или таск передаётся при запуске, поэтому завершение не теряется, даже если звук закончился сразу. Из колбэка можно сразу запускать следующий звук.
- **Без `MonoBehaviour` и `Update` на каждый звук.** Звуки отслеживаются прямо в `PlayerLoop`, а раннеры и `AudioSource` переиспользуются из пула.
- **`async`/`await` без мусора.** `PlayAsync` возвращает и таск, и хэндл, поддерживает отмену токеном и после прогрева не аллоцирует.

## Установка

Пакет ставится через Package Manager: **Add package from git URL**. Unity не подтягивает git-зависимости сама, поэтому сначала устанавливаются зависимости, затем сам пакет:

```
https://github.com/CatCodeGames/UniLoop.git?path=Assets/UniLoop
https://github.com/CatCodeGames/EventPrimitives.git?path=Assets/EventPrimitives
https://github.com/CatCodeGames/AudioSystem.git?path=Assets/AudioSystem
```

[UniTask](https://github.com/Cysharp/UniTask) необязателен. Если он установлен, становится доступен `PlayAsync`.

Пример со сценой лежит в репозитории в `Assets/Example` и в пакет не входит.

## Настройка

Добавьте `AudioService` на сцену и укажите в нём все `AudioMixer`, группы которых будут использоваться.

## Воспроизведение

```csharp
// Запуск без колбэка
audioService.Play(clip, group);

// Запуск с параметрами и колбэком завершения
var options = new AudioPlayOptions(volume: 0.8f, loop: true);
AudioHandle handle = audioService.Play(clip, group, options, result => Debug.Log(result));

// Управление через хэндл
handle.Volume = 0.5f;
handle.Pitch = 1.2f;
handle.Loop = false;
handle.Mute = true;
handle.OutputAudioMixerGroup = otherGroup;

handle.Pause();
handle.Resume();
handle.Stop();          // колбэк получит PlaybackResult.Interrupted

// Чтение параметров
if (handle.TryGetVolume(out var volume))
    Debug.Log(volume);

// После завершения хэндл становится невалидным, обращения через него безопасны
bool isActive = handle.IsValid;

// Колбэк без замыкания: состояние передаётся отдельно
audioService.Play(clip, group, options, this, static (self, result) => self.OnFinished(result));

// One-shot
audioService.PlayOneShot(clip, group, volume: 0.7f);
```

Из колбэка завершения можно сразу запускать следующий звук.

## Пауза и Mute групп

`AudioService` строит зеркало каждого `AudioMixer` с той же иерархией групп. У каждой группы есть два состояния, `Pause` и `Mute`:
- `LocalValue` — значение, заданное для самой группы;
- `TotalValue` — итоговое значение с учётом всех родителей.

Звуки следят за итоговым значением своей группы. Поэтому пауза меню, тишина в катсцене или отключение музыки делаются одним вызовом, без обхода всех звуков вручную.

```csharp
AudioMixerGroupMirror mirror = audioService.GetAudioMixerGroupMirror(group);

// Ставит на паузу все звуки группы и её дочерних групп, включая one-shot
mirror.Pause.SetValue(true);
mirror.Mute.SetValue(true);

bool local = mirror.Pause.LocalValue;  // значение самой группы
bool total = mirror.Pause.TotalValue;  // с учётом родительских групп

// Подписка на итоговое значение, например для иконки в UI
var subscription = mirror.Mute.Subscribe(muted => Debug.Log(muted));
subscription.Unsubscribe();
```

Громкость и другие параметры группы по-прежнему задаются через сам `AudioMixer`, например `audioMixer.SetFloat("SfxVolume", -10f)`.

## Async

```csharp
var task = audioService.PlayAsync(clip, group, options, out var handle, cancellationToken);

handle.Volume = 0.5f;   // звуком можно управлять, пока он играет

PlaybackResult result = await task;   // Completed или Interrupted
```

Отмена токена останавливает звук, а таск завершается с `Interrupted` без исключения. Один токен можно передать в несколько вызовов и остановить все звуки сразу.

## Как устроено

- Каждый звук воспроизводит `AudioPlaybackRunner` из пула со своим `AudioSource`. Завершение отслеживается через `UniLoop` прямо в `PlayerLoop`.
- `AudioHandle` хранит идентификатор слота с поколением, поэтому после завершения звука старый хэндл не может управлять новым звуком в том же слоте.
- `AudioService` строит зеркало иерархии `AudioMixer` в плоском массиве. Каждый звук подписан на итоговые `Pause` и `Mute` своей группы.
