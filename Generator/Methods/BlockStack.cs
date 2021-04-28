namespace Generator.Methods
{
    internal class BlockStack
    {
        private Block? _root;
        private Block? _latest;

        public void Nest(Block block)
        {
            if (_latest is null)
            {
                _root = block;
                _latest = block;
            }
            else
            {
                _latest.Inner = block;
                _latest = block;
            }
        }

        public void AddWhitespace()
            => Nest(new Block());

        public string Build()
            => _root.Build();
    }
}
