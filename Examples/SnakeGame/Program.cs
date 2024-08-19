using GMutagen.v1;

using SnakeGame;

using Object = GMutagen.v1.Object;


// Templates

var snakeTemplate = new ObjectTemplate()
    .Add<Segments>();

var snakeViewTemplate = new ObjectTemplate()
    .Add<ConsoleViewCompose>();

var snakeSegmentTemplate = new ObjectTemplate()
    .Add<IPosition>(new Position())
    .Add<IColor>(new Color(ConsoleColor.DarkGreen))
    .Add<ISymbol>(new Symbol('o'));

var snakeSegmentViewTemplate = new ObjectTemplate()
    .Add<ConsoleView>();


// Objects

// public class A 
// {
//     private const string Vector2 = "Vector2/";
//
//     public const string Position = Vector2 + "Position";
// }
// interface Snake
// {
//     interface Vector2
//     {
//         interface Position;
//     }
// }

// Snake.Position;
// Snake.TailPosition;

// snake.GetAllId();
// firstId, secondId
// snake.Replace(firstId, secondId)
// dictionary<Id, IValue<string>>
// dictionary<Type, dictionary<Id, IValue<string>>>

// snake.Replace<Snake.Position, Snake.TailPosition>();

var snake = snakeTemplate.Create();

var snakeSegments = snake.Get<Segments>();

var previousHeadPosition = new Position();
var snakeHeadPosition = new PositionRecorder(new Position(), previousHeadPosition);
var snakeHead = snakeSegmentTemplate.Create()
    .Set<IPosition>(snakeHeadPosition)
    .Set<IColor>(new Color(ConsoleColor.Green))
    .Set<ISymbol>(new Symbol('O'));
var snakeTail = snakeSegmentTemplate.Create()
    .Set<IPosition>(previousHeadPosition);

snakeSegments.Value.AddFirst(snakeHead);
snakeSegments.Value.AddLast(snakeTail);

var inputDirection = new ConsoleInputDirection();

//var position1 = snakeHead.Get<IStats<Vector2>>().Get(id);
//var position2 = snakeHead.Get<TailPosition, IValue<Vector2>>();
    //public enum HeadPosition;
    //public enum TailPosition;


// Program

    Console.CursorVisible = false;

    var gameLoop = new Loop();
    var renderLoop = new Loop();

    while (true)
    {

        //gameLoop.Update();
        //renderLoop.Update();

        foreach (var segment in snakeSegments.Value)
        {
            var position = segment.Get<IPosition>();
            var symbol = segment.Get<ISymbol>();
            var color = segment.Get<IColor>();

            ScreenMatrix.Draw(position.Value, symbol.Value, color.Value);
        }

        var direction = inputDirection.Value;

        var headPosition = snakeHead.Get<IPosition>();
        headPosition.Value = new Vector2(headPosition.Value.X + direction.X, headPosition.Value.Y + direction.Y);

        Console.Clear();
    }



    public class Segments
    {
        public readonly LinkedList<Object> Value = new();
    }

    public class Loop : IUpdate
    {
        private readonly List<IUpdate> _updates;

        public Loop()
        {
            _updates = new List<IUpdate>();
        }

        public void Update() 
        {
            foreach(var update in _updates)
                update.Update();
        }

        public Loop Add(IUpdate update) 
        { 
            _updates.Add(update);
            return this;
        }

        public Loop Remove(IUpdate update)
        {
            _updates.Remove(update);
            return this;
        }
    }

    public interface IUpdate
    {
        void Update();
    }