using System.Numerics;

using GMutagen;

var bulletTemplate = new ObjectTemplate()
    .Add<IPosition>(new PositionInMemory())
    .Add<IThrow>(new DefaultThrow());

var homingBulletTemplate = new ObjectTemplate(bulletTemplate)
    .Add<ITarget>(new Target());


var bullet = bulletTemplate.Create();

var homingBullet = homingBulletTemplate.Create()
    .Set<IThrow>(new ThrowWithTarget());


Console.WriteLine("Hello, World!");


public interface IPosition
{
    Vector2 Value { get; set; }
}

public class PositionInMemory : IPosition
{
    public Vector2 Value { get; set; }
}

public interface ITarget
{
}

public class Target : ITarget
{
}

public interface IThrow
{
}
public class DefaultThrow : IThrow
{

}

public class ThrowWithTarget : IThrow
{
}