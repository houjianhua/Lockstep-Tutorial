﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Lockstep.Network
{
    public class CircularBuffer: Stream
    {
        public int ChunkSize = 8192;

        private readonly Queue<byte[]> bufferQueue = new Queue<byte[]>();

        private readonly Queue<byte[]> bufferCache = new Queue<byte[]>();

        public int LastIndex { get; set; }

        public int FirstIndex { get; set; }
		
        private byte[] lastBuffer;

        public CircularBuffer()
        {
            this.AddLast();
        }

        public CircularBuffer(int chunkSize)
        {
            this.ChunkSize = chunkSize;
            this.AddLast();
        }

        public override long Length
        {
            get
            {
                int c = 0;
                if (this.bufferQueue.Count == 0)
                {
                    c = 0;
                }
                else
                {
                    c = (this.bufferQueue.Count - 1) * ChunkSize + this.LastIndex - this.FirstIndex;
                }
                if (c < 0)
                {
                    LLog.Error(string.Format("TBuffer count < 0: {0}, {1}, {2}",this.bufferQueue.Count, this.LastIndex, this.FirstIndex));
                }
                return c;
            }
        }

        public void AddLast()
        {
            byte[] buffer;
            if (this.bufferCache.Count > 0)
            {
                buffer = this.bufferCache.Dequeue();
            }
            else
            {
                buffer = new byte[ChunkSize];
            }
            this.bufferQueue.Enqueue(buffer);
            this.lastBuffer = buffer;
        }

        public void RemoveFirst()
        {
            this.bufferCache.Enqueue(bufferQueue.Dequeue());
        }

        public byte[] First
        {
            get
            {
                if (this.bufferQueue.Count == 0)
                {
                    this.AddLast();
                }
                return this.bufferQueue.Peek();
            }
        }

        public byte[] Last
        {
            get
            {
                if (this.bufferQueue.Count == 0)
                {
                    this.AddLast();
                }
                return this.lastBuffer;
            }
        }

		/// <summary>
		/// 从CircularBuffer读取到stream流中
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public async Task ReadAsync(Stream stream)
	    {
		    long buffLength = this.Length;
			int sendSize = this.ChunkSize - this.FirstIndex;
		    if (sendSize > buffLength)
		    {
			    sendSize = (int)buffLength;
		    }
			
		    await stream.WriteAsync(this.First, this.FirstIndex, sendSize);

		    this.FirstIndex += sendSize;
		    if (this.FirstIndex == this.ChunkSize)
		    {
			    this.FirstIndex = 0;
			    this.RemoveFirst();
		    }
		}

		/// <summary>
		/// 从stream流写到CircularBuffer中
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public async Task<int> WriteAsync(Stream stream)
	    {
		    int size = this.ChunkSize - this.LastIndex;
			
		    int n = await stream.ReadAsync(this.Last, this.LastIndex, size);

		    if (n == 0)
		    {
			    return 0;
		    }

		    this.LastIndex += n;

		    if (this.LastIndex == this.ChunkSize)
		    {
			    this.AddLast();
			    this.LastIndex = 0;
		    }

		    return n;
	    }

        public override int Read(byte[] buffer, int offset, int count)
        {
	        if (buffer.Length < offset + count)
	        {
		        throw new Exception($"bufferList length < coutn, buffer length: {buffer.Length} {offset} {count}");
	        }

	        long length = this.Length;
			if (length < count)
            {
	            count = (int)length;
            }

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int n = count - alreadyCopyCount;
				if (ChunkSize - this.FirstIndex > n)
                {
                    Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, n);
                    this.FirstIndex += n;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(this.First, this.FirstIndex, buffer, alreadyCopyCount + offset, ChunkSize - this.FirstIndex);
                    alreadyCopyCount += ChunkSize - this.FirstIndex;
                    this.FirstIndex = 0;
                    this.RemoveFirst();
                }
            }

	        return count;
        }

        public void Write(byte[] buffer)
        {
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < buffer.Length)
            {
                if (this.LastIndex == ChunkSize)
                {
                    this.AddLast();
                    this.LastIndex = 0;
                }

                int n = buffer.Length - alreadyCopyCount;
                if (ChunkSize - this.LastIndex > n)
                {
                    Array.Copy(buffer, alreadyCopyCount, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += buffer.Length - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, alreadyCopyCount, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                if (this.LastIndex == ChunkSize)
                {
                    this.AddLast();
                    this.LastIndex = 0;
                }

                int n = count - alreadyCopyCount;
                if (ChunkSize - this.LastIndex > n)
                {
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, n);
                    this.LastIndex += count - alreadyCopyCount;
                    alreadyCopyCount += n;
                }
                else
                {
                    Array.Copy(buffer, alreadyCopyCount + offset, this.lastBuffer, this.LastIndex, ChunkSize - this.LastIndex);
                    alreadyCopyCount += ChunkSize - this.LastIndex;
                    this.LastIndex = ChunkSize;
                }
            }
        }

	    public override void Flush()
	    {
		    throw new NotImplementedException();
		}

	    public override long Seek(long offset, SeekOrigin origin)
	    {
			throw new NotImplementedException();
	    }

	    public override void SetLength(long value)
	    {
		    throw new NotImplementedException();
		}

	    public override bool CanRead
	    {
		    get
		    {
			    return true;
		    }
	    }

	    public override bool CanSeek
	    {
		    get
		    {
			    return false;
		    }
	    }

	    public override bool CanWrite
	    {
		    get
		    {
			    return true;
		    }
	    }

	    public override long Position { get; set; }
    }
}