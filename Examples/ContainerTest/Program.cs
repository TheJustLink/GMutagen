using System.Numerics;
using GMutagen.v5;
using GMutagen.v5.Container;


var value2Generator = DefaultGenerators.GetExternalValueGenerator(typeof(Vector2));
var defaultPositionGenerator = new DefaultPositionGenerator();
var objectTemplateContainer = new ObjectTemplateContainer();
objectTemplateContainer.Add<IPosition>().As<DefaultPosition>().FromGenerator(defaultPositionGenerator);
var position = objectTemplateContainer.Resolve<IPosition>();


Console.WriteLine("Hello, World!");

class DefaultPositionGenerator : IGenerator<DefaultPosition>
{
    public DefaultPosition Generate()
    {
        var value2Generator = DefaultGenerators.GetExternalValueGenerator(typeof(Vector2));
        var prevPosition = (IValue<Vector2>)value2Generator.Generate();
        var curPosition = (IValue<Vector2>)value2Generator.Generate();

        prevPosition.Value = new Vector2(2, 2);
        curPosition.Value = new Vector2(1, 1);
        
        return new DefaultPosition(curPosition, prevPosition);
    }
}