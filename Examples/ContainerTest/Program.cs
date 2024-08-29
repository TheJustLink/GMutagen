using System.Numerics;
using GMutagen.v5;
using GMutagen.v5.Container;


var value2Generator = DefaultGenerators.GetExternalValueGenerator(typeof(Vector2));
var objectTemplateContainer = new ObjectTemplateContainer();

objectTemplateContainer
    .Add<IValue<Vector2>>().As<ExternalValue<int, Vector2>>().FromGenerator(value2Generator);

var gameObjectTemplate = new ObjectTemplate(objectTemplateContainer)
    .Add<IPosition>();

var obj = gameObjectTemplate.Create();


Console.WriteLine("Bye, World!");

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