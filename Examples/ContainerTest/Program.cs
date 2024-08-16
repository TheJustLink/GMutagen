using System.Numerics;
using GMutagen.v5;

var objectTemplateContainer = new ObjectTemplateContainer();
objectTemplateContainer.Add<IPosition>().As<DefaultPosition>();
var position = objectTemplateContainer.Resolve<IPosition>();

position.GetCurrentPositionWithOffset(new Vector2(0, 0));

Console.WriteLine("Hello, World!");