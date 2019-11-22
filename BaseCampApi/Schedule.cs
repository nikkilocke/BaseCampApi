using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BaseCampApi {
	public class Schedule : CreatedItem {
		public int position;
		public int entries_count;
		public string entries_url;

		async public static Task<Schedule> GetSchedule(Api api, long projectId, long scheduleId) {
			return await api.GetAsync<Schedule>(Api.Combine("buckets", projectId, "schedules", scheduleId));
		}

		async public Task<ApiList<ScheduleEntry>> GetScheduleEntries(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<ScheduleEntry>>(Api.UriToApi(entries_url), status == Status.active ? null : new { status });
		}
	}

	public class ScheduleEntry : RecordingWithComments {
		public Parent parent;
		public string description;
		public string summary;
		public bool all_day;
		public DateTime starts_at;
		public DateTime ends_at;
		public List<Person> participants;


		async public static Task<ApiList<ScheduleEntry>> GetScheduleEntries(Api api, long projectId, long scheduleId, Status status = Status.active) {
			return await api.GetAsync<ApiList<ScheduleEntry>>(Api.Combine("buckets", projectId, "schedules", scheduleId, "entries"),
				status == Status.active ? null : new { status });
		}

		async public static Task<ScheduleEntry> GetScheduleEntry(Api api, long projectId, long entryId) {
			return await api.GetAsync<ScheduleEntry>(Api.Combine("buckets", projectId, "schedule_entries", entryId));
		}

		async public static Task<ScheduleEntry> GetScheduleEntry(Api api, long projectId, long entryId, DateTime date) {
			return await api.GetAsync<ScheduleEntry>(Api.Combine("buckets", projectId, "schedule_entries", entryId,
				"occurrences", date.ToString("yyyyMMdd")));
		}

		async public static Task<ScheduleEntry> Create(Api api, long projectId, long scheduleId, string summary, DateTime starts_at, DateTime ends_at, 
			string description = null, long[] participants = null, bool all_day = true, bool notify = false) {
			return await api.PostAsync<ScheduleEntry>(Api.Combine("buckets", projectId, "schedules", scheduleId, "entries"), null,
				new {
					summary,
					starts_at = starts_at.ToString("o"),
					ends_at = ends_at.ToString("o"),
					description,
					participants,
					all_day,
					notify
				});
		}

		async public Task<ScheduleEntry> Update(Api api, string summary, DateTime starts_at, DateTime ends_at,
			string description, long[] participants, bool? all_day, bool? notify) {
			return await api.PutAsync<ScheduleEntry>(Api.Combine("buckets", bucket.id, "schedules", parent.id, "entries"), null,
				new {
					summary,
					starts_at = starts_at.ToString("o"),
					ends_at = ends_at.ToString("o"),
					description,
					participants,
					all_day,
					notify
				});
		}

	}
}
