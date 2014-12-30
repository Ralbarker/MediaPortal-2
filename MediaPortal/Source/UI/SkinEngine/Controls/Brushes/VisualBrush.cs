#region Copyright (C) 2007-2014 Team MediaPortal

/*
    Copyright (C) 2007-2014 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using MediaPortal.Common.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.DirectX11;
using MediaPortal.UI.SkinEngine.MpfElements;
using MediaPortal.UI.SkinEngine.Rendering;
using MediaPortal.UI.SkinEngine.ScreenManagement;
using MediaPortal.Utilities.DeepCopy;
using SharpDX;
using SharpDX.Direct2D1;
using Size = SharpDX.Size2;
using SizeF = SharpDX.Size2F;
using PointF = SharpDX.Vector2;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  public class VisualBrush : TileBrush, IRenderBrush
  {
    #region Protected fields

    protected AbstractProperty _visualProperty;
    protected AbstractProperty _autoLayoutContentProperty;
    protected Screen _screen = null;
    protected SizeF _visualSize = new SizeF();
    protected FrameworkElement _preparedVisual = null;
    protected String _renderTargetKey;
    protected static int _visualBrushId = 0;

    #endregion

    #region Ctor

    public VisualBrush()
    {
      _visualBrushId++;
      _renderTargetKey = String.Format("VisualBrush RenderTarget2D #{0}", _visualBrushId);

      Init();
      Attach();
    }

    public override void Dispose()
    {
      FrameworkElement visual = Visual;
      MPF.TryCleanupAndDispose(visual);
      base.Dispose();
    }

    void Init()
    {
      _visualProperty = new SProperty(typeof(FrameworkElement), null);
      _autoLayoutContentProperty = new SProperty(typeof(bool), true);
    }

    void Attach()
    {
      _visualProperty.Attach(OnVisualChanged);
    }

    void Detach()
    {
      _visualProperty.Detach(OnVisualChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      VisualBrush b = (VisualBrush)source;
      Visual = b.Visual; // Use the original Visual, copying isn't necessary
      AutoLayoutContent = b.AutoLayoutContent;
      Attach();
    }

    #endregion

    void OnVisualChanged(AbstractProperty prop, object oldValue)
    {
      PrepareVisual();
    }

    void UpdateRenderTarget(FrameworkElement fe)
    {
      RectangleF bounds = new RectangleF(0, 0, _vertsBounds.Size.Width, _vertsBounds.Size.Height);
      var oldTarget = GraphicsDevice11.Instance.Context2D1.Target;
      // Render visual to local render target (Bitmap)
      GraphicsDevice11.Instance.Context2D1.Target = _tex.Bitmap;
      //GraphicsDevice11.Instance.Context2D1.BeginDraw();
      fe.Render(new RenderContext(Matrix.Identity, bounds));
      //GraphicsDevice11.Instance.Context2D1.EndDraw();
      GraphicsDevice11.Instance.Context2D1.Target = oldTarget;
    }

    protected void PrepareVisual()
    {
      FrameworkElement visual = Visual;
      if (_preparedVisual != null && _preparedVisual != visual)
      {
        _preparedVisual.SetElementState(ElementState.Available);
        _preparedVisual.Deallocate();
        _preparedVisual = null;
      }
      if (_screen == null)
        return;
      if (visual == null)
        return;
      if (AutoLayoutContent)
      {
        // We must bypass normal layout or the visual will be layed out to screen/skin size
        visual.SetScreen(_screen);
        if (visual.ElementState == ElementState.Available)
          visual.SetElementState(ElementState.Running);
        // Here is _screen != null, which means we are allocated
        visual.Allocate();
        SizeF size = _vertsBounds.Size;
        visual.Measure(ref size);
        visual.Arrange(new RectangleF(0, 0, _vertsBounds.Size.Width, _vertsBounds.Size.Height));
      }
      _preparedVisual = visual;
    }

    #region Public properties

    public AbstractProperty VisualProperty
    {
      get { return _visualProperty; }
    }

    public FrameworkElement Visual
    {
      get { return (FrameworkElement)_visualProperty.GetValue(); }
      set { _visualProperty.SetValue(value); }
    }

    public AbstractProperty AutoLayoutContentProperty
    {
      get { return _autoLayoutContentProperty; }
    }

    public bool AutoLayoutContent
    {
      get { return (bool)_autoLayoutContentProperty.GetValue(); }
      set { _autoLayoutContentProperty.SetValue(value); }
    }

    public Bitmap1 Bitmap
    {
      get { return (_tex == null) ? null : _tex.Bitmap; }
    }

    protected override Vector2 BrushDimensions
    {
      get { return (_tex != null) ? new Vector2(_tex.Width, _tex.Height) : new Vector2(1.0f, 1.0f); }
    }

    protected override Vector2 TextureMaxUV
    {
      get { return new Vector2(1.0f, 1.0f); }
    }

    #endregion

    public override void SetupBrush(FrameworkElement parent, ref RectangleF boundary, float zOrder, bool adaptVertsToBrushTexture)
    {
      base.SetupBrush(parent, ref boundary, zOrder, adaptVertsToBrushTexture);
      _tex = ContentManager.Instance.GetRenderTarget2D(_renderTargetKey);
      _screen = parent.Screen;
      PrepareVisual();
    }

    public bool RenderContent(RenderContext renderContext)
    {
      FrameworkElement fe = _preparedVisual;
      if (fe == null) return false;
      ((RenderTarget2DAsset)_tex).AllocateRenderTarget((int)_vertsBounds.Width, (int)_vertsBounds.Height);

      UpdateRenderTarget(fe);
      return true;
    }

    public override void Allocate()
    {
      base.Allocate();
      FrameworkElement fe = _preparedVisual;
      if (fe != null)
        fe.Allocate();
      if (_tex != null)
      {
        ((RenderTarget2DAsset)_tex).AllocateRenderTarget((int)_vertsBounds.Width, (int)_vertsBounds.Height);

        if (!_tex.IsAllocated)
          return;
        BitmapBrushProperties props = new BitmapBrushProperties
        {
          ExtendModeX = ExtendMode.Clamp,
          ExtendModeY = ExtendMode.Clamp,
        };
        SetBrush(new BitmapBrush(GraphicsDevice11.Instance.Context2D1, _tex.Bitmap, props));
      }
    }

    public override void Deallocate()
    {
      base.Allocate();
      FrameworkElement fe = _preparedVisual;
      if (fe != null)
        fe.Deallocate();
    }
  }
}
