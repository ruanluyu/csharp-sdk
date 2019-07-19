﻿using System;
using System.Collections.Generic;

namespace LeanCloud.Storage.Internal {
  /// <summary>
  /// So here's the deal. We have a lot of internal APIs for AVObject, AVUser, etc.
  ///
  /// These cannot be 'internal' anymore if we are fully modularizing things out, because
  /// they are no longer a part of the same library, especially as we create things like
  /// Installation inside push library.
  ///
  /// So this class contains a bunch of extension methods that can live inside another
  /// namespace, which 'wrap' the intenral APIs that already exist.
  /// </summary>
  public static class AVFileExtensions {
    public static AVFile Create(string name, Uri uri, string mimeType = null) {
      return new AVFile(name, uri, mimeType);
    }
  }
}
