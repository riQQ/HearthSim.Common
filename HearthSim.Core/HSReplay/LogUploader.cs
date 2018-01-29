#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HearthSim.Core.Util.EventArgs;
using HearthSim.Core.Util.Exceptions;
using HearthSim.Util.Logging;
using HSReplay;
using HSReplay.LogValidation;

#endregion

namespace HearthSim.Core.HSReplay
{
	public class LogUploader
	{
		private readonly ApiWrapper _api;

		internal LogUploader(ApiWrapper api)
		{
			_api = api;
		}

		private readonly List<UploaderItem> _inProgress = new List<UploaderItem>();

		public event Action<UploadCompleteEventArgs> UploadComplete;
		public event Action<UploadErrorEventArgs> UploadError;

		public async Task<UploadStatus> Upload(string[] logLines, UploadMetaData data)
		{
			var result = LogValidator.Validate(logLines);
			if(!result.IsValid)
			{
				UploadError?.Invoke(new UploadErrorEventArgs(result.Reason));
				return new UploadStatus(new InvalidLogException(result.Reason));
			}

			var log = string.Join(Environment.NewLine, logLines);
			var item = new UploaderItem(log.GetHashCode());
			if(_inProgress.Contains(item))
			{
				Log.Debug($"{item.Hash} already in progress. Waiting for it to complete...");
				_inProgress.Add(item);
				return await item.Status;
			}
			_inProgress.Add(item);
			Log.Debug($"Uploading {item.Hash}...");
			UploadStatus status;
			try
			{
				status = await TryUpload(logLines, data);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				status = new UploadStatus(ex);
			}
			Log.Debug($"{item.Hash} complete. Success={status.Success}");
			UploadComplete?.Invoke(new UploadCompleteEventArgs(data, status));
			foreach(var waiting in _inProgress.Where(x => x.Hash == item.Hash))
				waiting.Complete(status);
			_inProgress.RemoveAll(x => x.Hash == item.Hash);
			return status;
		}

		private async Task<UploadStatus> TryUpload(IEnumerable<string> logLines, UploadMetaData data)
		{
			try
			{
				var lines = logLines.SkipWhile(x => !x.Contains("CREATE_GAME")).ToArray();
				Log.Debug("Creating upload request...");
				var uploadRequest = await _api.CreateUploadRequest(data);
				Log.Debug("Upload Id: " + uploadRequest.ShortId);
				await _api.UploadLog(uploadRequest, lines);
				Log.Debug("Upload complete");
				return new UploadStatus(uploadRequest.ShortId, uploadRequest.ReplayUrl);
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				return new UploadStatus(ex);
			}
		}

		public async Task<UploadStatus> FromFile(string filePath)
		{
			string content;
			using(var sr = new StreamReader(filePath))
				content = sr.ReadToEnd();
			return await Upload(content.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToArray(),
				null);
		}

		public class UploadStatus
		{
			public UploadStatus(Exception exception)
			{
				Success = false;
				Exception = exception;
			}

			public UploadStatus(string shortId, string url)
			{
				Success = true;
				ShortId = shortId;
				ReplayUrl = url;
			}

			public bool Success { get; }
			public Exception Exception { get; }
			public string ShortId { get; }
			public string ReplayUrl { get; }
		}

		private class UploaderItem
		{
			private readonly TaskCompletionSource<UploadStatus> _tcs = new TaskCompletionSource<UploadStatus>();

			public UploaderItem(int hash)
			{
				Hash = hash;
			}

			public int Hash { get; }

			public Task<UploadStatus> Status => _tcs.Task;

			public override bool Equals(object obj)
			{
				var uObj = obj as UploaderItem;
				return uObj != null && Equals(uObj);
			}

			public override int GetHashCode() => Hash;

			private bool Equals(UploaderItem obj) => obj.Hash == Hash;

			public void Complete(UploadStatus result) => _tcs.SetResult(result);
		}
	}
}
