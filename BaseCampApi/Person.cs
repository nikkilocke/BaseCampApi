using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseCampApi {
	/// <summary>
	/// People can optionally belong to a Company
	/// </summary>
	public class Company : ApiEntry {
		public int id;
		public string name;
	}

	/// <summary>
	/// For adding a new person to Basecamp
	/// </summary>
	public class NewPerson : ApiEntryBase {
		public string name;
		public string email_address;
		public string title;
		public string company_name;
	}

	/// <summary>
	/// Used when adding people to a project
	/// </summary>
	public class UpdateProjectUsersList {
		public int [] grant;
		public int [] revoke;
		public NewPerson [] create;
	}

	/// <summary>
	/// Result returned when adding people to a project
	/// </summary>
	public class UpdateProjectUsersResult : ApiEntry {
		public Person [] granted;
		public Person [] revoked;
	}

	public class Person : ApiEntry {
		public int id;
		public string attachable_sgid;
		public string name;
		public string email_address;
		public string personable_type;
		public string title;
		public string bio;
		public DateTime created_at;
		public DateTime updated_at;
		public bool admin;
		public bool owner;
		public string time_zone;
		public string avatar_url;
		public Company company;
		public bool client;

		static async public Task<ApiList<Person>> GetAllPeople(Api api) {
			return await api.GetAsync<ApiList<Person>>("people");
		}

		static async public Task<ApiList<Person>> GetPeopleOnProject(Api api, int projectId) {
			return await api.GetAsync<ApiList<Person>>(Api.Combine("projects", projectId, "people"));
		}

		/// <summary>
		/// Update who can access a project
		/// </summary>
		static async public Task<UpdateProjectUsersResult> UpdateProjectUsers(Api api, int projectId, UpdateProjectUsersList changes) {
			return await api.PutAsync<UpdateProjectUsersResult>(Api.Combine("projects", projectId, "people", "users"), null, changes);
		}

		static async public Task<ApiList<Person>> GetPingablePeople(Api api) {
			return await api.GetAsync<ApiList<Person>>(Api.Combine("circles", "people"));
		}

		static async public Task<Person> GetPerson(Api api, int id) {
			return await api.GetAsync<Person>(Api.Combine("people", id));
		}

		static async public Task<Person> GetMyProfile(Api api) {
			return await api.GetAsync<Person>(Api.Combine("my", "profile"));
		}

	}
}
