using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SummerBoot.Repository.Core
{
    public class GridReader : IDisposable
    {
        private int gridIndex;
        private IDataReader Reader { set; get; }
        private IDbCommand Command { set; get; }
        private DatabaseUnit DatabaseUnit { get; set; }


        public GridReader(IDataReader reader, IDbCommand command, DatabaseUnit databaseUnit)
        {
            Reader = reader;
            Command = command;
            DatabaseUnit = databaseUnit;
        }

        public IEnumerable<T> Read<T>()
        {
            return ReadSingleResult<T>(gridIndex).ToList();
        }

        public async Task<IEnumerable<T>> ReadAsync<T>()
        {
            return (await ReadSingleResultAsync<T>(gridIndex)).ToList();
        }

        private async Task<IEnumerable<T>> ReadSingleResultAsync<T>(int index)
        {
            if (this.Reader is DbDataReader dataReader)
            {
                try
                {
                    var result = new List<T>();
                    while (index == gridIndex && await dataReader.ReadAsync(new CancellationToken()).ConfigureAwait(false))
                    {
                        var func = DatabaseContext.GetDeserializer(typeof(T), Reader, DatabaseUnit);
                        object val = func(Reader);
                        var item= DatabaseContext.GetValue<T>(Reader, typeof(T), val);
                        result.Add(item);
                    }
                    return result;
                }
                finally 
                {
                    if (index == gridIndex)
                    {
                        await NextResultAsync().ConfigureAwait(false);
                    }
                }
            }

            return default;
        }

        private async Task NextResultAsync()
        {
            if (await ((DbDataReader)Reader).NextResultAsync(new CancellationToken()).ConfigureAwait(false))
            {
                gridIndex++;
            }
            else
            {
                Reader.Dispose();
                Reader = null;
                //callbacks?.OnCompleted();
                Dispose();
            }
        }

        private IEnumerable<T> ReadSingleResult<T>(int index)
        {
            try
            {
                while (index == gridIndex && Reader.Read())
                {
                    var func = DatabaseContext.GetDeserializer(typeof(T), Reader, DatabaseUnit);
                    object val = func(Reader);
                    yield return DatabaseContext.GetValue<T>(Reader, typeof(T), val);
                }
            }
            finally // finally so that First etc progresses things even when multiple rows
            {
                if (index == gridIndex)
                {
                    NextResult();
                }
            }
        }


        private void NextResult()
        {
            if (Reader.NextResult())
            {
                gridIndex++;
               
            }
            else
            {
                Reader.Dispose();
                Reader = null;
                //callbacks?.OnCompleted();
                Dispose();
            }
        }

        public void Dispose()
        {
            if (Reader != null)
            {
                if (!Reader.IsClosed) Command?.Cancel();
                Reader.Dispose();
                Reader = null;
            }
            if (Command != null)
            {
                Command.Parameters.Clear();
                Command.Dispose();
                Command = null;
            }
            
            GC.SuppressFinalize(this);
        }
    }


}

