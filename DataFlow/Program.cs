namespace DataFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var updateFlow = AFlow;

            updateFlow.Append(new BaseFlow(), new TestFilter());

            for (int i = 0; i < 1000; i++)
            {
                updateFlow.Schedule();
            }
        }

        public static ActionFlow AFlow = AAction;

        private static Action AAction = () => { };
    }

    public class Health
    {
        private int _amount;

        public BaseFlow TakeDamageFlow = new();
        
        public void TakeDamage(int amount)
        {
            _amount -= amount;
            TakeDamageFlow.Schedule();
        }
    }
}