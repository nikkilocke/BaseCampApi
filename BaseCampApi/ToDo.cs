using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCampApi {
	public class ToDoSet : CreatedItem {
		public int position;
		public bool completed;
		public string completed_ratio;
		public string name;
		public int todolists_count;
		public string todolists_url;
		public string app_todoslists_url;

		async static public Task<ToDoSet> GetToDoSet(Api api, long projectId, long toDoSetId) {
			return await api.GetAsync<ToDoSet>(Api.Combine("buckets", projectId, "todosets", toDoSetId));
		}

		async public Task<ApiList<ToDoList>> GetAllToDoLists(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDoList>>(Api.UrlToApi(todolists_url), status == Status.active ? null : new { status });
		}

		async public Task<ToDoList> GetToDoList(Api api, long toDoListId) {
			return await ToDoList.GetToDoList(api, bucket.id, toDoListId);
		}

		async public Task<ToDoList> CreateToDoList(Api api, string name, string description) {
			return await ToDoList.Create(api, bucket.id, id, name, description);
		}

#if false
		async public Task<ToDoSet> Update(Api api, string name, string description) {
			return await api.PutAsync<ToDoSet>(Api.Combine("buckets", bucket.id, "todosets", id), null, new {
				name,
				description
			});
		}
#endif
	}

	public class ToDoList : RecordingWithComments {
		public int position;
		public Parent parent;
		public string description;
		public bool completed;
		public string completed_ratio;
		public string name;
		public string todos_url;
		public string groups_url;
		public string app_todos_url;

		async static public Task<ApiList<ToDoList>> GetAllToDoLists(Api api, long projectId, long toDoSetId, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDoList>>(Api.Combine("buckets", projectId, "todosets", toDoSetId, "todolists"),
				status == Status.active ? null : new { status });
		}

		async static public Task<ToDoList> GetToDoList(Api api, long projectId, long toDoListId) {
			return await api.GetAsync<ToDoList>(Api.Combine("buckets", projectId, "todolists", toDoListId));
		}

		async static public Task<ToDoList> Create(Api api, long projectId, long toDoSetId, string name, string description) {
			return await api.PostAsync<ToDoList>(Api.Combine("buckets", projectId, "todosets", toDoSetId, "todolists"), null, new {
				name,
				description
			});
		}

		async public Task<ToDoList> Update(Api api, string name, string description) {
			return await api.PutAsync<ToDoList>(Api.Combine("buckets", bucket.id, "todolists", id), null, new {
				name,
				description
			});
		}

		async public Task<ApiList<ToDoListGroup>> GetAllToDoListGroups(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDoListGroup>>(Api.UrlToApi(groups_url), status == Status.active ? null : new { status });
		}

		async public Task<ToDoListGroup> GetToDoListGroup(Api api, long toDoListGroupId) {
			return await ToDoListGroup.GetToDoListGroup(api, bucket.id, toDoListGroupId);
		}

		async public Task<ToDoListGroup> CreateGroup(Api api, string name) {
			return await ToDoListGroup.Create(api, bucket.id, id, name);
		}

		async public Task<ApiList<ToDo>> GetAllToDos(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDo>>(Api.UrlToApi(todos_url), status == Status.active ? null : new { status });
		}
	}

	public class ToDoListGroup : RecordingWithComments {
		public int position;
		public Parent parent;
		public string description;
		public bool completed;
		public string completed_ratio;
		public string name;
		public string todos_url;
		public string group_position_url;
		public string app_todos_url;

		async static public Task<ApiList<ToDoListGroup>> GetAllToDoListGroups(Api api, long projectId, long toDoListId, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDoListGroup>>(Api.Combine("buckets", projectId, "todolists", toDoListId, "groups"),
				status == Status.active ? null : new { status });
		}

		async static public Task<ToDoListGroup> GetToDoListGroup(Api api, long projectId, long toDoListGroupId) {
			return await api.GetAsync<ToDoListGroup>(Api.Combine("buckets", projectId, "todolists", toDoListGroupId));
		}

		async static public Task<ToDoListGroup> Create(Api api, long projectId, long toDoListId, string name) {
			return await api.PostAsync<ToDoListGroup>(Api.Combine("buckets", projectId, "todolists", toDoListId, "groups"), null, new {
				name
			});
		}

		async public Task Reposition(Api api, int newPosition) {
			await api.PutAsync(Api.UrlToApi(group_position_url), null, new {
				position
			});
		}

		/// <summary>
		/// Not documented in api
		/// </summary>
		async public Task<ToDoListGroup> Update(Api api, string name, string description) {
			return await api.PutAsync<ToDoListGroup>(Api.Combine("buckets", bucket.id, "todolists", id), null, new {
				name,
				description
			});
		}

		async public Task<ApiList<ToDo>> GetAllToDos(Api api, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDo>>(Api.UrlToApi(todos_url), status == Status.active ? null : new { status });
		}
	}

	public class ToDoData : ApiEntryBase {
		public string content;
		public long[] assignee_ids;
		public long[] completion_subscriber_ids;
		public bool? notify;
		public DateTime due_on;
		public DateTime starts_on;

		public JObject ToJObject() {
			JObject j = new JObject();
			if (!string.IsNullOrEmpty(content))
				j["content"] = content;
			if (assignee_ids != null)
				j["assignee_ids"] = new JArray(assignee_ids);
			if (completion_subscriber_ids != null)
				j["completion_subscriber_ids"] = new JArray(completion_subscriber_ids);
			if (notify != null)
				j["notify"] = notify;
			if (due_on != DateTime.MinValue)
				j["due_on"] = due_on;
			if (starts_on != DateTime.MinValue)
				j["starts_on"] = starts_on;
			return j;
		}
	}

	public class ToDo : RecordingWithComments {
		public int position;
		public Parent parent;
		public string description;
		public bool completed;
		public string content;
		public string starts_on;
		public string due_on;
		public List<Person> assignees;
		public List<Person> completion_subscribers;
		public string completion_url;

		async static public Task<ApiList<ToDo>> GetAllToDos(Api api, long projectId, long toDoListId, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDo>>(Api.Combine("buckets", projectId, "todolists", toDoListId),
				status == Status.active ? null : new { status });
		}

		async static public Task<ApiList<ToDo>> GetCompletedToDos(Api api, long projectId, long toDoListId, Status status = Status.active) {
			return await api.GetAsync<ApiList<ToDo>>(Api.Combine("buckets", projectId, "todolists", toDoListId),
				status == Status.active ? (object)new { completed = true } : (object)new { status, completed = true });
		}

		async static public Task<ToDo> GetToDo(Api api, long projectId, long toDoId) {
			return await api.GetAsync<ToDo>(Api.Combine("buckets", projectId, "todos", toDoId));
		}

		async static public Task<ToDo> CreateToDo(Api api, long projectId, long toDoListId, ToDoData data) {
			if (string.IsNullOrEmpty(data.content))
				throw new ApplicationException("Content not supplied");
			return await api.PostAsync<ToDo>(Api.Combine("buckets", projectId, "todolists", toDoListId, "todos"), null, data.ToJObject());
		}

		async public Task<ToDo> Update(Api api, ToDoData data) {
			return await api.PostAsync<ToDo>(Api.Combine("buckets", bucket.id, "todos", id, "todos"), null, data.ToJObject());
		}

		async public Task Complete(Api api) {
			await api.PostAsync(Api.UrlToApi(completion_url));
		}

		async public Task Uncomplete(Api api) {
			await api.DeleteAsync(Api.UrlToApi(completion_url));
		}

		async public Task Reposition(Api api, int newPosition) {
			await api.PutAsync(Api.Combine("buckets", bucket.id, "todos", id, "position"), null, new {
				position
			});
		}

	}
}
