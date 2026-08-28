[RU README](https://github.com/CatCodeGames/AudioSystem/blob/main/README_RU.md)  
[EN README](https://github.com/CatCodeGames/AudioSystem/blob/main/README.md)

# Audio System

Аудиосистема для Unity, созданная для управления 2D-звуками.

Основные особенности:
- простой API управления воспроизведением через безопасный и лёгкий `AudioHandle`;
- управление процессами без привязки к `MonoBehaviour` и `Update`;
- минимизация аллокаций за счёт пулов и использования структур;
- `Mute` и `Paus`e для `AudioMixerGroup `с учётом иерархии групп.

На данный момент система работает только с 2D-звуками.

Некоторые решения не привязаны непосредственно к аудио и могут применяться в других системах, где требуется запускать процессы и управлять их состоянием и жизненным циклом.

Система использует несколько внешних библиотек. Перед использованием установите необходимые зависимости:
- [UniLoop](https://github.com/CatCodeGames/UniLoop)
- [EventPrimitives](https://github.com/CatCodeGames/EventPrimitives)
- [UniTask](https://github.com/Cysharp/UniTask)

## Использование

Для работы системы необходимо создать и настроить экземпляр AudioService на сцене.
  
Запуск и управление воспроизведением:

```csharp
AudioPlayOptions options = AudioPlayOptions.Default;
AudioHandle handle = audioService.Play(audioClip, audioMixerGroup, options, OnFinished);

// Управление воспроизведением
handle.Volume = 0.5f;
handle.Loop = true;

handle.Pause();
handle.Resume();
handle.Stop();

// Получение текущих значений
if (handle.TryGetVolume(out var volume))
    Debug.Log($"Volume : {volume}");

void OnFinished(PlaybackResult result)  
    => Debug.Log($"Result : {result}");

```

## API

### `AudioPlayOptions`  
Структура с начальными параметрами воспроизведения: `volume`,`pitch` и `loop`.

### `AudioHandle`  
Лёгкая структура для управления уже запущенным воспроизведением.  
`AudioHandle` не владеет процессом воспроизведением звука — он предоставляет к нему доступ. После завершения воспроизведения `AudioHandle` (и все его копии) становится невалидным, а любые обращения через него остаются безопасными.

- `IsValid` - проверяет, связан ли `AudioHandle` с активным воспроизведением. Для default-значения и после завершения воспроизведения возвращает false.
- `Volume`, `Pitch`, `Loop`, `OutputAudioMixerGroup` - параметры, которые можно изменять через `AudioHandle`. Если воспроизведение завершено, то такие обращения безопасны и не вызовот исключения.
- `TryGetVolume`,`TryGetPitch`,`TryGetLoop`,`TryGetOutputAudioMixerGroup` - используются для чтения параметров. Возвращают `false` если процесс воспроизведения звука больше не активен.
- `Pause`, `Resume`, `Stop` - методы для управления воспроизведением звука.
- `PlaybackResult` — результат завершения воспроизведения: Completed или Interrupted.

`AudioHandle` можно изначально создать в невалидном состоянии используя `None`. В отличие от default, такой `AudioHandle` содержит корректное внутреннее состояние и безопасен для дальнейшего использования.

### Передача состояния в колбэк

Помимо самого делегата, в `Play()` можно передать объект состояния. 

Этот объект будет передан в колбэк при завершении воспроизведения. Такой подход позволяет передавать данные в колбэк без замыкания внешних переменных.

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
`AudioMixerGroup` позволяет управлять параметрами всех звуков, которые принадлежат ей. Например, можно отдельно регулировать громкость всех эффектов, диалогов или музыки.

```csharp
audioMixerGroup.audioMixer.SetFloat("EffectsVolume", 0.5f);
```

Помимо стандартного управления параметрами, система добавляет возможность ставить группу на паузу и отключать её звук.
Для этого система создаёт зеркальную структуру для `AudioMixer`, полностью повторяющую исходную иерархию групп. Изменение состояния родительской группы автоматически учитывается и для всех её дочерних групп.

```csharp
AudioMixerGroupMirror mirror = audioService.GetAudioMixerGroupMirror(audioMixerGroup);

// Поставить на паузу все AudioSource запущенные через AudioService
// и принадлежащие группе,
mirror.Pause.SetValue(true);

// Состояние самой группы
var localValue = mirror.Pause.LocalValue;

// Итоговое состояние с учётом родительских групп
var totalValue = mirror.Pause.TotalValue;

// Аналогично для Mute
mirror.Mute.SetValue(false);

```

- `LocalValue` - состояние, заданное непосредственно для этой группы через `SetValue()`  
- `TotalValue` - итоговое состояние с учётом её положения в иерархии.

Для отслеживания изменения состояния группы — `Mute` или `Pause` — можно использовать `Subscribe()`:

```csharp
// Подписаться на изменение результирующего значения
var subscription = mirror.Mute.Subscribe(value =>
{
});

subscription.Unsubscribe();
```

## Асинхронность
Для асинхронного ожидания завершения воспроизведения предусмотрено два варианта.

### Через внешний источник завершения
Можно самостоятельно создать `UniTaskCompletionSource`, `TaskCompletionSource` или другой источник завершения и передать его в `Play()` вместе с колбэком.
Переданный объект состояния будет доступен в колбэке:

``` csharp
var tcs = AutoResetUniTaskCompletionSource<PlaybackResult>.Create();
var handle = audioService.Play(audioClip, audioMixerGroup, options, tcs, OnFinished);

var result = await tcs.Task;

void OnFinished(AutoResetUniTaskCompletionSource<PlaybackResult> tcs, PlaybackResult result)
{
    tcs.TrySetResult(result);
    // Или считать Interrupted отменой:
    // if (result != PlaybackResult.Completed)
    //     tcs.TrySetCanceled();
}
```

Такой вариант удобен, т.к. источник завершения может быть любым и находится полностью под контролем вызывающего кода.

### Через PlayAsync

`AudioService` также предоставляет готовый асинхронный метод `PlayAsync()`, который возвращает `UniTask` и не требует самостоятельно создавать источник завершения.

```csharp
var control = new AudioControl();           
var cts = new CancellationTokenSource();

await audioService.PlayAsync(audioClip, audioMixerGroup, control, cts.Token);

control.Volume = 1f;
control.Pause();

```

`AudioControl` позволяет управлять одним или несколькими запущенными воспроизведениями. Один объект можно передать нескольким процессам и изменять их параметры одновременно.


## Устройство

### `AudioPlaybackRunner`

При запуске воспроизведения `AudioService` берёт свободный `AudioSource` и передаёт его в `AudioPlaybackRunner`. `AudioPlaybackRunner` управляет этим воспроизведением, отслеживает его завершение и сообщает о результате.
Для отслеживания воспроизведения `AudioPlaybackRunner` использует библиотеку `UniLoop`. Процесс выполняется непосредственно в `PlayerLoop`, без привязки к `MonoBehaviour` и с минимальными накладными расходами.  
Все активные воспроизведения регистрируются в AudioService, что позволяет обращаться к ним через AudioHandle.

### `AudioHandle`
`AudioHandle` — лёгкая структура для управления запущенным воспроизведением.
Все его копии содержат один и тот же идентификатор и управляют одной записью в реестре.
После завершения воспроизведения запись удаляется из реестра, а поколение её слота изменяется. Поэтому старые идентификаторы не совпадут с идентификатором следующего процесса, занявшего тот же слот.

### `AudioMixerMirror`
`AudioMixerMirror` расширяет управление `AudioMixerGroup`, позволяя работать не только с параметрами, которые предоставляет сам `AudioMixer`. `AudioMixerMirror` создаёт зеркальное представление `AudioMixer`, сохраняя его иерархию `AudioMixerGroup`.  
Внутреннее дерево `AudioMixerGroupMirror` хранится в виде массиве. Для каждой группы отдельно хранится состояние `Mute` и `Pause`, которое учитывает её положение в иерархии. `AudioPlaybackRunner` подписывается на изменения результирующих состояний `Mute` и `Pause` своей `AudioMixerGroup`. Поэтому изменение состояния родительской группы автоматически применяется ко всем активным воспроизведениям в её поддереве.