using System;
using System.IO;
using System.Collections.Generic;

namespace code_todo_to_taskjuggler
{		
	/*
	* 0 = TODO
	* 1 = task name
	* 2 = allocated
	* 3 = effort
	* 4 = priority
	* 5 = descr
	* */
	class TodoEntry
	{
		public string name;
		public string allocated;
		public string effort;
		public string priority;
		public string descr;
	} ;

	class MainClass
	{
		private static StreamWriter outfile;
		private static List<TodoEntry> TodoEntries;
		private static string[] wrapper;


		private static string[] GetAllFiles(string dir)
		{
			return Directory.GetFiles(dir,"*.*",SearchOption.AllDirectories);
		}

		private static void ShowArgs(string[] args)
		{
			Console.WriteLine ("Arguments");
			foreach (string s in args)
			{
				Console.WriteLine ("Arg: " + s);
			}
			Console.WriteLine ();
		}

		private static int CheckTodoLine(string s)
		{
			string temp = s.ToUpper ();

			if (temp.Contains ("TODO"))
			{
				return temp.IndexOf ("TODO");
			}
			else
			{
				return -1;
			}
		}
			
		private static void emittabs(int level)
		{
			for(int i=0; i<level; i++)
			{
				outfile.Write("\t");
			}
		}
		/*
		 * task gps "GPS Firmware" {
						journalentry 2015-06-19 "GPS critical for two projects now - complete by this weekend" {				
							alert yellow
							summary -8<-
								Finish GPS this weekend still
								->8-				
						}
						allocate aj
						effort 8h
						complete 0					
					}				
		 * */
		private static void WriteTaskJugglerTask(int level, string[] todofields, bool start, bool body, bool end)
		{
			String[] arguments = Environment.GetCommandLineArgs();

			if (start)
			{
				emittabs (level);
				outfile.Write ("task {0} \"{1}\" ", todofields [1], todofields [5].Trim ());
				outfile.WriteLine ("{");
			}

			if (body)
			{
				emittabs (level);
				outfile.WriteLine ("    priority {0}",arguments[3] + todofields[4]);
				emittabs (level);
				outfile.WriteLine ("    allocate {0}",todofields[2]);
				emittabs (level);
				outfile.WriteLine ("    effort {0}h",todofields[3]);
				emittabs (level);
				outfile.WriteLine ("    complete 0");
			}

			if (end)
			{
				emittabs (level);
				outfile.WriteLine ("}");
			}
		}

		/*
		 * TODO fields:
		 * 0 = TODO
		 * 1 = task name
		 * 2 = allocated
		 * 3 = effort
		 * 4 = priority
		 * 5 = descr
		 * */
		private static void ProcessFile(string f)
		{
			int linecnt = 1;
			string[] lines = System.IO.File.ReadAllLines (f);

			foreach (string line in lines)
			{
				int idx = CheckTodoLine (line);

				if (idx >= 0)
				{
					string temp = line.Substring (idx);
					string[] tempfields = temp.Split (new char[] { ':' },6);
					if (tempfields.GetLength (0) != 6)
					{
						Console.WriteLine (line);
						throw new Exception ("TODO entry not in correct format: line "+linecnt.ToString() + ", file: " + f);
					}
					Console.Write ("TODO ");
					Console.Write ("Task: {0}, ",tempfields[1]);
					Console.Write ("Allocated: {0}, ",tempfields[2]);
					Console.Write ("Effort: {0}, ",tempfields[3]);
					Console.Write ("Priority: {0}, ",tempfields[4]);
					Console.WriteLine(" Descr: {0}",tempfields[5]);

					WriteTaskJugglerTask (1,tempfields,true,true,true);

					TodoEntry tde = new TodoEntry ();
					tde.name = tempfields [1];
					tde.allocated = tempfields [2];
					tde.effort = tempfields [3];
					tde.priority = tempfields [4];
					tde.descr = tempfields [5];

					TodoEntries.Add (tde);
				}
				linecnt++;
			}
		}

		private static void ProcessFiles(string[] files)
		{
			DateTime dt = DateTime.Now;

			WriteTaskJugglerTask(0,wrapper,true,false,false);

			outfile.WriteLine ("\tstart " + 
				dt.Year.ToString() + "-" + 
				dt.Month.ToString() + "-" + 
				dt.Day.ToString());

			Console.WriteLine ("Files to check:");
			foreach (string s in files)
			{
				Console.WriteLine ("File: " + s);
				ProcessFile (s);
			}
			Console.WriteLine ();
			WriteTaskJugglerTask(0,wrapper,false,false,true);
		}

		private static void CheckDuplicateTaskIds()
		{
			TodoEntry[] arr = TodoEntries.ToArray ();
			int i, j;

			for (i = 0; i < (arr.GetLength (0)-1); i++)
			{
				for (j = i + 1; j < arr.GetLength (0); j++)
				{
					// check for duplicates
					if (arr [i].name == arr [j].name)
					{
						Console.WriteLine ("Duplicate Task ID: {0}", arr [i].name);
						throw new Exception ("Duplicate task ids found");
					}
				}
			}
		}

		private static void WriteTodoList(string fn)
		{
			using (StreamWriter s = new StreamWriter(fn))
			{
				foreach(TodoEntry t in TodoEntries)
				{
					s.Write ("TODO ");
					s.Write ("Task: {0}, ",t.name);
					s.Write ("Allocated: {0}, ",t.allocated);
					s.Write ("Effort: {0}, ",t.effort);
					s.Write ("Priority: {0}, ",t.priority);
					s.WriteLine(" Descr: {0}",t.descr);
				}
			}
		}

		public static void Main (string[] args)
		{
			try
			{
				ShowArgs (args);

				wrapper = new string[6] {
					"TODO",
					args[1],
					"",
					"",
					"",
					args[1]
				};

				if (args.GetLength (0) < 3)
				{
					Console.WriteLine ("Usage code_todo_to_taskjuggler <source directory> <projectname> <prioritybase>");
					return;
				}

				outfile = new StreamWriter(args[1]+".todo.tji");

				TodoEntries = new List<TodoEntry>(1000);

				string[] filelist = GetAllFiles (args[0]);
				ProcessFiles(filelist);
				CheckDuplicateTaskIds();
				outfile.Close();

				TodoEntries.Sort(delegate(TodoEntry c1, TodoEntry c2) { return -c1.priority.CompareTo(c2.priority); });
				WriteTodoList(args[1] + ".todo");

				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine ("Processed succesfully.");
				Console.ResetColor();
			}
			catch(Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				outfile.Close();
				Console.WriteLine ("An exception occured");
				Console.WriteLine (ex.ToString ());
				Console.ResetColor();
			}
		}
	}
}
