using GMutagen.v7;

var scene = new Scene();

var obj1 = scene.AddObject();
obj1.Set<ByeContractImplementation>();

var obj2 = scene.AddObject();
obj2.Set<IHelloContract, HelloContractImplementation>();
obj2.Set<IByeContract>(obj1);

var byecontract = obj2.Get<IByeContract>();
byecontract.Bye();

obj2.Hello();
obj2.Execute<IByeContract>();

// objects:2:contracts:IHelloContract Value

public static class ObjectExtensions
{
    public static void Hello(this IObject @object)
    {
        var helloContract = @object.Get<IHelloContract>();
        helloContract.Hello();
    }
}

public interface IPosition
{

}
public interface IHelloContract
{
    void Hello();
}
public interface IByeContract
{
    void Bye();
}
public class ByeContractImplementation : IByeContract
{
    public void Bye() => Console.WriteLine("Bye!");
}
public class HelloContractImplementation : IHelloContract
{
    public void Hello() => Console.WriteLine("Hello, world!");
}