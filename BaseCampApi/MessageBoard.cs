using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaseCampApi {
	public class MessageBoard : CreatedItem {
		public bool visible_to_clients;
		public int position;
		public int messages_count;
		public string messages_url;
		public string app_messages_url;

		static async public Task<MessageBoard> GetMessageBoard(Api api, int projectId, int messageBoardId) {
			return await api.GetAsync<MessageBoard>(Api.Combine("buckets", projectId, "message_boards", messageBoardId));
		}

		async public Task<ApiList<Message>> GetMessages(Api api, Status status = Status.active) {
			if (messages_count == 0)
				return ApiList<Message>.EmptyList(Api.UriToApi(messages_url));
			return await api.GetAsync<ApiList<Message>>(Api.UriToApi(messages_url), status == Status.active ? null : new { status });
		}

		async public Task<Message> GetMessage(Api api, int messageId) {
			return await Message.GetMessage(api, bucket.id, messageId);
		}

		async public Task<Message> CreateMessage(Api api, string subject, string content, int category_id = 0) {
			return await api.PostAsync<Message>(Api.Combine("buckets", bucket.id, "messages"), null, new {
				subject,
				content,
				category_id
			});
		}

	}

	public class MessageType : ApiEntry {
		public int id;
		public string name;
		public string icon;
		public DateTime created_at;
		public DateTime updated_at;

		async public static Task<ApiList<MessageType>> GetMessageTypes(Api api, int projectId) {
			return await api.GetAsync<ApiList<MessageType>>(Api.Combine("buckets", projectId, "categories"));
		}

		async public static Task<MessageType> GetMessageType(Api api, int projectId, int messageTypeId) {
			return await api.GetAsync<MessageType>(Api.Combine("buckets", projectId, "categories", messageTypeId));
		}

		async public static Task<MessageType> Create(Api api, int projectId, string name, string icon) {
			return await api.PostAsync<MessageType>(Api.Combine("buckets", projectId, "categories"), null, new {
				name,
				icon
			});
		}

		async public Task<MessageType> Update(Api api, int projectId, string name, string icon) {
			return await api.PutAsync<MessageType>(Api.Combine("buckets", projectId, "categories", id), null, new {
				name,
				icon
			});
		}

		async public Task Destroy(Api api, int projectId) {
			await api.DeleteAsync(Api.Combine("buckets", projectId, "categories", id));
		}
	}

	public class Message : RecordingWithComments {
		public Parent parent;
		public MessageType category;
		public string content;
		public string subject;

		static async public Task<ApiList<Message>> GetAllMessages(Api api, int projectId, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc) {
			return await GetAllRecordings<Message>(api, RecordingType.Message, projectId, status, sort, direction);
		}

		static async public Task<ApiList<Message>> GetAllMessages(Api api, int[] projectIds = null, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc) {
			return await GetAllRecordings<Message>(api, RecordingType.Message, projectIds, status, sort, direction);
		}

		static async public Task<Message> GetMessage(Api api, int projectId, int messageId) {
			return await api.GetAsync<Message>(Api.Combine("buckets", projectId, "messages", messageId));
		}

		async public Task<Message> Update(Api api) {
			return await api.PutAsync<Message>(Api.Combine("buckets", bucket.id, "messages", id), null, new {
				subject,
				content,
				category_id = category.id
			});
		}

		async public Task<Comment> GetComment(Api api, int commentId) {
			return await Comment.GetComment(api, bucket.id, commentId);
		}

		async public Task<Comment> CreateComment(Api api, string content) {
			return await api.PostAsync<Comment>(Api.Combine("buckets", bucket.id, "recordings", id, "comments"), null, new {
				content
			});
		}

	}

	public class Comment : Recording {
		public Parent parent;
		public string content;

		static async public Task<ApiList<Comment>> GetAllComments(Api api, int projectId, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc) {
			return await GetAllRecordings<Comment>(api, RecordingType.Comment, projectId, status, sort, direction);
		}

		static async public Task<ApiList<Comment>> GetAllComments(Api api, int[] projectIds = null, Status status = Status.active, DateSort sort = DateSort.created_at, SortDirection direction = SortDirection.desc) {
			return await GetAllRecordings<Comment>(api, RecordingType.Comment, projectIds, status, sort, direction);
		}

		static async public Task<Comment> GetComment(Api api, int projectId, int commentId) {
			return await api.GetAsync<Comment>(Api.Combine("buckets", projectId, "comments", commentId));
		}

		async public Task<Comment> Update(Api api, string content) {
			return await api.PutAsync<Comment>(Api.Combine("buckets", bucket.id, "comments", id), null, new {
				content
			});
		}

	}

}
