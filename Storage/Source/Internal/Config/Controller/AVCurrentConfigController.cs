﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using LeanCloud.Storage.Internal;

namespace LeanCloud.Storage.Internal {
  /// <summary>
  /// LeanCloud current config controller.
  /// </summary>
  internal class AVCurrentConfigController : IAVCurrentConfigController {
    private const string CurrentConfigKey = "CurrentConfig";

    private readonly TaskQueue taskQueue;
    private AVConfig currentConfig;

    private IStorageController storageController;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeanCloud.Storage.Internal.AVCurrentConfigController"/> class.
    /// </summary>
    public AVCurrentConfigController(IStorageController storageController) {
      this.storageController = storageController;

      taskQueue = new TaskQueue();
    }

    public Task<AVConfig> GetCurrentConfigAsync() {
      return taskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => {
        if (currentConfig == null) {
          return storageController.LoadAsync().OnSuccess(t => {
            object tmp;
            t.Result.TryGetValue(CurrentConfigKey, out tmp);

            string propertiesString = tmp as string;
            if (propertiesString != null) {
              var dictionary = AVClient.DeserializeJsonString(propertiesString);
              currentConfig = new AVConfig(dictionary);
            } else {
              currentConfig = new AVConfig();
            }

            return currentConfig;
          });
        }

        return Task.FromResult(currentConfig);
      }), CancellationToken.None).Unwrap();
    }

    public Task SetCurrentConfigAsync(AVConfig config) {
      return taskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => {
        currentConfig = config;

        var jsonObject = ((IJsonConvertible)config).ToJSON();
        var jsonString = AVClient.SerializeJsonString(jsonObject);

        return storageController.LoadAsync().OnSuccess(t => t.Result.AddAsync(CurrentConfigKey, jsonString));
      }).Unwrap().Unwrap(), CancellationToken.None);
    }

    public Task ClearCurrentConfigAsync() {
      return taskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => {
        currentConfig = null;

        return storageController.LoadAsync().OnSuccess(t => t.Result.RemoveAsync(CurrentConfigKey));
      }).Unwrap().Unwrap(), CancellationToken.None);
    }

    public Task ClearCurrentConfigInMemoryAsync() {
      return taskQueue.Enqueue(toAwait => toAwait.ContinueWith(_ => {
        currentConfig = null;
      }), CancellationToken.None);
    }
  }
}
