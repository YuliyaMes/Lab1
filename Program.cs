using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Globalization;


namespace Lab1
{
	class Program
	{ 
		struct DataItem
		{
			public double x { get; set; }
			public double y { get; set; }
			public Vector2 vec { get; set; }
			public DataItem(double rx, double ry, Vector2 vector)
			{
				x = rx;
				y = ry;
				vec = vector;
			}
			public string ToLongString(string format)
			{
				return String.Format("point = ({0}, {1})   field = {2}   mod = {3}\n", x, y, vec.ToString(format), vec.Length().ToString(format)); 
			}
			public override string ToString()
			{
				return String.Format("{0};{1} - {2};{3}", x, y, vec.X, vec.Y);
			}
		}

		delegate Vector2 FdblVector2(double x, double y);

		abstract class V3Data:IEnumerable<DataItem>
		{
			public string id_data { get; protected set; }
			public DateTime tm { get; protected set; }
			protected V3Data() {}
			public V3Data(string a, DateTime t)
			{
				id_data = a;
				tm = t;
			}
			public abstract int Count { get; }
			public abstract double MaxDistance { get; }
			public abstract string ToLongString(string format);
			public override string ToString()
			{
				return String.Format("V3Data id_data = {0}, tm = {1}\n\n", id_data, tm);
			}
			public abstract IEnumerator<DataItem> GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator()
            {
				return GetEnumerator();
            }
		}
			
		class V3DataList : V3Data
		{
			public List<DataItem> data_list { get; }
			public V3DataList(string ob, DateTime t)
			{
				id_data = ob;
				tm = t;
				data_list = new List<DataItem>();
			}
			public bool Add(DataItem newItem)
			{
				if (data_list.Contains(newItem))
				{
					return false;
				}
				else
				{
					data_list.Add(newItem);
				}
				return true;
			}
			public int AddDefaults(int nItems, FdblVector2 F)
			{
				int s = 0;
				for (int i = 0; i < nItems; i++)
				{
					if (this.Add(new DataItem(i, i, F(i, i))))
					{
						s = s + 1;
					}
				}
				return s;
			}
			public override int Count
			{
				get
				{
					return data_list.Count;
				}
			}
			public override double MaxDistance
			{
				get
				{
					double maxD = 0;
					double dist2;
					foreach (DataItem it1 in data_list)
					{
						foreach (DataItem it2 in data_list)
						{
							dist2 = (it1.x - it2.x) * (it1.x - it2.x) + (it1.y - it2.y) * (it1.y - it2.y);
							if (dist2 > maxD)
							{
								maxD = dist2;
							}
						}
					}
					return Math.Sqrt(maxD);
				}
			}
			public override string ToString()
			{
				return String.Format("V3DataList {0} {1} Count = {2}\n\n", id_data, tm, Count);
			}
			public override string ToLongString(string format)
			{
				string str = String.Format("V3DataList {0} {1} Count = {2}\n", id_data, tm, Count);
				foreach (DataItem it in data_list)
				{
					str = str + String.Format("point = ({0}, {1})   field = {2}   mod = {3}\n", it.x, it.y, it.vec.ToString(format), it.vec.Length().ToString(format));
				}
				str = str + "\n";
				return str;
			}
			public override IEnumerator<DataItem> GetEnumerator()
            {
				return new V3DataListEnum(this);
            }
			public static CultureInfo culture_info = new CultureInfo("en-US");
			public bool SaveAsText(string filename)
            {
				bool ok = true;
				FileStream file_stream = File.Open(filename, FileMode.OpenOrCreate);
				try
				{
					StreamWriter sw = new StreamWriter(file_stream);
					sw.WriteLine(id_data);
					sw.WriteLine(tm.ToString(culture_info.DateTimeFormat));
					sw.WriteLine(Count);
					foreach (DataItem data in data_list)
                    {
						sw.WriteLine(data.ToString());
					}
					sw.Flush();
					sw.Dispose();
					sw.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					ok = false;
				}
				finally
				{
					if (file_stream != null) file_stream.Close();
				}
				return ok;
			}
			public static bool LoadAsText(string filename, ref V3DataList v3)
            {
				bool ok = true;
				FileStream file_stream = File.Open(filename, FileMode.Open);
				try
				{
					StreamReader sw = new StreamReader(file_stream);
					v3.id_data = sw.ReadLine();
					v3.tm = DateTime.Parse(sw.ReadLine(), culture_info);
					int count = int.Parse(sw.ReadLine());
					v3 = new V3DataList(v3.id_data, v3.tm);
					string[] data;
					Vector2 vec;
					double x, y;
					for (int i = 0; i< count; i++)
					{
						data = sw.ReadLine().Split(' ');
						x = double.Parse(data[0].Split(';')[0]);
						y = double.Parse(data[0].Split(';')[1]);
						vec = new Vector2(float.Parse(data[2].Split(';')[0]), float.Parse(data[2].Split(';')[1]));
						v3.Add(new DataItem(x, y, vec));
					}
					sw.Dispose();
					sw.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					ok = false;
				}
				finally
				{
					if (file_stream != null) file_stream.Close();
				}
				return ok;
			}
		}	

		class V3DataListEnum:IEnumerator<DataItem>
        {
			private int current = -1;
			V3DataList list;
			public V3DataListEnum(V3DataList list)
            {
				this.list = list;
            }
			public DataItem Current
            { get{ return list.data_list[current]; } }
			object IEnumerator.Current
			{ get { return Current; } }
			public void Reset()
            {
				current = -1;
            }
			public bool MoveNext()
            {
				current++;
				return (current < list.Count);
            }
			void IDisposable.Dispose() { }
		}

		class V3DataArray : V3Data
		{
			public int num_x { get; private set; }
			public int num_y { get; private set; }
			public double step_x { get; private set; }
			public double step_y { get; private set; }
			public Vector2[,] data_m { get; private set; }
			public V3DataArray(string a, DateTime t)
			{
				id_data = a;
				tm = t;
				data_m = new Vector2[0, 0];
			}
			public V3DataArray(string a, DateTime t, int nx, int ny, double sx, double sy, FdblVector2 F)
			{
				id_data = a;
				tm = t;
				num_x = nx;
				num_y = ny;
				step_x = sx;
				step_y = sy;
				data_m = new Vector2[nx, ny];
				for (int i = 0; i < nx; i++)
				{
					for (int j = 0; j < ny; j++)
					{
						data_m[i, j] = F(i * step_x, j * step_y);
					}
				}
			}
			public override int Count
			{
				get
				{
					return num_x * num_y;
				}
			}
			public override double MaxDistance
			{
				get
				{
					if (num_x == 0 || num_y == 0) return 0;
					return Math.Sqrt((num_x-1) * step_x * (num_x-1) * step_x + (num_y-1) * step_y * (num_y-1) * step_y);
				}
			}
			public override string ToString()
			{
				return String.Format("V3DataArray {0} {1} num_x = {2} num_y = {3} step_x = {4} step_y = {5}\n\n", id_data, tm, num_x, num_y, step_x, step_y);
			}
			public override string ToLongString(string format)
			{
				double a, b;
				string str = String.Format("V3DataArray {0} {1} num_x = {2} num_y = {3} step_x = {4} step_y = {5}\n", id_data, tm, num_x, num_y, step_x, step_y);
				for (int i = 0; i < num_x; i++)
				{
					for (int j = 0; j < num_y; j++)
					{
						a = i * step_x;
						b = j * step_y;
						str = str + String.Format("point = ({0}, {1})   field = {2}   mod = {3}\n", a.ToString(format), b.ToString(format),
							data_m[i, j].ToString(format), data_m[i, j].Length().ToString(format));
					}
				}
				str = str + "\n";
				return str;
			}
			public static implicit operator V3DataList(V3DataArray arr)
			{
				V3DataList list = new V3DataList(arr.id_data, arr.tm);
				for (int i = 0; i < arr.num_x; i++)
				{
					for (int j = 0; j < arr.num_y; j++)
					{
						list.data_list.Add(new DataItem(i * arr.step_x, j * arr.step_y, arr.data_m[i, j]));
					}
				}
				return list;
			}
			public override IEnumerator<DataItem> GetEnumerator()
			{
				return new V3DataArrayEnum(this);
			}
			public static CultureInfo culture_info = new CultureInfo("en-US");
			public bool SaveBinary(string filename)
            {
				bool ok = true;
				FileStream file_stream = null;
				try
				{
					file_stream = File.Open(filename, FileMode.OpenOrCreate);
					BinaryWriter write_func = new BinaryWriter(file_stream);
					write_func.Write(id_data);
					write_func.Write(tm.ToString(culture_info.DateTimeFormat));
					write_func.Write(num_x);
					write_func.Write(num_y);
					write_func.Write(step_x);
					write_func.Write(step_y);
					for (int i = 0; i < num_x; i++)
                    {
						for (int j = 0; j < num_y; j++)
                        {
							write_func.Write(data_m[i, j].X);
							write_func.Write(data_m[i, j].Y);
						}
                    }
					write_func.Flush();
					write_func.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					ok = false;
				}
				finally
				{
					if (file_stream != null) file_stream.Close();
				}
				return ok;
			}
			public static bool LoadBinary(string filename, ref V3DataArray v3)
            {
				bool ok = true;
				FileStream file_stream = null;
				try
				{
					file_stream = File.Open(filename, FileMode.OpenOrCreate);
					BinaryReader read_func = new BinaryReader(file_stream);
					v3.id_data = read_func.ReadString(); 
					v3.tm = DateTime.Parse(read_func.ReadString(), culture_info); 
					v3.num_x = read_func.ReadInt32();
					v3.num_y = read_func.ReadInt32();
					v3.step_x = read_func.ReadDouble(); 
					v3.step_y = read_func.ReadDouble(); 
					v3 = new V3DataArray(v3.id_data, v3.tm, v3.num_x, v3.num_y, v3.step_x, v3.step_y, Functions.F0);
					for (int i = 0; i < v3.num_x; i++)
					{
						for (int j = 0; j < v3.num_y; j++)
						{
							v3.data_m[i, j] = new Vector2(read_func.ReadSingle(), read_func.ReadSingle());
						}
					}
					read_func.Close();
				}
				catch (Exception e)
                {
					Console.WriteLine(e.Message);
					ok = false;
                }
				finally
                {
					if (file_stream != null) file_stream.Close();
				}
				return ok;
			}
		}

		class V3DataArrayEnum : IEnumerator<DataItem>
		{
			private int current = -1;
			V3DataArray array;
			public V3DataArrayEnum(V3DataArray array)
			{
				this.array = array;
			}
			public DataItem Current
			{ 
				get 
				{
					int i = current / array.num_y;
					int j = current % array.num_y;
					return new DataItem(i * array.step_x, j * array.step_y, array.data_m[i, j]);
				} 
			}
			object IEnumerator.Current
			{ get { return Current; } }
			public void Reset()
			{
				current = -1;
			}
			public bool MoveNext()
			{
				current++;
				return (current < array.Count);
			}
			void IDisposable.Dispose() { }
		}

		class V3MainCollection
		{
			private List<V3Data> list;
			public List<V3Data> list_mc { get { return list; } }
			public V3MainCollection()
            {
                list = new List<V3Data>();
            }
			public int Count
			{
				get
				{
					return list_mc.Count;
				}
			}
			public V3Data this[int i]
			{
				get
				{
					return list_mc[i];
				}
			}
			public bool Contains(string ID)
			{
				bool is_in = false;
				foreach (V3Data item in list_mc)
                {
					if (item.id_data == ID) is_in = true;
                }
				return is_in;
			}
			public bool Add(V3Data data)
			{
				if (this.Contains(data.id_data))
				{
					return false;
				}
				else
				{
					list_mc.Add(data);
				}
				return true;
			}
			public string ToLongString(string format)
			{
				string str = String.Format("\nV3MainCollection   Count = {0}\n\n-----------------\n", this.Count);
				foreach (V3Data data in list_mc)
				{
					str = str + data.ToLongString(format) + "-----------------\n";
				}
				return str;
			}
			public override string ToString()
			{
				string str = String.Format("V3MainCollection   Count = {0}\n\n-----------------\n", this.Count); ;
				foreach (V3Data data in list_mc)
				{
					str = str + data.ToString() + "-----------------\n";
				}
				return str;
			}
			public double AverageDist
            {
                get
                {
					if (this.Count == 0) return double.NaN;
					var query = from V3Data data in this.list_mc
								from DataItem data_it in data
								select Math.Sqrt(data_it.x*data_it.x + data_it.y * data_it.y);
					double average_dist;
					try { average_dist = query.Average(); }
					catch (System.InvalidOperationException) { average_dist = double.NaN; }
					return average_dist;
                }
            }
			public IEnumerable<float> DiffList
            {
                get
                {
					if (this.Count == 0) return null;
					var diff_query = from V3Data data in this.list_mc
									 where data.Count > 0
									 select (data.Max(x => x.vec.Length()) - data.Min(x => x.vec.Length()));
					return diff_query;
                }
            }
			public IEnumerable<IGrouping<double, DataItem>> Group_x
            {
                get
                {
					if (this.Count == 0) return null;
					var query_x = from V3Data data in this.list_mc
								  from DataItem data_it in data
								  group data_it by data_it.x into group_x
								  select group_x;
					return query_x;
				}
            }
		}	

		static class Functions
		{
			public static Vector2 F0(double x, double y)
			{
				return new Vector2((float)0, (float)0);
			}
			public static Vector2 Field1(double x, double y)
			{
				return (new Vector2((float)(x * 2), (float)(y * 3)));
			}
			public static Vector2 Field2(double x, double y)
			{
				return (new Vector2((float)(x / (x * x + y * y)), (float)(y / (x * x + y * y))));
			}
			public static Vector2 Field3(double x, double y)
			{
				return (new Vector2((float)(-y), (float)(-x)));
			}
		}




		static void TestWriteRead()
        {
			V3DataArray array_1 = new V3DataArray("array_1", DateTime.Now, 2, 4, 1.5, 2, Functions.Field1);
			V3DataArray array_2 = new V3DataArray("array_2", DateTime.Now);
			array_1.SaveBinary("file_1.txt");
			V3DataArray.LoadBinary("file_1.txt", ref array_2);
			Console.WriteLine("Saved V3DataArray:\n" + array_1.ToLongString("F3"));
			Console.WriteLine("Loaded V3DataArray:\n" + array_2.ToLongString("F3"));

			V3DataList list_1 = new V3DataList("list_1", DateTime.Now);
			list_1.AddDefaults(5, Functions.Field3);
			V3DataList list_2 = new V3DataList("list_2", DateTime.Now);
			list_1.SaveAsText("file_2.txt");
			V3DataList.LoadAsText("file_2.txt", ref list_2);
			Console.WriteLine("Saved V3DataList:\n" + list_1.ToLongString("F3"));
			Console.WriteLine("Loaded V3DataList:\n" + list_2.ToLongString("F3"));
		}

		static void TestLinq()
        {
			V3MainCollection collection_1 = new V3MainCollection();

			V3DataArray array_1 = new V3DataArray("array_1", DateTime.Now, 2, 1, 1.5, 1, Functions.Field1);
			V3DataArray array_2 = new V3DataArray("array_2", DateTime.Now, 5, 0, 2.5, 1.0, Functions.Field3);
			V3DataList list_1 = new V3DataList("list_1", DateTime.Now);
			list_1.AddDefaults(2, Functions.Field3);
			V3DataList list_2 = new V3DataList("list_2", DateTime.Now);
			list_2.AddDefaults(3, Functions.F0);
			V3DataList list_3 = new V3DataList("list_3", DateTime.Now);

			collection_1.Add(array_1);
			collection_1.Add(array_2);
			collection_1.Add(list_1);
			collection_1.Add(list_2);
			collection_1.Add(list_3);

			Console.WriteLine(collection_1.ToLongString("F3"));

			Console.WriteLine(String.Format("AverageDist = {0}\n\n", collection_1.AverageDist));

			Console.WriteLine("List of max-min field difference:\n");
			foreach (float f in collection_1.DiffList)
			{
				Console.WriteLine($"{f}\n");
			}

			Console.WriteLine("List of DataItems for each x-key:\n");
			foreach (var x in collection_1.Group_x)
			{
				Console.WriteLine($"     Key: {x.Key}\n");
				foreach (DataItem data in x)
				{
					Console.WriteLine(data.ToLongString("F3"));
				}
			}
		}

		static void Main(string[] args)
		{
			try
			{
				TestWriteRead();
				TestLinq();
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}\n");
			}
		}
	}
}
