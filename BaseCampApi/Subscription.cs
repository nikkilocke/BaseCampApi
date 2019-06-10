using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaseCampApi {

	public interface ISubscribable {
		string SubscriptionUrl { get; }
	}
	public class Subscription : ApiEntryBase {
		public bool subscribed;
		public int count;
		public string url;
		public Person[] subscribers;

		static async public Task<Subscription> GetSubscription(Api api, long projectId, long recordingId) {
			return await api.GetAsync<Subscription>(Api.Combine("buckets", projectId, "recordings", recordingId, "subscription"));
		}

		static async public Task<Subscription> GetSubscription(Api api, ISubscribable item) {
			return await api.GetAsync<Subscription>(item.SubscriptionUrl);
		}

		static async public Task<Subscription> SubscribeMe(Api api, long projectId, long recordingId) {
			return await api.PostAsync<Subscription>(Api.Combine("buckets", projectId, "recordings", recordingId, "subscription"));
		}

		static async public Task<Subscription> SubscribeMe(Api api, ISubscribable item) {
			return await api.PostAsync<Subscription>(item.SubscriptionUrl);
		}

		static async public Task UnsubscribeMe(Api api, long projectId, long recordingId) {
			await api.DeleteAsync<Subscription>(Api.Combine("buckets", projectId, "recordings", recordingId, "subscription"));
		}

		static async public Task UnsubscribeMe(Api api, ISubscribable item) {
			await api.DeleteAsync<Subscription>(item.SubscriptionUrl);
		}

		static async public Task<Subscription> Update(Api api, long projectId, long recordingId, UpdateSubscriptionsList updates) {
			return await api.PutAsync<Subscription>(Api.Combine("buckets", projectId, "recordings", recordingId, "subscription"), null, updates);
		}

		static async public Task<Subscription> Update(Api api, ISubscribable item, UpdateSubscriptionsList updates) {
			return await api.PutAsync<Subscription>(item.SubscriptionUrl, null, updates);
		}

	}

	/// <summary>
	/// Used when adding and removing subscriptions
	/// </summary>
	public class UpdateSubscriptionsList {
		public long[] subscriptions;
		public long[] unsubscriptions;
	}


}
