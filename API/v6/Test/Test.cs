using System.Numerics;

using GMutagen.v6.Id;
using GMutagen.v6.IO;
using GMutagen.v6.IO.Repositories;
using GMutagen.v6.Values;
using Microsoft.Extensions.DependencyInjection;

namespace GMutagen.v6.Test;

class Test
{
    public static void Main()
    {
        Game(new GuidGenerator());
        Game(new IncrementalGenerator<int>());
    }

    private static void Game<TId>(IGenerator<TId> idGenerator)
    {
        var positionGenerator = GetDefaultPositionGenerator(idGenerator);


        var serviceCollection = new ServiceCollection()
            .AddSingleton(idGenerator)
            .AddScoped<IGenerator<IPosition, ITypeRead<IGenerator<object>>>, DefaultPositionGenerator>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        IGenerator<TId>? idGen1 = serviceProvider.GetService<IGenerator<TId>>();
        IGenerator<TId> idGen2 = serviceProvider.GetRequiredService<IGenerator<TId>>();

        using (var scope = serviceProvider.CreateScope())
        {
            // Unique position generator instance for this scope
            var scopedPositionGenerator = scope.ServiceProvider
                .GetRequiredService<IGenerator<IPosition, ITypeRead<IGenerator<object>>>>();
        }


        var playerTemplate = (dynamic)new object();
        ; // new Container.ObjectTemplate();
        playerTemplate.Add<IPosition>();

        var player = playerTemplate.Create();
        //player.Set<IPosition>(positionGenerator.Generate());
    }

    public static IGenerator<IPosition> GetDefaultPositionGenerator<TId>(IGenerator<TId> idGenerator)
    {
        IReadWrite<TId, Vector2> positions = new MemoryRepository<TId, Vector2>();
        IGenerator<IValue<Vector2>> positionValueGenerator =
            new ExternalValueGenerator<TId, Vector2>(idGenerator, positions);
        IGenerator<IValue<Vector2>> lazyPositionValueGenerator =
            new GeneratorDecorator<IValue<Vector2>>(positionValueGenerator, new LazyValueGenerator<Vector2>());

        ITypeReadWrite<IGenerator<object>>
            valueGenerators = new TypeRepository<IGenerator<object>>(); // fix it pls later pls pls pls
        valueGenerators.Write(lazyPositionValueGenerator);

        ITypeRead<IGenerator<object>> universalGenerator = valueGenerators;

        var positionGenerator = CreateContractGenerator(universalGenerator, new DefaultPositionGenerator());
        return positionGenerator;
    }

    public static IGenerator<TContract> CreateContractGenerator<TContract>(
        ITypeRead<IGenerator<object>> universalGenerator,
        IGenerator<TContract, ITypeRead<IGenerator<object>>> contractGenerator)
    {
        // return new GeneratorCache<TContract>(universalGenerator, contractGenerator);
        return null;
    }
}