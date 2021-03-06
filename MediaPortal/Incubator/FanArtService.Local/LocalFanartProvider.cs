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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.Services.ResourceAccess;
using MediaPortal.Extensions.UserServices.FanArtService.Interfaces;

namespace MediaPortal.Extensions.UserServices.FanArtService.Local
{
  public class LocalFanartProvider : IFanArtProvider
  {
    private readonly Guid[] _necessarryMIAs = new[] { ProviderResourceAspect.ASPECT_ID };

    /// <summary>
    /// Gets a list of <see cref="FanArtImage"/>s for a requested <paramref name="mediaType"/>, <paramref name="fanArtType"/> and <paramref name="name"/>.
    /// The name can be: Series name, Actor name, Artist name depending on the <paramref name="mediaType"/>.
    /// </summary>
    /// <param name="mediaType">Requested FanArtMediaType</param>
    /// <param name="fanArtType">Requested FanArtType</param>
    /// <param name="name">Requested name of Series, Actor, Artist...</param>
    /// <param name="maxWidth">Maximum width for image. <c>0</c> returns image in original size.</param>
    /// <param name="maxHeight">Maximum height for image. <c>0</c> returns image in original size.</param>
    /// <param name="singleRandom">If <c>true</c> only one random image URI will be returned</param>
    /// <param name="result">Result if return code is <c>true</c>.</param>
    /// <returns><c>true</c> if at least one match was found.</returns>
    public bool TryGetFanArt(FanArtConstants.FanArtMediaType mediaType, FanArtConstants.FanArtType fanArtType, string name, int maxWidth, int maxHeight, bool singleRandom, out IList<IResourceLocator> result)
    {
      result = null;
      Guid mediaItemId;

      // Don't try to load "fanart" for images
      if (!Guid.TryParse(name, out mediaItemId) || mediaType == FanArtConstants.FanArtMediaType.Image)
        return false;

      IMediaLibrary mediaLibrary = ServiceRegistration.Get<IMediaLibrary>(false);
      if (mediaLibrary == null)
        return false;

      IFilter filter = new MediaItemIdFilter(mediaItemId);
      IList<MediaItem> items = mediaLibrary.Search(new MediaItemQuery(_necessarryMIAs, filter), false);
      if (items == null || items.Count == 0)
        return false;

      MediaItem mediaItem = items.First();
      string fileSystemPath = null;
      List<string> localPatterns = new List<string>();
      List<IResourceLocator> files = new List<IResourceLocator>();
      // File based access
      try
      {
        var resourceLocator = mediaItem.GetResourceLocator();
        using (var accessor = resourceLocator.CreateAccessor())
        {
          ILocalFsResourceAccessor fsra = accessor as ILocalFsResourceAccessor;
          if (fsra != null)
          {
            fileSystemPath = fsra.LocalFileSystemPath;
            var path = Path.GetDirectoryName(fileSystemPath);
            var file = Path.GetFileNameWithoutExtension(fileSystemPath);

            if (fanArtType == FanArtConstants.FanArtType.Poster || fanArtType == FanArtConstants.FanArtType.Thumbnail)
            {
              localPatterns.Add(Path.ChangeExtension(fileSystemPath, ".jpg"));
              localPatterns.Add(Path.Combine(path, file + "-poster.jpg"));
              localPatterns.Add(Path.Combine(path, "folder.jpg"));
            }
            if (fanArtType == FanArtConstants.FanArtType.FanArt)
            {
              localPatterns.Add(Path.Combine(path, "backdrop.jpg"));
              localPatterns.Add(Path.Combine(path, file + "-fanart*.jpg"));
              localPatterns.Add(Path.Combine(path, "ExtraFanArt\\*.jpg"));
            }

            // Copy all patterns for .jpg -> .png + .tbn
            localPatterns.AddRange(new List<string>(localPatterns).Select(p => p.Replace(".jpg", ".png")));
            localPatterns.AddRange(new List<string>(localPatterns).Select(p => p.Replace(".jpg", ".tbn")));

            foreach (var pattern in localPatterns)
            {
              try
              {
                var pathPart = Path.GetDirectoryName(pattern);
                var filePart = Path.GetFileName(pattern);
                DirectoryInfo directoryInfo = new DirectoryInfo(pathPart);
                if (directoryInfo.Exists)
                {
                  files.AddRange(directoryInfo.GetFiles(filePart)
                    .Select(f => f.FullName)
                    .Select(fileName => new ResourceLocator(resourceLocator.NativeSystemId, ResourcePath.BuildBaseProviderPath(resourceLocator.NativeResourcePath.LastPathSegment.ProviderId, fileName))));
                }
              }
              catch
              {
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        ServiceRegistration.Get<ILogger>().Warn("Error while search fanart of type '{0}' for path '{1}'", ex, fanArtType, fileSystemPath);
#endif
      }
      result = files;
      return files.Count > 0;
    }
  }
}
