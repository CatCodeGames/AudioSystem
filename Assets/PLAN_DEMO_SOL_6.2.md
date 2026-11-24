# AudioSystem · WebGL-демо

Посетитель открывает страницу и за 30 секунд видит, как `Master` управляет двумя ветками микшера, а `AudioHandle` и `PlayAsync` показывают состояние и результат отдельного звука. Вся проверка проходит на одной странице.

## Вид

```text
CatCode AudioSystem                                       [ Reset ]
Play both tracks, then pause Master to pause the whole mix.

Mixer / DemoAudioMixer               Music / Handle
Master   [x] Pause  [ ] Mute          [ Play ] [ Pause ] [ Stop ]
├─ Music     Paused by Master         Volume / Loop / Mute
└─ Effects   Paused by Master
                                     Effects / Async
                                     [ Play ] [ Pause ] [ Stop ]
                                     Volume / Loop / Mute
                                     OneShot  [ Play ]
```

Сцена использует uGUI в стиле `Example` с разрешением 1280 × 720. Две компактные колонки, тёмные панели, Cascadia Mono и кнопки в скобках. Все подписи в сцене на английском.

## Поведение

- **Первый клик.** У каждой дорожки своя кнопка Play. В Effects — отдельный ритмический клип на 5,6 секунды, чтобы успеть проверить паузу.
- **Группы.** Дерево `DemoAudioMixer` показывает `Master → Music, Effects`. Галочки меняют `LocalValue`, строка состояния показывает `TotalValue` и причину. У каждой группы своя громкость.
- **Один звук.** У каждого плеера есть Loop, Volume, Mute и локальная пауза. Конец эффекта показывает `Completed`, Stop через отмену токена — `Interrupted`. Отдельный блок OneShot отправляет клик в Effects. Reset останавливает звуки и восстанавливает настройки.
- **Настройка на странице.** Под плеерами указаны маршруты и поясняется разница между Pause и Mute.

## Этапы

1. **Сцена.** `Assets/Demo/AudioSystemDemo.unity` собирает префабы микшера, двух плееров и OneShot. Внутри них переиспользуются префабы группы, управления, кнопки, галочки и слайдера.
2. **Ответственность.** `AudioDemo` связывает блоки и запускает Reset. `MixerPanel` владеет деревом панелей, каждая `MixerGroupPanel` работает со своей группой. `MusicPlayerPanel` показывает завершение через callback, `EffectsPlayerPanel` ожидает результат через `PlayAsync`. Оба используют `PlayerControls` для управления через хэндл. `OneShotPanel` отвечает за отдельный клик.
3. **WebGL.** Установить Web Build Support и выбрать `Assets/Demo/AudioSystemDemo.unity` в списке сцен Web-профиля. Первую сборку для GitHub Pages сделать с `Compression Format = Disabled`, проверить через HTTP локально, затем опубликовать с относительными путями к файлам билда. [Аудио в Unity Web](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/develop/audio) · [Развёртывание Unity Web](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/webgl/building-distribution/deploying)

## Приёмка

- Чистая вкладка молчит до клика. Музыка и эффект слышны отдельно и одновременно.
- Пауза `Master` приостанавливает оба звука на текущей позиции, а возобновление продолжает их. Mute `Master` глушит обе ветки, при этом эффект доходит до `Completed`.
- Локальный Pause или Mute ветки переживает включение и выключение соответствующего переключателя `Master`. Вторая ветка сохраняет собственное состояние.
- После остановки музыки её хэндл недействителен. Отмена эффекта даёт `Interrupted`, новый запуск работает сразу.
- Дерево, управление и объяснение помещаются в опубликованный WebGL-плеер 1280 × 720. Проверить в Chrome и Firefox.
