#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Eliason.TextEditor.Native;
using Eliason.Common;

#endregion

namespace Eliason.TextEditor.TextView
{
    partial class TextView
    {
        private static SafeHandleGDI staticBackgroundHBitmap;
        private SafeHandleGDI backBrush;
        private SafeHandleGDI caretBrushSelection;
        private SafeHandleGDI hFont;

        #region buf

        private SafeHandleGDI bufHBitmap;
        private Size bufHBitmapSize;

        private SafeHandleDC bufHdc;
        private SafeHandleDC tempHdc;

        private void DisposeBuffer()
        {
            if (this.bufHBitmap != null)
            {
                this.bufHBitmap.Dispose();
                this.bufHBitmap = null;
            }

            if (this.bufHdc != null)
            {
                this.bufHdc.Dispose();
                this.bufHdc = null;
            }

            if (this.tempHdc != null)
            {
                this.tempHdc.Dispose();
                this.tempHdc = null;
            }

            if (this.hFont != null)
            {
                this.hFont.Dispose();
                this.hFont = null;
            }
        }

        #endregion

        #region ITextView Members

        public IntPtr GetHdcDangerous()
        {
            return this.GetHdc().DangerousGetHandle();
        }

        public void ReleaseHdc()
        {
            if (this.tempHdc != null)
            {
                this.tempHdc.Dispose();
                this.tempHdc = null;
            }
        }

        public Color LineHighlightColor
        {
            get { return this.Settings.LineHighlightColor; }
        }

        #endregion

        public SafeHandleDC GetHdc()
        {
            if (this.bufHdc == null)
            {
                if (this.tempHdc != null)
                {
                    return this.tempHdc;
                }

                var handle = IntPtr.Zero;

                using (var graphics = Graphics.FromHwnd(Handle))
                {
                    var hdc = graphics.GetHdc();
                    handle = SafeNativeMethods.CreateCompatibleDC(hdc);
                    graphics.ReleaseHdc();
                }

                this.SetDefaultPaintState(handle);

                return this.tempHdc = new SafeHandleDC(handle);
            }

            return this.bufHdc;
        }

        private void SetDefaultPaintState(IntPtr hdc)
        {
            if (this.hFont == null)
            {
                this.hFont = new SafeHandleGDI(this.Font.ToHfont());
            }

            var styleDefault = this.GetTextStyle("Default") as TextStyles.TextStyleManual;

            SafeNativeMethods.SetBkMode(hdc, styleDefault.ColorBackground == Color.Transparent ? NativeConstants.TRANSPARENT : NativeConstants.OPAQUE);
            SafeNativeMethods.SetBkColor(hdc, ColorTranslator.ToWin32(styleDefault.ColorBackground));
            SafeNativeMethods.SetTextColor(hdc, ColorTranslator.ToWin32(styleDefault.ColorFont));
            SafeNativeMethods.SelectObject(hdc, this.hFont.DangerousGetHandle());
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do nothing.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.EnsureSelectionArrays(this.CurrentTextColumnIndex);

            #region buf

            if (this.bufHBitmap != null)
            {
                if (this.bufHBitmapSize.Width != Width || this.bufHBitmapSize.Height != Height)
                {
                    this.DisposeBuffer();
                }
            }

            var destHdc = e.Graphics.GetHdc();

            if (this.bufHdc == null || this.bufHdc.IsInvalid)
            {
                this.bufHdc = new SafeHandleDC(SafeNativeMethods.CreateCompatibleDC(destHdc));
            }

            if (this.bufHBitmap == null || this.bufHBitmap.IsInvalid)
            {
                var width = Width;
                var height = Height;

                if (width <= 0 || height <= 0)
                {
                    e.Graphics.ReleaseHdc(destHdc);
                    return;
                }

                this.bufHBitmap = new SafeHandleGDI(SafeNativeMethods.CreateCompatibleBitmap(destHdc, width, height));
                this.bufHBitmapSize = new Size(width, height);

                SafeNativeMethods.SelectObject(this.bufHdc.DangerousGetHandle(), this.bufHBitmap.DangerousGetHandle());
            }

            #endregion

            #region Initialize GDI resources

            if (this.backBrush == null)
            {
                var c = this.BackColor;
                this.backBrush = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(c)));

                c = Color.FromArgb((byte)(255 - SystemColors.Highlight.R), (byte)(255 - SystemColors.Highlight.G), (byte)(255 - SystemColors.Highlight.B));

                this.caretBrushSelection = new SafeHandleGDI(SafeNativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(c)));
            }

            #endregion

            var hdc = this.bufHdc;

            this.SetDefaultPaintState(hdc.DangerousGetHandle());

            var clientRect = new RECT
            {
                right = ClientRectangle.Width,
                bottom = ClientRectangle.Height
            };

            if (BackgroundImage != null)
            {
                if (staticBackgroundHBitmap == null)
                {
                    staticBackgroundHBitmap = new SafeHandleGDI((BackgroundImage as Bitmap).GetHbitmap());
                }

                var bkgHdc = SafeNativeMethods.CreateCompatibleDC(destHdc);
                var bkgHdcPrev = SafeNativeMethods.SelectObject(bkgHdc, staticBackgroundHBitmap.DangerousGetHandle());

                for (var imgX = 0; imgX < ClientSize.Width; imgX += BackgroundImage.Width)
                {
                    for (var imgY = 0; imgY < ClientSize.Height; imgY += BackgroundImage.Height)
                    {
                        SafeNativeMethods.BitBlt(hdc.DangerousGetHandle(), imgX, imgY, BackgroundImage.Width, BackgroundImage.Height, bkgHdc, 0, 0, NativeConstants.SRCCOPY);
                    }
                }

                SafeNativeMethods.SelectObject(bkgHdc, bkgHdcPrev);
                SafeNativeMethods.DeleteDC(bkgHdc);
            }
            else
            {
                SafeNativeMethods.FillRect(hdc.DangerousGetHandle(), ref clientRect, this.backBrush.DangerousGetHandle());
            }

            #region Paint overlay under text if the control is read-only

            if (this.IsReadOnly)
            {
                using (var g = Graphics.FromHdc(hdc.DangerousGetHandle()))
                {
                    using (var hatchBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.FromArgb(50, Color.Gray), Color.Transparent))
                    {
                        g.FillRectangle(hatchBrush, 0, 0, Width, Height);
                    }
                }
            }

            #endregion

            this._renderer.Render(hdc.DangerousGetHandle(), this.ScrollHost.ScrollPosH + this.GetTextRectangle(false).Left, this.ScrollHost.ScrollPosVIntegral, ClientSize);

            #region Paint overlay if control is not enabled or virtual

            if (Enabled == false)
            {
                using (var g = Graphics.FromHdc(hdc.DangerousGetHandle()))
                {
                    using (var hatchBrush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.Gray, Color.Transparent))
                    {
                        g.FillRectangle(hatchBrush, 0, 0, Width, Height);
                    }
                }
            }
            else if (this.IsVirtual)
            {
                this.SetDefaultPaintState(hdc.DangerousGetHandle());

                var brush = SafeNativeMethods.CreateHatchBrush(NativeConstants.HS_FDIAGONAL, ColorTranslator.ToWin32(Color.Gray));

                var previousBrush = SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), brush);

                var leftsideFillRect = new RECT
                {
                    bottom = Height,
                    right = Padding.Left
                };

                SafeNativeMethods.FillRect(hdc.DangerousGetHandle(), ref leftsideFillRect, brush);

                SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), previousBrush);
                SafeNativeMethods.DeleteObject(brush);
            }

            #endregion

            #region Paint caret

            if (this._imeComposition != null && this._imeComposition.IsCompositioning)
            {
                this._imeComposition.Paint(hdc.DangerousGetHandle(), this.Caret.Location.X, this.Caret.Location.Y, this.Caret);
            }
            else
            {
                this.Caret.Render(hdc.DangerousGetHandle());
            }

            #endregion

            #region Paint text limitation guides (when scrolling outside)

            if (this.CanScrollOutside)
            {
                if (this.ScrollHost != null)
                {
                    var endY = this.ScrollHost.VerticalMax - this.ScrollHost.ScrollPosVIntegral - this.LineHeight;

                    if (endY + this.LineHeight < ClientRectangle.Height)
                    {
                        // TODO: Should not be gray. Should be ControlPaint.Light(255 - this.BackColor) 
                        var penHandle = SafeNativeMethods.CreatePen(NativeConstants.PS_DOT, -1, ColorTranslator.ToWin32(Color.Gray));
                        var previous = SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), penHandle);

                        SafeNativeMethods.MoveToEx(hdc.DangerousGetHandle(), 0, endY, IntPtr.Zero);
                        SafeNativeMethods.LineTo(hdc.DangerousGetHandle(), ClientRectangle.Width, endY);

                        SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), previous);
                        SafeNativeMethods.DeleteObject(penHandle);
                    }

                    var endX = this.ScrollHost.HorizontalMax - this.ScrollHost.ScrollPosH + Padding.Left + Padding.Right;

                    if (endX + this.LineHeight < ClientRectangle.Width)
                    {
                        var penHandle = SafeNativeMethods.CreatePen(NativeConstants.PS_DOT, -1,
                            ColorTranslator.ToWin32(Color.Gray));
                        var previous = SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), penHandle);

                        SafeNativeMethods.MoveToEx(hdc.DangerousGetHandle(), endX, 0, IntPtr.Zero);
                        SafeNativeMethods.LineTo(hdc.DangerousGetHandle(), endX, endY == -1
                            ? ClientRectangle.Height
                            : endY);

                        SafeNativeMethods.SelectObject(hdc.DangerousGetHandle(), previous);
                        SafeNativeMethods.DeleteObject(penHandle);
                    }
                }
            }

            #endregion

            SafeNativeMethods.BitBlt(destHdc, 0, 0, this.bufHBitmapSize.Width, this.bufHBitmapSize.Height, hdc.DangerousGetHandle(), 0, 0, (int)NativeConstants.SRCCOPY);

            e.Graphics.ReleaseHdc(destHdc);
            destHdc = IntPtr.Zero;

            base.OnPaint(e);
        }
    }
}