
namespace WtTools.Web.Data.Wrpl
{
    public class CraftsInfoEnum : IEnumerator<CraftInfo>
    {
        private readonly CraftsInfoArray _crafts;
        private int _position = -1;
        private bool disposedValue;

        public CraftsInfoEnum(CraftsInfoArray crafts)
        {
            _crafts = crafts;
        }
        public CraftInfo Current
        {
            get
            {
                try
                {
                    return _crafts[_position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        } 

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _position++;
            return _position < _crafts.Length;
        }

        public void Reset()
        {
            _position = -1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CraftsInfoEnum()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
