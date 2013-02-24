﻿#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
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

using System.Drawing;

namespace MediaPortal.UI.SkinEngine.DirectX.RenderPipelines
{
  /// <summary>
  /// <see cref="SBSRenderPipeline"/> implements a Side-By-Side rendering pipeline, where the first pass represents the left eye frame.
  /// </summary>
  internal class SBSRenderPipeline : AbstractMultiPassRenderPipeline
  {
    public override void BeginRenderPass()
    {
      base.BeginRenderPass();
      _firstFrameTargetRect = new Rectangle(0, 0, _renderTarget.Width / 2, _renderTarget.Height);
      _seconfFrameTargetRect = new Rectangle(_renderTarget.Width / 2, 0, _renderTarget.Width / 2, _renderTarget.Height);
    }
  }
}