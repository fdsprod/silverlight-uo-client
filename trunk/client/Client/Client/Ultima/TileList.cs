
namespace Client.Ultima
{
    public class TileList
    {
        private Tile[] _iles;
        private int _count;

        public TileList()
        {
            _iles = new Tile[8];
            _count = 0;
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public void Add(short id, sbyte z)
        {
            if ((_count + 1) > _iles.Length)
            {
                Tile[] old = _iles;
                _iles = new Tile[old.Length * 2];

                for (int i = 0; i < old.Length; ++i)
                    _iles[i] = old[i];
            }

            _iles[_count++].Set(id, z);
        }

        public Tile[] ToArray()
        {
            Tile[] tiles = new Tile[_count];

            for (int i = 0; i < _count; ++i)
                tiles[i] = _iles[i];

            _count = 0;

            return tiles;
        }
    }
}
