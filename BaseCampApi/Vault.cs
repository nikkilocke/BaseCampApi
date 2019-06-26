using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BaseCampApi {
	/// <summary>
	/// Basecamp Folder
	/// </summary>
	public class Vault : Recording {
		public Parent parent;
		public int position;
		public int documents_count;
		public string documents_url;
		public int uploads_count;
		public string uploads_url;
		public int vaults_count;
		public string vaults_url;

		static async public Task<ApiList<Vault>> GetVaults(Api api, int projectId, int vaultId) {
			return await api.GetAsync<ApiList<Vault>>(Api.Combine("buckets", projectId, "vaults", vaultId, "vaults"));
		}

		static async public Task<Vault> GetVault(Api api, int projectId, int vaultId) {
			return await api.GetAsync<Vault>(Api.Combine("buckets", projectId, "vaults", vaultId));
		}

		async public Task<ApiList<Vault>> GetVaults(Api api) {
			return await GetVaults(api, bucket.id, id);
		}

		async public Task<Vault> CreateVault(Api api, string title) {
			return await api.PostAsync<Vault>(Api.Combine("buckets", bucket.id, "vaults", id, "vaults"), null, new {
				title
			});
		}

		async public Task<Vault> Update(Api api, string title) {
			return await api.PutAsync<Vault>(Api.Combine("buckets", bucket.id, "vaults", id), null, new {
				title
			});
		}

		async public Task<ApiList<Document>> GetDocuments(Api api) {
			return await Document.GetDocuments(api, bucket.id, id);
		}

		async public Task<Document> GetDocument(Api api, int documentId) {
			return await Document.GetDocument(api, bucket.id, documentId);
		}

		async public Task<Document> CreateDocument(Api api, string title, string content, Status status = Status.active) {
			return await Document.CreateDocument(api, bucket.id, id, title, content, status);
		}

		async public Task<ApiList<Upload>> GetUploads(Api api) {
			return await Upload.GetUploads(api, bucket.id, id);
		}

		async public Task<Upload> GetUpload(Api api, int uploadId) {
			return await Upload.GetUpload(api, bucket.id, uploadId);
		}

		async public Task<Upload> CreateUpload(Api api, string file, string description, string basename = null) {
			return await Upload.CreateUpload(api, bucket.id, id, file, description, basename);
		}

	}

	public class Document : RecordingWithComments {
		public Parent parent;
		public int position;
		public string content;

		async public static Task<ApiList<Document>> GetDocuments(Api api, int projectId, int vaultId) {
			return await api.GetAsync<ApiList<Document>>(Api.Combine("buckets", projectId, "vaults", vaultId, "documents"));
		}

		async public static Task<Document> GetDocument(Api api, int projectId, int documentId) {
			return await api.GetAsync<Document>(Api.Combine("buckets", projectId, "documents", documentId));
		}

		async public static Task<Document> CreateDocument(Api api, int projectId, int vaultId, 
				string title, string content, Status status = Status.active) {
			return await api.PostAsync<Document>(Api.Combine("buckets", projectId, "vaults", vaultId, "documents"), null, new {
				title,
				content,
				status
			});
		}

		async public Task<Document> Update(Api api, string title, string content) {
			return await api.PutAsync<Document>(Api.Combine("buckets", bucket.id, "documents", id), null, new {
				title,
				content
			});
		}

	}

	/// <summary>
	/// Items like uploaded files, and embedded graphics are held in Attachments
	/// </summary>
	public class Attachment : ApiEntry {
		public string attachable_sgid;

		async public static Task<Attachment> CreateAttachment(Api api, string file) {
			string name = Path.GetFileName(file);
			using (FileStream f = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return await api.PostAsync<Attachment>("attachments", new { name }, f);
		}
	}

	public class Upload : RecordingWithComments {
		public int position;
		public Parent parent;
		public string description;
		public string filename;
		public string content_type;
		public int byte_size;
		public int width;
		public int height;
		public string download_url;
		public string app_download_url;

		async public static Task<ApiList<Upload>> GetUploads(Api api, int projectId, int vaultId) {
			return await api.GetAsync<ApiList<Upload>>(Api.Combine("buckets", projectId, "vaults", vaultId, "uploads"));
		}

		async public static Task<Upload> GetUpload(Api api, int projectId, int uploadId) {
			return await api.GetAsync<Upload>(Api.Combine("buckets", projectId, "uploads", uploadId));
		}

		async public static Task<Upload> CreateUpload(Api api, int projectId, int vaultId, string file, string description, string base_name = null) {
			Attachment a = await Attachment.CreateAttachment(api, file);
			if (base_name == null)
				base_name = file;
			base_name = Path.GetFileNameWithoutExtension(base_name);
			return await api.PostAsync<Upload>(Api.Combine("buckets", projectId, "vaults", vaultId, "uploads"), null,
				new {
					a.attachable_sgid,
					base_name,
					description
				});
		}

		async public Task<Upload> Update(Api api, string description, string base_name = null) {
			if (base_name == null)
				base_name = Path.GetFileNameWithoutExtension(filename);
			return await api.PutAsync<Upload>(Api.Combine("buckets", bucket.id, "uploads", id), null, new {
				base_name,
				description
			});
		}
	}
}
