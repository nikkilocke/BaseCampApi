using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaseCampApi {
	public class Campfire : CreatedItem, ISubscribable{
		public bool visible_to_clients;
		public string subscription_url;
		public int position;
		public string topic;
		public string lines_url;

		public string SubscriptionUrl => subscription_url;

		static async public Task<ApiList<Campfire>> GetAllCampfires(Api api) {
			return await api.GetAsync<ApiList<Campfire>>("chats");
		}

		static async public Task<Campfire> GetCampfire(Api api, long projectId, long campfireId) {
			return await api.GetAsync<Campfire>(Api.Combine("buckets", projectId, "chats", campfireId));
		}

		async public Task<ApiList<CampfireLine>> GetLines(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<CampfireLine>>(Api.UriToApi(lines_url), status == Status.active ? null : new { status });
		}

		async public Task<CampfireLine> GetLine(Api api, long lineId) {
			return await CampfireLine.GetLine(api, bucket.id, id, lineId);
		}

		async public Task<CampfireLine> CreateLine(Api api, string content) {
			return await api.PostAsync<CampfireLine>(Api.Combine("buckets", bucket.id, "chats", id, "lines"), null, new {
				content
			});
		}

	}

	public class CampfireLine : Campfire {
		public class Attachment : ApiEntryBase {
			public string title;
			public string url;
		}

		public Parent parent;
		public string content;
		public List<Attachment> attachments;

		static async public Task<CampfireLine> GetLine(Api api, long projectId, long campfireId, long lineId) {
			return await api.GetAsync<CampfireLine>(Api.Combine("buckets", projectId, "chats", campfireId, "lines", lineId));
		}

		async public Task Delete(Api api) {
			await api.DeleteAsync(Api.Combine("buckets", bucket.id, "chats", parent.id, "lines", id));
		}

	}
}
