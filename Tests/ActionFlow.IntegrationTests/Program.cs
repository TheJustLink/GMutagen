namespace ActionFlow.IntegrationTests;

public class TestFlow
{
    public static async Task Main()
    {
        var func = SayAmoga;
        var flow = func.AsAsyncFlow();

        var emailHandler = new Action<MyEvent>(e => Console.WriteLine($"Обрабатываем email: {e.Payload}"));
        var smsHandler = new Action<MyEvent>(e => Console.WriteLine($"Обрабатываем SMS: {e.Payload}"));

        var newFlow1 = flow
            .When(e => e.Type == "email")
            .Add(emailHandler.AsAsyncFlow());

        var newFlow2 = flow
            .When(e => e.Type == "sms")
            .Add(smsHandler.AsAsyncFlow());
        
        var newFlow3 = flow
                .When(e => e.Type == "log")
                .Add(new Func<MyEvent, Task>(async e =>
                {
                    await Task.Delay(100);
                    Console.WriteLine($"Логируем событие: {e.Payload}");
                }).AsAsyncFlow())
            ;

        await newFlow1.Invoke(new MyEvent { Type = "email", Payload = "user@example.com" });
        await newFlow1.Invoke(new MyEvent { Type = "sms", Payload = "123456789" });
        await newFlow1.Invoke(new MyEvent { Type = "log", Payload = "User logged in" });
        await newFlow1.Invoke(new MyEvent { Type = "unknown", Payload = "???" });
        
        await newFlow2.Invoke(new MyEvent { Type = "email", Payload = "user@example.com" });
        await newFlow2.Invoke(new MyEvent { Type = "sms", Payload = "123456789" });
        await newFlow2.Invoke(new MyEvent { Type = "log", Payload = "User logged in" });
        await newFlow2.Invoke(new MyEvent { Type = "unknown", Payload = "???" });
        
        await newFlow3.Invoke(new MyEvent { Type = "email", Payload = "user@example.com" });
        await newFlow3.Invoke(new MyEvent { Type = "sms", Payload = "123456789" });
        await newFlow3.Invoke(new MyEvent { Type = "log", Payload = "User logged in" });
        await newFlow3.Invoke(new MyEvent { Type = "unknown", Payload = "???" });
        
        Console.WriteLine();
        
        var dot = FlowGraphExporter.ToDot(flow);
        Console.WriteLine("DOT Graph:\n" + dot);
        
        var dot1 = FlowGraphExporter.ToDot(newFlow1);
        Console.WriteLine("DOT Graph:\n" + dot1);
        
        var dot2 = FlowGraphExporter.ToDot(newFlow2);
        Console.WriteLine("DOT Graph:\n" + dot2);
        
        var dot3 = FlowGraphExporter.ToDot(newFlow3);
        Console.WriteLine("DOT Graph:\n" + dot3);
    }

    private static Task SayAmoga(MyEvent obj)
    {
        Console.WriteLine("Amoga");
        return Task.CompletedTask;
    }
}

public class MyEvent
{
    public string Type { get; set; } = "";
    public string Payload { get; set; } = "";
}