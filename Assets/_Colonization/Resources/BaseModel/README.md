# Base Model Preview Shader

Шейдер полупрозрачной превью-модели для режима строительства (RTS-style ghost preview). Отображает здание с анимированным градиентным переливом между двумя цветами для визуальной обратной связи при размещении.

## Использование в проекте

### 1. Подготовка

1. Создайте или выберите **Material** в папке `Resources/BaseModel/`.
2. В инспекторе материала выберите шейдер `Custom/BaseModelPreview`.
3. Назначьте материал на префаб `BaseModel.prefab` (или аналогичную ghost-модель).

### 2. Настройка префаба

| Компонент | Описание |
|---|---|
| `MeshFilter` | Меш целевого здания (например, `MeshBuildingBarn`) |
| `MeshRenderer` | Материал с шейдером `Custom/BaseModelPreview` |

## Параметры материала (Inspector)

### Texture
`Texture` — **Основная текстура** модели. Накладывается на меш и смешивается с градиентным цветом.

### Color
`Color` — **Первый цвет градиента**. Базовый оттенок превью.

### Secondary Color
`Color` — **Второй цвет градиента**. Перелив анимированно переходит от `Color` к `SecondaryColor` и обратно.

### Alpha
`Range(0, 1)` — **Общая прозрачность**. При 0 модель полностью прозрачна, при 1 — непрозрачна (с учётом `Color.a` текстуры).

### Iridescence Speed
`Range(0, 5)` — **Скорость перелива**. Частота анимации градиента.

### Iridescence Intensity
`Range(0, 1)` — **Интенсивность перелива**. При 0 видна только текстура, умноженная на `Color`. При 1 текстура полностью замещается анимированным градиентом.

## Алгоритм работы

### Vertex Shader (`vert`)
Преобразует вершины меша в мировое и экранное пространство. Передаёт UV, нормаль и направление взгляда во фрагментный шейдер.

### Fragment Shader (`frag`)
1. **Сэмпл текстуры**: чтение `_MainTex` по UV-координатам.
2. **NdotV**: косинус угла между нормалью и направлением на камеру (`saturate(dot(normalWS, viewDirWS))`).
3. **Фаза перелива**: `NdotV + _Time.y * _IridescenceSpeed` — зависит от угла обзора и времени.
4. **Градиент**: `lerp(_Color, _SecondaryColor, gradientFactor)`, где `gradientFactor = sin(phase * 2π) * 0.5 + 0.5`.
5. **Финальный цвет**: `lerp(texture * _Color, gradientColor, _IridescenceIntensity)`.
6. **Альфа**: `texture.a * _Color.a * _Alpha`.

### Shadow Caster Pass
Стандартный проход тени с `ApplyShadowBias` для корректного отбрасывания теней прозрачной превью-моделью.

## Использованные технологии

- **URP Transparent Rendering** — `Queue` = Transparent, `ZWrite Off`, `Blend SrcAlpha OneMinusSrcAlpha`.
- **View-angle-dependent animation** — `NdotV` задаёт фазу градиента, создавая иллюзию объёмного перелива.
- **Two-pass rendering** — Forward + ShadowCaster для корректных теней.
- **Procedural gradient animation** — синусоидальный переход между двумя цветами без использования текстур.
- **Смешивание с текстурой модели** — `_IridescenceIntensity` контролирует соотношение оригинальной текстуры и градиента.
