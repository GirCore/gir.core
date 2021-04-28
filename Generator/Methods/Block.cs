using System.Text;

namespace Generator.Methods
{
    internal record Block
    {
        public Block? Inner { get; set; }
        public string? Start { get; set; }
        public string? End { get; set; }

        public string Build()
        {
            var b = new StringBuilder();
            var builder = Build(b);
            return builder.ToString();
        }

        private StringBuilder Build(StringBuilder builder)
        {
            builder.AppendLine(Start);
            Inner?.Build(builder);
            builder.AppendLine(End);
            return builder;
        }
    }
}
