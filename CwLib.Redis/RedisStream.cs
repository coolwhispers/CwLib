using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace CwLib.Redis
{
    public sealed class RedisStream : Stream
    {
        private RedisBuffer _buffer;

        public RedisStream(IDatabase db, string key, TimeSpan? timeout = null, string tag = "RedisStream")
        {
            var mainKey = string.Format("{0}:{1}", tag, key);
            _buffer = new RedisBuffer(mainKey, db, timeout);
            Position = 0;
            _canWrite = true;
            _canRead = true;
            _canSeek = true;
        }

        public RedisStream(IDatabase db, string key, int timeoutSec, string tag = "RedisStream") : this(db, key, new TimeSpan(0, 0, timeoutSec), tag)
        {
        }

        #region Property

        private bool _canRead;
        public override bool CanRead { get { return _canRead; } }

        private bool _canSeek;
        public override bool CanSeek { get { return _canSeek; } }

        private bool _canWrite;
        public override bool CanWrite { get { return _canWrite; } }

        public override long Position { get; set; }

        public override long Length
        {
            get
            {
                return _buffer.Length;
            }
        }
        #endregion

        #region Method

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }

            long currentIndex = Position / _buffer.BuffSize;
            var currentBuff = _buffer.GetBuffer(currentIndex);

            for (var i = 0; i < count; i++)
            {
                var thisPosition = Position + i;

                var index = thisPosition / _buffer.BuffSize;
                var iCount = thisPosition % _buffer.BuffSize;

                if (index != currentIndex)
                {
                    currentIndex = index;
                    currentBuff = _buffer.GetBuffer(currentIndex);
                }

                currentBuff[iCount] = buffer[i];
            }

            Position += count;
            if (Position > _buffer.Length)
            {
                _buffer.Length = Position;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if (buffer.Length - offset < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }

            if (Position >= _buffer.Length)
            {
                return 0;
            }

            Position += offset;

            var readCount = 0;

            var currentIndex = Position / _buffer.BuffSize;

            var currentBuff = _buffer.GetBuffer(currentIndex);

            for (var i = 0; i < count; i++)
            {
                var index = Position / _buffer.BuffSize;
                var iCount = Position % _buffer.BuffSize;

                if (index != currentIndex)
                {
                    currentBuff = _buffer.GetBuffer(index);
                    currentIndex = index;
                }

                buffer[i] = currentBuff[iCount];

                Position++;
                readCount++;

                if (Position >= _buffer.Length)
                {
                    break;
                }
            }

            return readCount;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (CanSeek)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        Position = offset;
                        break;
                    case SeekOrigin.Current:
                        Position += offset;
                        break;
                    case SeekOrigin.End:
                        Position = _buffer.Length - 1 + offset;
                        break;
                }

                if (Position < 0)
                {
                    Position = 0;
                }

                if (Position > _buffer.Length)
                {
                    Position = _buffer.Length;
                }

                return Position;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override void SetLength(long value)
        {
            if (value < 0 || value > Int32.MaxValue)
            {
                throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
            }

            var newLength = value - _buffer.Length;

            if (newLength > 0)
            {
                var frist = _buffer.Length / _buffer.BuffSize;
                var fristCount = (_buffer.Length - 1) % _buffer.BuffSize;
                var last = value / _buffer.BuffSize;

                var currentBuff = _buffer.GetBuffer(frist);
                for (var i = fristCount; i < _buffer.BuffSize; i++)
                {
                    currentBuff[i] = 0;
                }
            }
            else if (newLength < 0)
            {
                var frist = value / _buffer.BuffSize;
                var fristCount = value % _buffer.BuffSize;
                var last = (_buffer.Length - 1) / _buffer.BuffSize;
                var currentBuff = _buffer.GetBuffer(frist);

                if (value == 0)
                {
                    for (var index = frist; index <= last; index++)
                    {
                        _buffer.DeleteBuffer(index);
                    }
                }
                else
                {
                    for (var index = fristCount; index < _buffer.BuffSize; index++)
                    {
                        currentBuff[index] = 0;
                    }

                    for (var index = frist + 1; index <= last; index++)
                    {
                        _buffer.DeleteBuffer(index);
                    }
                }
                Position = value;
            }

            _buffer.Length = value;
        }

        public override void Flush()
        {
            _buffer.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _buffer = null;

            base.Dispose(disposing);

            GC.Collect();
        }

        public byte[] ToArray()
        {
            var buffer = new byte[_buffer.Length];

            Position = 0;

            Read(buffer, 0, buffer.Length);

            return buffer;
        }

        #endregion

        private class RedisBuffer
        {
            #region Field            
            /// <summary>
            /// 是否為新Stream
            /// </summary>
            private bool _isNewStream;

            /// <summary>
            /// Redis Key
            /// </summary>
            string _key;

            /// <summary>
            /// Timeout
            /// </summary>
            private TimeSpan? _timeout;

            /// <summary>
            /// 儲存長度Field
            /// </summary>
            private string _redisStreamLengthKey;

            /// <summary>
            /// Cache Buffer(Flush時寫入Redis)
            /// </summary>
            private ConcurrentDictionary<long, byte[]> _cache;

            /// <summary>
            /// 目前Redis Db
            /// </summary>
            private IDatabase _db;

            #endregion

            #region Property
            private int _buffSize;
            /// <summary>
            /// 每個Buffer Size
            /// </summary>
            public int BuffSize { get { return _buffSize; } }

            /// <summary>
            /// Buffer長度
            /// </summary>
            public long Length { get; set; }

            #endregion

            /// <summary>
            /// 初始化SrmRedisBuffer
            /// </summary>
            /// <param name="key">Key</param>
            /// <param name="db">Db</param>
            /// <param name="timeoutSec">逾時時間</param>
            public RedisBuffer(string key, IDatabase db, TimeSpan? timeout)
            {
                _buffSize = 256000;
                _redisStreamLengthKey = "Length";
                _key = key;
                _db = db;
                _timeout = timeout;
                _isNewStream = !_db.KeyExists(_key);
                _cache = new ConcurrentDictionary<long, byte[]>();
                Length = GetLength();
                _nextIndex = -1;
                _lockNext = new object();
            }

            /// <summary>
            /// 取得Buffer
            /// </summary>
            /// <param name="index">Index</param>
            /// <returns></returns>
            public byte[] GetBuffer(long index)
            {
                return _cache.GetOrAdd(index, x => GetBufferFromTemp(x));
            }

            /// <summary>
            /// 將Cache Buffer寫入Redis並清空
            /// </summary>
            public void Flush()
            {
                var batch = _db.CreateBatch();

                var list = new List<Task>();

                foreach (var keyValue in _cache)
                {
                    var task = batch.HashSetAsync(_key, keyValue.Key, keyValue.Value);
                    list.Add(task);
                }

                list.Add(batch.HashSetAsync(_key, _redisStreamLengthKey, Length));

                if (_timeout.HasValue)
                {
                    list.Add(batch.KeyExpireAsync(_key, _timeout));
                }

                batch.Execute();

                Task.WaitAll(list.ToArray());
            }

            #region temp buffer

            /// <summary>
            /// 下一個Buffer
            /// </summary>
            byte[] _nextBuff;

            /// <summary>
            /// 目前Buffer Index
            /// </summary>
            long _nextIndex;

            /// <summary>
            /// 讀取下一個Buffer的Task
            /// </summary>
            Task _nextTask;

            /// <summary>
            /// 鎖定防止多個next
            /// </summary>
            object _lockNext;

            #endregion

            /// <summary>
            /// 從Redis取得Buffer
            /// </summary>
            /// <param name="index">Index</param>
            /// <returns></returns>
            public byte[] GetBufferFromTemp(long index)
            {
                try
                {
                    if (_nextIndex != -1 && _nextIndex == index && _nextTask != null)
                    {
                        _nextTask.Wait();

                        return _nextBuff;
                    }

                    return GetBufferFromRedis(index);
                }
                finally
                {
                    _nextTask = Task.Factory.StartNew(() =>
                    {
                        lock (_lockNext)
                        {
                            _nextIndex = index + 1;
                            _nextBuff = GetBufferFromRedis(_nextIndex);
                        }
                    });
                }
            }

            /// <summary>
            /// 從Redis取得Buffer
            /// </summary>
            /// <returns></returns>
            private byte[] GetBufferFromRedis(long index)
            {
                var result = new byte[BuffSize];

                if (_isNewStream)
                {
                    return result;
                }

                var buff = _db.HashGet(_key, index, CommandFlags.PreferSlave);

                if (buff.HasValue)
                {
                    result = buff;
                }

                return result;
            }

            /// <summary>
            /// 刪除Buffer
            /// </summary>
            /// <param name="index">The index.</param>
            public void DeleteBuffer(long index)
            {
                _db.HashDelete(_key, index);
            }

            /// <summary>
            /// 取得Buffer長度
            /// </summary>
            /// <returns></returns>
            private long GetLength()
            {
                if (_isNewStream)
                {
                    return 0;
                }

                var l = _db.HashGet(_key, _redisStreamLengthKey);

                if (_timeout.HasValue)
                {
                    _db.KeyExpire(_key, _timeout);
                }

                if (l.HasValue)
                {
                    return Convert.ToInt64(l);
                }

                return 0;
            }

        }
    }
}