# Правила кодогенерации

## 1. C#: Именование (Microsoft Conventions)

- Классы, методы, свойства, public поля — **PascalCase**
- Параметры методов, локальные переменные — **camelCase**
- Private поля — **`_camelCase`** (underscore prefix)
- Константы — **PascalCase** (без префикса `_`)
- Методы обязательно начинаются с **глагола**
- Никаких аббревиатур: `verts` → `vertices`, `uvs` → `uvCoordinates`, `tris` → `triangles`, `bladeMesh` → `pointMesh`, `props` → `propertyBlock`
- Допустимые исключения: `uvX` в шейдерах (локальные координаты UV)
- Нельзя использовать `static` для полей — только `const` или `readonly`

## 2. C#: Порядок членов класса

```
const → readonly → [SerializeField] → private → public → events → Awake → OnEnable → Start → Update → OnDestroy → public методы → private методы
```

Пример:
```csharp
private const int Stride = 16;

private readonly int _propertyId = Shader.PropertyToID("_Name");

[SerializeField] private int _count;

private Transform _transform;

public event Action<Butterfly> Died;

private void Awake() { }
public void Initialize() { }
private void Update() { }
```

## 3. C#: Магические числа

- Все литералы (кроме `0`, `1`, `-1`) — в именованные `const`-поля вверху класса
- Параметры `Random.Range()` тоже выносятся в `const`
- Названия описывают **смысл**, а не просто значение

```csharp
// Плохо
if (_elapsed < 1f) { }

// Хорошо
private const float FadeDuration = 1f;
if (_elapsed < FadeDuration) { }
```

## 4. C#: Shader.PropertyToID

- Только `private readonly` (никаких `static readonly`)
- Инициализируется инлайн: `private readonly int _propertyId = Shader.PropertyToID("_Name");`
- Порядок: сразу после `const`, до `[SerializeField]`

## 5. C#: Аллокации

- `WaitForSeconds` кэшировать в поле, создавать 1 раз в `Awake`
- Лямбды, захватывающие переменные (особенно в `actionOnDestroy` пула) — выносить в отдельный метод
- Не создавать новые объекты в `Update`

## 6. C#: Mesh

- Кэш словарей для повторяющихся мешей — `Dictionary<Sprite, Mesh>`
- Имя меша должно быть информативным: `$"MeshType_{identifier}"`
- Для точечных мешей, используемых только как триггер для геометрического шейдера — `CreatePointMesh()`

## 7. C#: Комментарии

- Комментарии в коде **запрещены**. Код должен быть самодокументируемым через имена переменных и методов

## 8. Шейдеры HLSL: Структуры

| Было (Unity default) | Стало |
|---|---|
| `appdata` | `Attributes` |
| `v2f`, `v2g` | `VertexToFragment`, `VertexToGeometry` |
| `g2f` | `GeometryToFragment` |
| `o` (struct instance) | `output` |
| `v` (vertex input) | `input` |
| `i` (fragment input) | `input` |

## 9. Шейдеры HLSL: Локальные переменные

- Никаких однобуквенных имён (кроме счётчиков циклов `i`, `j`)
- Для шейдера цветов (`FlowerGeometry`): `ft` → `flowerType`, `bL/bR/tL/tR` → `bottomLeft/bottomRight/topLeft/topRight`, `cSize/cUp/cView/cRight` → `centerSize/centerUp/centerViewDir/centerRightDir`
- Все части устаревших сокращений (`tR` в `GetRandom`, `tL` в `NdotL`) проверять на побочные эффекты

## 10. Шейдеры HLSL: Константы

- Математические константы — `#define` с точным именем: `#define TWO_PI 6.28318530718`
- Размещать после `#pragma target` в каждом pass

## 11. Работа с файлами

- Не создавать новые файлы без необходимости. Использовать редактирование существующих
- Не создавать документацию (`*.md`) если пользователь явно не попросил
- При переименовании класса — переименовывать и файл

## 12. Процесс работы

1. Прочитать файл перед любыми изменениями
2. Понять контекст: импорты, нейминг соседних файлов, используемые библиотеки
3. Вносить изменения последовательно, проверяя каждое
4. После завершения — проверить итоговый файл на целостность
