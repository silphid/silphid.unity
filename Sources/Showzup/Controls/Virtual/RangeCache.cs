using Silphid.DataTypes;
using Silphid.Extensions;
using Silphid.Showzup.Virtual.Layout;
using UnityEngine;

namespace Silphid.Showzup.Virtual
{
    public class RangeCache
    {
        #region Fields

        private readonly ILayoutCollection _collection;
        private bool _isValid;
        private IntRange _layoutedIndices;
        private Rect _layoutedRect;
        private IntRange _lastLayoutedIndices;
        private Rect _lastLayoutedRect;
        private IntRange _loadedIndices;

        #endregion

        #region Constructor

        public RangeCache(ILayoutCollection collection)
        {
            _collection = collection;
        }

        #endregion

        #region Properties

        public IntRange LayoutedIndices
        {
            get
            {
                EnsureValid();
                return _layoutedIndices;
            }
        }

        public Rect LayoutedRect
        {
            get
            {
                EnsureValid();
                return _layoutedRect;
            }
        }

        public IntRange LastLayoutedIndices
        {
            get
            {
                EnsureValid();
                return _lastLayoutedIndices;
            }
        }

        public Rect LastLayoutedRect
        {
            get
            {
                EnsureValid();
                return _lastLayoutedRect;
            }
        }

        public IntRange LoadedIndices
        {
            get
            {
                EnsureValid();
                return _loadedIndices;
            }
        }

        #endregion

        #region Public methods

        public void Invalidate()
        {
            Reset();
            _isValid = false;
        }

        public void Reset()
        {
            _lastLayoutedIndices = _layoutedIndices = _loadedIndices = IntRange.Empty;
            _lastLayoutedRect = _layoutedRect = Rect.zero;
        }

        public void OnItemLoaded(int index)
        {
            if (LoadedIndices.Contains(index))
                return;

            _loadedIndices = LoadedIndices.Union(index);
        }

        public void OnItemLayouted(int index)
        {
            if (EnsureValid())
            {
                if (LayoutedIndices.Contains(index))
                    return;

                _lastLayoutedIndices = _layoutedIndices = LayoutedIndices.Union(index);
                _lastLayoutedRect = _layoutedRect = LayoutedRect.Union(_collection.GetRect(index));
            }
        }

        public void OnItemUnloaded(int index)
        {
            if (EnsureValid())
            {
                if (!_layoutedIndices.Contains(index))
                    return;

                var oldLoadedIndices = _loadedIndices;
            
                _loadedIndices = IntRange.Empty;
                _layoutedIndices = IntRange.Empty;
                _layoutedRect = Rect.zero;

                for (int i = oldLoadedIndices.Start; i < oldLoadedIndices.End; i++)
                {
                    if (_collection.IsLoaded(i))
                        _loadedIndices = _loadedIndices.Union(i);

                    if (_collection.IsLayouted(i))
                    {
                        _layoutedIndices = _layoutedIndices.Union(i);
                        _layoutedRect = _layoutedRect.Union(_collection.GetRect(i));
                    }
                }

                if (!_layoutedIndices.IsEmpty)
                {
                    _lastLayoutedIndices = _layoutedIndices;
                    _lastLayoutedRect = _layoutedRect;
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Completely recalculates ranges from scratch, but only if were marked as invalid,
        /// and returns whether they were already valid before.  
        /// </summary>
        private bool EnsureValid()
        {
            if (_isValid)
                return true;
            
            _loadedIndices = IntRange.Empty;
            _layoutedIndices = IntRange.Empty;
            _layoutedRect = Rect.zero;

            for (int i = 0; i < _collection.Count; i++)
            {
                if (_collection.IsLoaded(i))
                {
                    var rect = _collection.GetRect(i);

                    _loadedIndices = _loadedIndices.Union(i);

                    if (_collection.IsLayouted(i))
                    {
                        _layoutedIndices = _layoutedIndices.Union(i);
                        _layoutedRect = _layoutedRect.Union(rect);
                    }
                }
            }

            _isValid = true;
            return true;
        }

        #endregion
    }
}