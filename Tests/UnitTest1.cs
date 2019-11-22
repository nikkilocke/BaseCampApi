using Microsoft.VisualStudio.TestTools.UnitTesting;
using BaseCampApi;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace Tests {
	public class Settings : BaseCampApi.Settings {
		public bool LoginTests = false;
		public bool ModifyTests = true;
		public bool DestructiveTests = false;
		public long LoginUser;
		public long TestProject;
		public long TestUser;
		public long TestUser2;
		public long TestCampfire;
		public long TestCampfireLine;
		public long TestMessageBoard;
		public long TestMessage;
		public long TestComment;
		public long TestVault;
		public long TestDocument;
		public long TestUpload;
		public long TestMessageType;
		public long AnnouncementMessageType;
		public long TestSchedule;
		public long TestEvent;
		public long TestToDoSet;
		public long TestToDoList;
		public long TestToDoListGroup;
		public long TestQuestionnaire;
		public long TestQuestion;
		public long TestAnswer;
		public override List<string> Validate() {
			List<string> errors = base.Validate();
			if (LoginUser == 0) errors.Add("LoginUser missing");
			if (TestProject == 0) errors.Add("TestProject missing");
			if (TestUser == 0) errors.Add("TestUser missing");
			if (TestUser2 == 0) errors.Add("TestUser2 missing");
			if (TestCampfire == 0) errors.Add("TestCampfire missing");
			if (TestCampfireLine == 0) errors.Add("TestCampfireLine missing");
			if (TestMessageBoard == 0) errors.Add("TestMessageBoard missing");
			if (TestMessage == 0) errors.Add("TestMessage missing");
			if (TestComment == 0) errors.Add("TestComment missing");
			if (TestVault == 0) errors.Add("TestVault missing");
			if (TestDocument == 0) errors.Add("TestDocument missing");
			if (TestUpload == 0) errors.Add("TestUpload missing");
			if (TestMessageType == 0) errors.Add("TestMessageType missing");
			if (AnnouncementMessageType == 0) errors.Add("AnnouncementMessageType missing");
			if (TestSchedule == 0) errors.Add("TestSchedule missing");
			if (TestEvent == 0) errors.Add("TestEvent missing");
			if (TestToDoSet == 0) errors.Add("TestToDoSet missing");
			if (TestToDoList == 0) errors.Add("TestToDoList missing");
			if (TestToDoListGroup == 0) errors.Add("TestToDoListGroup missing");
			if (TestQuestionnaire == 0) errors.Add("TestQuestionnaire missing");
			if (TestQuestion == 0) errors.Add("TestQuestion missing");
			if (TestAnswer == 0) errors.Add("TestAnswer missing");
			return errors;
		}
	}

	public class TestBase {
		static Settings _settings;
		static Api _api;

		public static Api Api {
			get {
				if (_api == null) {
					_api = new Api(Settings);
				}
				return _api;
			}
		}

		public static Settings Settings {
			get {
				if(_settings == null) {
					string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BaseCampApi");
					Directory.CreateDirectory(dataPath);
					string filename = Path.Combine(dataPath, "TestSettings.json");
					_settings = new Settings();
					_settings.Load(filename);
					List<string> errors = _settings.Validate();
					if (errors.Count > 0)
						throw new ApplicationException(string.Join("\r\n", errors));
				}
				return _settings;
			}
		}


		public static T RunTest<T>(Task<T> task) {
			T t = task.Result;
			Console.WriteLine(t);
			return t;
		}

		public static void RunTest(Task task) {
			task.Wait();
		}

	}
	[TestClass]
	public class LoginTests : TestBase {
		[TestMethod]
		public void LoginMethod() {
			if (!Settings.LoginTests)
				return;
			RunTest(Api.LoginAsync());
		}
	}
	[TestClass]
	public class PeopleTests : TestBase {
		[TestMethod]
		public void AuthorizationMethod() {
			Authorization a = RunTest(Api.GetAuthorization());
			Assert.IsTrue(a.AuthorisedFor(Settings.CompanyId));
		}
		[TestMethod]
		public void GetAllPeople() {
			ApiList<Person> people = RunTest(Person.GetAllPeople(Api));
			List<Person> all = people.All(Api).ToList();
			Assert.IsTrue(all.Any(p => p.id == Settings.TestUser));
			Assert.IsTrue(all.Any(p => p.id == Settings.TestUser2));
		}
		[TestMethod]
		public void GetPingablePeople() {
			ApiList<Person> people = RunTest(Person.GetPingablePeople(Api));
			Person psn = people.All(Api).FirstOrDefault(p => p.id == Settings.TestUser);
			psn = people.All(Api).FirstOrDefault(p => p.id == Settings.TestUser2);
			List<Person> all = people.All(Api).ToList();
			Assert.IsTrue(all.Any(p => p.id == Settings.TestUser));
			Assert.IsTrue(all.Any(p => p.id == Settings.TestUser2));
		}
		[TestMethod]
		public void GetMyProfile() {
			Person p = RunTest(Person.GetMyProfile(Api));
			Assert.AreEqual(p.id, Settings.LoginUser);
		}
		[TestMethod]
		public void GetPerson() {
			Person p = RunTest(Person.GetPerson(Api, Settings.TestUser));
			Assert.AreEqual(Settings.TestUser, p.id);
		}
	}

	[TestClass]
	public class ProjectTests : TestBase {
		[TestMethod]
		public void GetAllProjects() {
			ApiList<Project> projects = RunTest(Project.GetAllProjects(Api));
			Assert.IsTrue(projects.All(Api).Any(p => p.id == Settings.TestProject));
		}
		[TestMethod]
		public void GetArchivedProjects() {
			ApiList<Project> projects = RunTest(Project.GetAllProjects(Api, Status.archived));
		}
		[TestMethod]
		public void GetTrashedProjects() {
			ApiList<Project> projects = RunTest(Project.GetAllProjects(Api, Status.trashed));
		}
		[TestMethod]
		public void GetProject() {
			Project p = RunTest(Project.GetProject(Api, Settings.TestProject));
			Assert.IsTrue(p.id == Settings.TestProject);
		}
		[TestMethod]
		public void GetPeopleOnProject() {
			ApiList<Person> people = RunTest(Person.GetPeopleOnProject(Api, Settings.TestProject));
			List<Person> all = people.All(Api).ToList();
			Assert.IsTrue(all.Any(p => p.id == Settings.LoginUser));
			Assert.IsTrue(all.Any(p => p.id == Settings.TestUser));
		}
		[TestMethod]
		public void CreateProjectAndTrash() {
			if (!Settings.DestructiveTests)
				return;
			Project p = Project.CreateProject(Api, "Nikki Test 2").Result;
			RunTest(p.Trash(Api));
			ApiList<Project> projects = RunTest(Project.GetAllProjects(Api, Status.trashed));
			Assert.IsTrue(projects.All(Api).Any(t => t.id == p.id));
		}
		[TestMethod]
		public void UpdateProject() {
			if (!Settings.ModifyTests)
				return;
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			string description = "Test altered description 1";
			Project changed = p.Update(Api, p.name, description).Result;
			Assert.AreEqual(description, changed.description);
			description = p.description == description ? "Test original description" : p.description;
			changed = p.Update(Api, p.name, description).Result;
			Assert.AreEqual(description, changed.description);
		}
		[TestMethod]
		public void UpdateProjectUsers() {
			if (!Settings.ModifyTests)
				return;
			ApiList<Person> people = Person.GetPeopleOnProject(Api, Settings.TestProject).Result;
			if(people.All(Api).Any(p => p.id == Settings.TestUser2)) {
				Person.UpdateProjectUsers(Api, Settings.TestProject, new UpdateProjectUsersList() {
					revoke = new long[] { Settings.TestUser2 }
				}).Wait();
			}
			UpdateProjectUsersResult result = RunTest(Person.UpdateProjectUsers(Api, Settings.TestProject, new UpdateProjectUsersList() {
				grant = new long[] { Settings.TestUser2 }
			}));
			Assert.IsTrue(result.granted.Length == 0 || result.granted[0].id == Settings.TestUser2);
			people = Person.GetPeopleOnProject(Api, Settings.TestProject).Result;
			Assert.IsTrue(people.All(Api).Any(p => p.id == Settings.TestUser2));
			result = RunTest(Person.UpdateProjectUsers(Api, Settings.TestProject, new UpdateProjectUsersList() {
				revoke = new long[] { Settings.TestUser2 }
			}));
			Assert.AreEqual(Settings.TestUser2, result.revoked[0].id);
			people = Person.GetPeopleOnProject(Api, Settings.TestProject).Result;
			Assert.IsFalse(people.All(Api).Any(p => p.id == Settings.TestUser2));
		}
	}

	[TestClass]
	public class CampfireTests : TestBase {
		[TestMethod]
		public void GetAllCampfires() {
			ApiList<Campfire> campfires = RunTest(Campfire.GetAllCampfires(Api));
			Assert.IsTrue(campfires.All(Api).Any(c => c.id == Settings.TestCampfire));
		}
		[TestMethod]
		public void GetCampfireFromProject() {
			Project p = Project.GetProject(Api, TestBase.Settings.TestProject).Result;
			Campfire c = RunTest(p.GetCampfire(Api));
			Assert.AreEqual(Settings.TestCampfire, c.id);
		}
		[TestMethod]
		public void GetCampfire() {
			Campfire c = RunTest(Campfire.GetCampfire(Api, Settings.TestProject, Settings.TestCampfire));
			Assert.AreEqual(Settings.TestCampfire, c.id);
		}
		[TestMethod]
		public void GetLines() {
			Campfire c = Campfire.GetCampfire(Api, Settings.TestProject, Settings.TestCampfire).Result;
			ApiList<CampfireLine> lines = RunTest(c.GetLines(Api));
			Assert.IsTrue(lines.All(Api).Any(l => l.id == Settings.TestCampfireLine));
		}
		[TestMethod]
		public void GetLineFromProject() {
			Campfire c = Campfire.GetCampfire(Api, Settings.TestProject, Settings.TestCampfire).Result;
			CampfireLine l = RunTest(c.GetLine(Api, Settings.TestCampfireLine));
			Assert.AreEqual(Settings.TestCampfireLine, l.id);
		}
		[TestMethod]
		public void GetLine() {
			CampfireLine l = RunTest(CampfireLine.GetLine(Api, Settings.TestProject, Settings.TestCampfire, Settings.TestCampfireLine));
			Assert.AreEqual(Settings.TestCampfireLine, l.id);
		}
		[TestMethod]
		public void CreateAndDeleteLine() {
			if (!Settings.ModifyTests)
				return;
			Campfire c = Campfire.GetCampfire(Api, Settings.TestProject, Settings.TestCampfire).Result;
			CampfireLine l = c.CreateLine(Api, "Test Campfire Line").Result;
			ApiList<CampfireLine> lines = RunTest(c.GetLines(Api));
			Assert.IsTrue(lines.All(Api).Any(ln => ln.id == l.id));
			RunTest(l.Delete(Api));
			lines = RunTest(c.GetLines(Api));
			Assert.IsFalse(lines.All(Api).Any(ln => ln.id == l.id));
		}
		[TestMethod]
		public void GetSubscriptions() {
			Campfire c = Campfire.GetCampfire(Api, Settings.TestProject, Settings.TestCampfire).Result;
			RunTest(Subscription.GetSubscription(Api, c));
		}
	}

	[TestClass]
	public class MessageTests : TestBase {
		[TestMethod]
		public void GetMessageBoardFromProject() {
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			MessageBoard b = RunTest(p.GetMessageBoard(Api));
			Assert.AreEqual(Settings.TestMessageBoard, b.id);
		}
		[TestMethod]
		public void GetMessageBoard() {
			MessageBoard b = RunTest(MessageBoard.GetMessageBoard(Api, Settings.TestProject, Settings.TestMessageBoard));
			Assert.AreEqual(Settings.TestMessageBoard, b.id);
		}
		[TestMethod]
		public void GetMessages() {
			MessageBoard b = MessageBoard.GetMessageBoard(Api, Settings.TestProject, Settings.TestMessageBoard).Result;
			ApiList<Message> messages = RunTest(b.GetMessages(Api));
			Assert.IsTrue(messages.All(Api).Any(m => m.id == Settings.TestMessage));
		}
		[TestMethod]
		public void GetMessageFromMessageBoard() {
			MessageBoard b = MessageBoard.GetMessageBoard(Api, Settings.TestProject, Settings.TestMessageBoard).Result;
			Message m = RunTest(b.GetMessage(Api, Settings.TestMessage));
			Assert.AreEqual(Settings.TestMessage, m.id);
		}
		[TestMethod]
		public void GetMessage() {
			Message m = RunTest(Message.GetMessage(Api, Settings.TestProject, Settings.TestMessage));
			Assert.AreEqual(Settings.TestMessage, m.id);
		}
		[TestMethod]
		public void GetSubscriptions() {
			Message m = Message.GetMessage(Api, Settings.TestProject, Settings.TestMessage).Result;
			RunTest(Subscription.GetSubscription(Api, m));
		}
		[TestMethod]
		public void GetComments() {
			MessageBoard b = MessageBoard.GetMessageBoard(Api, Settings.TestProject, Settings.TestMessageBoard).Result;
			Message m = b.GetMessage(Api, Settings.TestMessage).Result;
			ApiList<Comment> comments = RunTest(m.GetComments(Api));
			Assert.IsTrue(comments.All(Api).Any(c => c.id == Settings.TestComment));
		}
		[TestMethod]
		public void GetCommentFromMessage() {
			Message m = Message.GetMessage(Api, Settings.TestProject, Settings.TestMessage).Result;
			Comment c = RunTest(m.GetComment(Api, Settings.TestComment));
			Assert.AreEqual(Settings.TestComment, c.id);
		}
		[TestMethod]
		public void GetComment() {
			Comment c = RunTest(Comment.GetComment(Api, Settings.TestProject, Settings.TestComment));
			Assert.AreEqual(Settings.TestComment, c.id);
		}
		[TestMethod]
		public void CreateComment() {
			if (!Settings.ModifyTests)
				return;
			Message m = Message.GetMessage(Api, Settings.TestProject, Settings.TestMessage).Result;
			Comment c = RunTest(m.CreateComment(Api, "Test new comment " + (m.comments_count + 1)));
			RunTest(c.SetStatus(Api, Status.trashed));
		}
		[TestMethod]
		public void UpdateComment() {
			Comment c = Comment.GetComment(Api, Settings.TestProject, Settings.TestComment).Result;
			string comment = "Modified comment";
			Comment changed = c.Update(Api, comment).Result;
			Assert.AreEqual(comment, changed.content);
			changed = changed.Update(Api, c.content).Result;
			Assert.AreEqual(c.content, changed.content);
		}
		[TestMethod]
		public void GetMessageTypes() {
			ApiList<MessageType> messageTypes = RunTest(MessageType.GetMessageTypes(Api, Settings.TestProject));
			Assert.IsTrue(messageTypes.All(Api).Any(m => m.id == Settings.AnnouncementMessageType));
;		}
		[TestMethod]
		public void GetMessageType() {
			RunTest(MessageType.GetMessageType(Api, Settings.TestProject, Settings.TestMessageType));
		}
		[TestMethod]
		public void CreateModifyDeleteMessageType() {
			if (!Settings.ModifyTests)
				return;
			string name = "Test message type";
			string icon = "t";
			string name2 = "Altered message type";
			string icon2 = "a";
			ApiList<MessageType> messages = RunTest(MessageType.GetMessageTypes(Api, Settings.TestProject));
			List<MessageType> all = messages.All(Api).ToList();
			MessageType d = all.FirstOrDefault(x => x.name == name);
			if(d != null) {
				d.Destroy(Api, Settings.TestProject).Wait();
			}
			d = all.FirstOrDefault(x => x.name == name2);
			if (d != null) {
				d.Destroy(Api, Settings.TestProject).Wait();
			}
			MessageType m = MessageType.Create(Api, Settings.TestProject, name, icon).Result;
			Assert.AreEqual(name, m.name);
			Assert.AreEqual(icon, m.icon);
			MessageType changed = m.Update(Api, Settings.TestProject, name2, icon2).Result;
			Assert.AreEqual(name2, changed.name);
			Assert.AreEqual(icon2, changed.icon);
			changed.Destroy(Api, Settings.TestProject).Wait();
		}
	}

	[TestClass]
	public class VaultTests : TestBase {
		[TestMethod]
		public void GetVaultFromProject() {
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			Vault v = RunTest(p.GetVault(Api));
			Assert.AreEqual(Settings.TestVault, v.id);
		}
		[TestMethod]
		public void GetVault() {
			Vault v = RunTest(Vault.GetVault(Api, Settings.TestProject, Settings.TestVault));
			Assert.AreEqual(Settings.TestVault, v.id);
		}
		[TestMethod]
		public void GetSubVaults() {
			Vault v = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			ApiList<Vault> s = RunTest(v.GetVaults(Api));
			//Assert.IsTrue(s.All(Api).Any(m => m.id == Settings.TestMessage));
		}
		[TestMethod]
		public void CreateVault() {
			if (!Settings.ModifyTests)
				return;
			Vault v = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			string name = "Test vault " + (v.vaults_count + 1);
			Vault s = RunTest(v.CreateVault(Api, name));
			Assert.AreEqual(name, s.title);
			Assert.AreEqual(v.id, s.parent.id);
			s.SetStatus(Api, Status.trashed).Wait();
		}
		[TestMethod]
		public void UpdateVault() {
			if (!Settings.ModifyTests)
				return;
			Vault v = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			string title = "Changed vault title";
			Vault changed = RunTest(v.Update(Api, title));
			Assert.AreEqual(title, changed.title);
			Vault t = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			Assert.AreEqual(changed.title, t.title);
			t = v.Update(Api, v.title).Result;
			Assert.AreEqual(v.title, t.title);
		}
		[TestMethod]
		public void CreateDocument() {
			if (!Settings.ModifyTests)
				return;
			Vault v = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			string name = "Test document " + (v.documents_count + 1);
			string content = "<b>" + name + "</b>";
			Document d = RunTest(v.CreateDocument(Api, name, content));
			Assert.AreEqual(name, d.title);
			Assert.AreEqual(content, d.content);
			Assert.AreEqual(v.id, d.parent.id);
			d.SetStatus(Api, Status.trashed).Wait();
		}
		[TestMethod]
		public void UpdateDocument() {
			if (!Settings.ModifyTests)
				return;
			Document d = Document.GetDocument(Api, Settings.TestProject, Settings.TestDocument).Result;
			string title = "Changed document title";
			string content = "Changed document content";
			Document changed = RunTest(d.Update(Api, title, content));
			Assert.AreEqual(changed.title, title);
			Assert.AreEqual(changed.content, content);
			Document t = Document.GetDocument(Api, Settings.TestProject, Settings.TestDocument).Result;
			Assert.AreEqual(changed.title, t.title);
			Assert.AreEqual(changed.content, t.content);
			t = d.Update(Api, d.title, d.content).Result;
			Assert.AreEqual(t.title, d.title);
			Assert.AreEqual(t.content, d.content);
		}
		[TestMethod]
		public void GetDocumentComments() {
			Document i = Document.GetDocument(Api, Settings.TestProject, Settings.TestDocument).Result;
			ApiList<Comment> comments = RunTest(i.GetComments(Api));
			Assert.AreEqual(i.comments_count, comments.TotalCount);
		}
		[TestMethod]
		public void GetDocumentSubscriptions() {
			Document i = Document.GetDocument(Api, Settings.TestProject, Settings.TestDocument).Result;
			RunTest(Subscription.GetSubscription(Api, i));
		}
		string GetFile() {
			string filename = "test.txt";
			if(!File.Exists(filename)) {
				using (FileStream f = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
					f.Write(Encoding.ASCII.GetBytes("This is a test text file\r\n"));
			}
			return filename;
		}
		[TestMethod]
		public void CreateUpload() {
			if (!Settings.ModifyTests)
				return;
			Vault v = Vault.GetVault(Api, Settings.TestProject, Settings.TestVault).Result;
			string file = GetFile();
			string description = "Test upload " + (v.uploads_count + 1);
			Upload u = RunTest(v.CreateUpload(Api, file, description));
			Assert.AreEqual(file, u.filename);
			Assert.AreEqual(description, u.description);
			Assert.AreEqual(v.id, u.parent.id);
			u.SetStatus(Api, Status.trashed).Wait();
		}
		[TestMethod]
		public void UpdateUpload() {
			if (!Settings.ModifyTests)
				return;
			Upload d = Upload.GetUpload(Api, Settings.TestProject, Settings.TestUpload).Result;
			string description = "Changed upload description";
			string basename = "changed";
			Upload changed = RunTest(d.Update(Api, description, basename));
			Assert.AreEqual(description, changed.description);
			Assert.AreEqual(basename, Path.GetFileNameWithoutExtension(changed.filename));
			Upload t = Upload.GetUpload(Api, Settings.TestProject, Settings.TestUpload).Result;
			Assert.AreEqual(changed.description, t.description);
			Assert.AreEqual(changed.filename, t.filename);
			t = d.Update(Api, d.description, Path.GetFileNameWithoutExtension(d.filename)).Result;
			Assert.AreEqual(d.description, t.description);
			Assert.AreEqual(d.filename, t.filename);
		}
		[TestMethod]
		public void GetUploadSubscriptions() {
			Upload d = Upload.GetUpload(Api, Settings.TestProject, Settings.TestUpload).Result;
			RunTest(Subscription.GetSubscription(Api, d));
		}
		[TestMethod]
		public void GetUploadComments() {
			Upload i = Upload.GetUpload(Api, Settings.TestProject, Settings.TestUpload).Result;
			ApiList<Comment> comments = RunTest(i.GetComments(Api));
			Assert.AreEqual(i.comments_count, comments.TotalCount);
		}
	}
	[TestClass]
	public class ScheduleTests : TestBase {
		[TestMethod]
		public void GetScheduleFromProject() {
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			Schedule s = RunTest(p.GetSchedule(Api));
			Assert.AreEqual(Settings.TestSchedule, s.id);
		}
		[TestMethod]
		public void GetSchedule() {
			Schedule s = RunTest(Schedule.GetSchedule(Api, Settings.TestProject, Settings.TestSchedule));
			Assert.AreEqual(Settings.TestSchedule, s.id);
		}
		[TestMethod]
		public void GetScheduleEntriesFromSchedule() {
			Schedule s = Schedule.GetSchedule(Api, Settings.TestProject, Settings.TestSchedule).Result;
			ApiList<ScheduleEntry> entries = RunTest(s.GetScheduleEntries(Api));
			Assert.IsTrue(entries.All(Api).Any(e => e.id == Settings.TestEvent));
		}
		[TestMethod]
		public void GetScheduleEntries() {
			ApiList<ScheduleEntry> entries = RunTest(ScheduleEntry.GetScheduleEntries(Api, Settings.TestProject, Settings.TestSchedule));
			Assert.IsTrue(entries.All(Api).Any(e => e.id == Settings.TestEvent));
		}
		[TestMethod]
		public void GetScheduleEntry() {
			ScheduleEntry e = RunTest(ScheduleEntry.GetScheduleEntry(Api, Settings.TestProject, Settings.TestEvent));
			Assert.AreEqual(Settings.TestEvent, e.id);
		}
		[TestMethod]
		public void GetScheduleEntryForDate() {
			DateTime date = new DateTime(2019, 9, 7);
			ScheduleEntry e = RunTest(ScheduleEntry.GetScheduleEntry(Api, Settings.TestProject, Settings.TestEvent, date));
			Assert.AreEqual(Settings.TestEvent, e.id);
			Assert.AreEqual(date, e.starts_at.Date);
		}
		[TestMethod]
		public void GetSubscriptions() {
			ScheduleEntry e = ScheduleEntry.GetScheduleEntry(Api, Settings.TestProject, Settings.TestEvent).Result;
			RunTest(Subscription.GetSubscription(Api, e));
		}
		[TestMethod]
		public void GetScheduleEntryComments() {
			DateTime date = new DateTime(2019, 9, 7);
			ScheduleEntry e = ScheduleEntry.GetScheduleEntry(Api, Settings.TestProject, Settings.TestEvent, date).Result;
			ApiList<Comment> comments = RunTest(e.GetComments(Api));
			Assert.AreEqual(e.comments_count, comments.TotalCount);
		}
	}
	[TestClass]
	public class ToDoTests : TestBase {
		[TestMethod]
		public void GetToDoSetFromProject() {
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			ToDoSet s = RunTest(p.GetToDoSet(Api));
			Assert.AreEqual(Settings.TestToDoSet, s.id);
		}
		[TestMethod]
		public void GetToDoSet() {
			ToDoSet s = RunTest(ToDoSet.GetToDoSet(Api, Settings.TestProject, Settings.TestToDoSet));
			Assert.AreEqual(Settings.TestToDoSet, s.id);
		}
		[TestMethod]
		public void GetAllToDoLists() {
			ApiList<ToDoList> lists = RunTest(ToDoList.GetAllToDoLists(Api, Settings.TestProject, Settings.TestToDoSet));
			Assert.IsTrue(lists.All(Api).Any(l => l.id == Settings.TestToDoList));
		}
		[TestMethod]
		public void GetToDoList() {
			ToDoList list = RunTest(ToDoList.GetToDoList(Api, Settings.TestProject, Settings.TestToDoList));
			Assert.AreEqual(Settings.TestToDoList, list.id);
		}
		[TestMethod]
		public void CreateToDoList() {
			string name = "Test todolist add";
			string description = "Test todolist description";
			ToDoList list = RunTest(ToDoList.Create(Api, Settings.TestProject, Settings.TestToDoSet, name, description));
			Assert.AreEqual(name, list.name);
			Assert.AreEqual(description, list.description);
			list.SetStatus(Api, Status.trashed).Wait();
		}
		[TestMethod]
		public void UpdateToDoList() {
			string name = "Test todolist changed";
			string description = "Test todolist changed description";
			ToDoList list = ToDoList.GetToDoList(Api, Settings.TestProject, Settings.TestToDoList).Result;
			ToDoList changed = RunTest(list.Update(Api, name, description));
			Assert.AreEqual(name, changed.name);
			Assert.AreEqual(description, changed.description);
			changed = RunTest(changed.Update(Api, list.name, list.description));
			Assert.AreEqual(list.name, changed.name);
			Assert.AreEqual(list.description, changed.description);
		}
		[TestMethod]
		public void GetToDoListSubscriptions() {
			ToDoList list = ToDoList.GetToDoList(Api, Settings.TestProject, Settings.TestToDoList).Result;
			RunTest(Subscription.GetSubscription(Api, list));
		}
		[TestMethod]
		public void GetToDoListComments() {
			ToDoList list = ToDoList.GetToDoList(Api, Settings.TestProject, Settings.TestToDoList).Result;
			ApiList<Comment> comments = RunTest(list.GetComments(Api));
			Assert.AreEqual(list.comments_count, comments.TotalCount);
		}
		[TestMethod]
		public void GetToDoListToDos() {
			ToDoList list = ToDoList.GetToDoList(Api, Settings.TestProject, Settings.TestToDoList).Result;
			RunTest(list.GetAllToDos(Api));
		}
		[TestMethod]
		public void GetAllToDoListGroups() {
			ApiList<ToDoListGroup> groups = RunTest(ToDoListGroup.GetAllToDoListGroups(Api, Settings.TestProject, Settings.TestToDoList));
			Assert.IsTrue(groups.All(Api).Any(g => g.id == Settings.TestToDoListGroup));
		}
		[TestMethod]
		public void GetToDoListGroup() {
			ToDoListGroup group = RunTest(ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoListGroup));
			Assert.AreEqual(Settings.TestToDoListGroup, group.id);
		}
		[TestMethod]
		public void CreateToDoListGroup() {
			string name = "Test Todo Group";
			ToDoListGroup group = RunTest(ToDoListGroup.Create(Api, Settings.TestProject, Settings.TestToDoList, name));
			Assert.AreEqual(name, group.name);
			group.SetStatus(Api, Status.trashed).Wait();
		}
		[TestMethod]
		public void RepositionToDoListGroup() {
			ToDoListGroup group = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoListGroup).Result;
			RunTest(group.Reposition(Api, 1));
		}
		[TestMethod]
		public void UpdateToDoListGroup() {
			string name = "Test Todo Group changed";
			string description = "description changed";
			ToDoListGroup list = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoListGroup).Result;
			ToDoListGroup changed = RunTest(list.Update(Api, name, description));
			Assert.AreEqual(name, changed.name);
			Assert.AreEqual(description, changed.description);
			changed = RunTest(changed.Update(Api, list.name, list.description));
			Assert.AreEqual(list.name, changed.name);
			Assert.AreEqual(list.description, changed.description);
		}
		[TestMethod]
		public void GetToDoLisGroupSubscriptions() {
			ToDoListGroup list = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoListGroup).Result;
			RunTest(Subscription.GetSubscription(Api, list));
		}
		[TestMethod]
		public void GetToDoListGroupComments() {
			ToDoListGroup group = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoListGroup).Result;
			ApiList<Comment> comments = RunTest(group.GetComments(Api));
			Assert.AreEqual(group.comments_count, comments.TotalCount);
		}
		[TestMethod]
		public void GetToDoListGroupToDos() {
			ToDoListGroup list = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoList).Result;
			RunTest(list.GetAllToDos(Api));
		}
		[TestMethod]
		public void GetToDoSubscriptions() {
			ToDoListGroup group = ToDoListGroup.GetToDoListGroup(Api, Settings.TestProject, Settings.TestToDoList).Result;
			ApiList<ToDo> list = group.GetAllToDos(Api).Result;
			RunTest(Subscription.GetSubscription(Api, list.List[0]));
		}
	}
	[TestClass]
	public class QuestionTests : TestBase {
		[TestMethod]
		public void GetQuestionnaireFromProject() {
			Project p = Project.GetProject(Api, Settings.TestProject).Result;
			Questionnaire s = RunTest(p.GetQuestionnaire(Api));
			Assert.AreEqual(Settings.TestQuestionnaire, s.id);
		}
		[TestMethod]
		public void GetQuestionnaire() {
			Questionnaire s = RunTest(Questionnaire.GetQuestionnaire(Api, Settings.TestProject, Settings.TestQuestionnaire));
			Assert.AreEqual(Settings.TestQuestionnaire, s.id);
		}
		[TestMethod]
		public void GetAllQuestions() {
			ApiList<Question> list = RunTest(Question.GetAllQuestions(Api, Settings.TestProject, Settings.TestQuestionnaire));
			Assert.IsTrue(list.All(Api).Any(q => q.id == Settings.TestQuestion));
		}
		[TestMethod]
		public void GetQuestion() {
			Question q = RunTest(Question.GetQuestion(Api, Settings.TestProject, Settings.TestQuestion));
			Assert.AreEqual(Settings.TestQuestion, q.id);
		}
#if false
		[TestMethod]
		public void CreateQuestion() {
			string title = "Test Question";
			Question q = RunTest(Question.Create(Api, Settings.TestProject, Settings.TestQuestionnaire, title));
			Assert.AreEqual(title, q.title);
		}
#endif
		[TestMethod]
		public void GetAnswers() {
			ApiList<QuestionAnswer> answers = RunTest(QuestionAnswer.GetAllQuestionAnswers(Api, Settings.TestProject, Settings.TestQuestion));
			Assert.IsTrue(answers.All(Api).Any(a => a.id == Settings.TestAnswer));
		}
		[TestMethod]
		public void GetAnswer() {
			QuestionAnswer answer = RunTest(QuestionAnswer.GetQuestionAnswer(Api, Settings.TestProject, Settings.TestAnswer));
			Assert.AreEqual(Settings.TestAnswer, answer.id);
		}
		[TestMethod]
		public void GetSubscriptions() {
			QuestionAnswer answer = QuestionAnswer.GetQuestionAnswer(Api, Settings.TestProject, Settings.TestAnswer).Result;
			RunTest(Subscription.GetSubscription(Api, answer));
		}
		[TestMethod]
		public void GetAnswerComments() {
			QuestionAnswer answer = QuestionAnswer.GetQuestionAnswer(Api, Settings.TestProject, Settings.TestAnswer).Result;
			RunTest(answer.GetComments(Api));
		}
#if false
		[TestMethod]
		public void CreateAnswer() {
			string content = "Test answer";
			QuestionAnswer answer = RunTest(QuestionAnswer.Create(Api, Settings.TestProject, Settings.TestQuestion, content));
			Assert.AreEqual(content, answer.content);
		}
#endif
	}

}
