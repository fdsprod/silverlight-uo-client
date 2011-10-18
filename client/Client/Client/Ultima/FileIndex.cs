using System.IO;
using Client.Configuration;

namespace Client.Ultima
{
    public class FileIndex
    {
        private readonly Entry3D[] _index;
        private readonly string _indexPath;
        private readonly string _mulPath;

        private Stream _stream;

        public Entry3D[] Index
        {
            get { return _index; }
        }

        public Stream Stream
        {
            get { return _stream; }
        }

        public string IndexPath
        {
            get { return _indexPath; }
        }

        public string MulPath
        {
            get { return _mulPath; }
        }

        public Stream Seek(int index, out int length, out int extra, out bool patched)
        {
            if (index < 0 || index >= _index.Length)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            Entry3D e = _index[index];

            if (e.lookup < 0)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            length = e.length & 0x7FFFFFFF;
            extra = e.extra;

            //if ((e.length & (1 << 31)) != 0)
            //{
            //    patched = true;

            //    Verdata.Stream.Seek(e.lookup, SeekOrigin.Begin);
            //    return Verdata.Stream;
            //}

            if (_stream == null)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            patched = false;

            InvalidateFileStream();

            _stream.Seek(e.lookup, SeekOrigin.Begin);
            return _stream;
        }

        private void InvalidateFileStream()
        {
            if (_stream == null || !_stream.CanRead || !_stream.CanSeek)
                _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public FileIndex(ClientEngine engine, string idxFile, string mulFile, int length, int file)
        {
            IConfigurationService configurationService = engine.Services.GetService<IConfigurationService>();

            string ultimaOnlineDirectory = configurationService.GetValue<string>(ConfigSections.UltimaOnline, ConfigKeys.UltimaOnlineDirectory);

            Asserter.AssertIsNotNullOrEmpty(ultimaOnlineDirectory, "ultimaOnlineDirectory");
            Asserter.AssertDirectoryExists(ultimaOnlineDirectory);

            _index = new Entry3D[length];

            _indexPath = Path.Combine(ultimaOnlineDirectory, idxFile);
            _mulPath = Path.Combine(ultimaOnlineDirectory, mulFile);

            Asserter.AssertFileExists(_indexPath);
            Asserter.AssertFileExists(_mulPath);

            if (_indexPath != null && _mulPath != null)
            {
                using (FileStream index = new FileStream(_indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryReader bin = new BinaryReader(index);
                    _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    int count = (int)(index.Length / 12);

                    for (int i = 0; i < count && i < length; ++i)
                    {
                        _index[i].lookup = bin.ReadInt32();
                        _index[i].length = bin.ReadInt32();
                        _index[i].extra = bin.ReadInt32();
                    }

                    for (int i = count; i < length; ++i)
                    {
                        _index[i].lookup = -1;
                        _index[i].length = -1;
                        _index[i].extra = -1;
                    }
                }
            }

            //Entry5D[] patches = Verdata.Patches;

            //for (int i = 0; i < patches.Length; ++i)
            //{
            //    Entry5D patch = patches[i];

            //    if (patch.file == file && patch.index >= 0 && patch.index < length)
            //    {
            //        m_Index[patch.index].lookup = patch.lookup;
            //        m_Index[patch.index].length = patch.length | (1 << 31);
            //        m_Index[patch.index].extra = patch.extra;
            //    }
            //}
        }
    }

    public struct Entry3D
    {
        public int lookup;
        public int length;
        public int extra;
    }
}