## Концепт

- Простота и удобство использования
- Расширяемость и гибкость (возможность кастомной настройки) / Скопы конфигураций? 🤔
- Абстрагирование от хранилища данных (память / бд / сеть)  - с возможностью кастомной настройки пайплайна, например, сначала данные идут в оперативу, пока мы не комитним их в файл.
- Мутабельность поведения объектов без зависимости от схемы, что убирает миграции и добавляет динамику (как в NoSQL базах). Любой объект может потерять/приобрести/изменить свои поведения. Противоречит принципу ниже.
- Бизнес логика должна быть простой и понятной, без бойлерплейта и деталей реализации. По факту она должна описывать объект и его контракты. Противоречит принципу выше.
- Stateless - все стейты только в хранилищах

- Мутабельность (динамика, отсутствие схем)
- VS
- Статичность (статичные контракты, схемы)
- FIGHT!

Где находиться точка модификации для новых фичей/изменений?
Бизнес бойс, да.

```cs
// Game domain
var game = new Object(scope)
	.Set<IGame, SnakeGame>();

// Scene domain
var scene = sceneBuilder.Create(scope, config);
// or
var scene = sceneBuilder.Create(scope)
	.Set<IScene, DefaultScene>();
	.Set<ITick, DefaultTick>((options) => options.TickRate = 20)
	.Build();

var objectBuilder = scene.GetObjectBuilder(defaultScope?, defaultConfig?);

// Object domain
var snake = objectBuilder.Create(scope, config);
// or
var snake = objectBuilder.Create(scope)
	.Set<I>
```