using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/*
20190119 现在只能使用DropDownList模式，DorpDown模式有问题
*/

namespace TX.Framework.WindowUI.Controls
{
    [ToolboxBitmap(typeof(ComboBox))]
    public class TXComboBoxEx : ComboBox
    {
        #region fileds

        //定义一些控件自身可以自定义的一些属性
        //1/控件的边框颜色
        Color borderColor;
        //2/控件的边框宽度
        int borderWidth;
        //3、是否绘制下拉图标框的边框
        bool drawIconBorder;
        //4、可输入区域的背景色
        Color editBackColor;
        //5、下拉图标区域的背景色
        Color iconBackColor;
        //6、下拉图标的大小
        Size iconSize;
        //7、icon的颜色
        Color iconColor;
        //8、选中时的颜色
        Color selectedColor;
        //9、没有选择项时需要在文本框中显示的提示内容，如果再深入一点，可以判断是否有内容，如果有内容就提示
        //请选择
        string noContentText = "请选择";

        /// <summary>
        /// 控件的状态
        /// </summary>
        private EnumControlState _ControlState;

        private IntPtr _EditHandle = IntPtr.Zero;

        private int _Margin = 2;

        private bool _BeginPainting = false;

        private int _CornerRadius = 0;

        private Color _BackColor = Color.White;

        #endregion

        #region Initializes

        public TXComboBoxEx()
            : base()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint,true);//使能触发OnPaint事件
            this.UpdateStyles();
            this.Size = new Size(150, 20);
            this._ControlState = EnumControlState.Default;

            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.DropDownStyle = ComboBoxStyle.DropDownList;

            borderColor = SkinManager.CurrentSkin.BorderColor; /*TXControlColorDefine.DefaultBorderColor*//*Color.FromArgb(182, 168, 192)*/;
            borderWidth = 1;
            drawIconBorder = true;
            editBackColor = _BackColor;
            iconBackColor = _BackColor;
            iconSize = new Size(12,7);
            iconColor = SkinManager.CurrentSkin.BorderColor;
            selectedColor = Color.Blue;
        }

        #endregion

        #region Properties

        [Category("自定义")]
        [Description("设置控件边框颜色")]
        public Color BorderColor
        {
            get { return borderColor; }
            set {
                if (value == null)
                    borderColor = SkinManager.CurrentSkin.BorderColor;
                else
                    borderColor = value;
                Invalidate();
            }
        }

        [Category("自定义")]
        [Description("设置控件边框宽度")]
        public int BorderWidth
        {
            get { return borderWidth; }
            set {
                if (value > 0)
                {
                    borderWidth = value;
                    Invalidate();
                }
            }
        }

        [Category("自定义")]
        [Description("是否绘制Icon的边框")]
        public bool DrawIconBorder
        {
            get { return drawIconBorder; }
            set {
                drawIconBorder = value;
                Invalidate();
            }
        }

        [Category("自定义")]
        [Description("可编辑区域的背景色")]
        public Color EditBackColor
        {
            get { return editBackColor; }
            set
            {
                if (value != null)
                {
                    editBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("自定义")]
        [Description("图标区域的背景色")]
        public Color IconBackColor
        {
            get { return iconBackColor; }
            set {
                if (value != null)
                {
                    iconBackColor = value;
                    Invalidate();
                }
            }
        }

        [Category("自定义")]
        [Description("图标大小")]
        public Size IconSize
        {
            get { return iconSize; }
            set {
                if (value != null)
                {
                    iconSize = value;
                    Invalidate();
                }
            }
        }

        [Category("自定义")]
        [Description("图标颜色")]
        public Color IconColor
        {
            get { return iconColor; }
            set { 
                if (value != null) 
                {
                    iconColor = value;
                    Invalidate();
                } 
            }
        }

        [Category("自定义")]
        [Description("下拉内容选中时的背景颜色")]
        public Color SelectedColor
        {
            set { if (value != null)
                    selectedColor = value;
            }
            get { return selectedColor; }
        }

        [Category("自定义")]
        [Description("无选择内容时需要显示的文字")]
        public string NoContentText
        {
            set {
                noContentText = value;
                Invalidate();
            }
            get { return noContentText; }
        }


        [Browsable(false)]
        public new DrawMode DrawMode
        {
            get { return DrawMode.OwnerDrawFixed; }
            set { base.DrawMode = DrawMode.OwnerDrawFixed; }
        }

        [Browsable(false)]
        public new ComboBoxStyle DropDownStyle
        {
            get { return ComboBoxStyle.DropDownList; }
            set { base.DropDownStyle = ComboBoxStyle.DropDownList; }
        }

        //下拉图标显示矩形
        internal Rectangle ButtonRect
        {
            get
            {
                return this.GetDropDownButtonRect();
            }
        }

        [Browsable(false)]
        public new Color BackColor
        {
            get { return _BackColor; }
        }

        [Browsable(false)]
        public new RightToLeft RightToLeft
        {
            get { return base.RightToLeft; }
        }

        //显示内容的矩形区域
        internal Rectangle EditRect
        {
            get
            {
                //if (this.DropDownStyle == ComboBoxStyle.DropDownList)
                Console.WriteLine("ButtonRect:x = {0},y = {1}", ButtonRect.Width, ButtonRect.Height);
                {
                    Rectangle rect = new Rectangle(
                        this._Margin, this._Margin, Width - this.ButtonRect.Width - this._Margin * 2, Height - this._Margin * 2);
                    if (RightToLeft == RightToLeft.Yes)
                    {
                        rect.X += this.ButtonRect.Right;
                    }

                    return rect;
                }

                //if (IsHandleCreated && this._EditHandle != IntPtr.Zero)
                //{
                //    RECT rcClient = new RECT();
                //    Win32.GetWindowRect(this._EditHandle, ref rcClient);
                //    Console.WriteLine("rcClient:x = {0},y = {1}", rcClient.Rect.Width, rcClient.Rect.Height);
                //    return RectangleToClient(rcClient.Rect);
                //}

                //return Rectangle.Empty;
            }
        }

        #endregion

        #region Override methods
#if true 
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            this.DrawComboBox(e.Graphics);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.State != (DrawItemState.NoAccelerator | DrawItemState.NoFocusRect | DrawItemState.ComboBoxEdit))//这个判断条件很重要
            {
                if (e.Index >= 0)
                {
                    Color backColor = _BackColor, foreColor = Color.Black;

                    if (e.State == (DrawItemState.NoAccelerator | DrawItemState.NoFocusRect) || e.State == DrawItemState.None)
                    {
                        //backColor = Color.Blue;
                        // = Color.Black;
                    }
                    else
                    {
                        backColor = selectedColor;
                        //foreColor = MetroPaint.ForeColor.Tile.Normal(Theme);
                    }

                    using (SolidBrush b = new SolidBrush(backColor))
                    {
                        e.Graphics.FillRectangle(b, new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height));
                    }

                    Rectangle textRect = new Rectangle(0, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
                    TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), this.Font, textRect, foreColor, backColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
                else
                {
                    base.OnDrawItem(e);
                }
            }
        }

        //protected override void OnLostFocus(EventArgs e)
        //{
        //    //isFocused = false;
        //    //isHovered = false;
        //    //isPressed = false;
        //    Invalidate();

        //    //base.OnLostFocus(e);
        //}
#else
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case (int)WindowMessages.WM_PAINT:
                switch (this.DropDownStyle)
                {
                    case ComboBoxStyle.DropDown:
                        if (!this._BeginPainting)
                        {
                            PAINTSTRUCT ps = new PAINTSTRUCT();
                            this._BeginPainting = true;
                            Win32.BeginPaint(m.HWnd, ref ps);
                            this.DrawComboBox(ref m);
                            Win32.EndPaint(m.HWnd, ref ps);
                            this._BeginPainting = false;
                            m.Result = Win32.TRUE;
                        }
                        else
                        {
                            base.WndProc(ref m);
                        }
                        break;
                    case ComboBoxStyle.DropDownList:
                        base.WndProc(ref m);
                        this.DrawComboBox(ref m);
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }
        
#endif

        #endregion

        #region private methods

        #region RenderComboBox

        /// <summary>
        /// 绘制复选框和内容.
        /// </summary>
        /// User:Ryan  CreateTime:2011-07-29 15:44.
        private void DrawComboBox(ref Message msg)
        {
            using (Graphics g = Graphics.FromHwnd(msg.HWnd))
            {
                this.DrawComboBox(g);
            }
        }

        /// <summary>
        /// 绘制下拉框区域.
        /// </summary>
        /// <param name="g">The Graphics.</param>
        /// User:Ryan  CreateTime:2011-07-29 15:44.
        private void DrawComboBox(Graphics g)
        {
            GDIHelper.InitializeGraphics(g);
            Rectangle rect = new Rectangle(Point.Empty, this.Size);
            rect.Width--; rect.Height--;
            ////背景
            RoundRectangle roundRect = new RoundRectangle(rect, new CornerRadius(this._CornerRadius));
            Color backColor = this.Enabled ? this._BackColor : SystemColors.Control;

            GDIHelper.FillRectangle(g, roundRect, backColor);
            Console.WriteLine("roundRect : x = {0},y = {1}", roundRect.Rect.Width,roundRect.Rect.Height);
            if (SelectedItem != null)
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString(SelectedItem.ToString(), Font, new SolidBrush(ForeColor), this.EditRect, sf);
            }
            else
            {
                if (!string.IsNullOrEmpty(noContentText))
                {
                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(noContentText, Font, new SolidBrush(ForeColor), this.EditRect, sf);
                    Console.WriteLine("EditRect : x = {0},y = {1}", EditRect.Width, EditRect.Height);
                }
            }
            //g.ResetClip();

            this.DrawButton(g);
            ////边框
            GDIHelper.DrawPathBorder(g, roundRect,borderColor,borderWidth);
        }

        /// <summary>
        ///  绘制按钮
        /// </summary>
        /// <param name="g">The Graphics.</param>
        /// User:Ryan  CreateTime:2011-08-02 14:23.
        private void DrawButton(Graphics g)
        {
            Rectangle btnRect;
            //EnumControlState btnState = this.GetComboBoxButtonPressed() ? EnumControlState.HeightLight : EnumControlState.Default;
            btnRect = new Rectangle(this.ButtonRect.X-2, this.ButtonRect.Y - 1, this.ButtonRect.Width + 1 + this._Margin, this.ButtonRect.Height + 2);
            RoundRectangle btnRoundRect = new RoundRectangle(btnRect, new CornerRadius(0, this._CornerRadius, 0, this._CornerRadius));
            //Blend blend = new Blend(3);
            //blend.Positions = new float[] { 0f, 0.5f, 1f };
            //blend.Factors = new float[] { 0f, 1f, 0f };
            GDIHelper.FillRectangle(g, btnRoundRect, iconBackColor);
            //Size btnSize = new Size(12, 7);
            ArrowDirection direction = ArrowDirection.Down;
            GDIHelper.DrawArrow(g, direction, btnRect, iconSize, 0f, iconColor);
            if (drawIconBorder)
            {GDIHelper.DrawGradientLine(g, borderColor, 90, btnRect.X, btnRect.Y, btnRect.X, btnRect.Bottom - 1);
            }
        }

        #endregion

        #region GetBoxInfo

        private ComboBoxInfo GetComboBoxInfo()
        {
            ComboBoxInfo cbi = new ComboBoxInfo();
            cbi.cbSize = Marshal.SizeOf(cbi);
            Win32.GetComboBoxInfo(base.Handle, ref cbi);
            return cbi;
        }

        private bool GetComboBoxButtonPressed()
        {
            ComboBoxInfo cbi = this.GetComboBoxInfo();
            return cbi.stateButton == ComboBoxButtonState.STATE_SYSTEM_PRESSED;
        }

        private Rectangle GetDropDownButtonRect()
        {
            ComboBoxInfo cbi = this.GetComboBoxInfo();
            return cbi.rcButton.Rect;
        }

        #endregion

        #region ResetRegion

        private void ResetRegion()
        {
            if (this._CornerRadius > 0)
            {
                Rectangle rect = new Rectangle(Point.Empty, this.Size);
                RoundRectangle roundRect = new RoundRectangle(rect, new CornerRadius(this._CornerRadius));
                if (this.Region != null)
                {
                    this.Region.Dispose();
                }

                this.Region = new Region(roundRect.ToGraphicsBezierPath());
            }
        }
        #endregion

        #endregion

        #region 附加

        #region 获取值
        /// <summary>
        /// 获取值
        /// </summary>
        private object Value
        {
            get
            {
                ComboBoxItem item = this.SelectedItem as ComboBoxItem;

                if ( item == null )
                {
                    return string.Empty;
                }

                return item.Value;
            }
        }
        #endregion

        #endregion
    }
}
