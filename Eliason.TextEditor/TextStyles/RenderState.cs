using System;
using System.Collections.Generic;
using Eliason.TextEditor.Native;
using Eliason.Common;

namespace Eliason.TextEditor.TextStyles
{
    public class RenderState
    {
        private readonly List<RenderStateItem> _items = new List<RenderStateItem>();
        private readonly RenderStateItem _defaultRenderStateItem;

        private int _previousBackColor = -1;
        private int _previousForeColor = -1;

        public RenderState(ITextEditor textEditor, TextStyleBase defaultStyle)
        {
            var rsi = new RenderStateItem();
            defaultStyle.FillRenderStateItem(textEditor, rsi);

            this._defaultRenderStateItem = rsi;
        }

        public bool Add(ITextEditor textEditor, ITextSegmentStyled segment)
        {
            var newRsi = new RenderStateItem {Segment = segment};
            segment.Style.FillRenderStateItem(textEditor, newRsi);

            this._items.Add(newRsi);

            return true;
        }

        public void Remove(ITextSegmentStyled segment)
        {
            for (var i = 0; i < this._items.Count; i++)
            {
                if (this._items[i].Segment == segment)
                {
                    this._items.RemoveAt(i);
                    break;
                }
            }
        }

        public IEnumerable<RenderStateItem> GetRenderStateItems()
        {
            return this._items;
        }

        public void Apply(IntPtr hdc)
        {
            var foreColorZIndex = -1;
            var foreColor = -1;

            var backColorZIndex = -1;
            var backColor = -1;

            foreach (var item in this._items)
            {
                if (item.ForeColor != -1)
                {
                    if (item.ForeColorZIndex > foreColorZIndex)
                    {
                        foreColorZIndex = item.ForeColorZIndex;
                        foreColor = item.ForeColor;
                    }
                }

                if (item.BackColor != -1)
                {
                    if (item.BackColorZIndex > backColorZIndex)
                    {
                        backColorZIndex = item.BackColorZIndex;
                        backColor = item.BackColor;
                    }
                }
            }

            if (this._previousBackColor != backColor)
            {
                if (backColor == -1)
                {
                    SafeNativeMethods.SetBkMode(hdc, NativeConstants.TRANSPARENT);
                }
                else
                {
                    SafeNativeMethods.SetBkMode(hdc, NativeConstants.OPAQUE);
                    SafeNativeMethods.SetBkColor(hdc, backColor);
                }

                this._previousBackColor = backColor;
            }

            if (this._previousForeColor != foreColor)
            {
                if (foreColor == -1)
                {
                    foreColor = this._defaultRenderStateItem.ForeColor;
                }

                SafeNativeMethods.SetTextColor(hdc, foreColor);
                this._previousForeColor = foreColor;
            }
        }
    }
}