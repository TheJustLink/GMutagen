using System.Reflection;
using System.Text;

namespace ActionFlow
{
    public static class FlowGraphExporter
    {
        private static int _nodeCounter = 0;

        public static string ToDot<T>(IFlow<T> flow)
        {
            var dotBuilder = new StringBuilder();
            dotBuilder.AppendLine("digraph AsyncFlowGraph {");
            dotBuilder.AppendLine("  rankdir=TB;");

            var nodes = new HashSet<string>();
            ExportFlow(flow, nodes, dotBuilder);

            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }

        public static string ToDot<T>(IAsyncFlow<T> flow)
        {
            var dotBuilder = new StringBuilder();
            dotBuilder.AppendLine("digraph AsyncFlowGraph {");
            dotBuilder.AppendLine("  rankdir=TB;");

            var nodes = new HashSet<string>();
            ExportFlow(flow, nodes, dotBuilder);

            dotBuilder.AppendLine("}");
            return dotBuilder.ToString();
        }

        private static void ExportFlow<T>(IFlow<T> flow, HashSet<string> nodes, StringBuilder dotBuilder)
        {
            var flowNodeId = GetUniqueNodeId(flow);
            var flowLabel = flow.GetType().Name;
            dotBuilder.AppendLine($"  \"{flowNodeId}\" [shape=box style=filled fillcolor=\"white\" label=\"{flowLabel}\"]");

            if (flow is ActionFlow<T> actionFlow)
            {
                ExportHandlers(new List<IFlow<T>>(){ actionFlow}, flowNodeId, "before", nodes, dotBuilder);
                return;
            }
            
            // Handle BeforeHandlers
            ExportHandlers(((BasicFlow<IFlow<T>>)flow).BeforeHandlers, flowNodeId, "before", nodes, dotBuilder);

            // Handle Handlers
            ExportHandlers(((BasicFlow<IFlow<T>>)flow).Handlers, flowNodeId, "handler", nodes, dotBuilder);

            foreach (var innerFlow in ((BasicFlow<IFlow<T>>)flow).Handlers.OfType<IFlow<T>>())
            {
                ExportFlow(innerFlow, nodes, dotBuilder);
            }
        }

        private static void ExportFlow<T>(IAsyncFlow<T> flow, HashSet<string> nodes, StringBuilder dotBuilder)
        {
            var flowNodeId = GetUniqueNodeId(flow);
            var flowLabel = flow.GetType().Name;
            dotBuilder.AppendLine($"  \"{flowNodeId}\" [shape=box style=filled fillcolor=\"white\" label=\"{flowLabel}\"]");

            if (flow is AsyncActionFlow<T> actionFlow)
            {
                ExportHandlers(new List<IAsyncFlow<T>>(){ actionFlow}, flowNodeId, "before", nodes, dotBuilder);
                return;
            }

            // Handle BeforeHandlers
            ExportHandlers(((BasicFlow<IAsyncFlow<T>>)flow).BeforeHandlers, flowNodeId, "before", nodes, dotBuilder);

            // Handle Handlers
            ExportHandlers(((BasicFlow<IAsyncFlow<T>>)flow).Handlers, flowNodeId, "handler", nodes, dotBuilder);

            foreach (var innerFlow in ((BasicFlow<IAsyncFlow<T>>)flow).Handlers.OfType<IAsyncFlow<T>>())
            {
                ExportFlow(innerFlow, nodes, dotBuilder);
            }
        }

        private static void ExportHandlers<T>(List<IFlow<T>> handlers, string fromNodeId, string label, HashSet<string> nodes, StringBuilder dotBuilder)
        {
            foreach (var handler in handlers)
            {
                var handlerNodeId = GetUniqueNodeId(handler);
                var handlerLabel = handler.GetType().Name;
                var methodName = GetDelegateMethodName(handler);

                // Add method name if available
                if (!string.IsNullOrEmpty(methodName))
                    handlerLabel += $"\nMethod: {methodName}";

                // Only add node if not already added
                if (!nodes.Contains(handlerNodeId))
                {
                    nodes.Add(handlerNodeId);
                    dotBuilder.AppendLine($"  \"{handlerNodeId}\" [shape=ellipse style=filled fillcolor=\"lightgray\" label=\"{handlerLabel}\"]");
                }

                // Create the edge (arrow)
                dotBuilder.AppendLine($"  \"{fromNodeId}\" -> \"{handlerNodeId}\" [style={(label == "before" ? "dashed" : "solid")} label=\"{label}\"];");
            }
        }

        private static void ExportHandlers<T>(List<IAsyncFlow<T>> handlers, string fromNodeId, string label, HashSet<string> nodes, StringBuilder dotBuilder)
        {
            foreach (var handler in handlers)
            {
                var handlerNodeId = GetUniqueNodeId(handler);
                var handlerLabel = handler.GetType().Name;
                var methodName = GetDelegateMethodName(handler);

                // Add method name if available
                if (!string.IsNullOrEmpty(methodName))
                    handlerLabel += $"\nMethod: {methodName}";

                // Only add node if not already added
                if (!nodes.Contains(handlerNodeId))
                {
                    nodes.Add(handlerNodeId);
                    dotBuilder.AppendLine($"  \"{handlerNodeId}\" [shape=ellipse style=filled fillcolor=\"lightgray\" label=\"{handlerLabel}\"]");
                }

                // Create the edge (arrow)
                dotBuilder.AppendLine($"  \"{fromNodeId}\" -> \"{handlerNodeId}\" [style={(label == "before" ? "dashed" : "solid")} label=\"{label}\"];");
            }
        }

        private static string GetUniqueNodeId(object obj)
        {
            return $"{obj.GetType().Name}_{_nodeCounter++}";
        }

        private static string GetDelegateMethodName(object obj)
        {
            if (obj is Delegate d)
                return d.Method?.Name ?? "";

            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(obj);
                if (fieldValue is Delegate del)
                {
                    return del.Method?.Name ?? "";
                }
            }

            return "";
        }
    }
}
