using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Client.Framework.Text;
using System.Diagnostics;

namespace Client.Network
{
	/// <summary>
	/// Provides functionality for writing primitive binary data.
	/// </summary>
	public class PacketWriter
	{
		private static Stack<PacketWriter> _pool = new Stack<PacketWriter>();

		public static PacketWriter CreateInstance()
		{
			return CreateInstance( 32 );
		}

		public static PacketWriter CreateInstance( int capacity )
		{
			PacketWriter pw = null;

			lock ( _pool )
			{
				if ( _pool.Count > 0 )
				{
					pw = _pool.Pop();

					if ( pw != null )
					{
						pw._capacity = capacity;
						pw._stream.SetLength( 0 );
					}
				}
			}

			if ( pw == null )
				pw = new PacketWriter( capacity );

			return pw;
		}

		public static void ReleaseInstance( PacketWriter pw )
		{
			lock ( _pool )
			{
				if ( !_pool.Contains( pw ) )
				{
					_pool.Push( pw );
				}
				else
				{
					try
					{
						using ( StreamWriter op = new StreamWriter( "neterr.log" ) )
						{
							op.WriteLine( "{0}\tInstance pool contains writer", DateTime.Now );
						}
					}
					catch
					{
						Console.WriteLine( "net error" );
					}
				}
			}
		}

		/// <summary>
		/// Internal stream which holds the entire packet.
		/// </summary>
		private MemoryStream _stream;

		private int _capacity;

		/// <summary>
		/// Internal format buffer.
		/// </summary>
		private static byte[] _buffer = new byte[8];

		/// <summary>
		/// Instantiates a new PacketWriter instance with the default capacity of 4 bytes.
		/// </summary>
		public PacketWriter() : this( 32 )
		{
		}

		/// <summary>
		/// Instantiates a new PacketWriter instance with a given capacity.
		/// </summary>
		/// <param name="capacity">Initial capacity for the internal stream.</param>
		public PacketWriter( int capacity )
		{
			_stream = new MemoryStream( capacity );
			_capacity = capacity;
		}

        public PacketWriter(byte[] buffer)
        {
            _stream = new MemoryStream(buffer);
            _capacity = buffer.Length;
        }

		/// <summary>
		/// Writes a 1-byte boolean value to the underlying stream. False is represented by 0, true by 1.
		/// </summary>
		public void Write( bool value )
		{
			_stream.WriteByte( (byte)(value ? 1 : 0) );
		}

		/// <summary>
		/// Writes a 1-byte unsigned integer value to the underlying stream.
		/// </summary>
		public void Write( byte value )
		{
			_stream.WriteByte( value );
		}

		/// <summary>
		/// Writes a 1-byte signed integer value to the underlying stream.
		/// </summary>
		public void Write( sbyte value )
		{
			_stream.WriteByte( (byte) value );
		}

		/// <summary>
		/// Writes a 2-byte signed integer value to the underlying stream.
		/// </summary>
		public void Write( short value )
		{
			_buffer[0] = (byte)(value >> 8);
			_buffer[1] = (byte) value;

			_stream.Write( _buffer, 0, 2 );
		}

		/// <summary>
		/// Writes a 2-byte unsigned integer value to the underlying stream.
		/// </summary>
		public void Write( ushort value )
		{
			_buffer[0] = (byte)(value >> 8);
			_buffer[1] = (byte) value;

			_stream.Write( _buffer, 0, 2 );
		}

		/// <summary>
		/// Writes a 4-byte signed integer value to the underlying stream.
		/// </summary>
		public void Write( int value )
		{
			_buffer[0] = (byte)(value >> 24);
			_buffer[1] = (byte)(value >> 16);
			_buffer[2] = (byte)(value >>  8);
			_buffer[3] = (byte) value;

			_stream.Write( _buffer, 0, 4 );
		}

		/// <summary>
		/// Writes a 4-byte unsigned integer value to the underlying stream.
		/// </summary>
		public void Write( uint value )
		{
			_buffer[0] = (byte)(value >> 24);
			_buffer[1] = (byte)(value >> 16);
			_buffer[2] = (byte)(value >>  8);
			_buffer[3] = (byte) value;

			_stream.Write( _buffer, 0, 4 );
		}

		/// <summary>
		/// Writes a 8-byte signed long value to the underlying stream.
		/// </summary>
		public void Write( long value )
		{
			_buffer[0] = (byte)(value >> 56);
			_buffer[1] = (byte)(value >> 48);
			_buffer[2] = (byte)(value >> 40);
			_buffer[3] = (byte)(value >> 32);
			_buffer[4] = (byte)(value >> 24);
			_buffer[5] = (byte)(value >> 16);
			_buffer[6] = (byte)(value >> 8);
			_buffer[7] = (byte)value;

			_stream.Write( _buffer, 0, 8 );
		}

		/// <summary>
		/// Writes a 8-byte unsigned long value to the underlying stream.
		/// </summary>
		public void Write( ulong value )
		{
			_buffer[0] = (byte)(value >> 56);
			_buffer[1] = (byte)(value >> 48);
			_buffer[2] = (byte)(value >> 40);
			_buffer[3] = (byte)(value >> 32);
			_buffer[4] = (byte)(value >> 24);
			_buffer[5] = (byte)(value >> 16);
			_buffer[6] = (byte)(value >> 8);
			_buffer[7] = (byte)value;

			_stream.Write( _buffer, 0, 8 );
		}

		/// <summary>
		/// Writes a 4-byte float value to the underlying stream.
		/// </summary>
		public void Write( float value )
		{
			uint num = (uint)value;
			_buffer[0] = (byte)(num >> 24);
			_buffer[1] = (byte)(num >> 16);
			_buffer[2] = (byte)(num >> 8);
			_buffer[3] = (byte)num;

			_stream.Write( _buffer, 0, 4 );
		}


		/// <summary>
		/// Writes a 8-byte double value to the underlying stream.
		/// </summary>
		public void Write( double value )
		{
			uint num = (uint)value;
			_buffer[0] = (byte)(num >> 56);
			_buffer[1] = (byte)(num >> 48);
			_buffer[2] = (byte)(num >> 40);
			_buffer[3] = (byte)(num >> 32);
			_buffer[4] = (byte)(num >> 24);
			_buffer[5] = (byte)(num >> 16);
			_buffer[6] = (byte)(num >> 8);
			_buffer[7] = (byte)num;

			_stream.Write( _buffer, 0, 8 );
		}

		/// <summary>
		/// Writes a sequence of bytes to the underlying stream
		/// </summary>
		public void Write( byte[] buffer, int offset, int size )
		{
			_stream.Write( buffer, offset, size );
		}

		/// <summary>
		/// Writes a fixed-length ASCII-encoded string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
		/// </summary>
		public void WriteAsciiFixed( string value, int size )
		{
			if ( value == null )
			{
                Debug.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
				value = String.Empty;
			}

			int length = value.Length;

			_stream.SetLength( _stream.Length + size );

			if ( length >= size )
                _stream.Position += ASCIIEncoding.Instance.GetBytes(value, 0, size, _stream.GetBuffer(), (int)_stream.Position);
			else
			{
                ASCIIEncoding.Instance.GetBytes(value, 0, length, _stream.GetBuffer(), (int)_stream.Position);
				_stream.Position += size;
			}

			/*byte[] buffer = Encoding.ASCII.GetBytes( value );

			if ( buffer.Length >= size )
			{
				m_Stream.Write( buffer, 0, size );
			}
			else
			{
				m_Stream.Write( buffer, 0, buffer.Length );
				Fill( size - buffer.Length );
			}*/
		}

		/// <summary>
		/// Writes a dynamic-length ASCII-encoded string value to the underlying stream, followed by a 1-byte null character.
		/// </summary>
		public void WriteAsciiNull( string value )
		{
			if ( value == null )
			{
				Debug.WriteLine( "Network: Attempted to WriteAsciiNull() with null value" );
				value = String.Empty;
			}

			int length = value.Length;

			_stream.SetLength( _stream.Length + length + 1 );

            ASCIIEncoding.Instance.GetBytes(value, 0, length, _stream.GetBuffer(), (int)_stream.Position);
			_stream.Position += length + 1;

			/*byte[] buffer = Encoding.ASCII.GetBytes( value );

			m_Stream.Write( buffer, 0, buffer.Length );
			m_Stream.WriteByte( 0 );*/
		}

		/// <summary>
		/// Writes a dynamic-length little-endian unicode string value to the underlying stream, followed by a 2-byte null character.
		/// </summary>
		public void WriteLittleUniNull( string value )
		{
			if ( value == null )
			{
                Debug.WriteLine("Network: Attempted to WriteLittleUniNull() with null value");
				value = String.Empty;
			}

			int length = value.Length;

			_stream.SetLength( _stream.Length + ( ( length + 1 ) * 2 ) );

			_stream.Position += Encoding.Unicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
			_stream.Position += 2;

			/*byte[] buffer = Encoding.Unicode.GetBytes( value );

			m_Stream.Write( buffer, 0, buffer.Length );

			m_Buffer[0] = 0;
			m_Buffer[1] = 0;
			m_Stream.Write( m_Buffer, 0, 2 );*/
		}

		/// <summary>
		/// Writes a fixed-length little-endian unicode string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
		/// </summary>
		public void WriteLittleUniFixed( string value, int size )
		{
			if ( value == null )
			{
                Debug.WriteLine("Network: Attempted to WriteLittleUniFixed() with null value");
				value = String.Empty;
			}

			size *= 2;

			int length = value.Length;

			_stream.SetLength( _stream.Length + size );

			if ( ( length * 2 ) >= size )
				_stream.Position += Encoding.Unicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
			else
			{
				Encoding.Unicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
				_stream.Position += size;
			}

			/*size *= 2;

			byte[] buffer = Encoding.Unicode.GetBytes( value );

			if ( buffer.Length >= size )
			{
				m_Stream.Write( buffer, 0, size );
			}
			else
			{
				m_Stream.Write( buffer, 0, buffer.Length );
				Fill( size - buffer.Length );
			}*/
		}

		/// <summary>
		/// Writes a dynamic-length big-endian unicode string value to the underlying stream, followed by a 2-byte null character.
		/// </summary>
		public void WriteBigUniNull( string value )
		{
			if ( value == null )
			{
                Debug.WriteLine("Network: Attempted to WriteBigUniNull() with null value");
				value = String.Empty;
			}

			int length = value.Length;

			_stream.SetLength( _stream.Length + ( ( length + 1 ) * 2 ) );

			_stream.Position += Encoding.BigEndianUnicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
			_stream.Position += 2;

			/*byte[] buffer = Encoding.BigEndianUnicode.GetBytes( value );

			m_Stream.Write( buffer, 0, buffer.Length );

			m_Buffer[0] = 0;
			m_Buffer[1] = 0;
			m_Stream.Write( m_Buffer, 0, 2 );*/
		}

		/// <summary>
		/// Writes a fixed-length big-endian unicode string value to the underlying stream. To fit (size), the string content is either truncated or padded with null characters.
		/// </summary>
		public void WriteBigUniFixed( string value, int size )
		{
			if ( value == null )
			{
                Debug.WriteLine("Network: Attempted to WriteBigUniFixed() with null value");
				value = String.Empty;
			}

			size *= 2;

			int length = value.Length;

			_stream.SetLength( _stream.Length + size );

			if ( ( length * 2 ) >= size )
				_stream.Position += Encoding.BigEndianUnicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
			else
			{
				Encoding.BigEndianUnicode.GetBytes( value, 0, length, _stream.GetBuffer(), (int)_stream.Position );
				_stream.Position += size;
			}

			/*size *= 2;

			byte[] buffer = Encoding.BigEndianUnicode.GetBytes( value );

			if ( buffer.Length >= size )
			{
				m_Stream.Write( buffer, 0, size );
			}
			else
			{
				m_Stream.Write( buffer, 0, buffer.Length );
				Fill( size - buffer.Length );
			}*/
		}

		/// <summary>
		/// Fills the stream from the current position up to (capacity) with 0x00's
		/// </summary>
		public void Fill()
		{
			Fill( (int) (_capacity - _stream.Length) );
		}

		/// <summary>
		/// Writes a number of 0x00 byte values to the underlying stream.
		/// </summary>
		public void Fill( int length )
		{
			if ( _stream.Position == _stream.Length )
			{
				_stream.SetLength( _stream.Length + length );
				_stream.Seek( 0, SeekOrigin.End );
			}
			else
			{
				_stream.Write( new byte[length], 0, length );
			}
		}

		/// <summary>
		/// Gets the total stream length.
		/// </summary>
		public long Length
		{
			get
			{
				return _stream.Length;
			}
		}

		/// <summary>
		/// Gets or sets the current stream position.
		/// </summary>
		public long Position
		{
			get
			{
				return _stream.Position;
			}
			set
			{
				_stream.Position = value;
			}
		}

		/// <summary>
		/// Gets the internal stream used by this PacketWriter instance.
		/// </summary>
		public MemoryStream UnderlyingStream
		{
			get
			{
				return _stream;
			}
		}

		/// <summary>
		/// Offsets the current position from an origin.
		/// </summary>
		public long Seek( long offset, SeekOrigin origin )
		{
			return _stream.Seek( offset, origin );
		}

		/// <summary>
		/// Gets the entire stream content as a byte array.
		/// </summary>
		public byte[] ToArray()
		{
			return _stream.ToArray();
		}
	}
}