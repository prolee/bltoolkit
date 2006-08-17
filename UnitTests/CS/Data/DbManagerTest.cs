using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Xml;
using NUnit.Framework;

using BLToolkit.Data;
using BLToolkit.EditableObjects;
using BLToolkit.Mapping;
using BLToolkit.Reflection;

namespace Data
{
	[TestFixture]
	public class DbManagerTest
	{
		public enum Gender
		{
			[MapValue("F")] Female,
			[MapValue("M")] Male,
			[MapValue("U")] Unknown,
			[MapValue("O")] Other
		}

		public class Person
		{
			[MapField("PersonID")]
			public int    ID;
			public string FirstName;
			public string MiddleName;
			public string LastName;
			public Gender Gender;
		}

		public class DataTypeTest
		{
			[MapField("DataTypeID")]
			public int       ID;
			[MapIgnore(false)]
			public Byte[]    Binary_;
			public Boolean   Boolean_;
			public Byte      Byte_;
			[MapIgnore(false)]
			public Byte[]    Bytes_;
			public Char      Char_;
			public DateTime  DateTime_;
			public Decimal   Decimal_;
			public Double    Double_;
			public Guid      Guid_;
			public Int16     Int16_;
			public Int32     Int32_;
			public Int64     Int64_;
			public Decimal   Money_;
			public SByte     SByte_;
			public Single    Single_;
			public Stream    Stream_;
			public String    String_;
			public UInt16    UInt16_;
			public UInt32    UInt32_;
			public UInt64    UInt64_;
#if FW2
			[MapIgnore(false)]
			public XmlReader Xml_;
#endif
		}

		public class DataTypeSqlTest
		{
			[MapField("DataTypeID")]
			public int ID;
			public SqlBinary   Binary_;
			public SqlBoolean  Boolean_;
			public SqlByte     Byte_;
			public SqlDateTime DateTime_;
			public SqlDecimal  Decimal_;
			public SqlDouble   Double_;
			public SqlGuid     Guid_;
			public SqlInt16    Int16_;
			public SqlInt32    Int32_;
			public SqlInt64    Int64_;
			public SqlMoney    Money_;
			public SqlSingle   Single_;
			public SqlString   String_;
#if FW2
			[MapIgnore(false)]
			public SqlBytes    Bytes_;
			[MapIgnore(false)]
			public SqlChars    Char_;
			[MapIgnore(false)]
			public SqlXml      Xml_;
#endif
		}

		[Test]
		public void ExecuteList1()
		{
			using (DbManager db = new DbManager("Sql"))
			{
				ArrayList list = db
					.SetCommand("SELECT * FROM Person")
					.ExecuteList(typeof(Person));
			}
		}

		[Test]
		public void ExecuteList2()
		{
			using (DbManager db = new DbManager("Sql"))
			{
				IList list = db
					.SetCommand("SELECT * FROM Person")
					.ExecuteList(new EditableArrayList(typeof(Person)), typeof(Person));
			}
		}

		[Test]
		public void ExecuteObject()
		{
			using (DbManager db = new DbManager())
			{
				Person p = (Person)db
					.SetCommand("SELECT * FROM Person WHERE PersonID = @id",
						db.Parameter("@id", 1))
					.ExecuteObject(typeof(Person));

				TypeAccessor.WriteConsole(p);
			}
		}

		[Test]
		public void ExecuteObject2()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeTest dt = (DataTypeTest)db
					.SetCommand("SELECT * FROM DataTypeTest WHERE DataTypeID = @id",
						db.Parameter("@id", 2))
					.ExecuteObject(typeof(DataTypeTest));

				TypeAccessor.WriteConsole(dt);
			}
		}

		[Test]
		public void ExecuteObject2Sql()
		{
			using (DbManager db = new DbManager())
			{
				DataTypeSqlTest dt = (DataTypeSqlTest)db
					.SetCommand("SELECT * FROM DataTypeTest WHERE DataTypeID = @id",
						db.Parameter("@id", 2))
					.ExecuteObject(typeof(DataTypeSqlTest));

				TypeAccessor.WriteConsole(dt);
			}
		}

		[Test]
		public void NewConnection()
		{
			string connectionString = "Server=.;Database=BLToolkitData;Integrated Security=SSPI";

			using (DbManager db = new DbManager(new SqlConnection(connectionString)))
			{
				db
					.SetSpCommand ("Person_SelectByName",
						db.Parameter("@firstName", "John"),
						db.Parameter("@lastName",  "Pupkin"))
					.ExecuteScalar();
			}
		}

		public class OutRefTest
		{
			public int    ID             = 5;
			public int    outputID;
			public int    inputOutputID  = 10;
			public string str            = "5";
			public string outputStr;
			public string inputOutputStr = "10";
		}

		[Test]
		public void MapOutput()
		{
			OutRefTest o = new OutRefTest();

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("OutRefTest", db.CreateParameters(o,
						new string[] {      "outputID",      "outputStr" },
						new string[] { "inputOutputID", "inputOutputStr" }))
					.ExecuteNonQuery(o);
			}

			Assert.AreEqual(5,     o.outputID);
			Assert.AreEqual(15,    o.inputOutputID);
			Assert.AreEqual("5",   o.outputStr);
			Assert.AreEqual("510", o.inputOutputStr);
		}

		public class PeturnParameter
		{
			public int Value;
		}

		[Test]
		public void MapReturnValue()
		{
			PeturnParameter e = new PeturnParameter();

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("Scalar_ReturnParameter")
					.ExecuteNonQuery("Value", e);

				Assert.AreEqual(12345, e.Value);
			}
		}

		[Test]
		public void InsertAndMapBack()
		{
			Person e = new Person();
			e.FirstName = "Crazy";
			e.LastName  = "Frog";
			e.Gender    =  Gender.Other;

			using (DbManager db = new DbManager())
			{
				db
					.SetSpCommand("Person_Insert", db.CreateParameters(e))
					.ExecuteObject(e);

				Assert.IsTrue(e.ID > 0);

				db
					.SetSpCommand("Person_Delete", db.CreateParameters(e))
					.ExecuteNonQuery();
			}
		}
	}
}