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

using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.Helpers;
using MediaPortal.UiComponents.Media.General;

namespace MediaPortal.UiComponents.Media.Models.Navigation
{
  public class MovieItem : VideoItem
  {
    public MovieItem(MediaItem mediaItem)
      : base(mediaItem)
    {
    }

    public override void Update(MediaItem mediaItem)
    {
      base.Update(mediaItem);
      MovieInfo movieInfo = new MovieInfo();
      MediaItemAspect movieAspect;
      if (!mediaItem.Aspects.TryGetValue(MovieAspect.ASPECT_ID, out movieAspect))
        return;

      MovieName = (string)movieAspect[MovieAspect.ATTR_MOVIE_NAME] ?? string.Empty;
      CollectionName = movieInfo.CollectionName = (string)movieAspect[MovieAspect.ATTR_COLLECTION_NAME] ?? string.Empty;
      FireChange();
    }

    public string MovieName
    {
      get { return this[Consts.KEY_SERIES_EPISODE_NAME]; }
      set { SetLabel(Consts.KEY_SERIES_EPISODE_NAME, value); }
    }

    public string CollectionName
    {
      get { return this[Consts.KEY_MOVIE_COLLECTION]; }
      set { SetLabel(Consts.KEY_MOVIE_COLLECTION, value); }
    }
  }
}