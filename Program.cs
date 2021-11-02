using System;
using System.Collections.Generic;
using System.Numerics;


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
				return String.Format(format, x, y, vec.ToString(), vec.Length());
			}
			public override string ToString()
			{
				return String.Format("({0}, {1}) - {2}", x, y, vec.ToString());
			}
			public bool Equals(DataItem other)
			{
				return ((this.x == other.x) && (this.y == other.y));
			}
		}

		delegate Vector2 FdblVector2(double x, double y);

		abstract class V3Data
		{
			public string id_data { get; set; }
			public DateTime tm { get; set; }
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
					str = str + String.Format("point = ({0}, {1})  field = {2}   mod = {3}\n", it.x, it.y, it.vec.ToString(format), it.vec.Length().ToString(format));
				}
				str = str + "\n";
				return str;
			}
		}	

		class V3DataArray : V3Data
		{
			public int num_x { get; }
			public int num_y { get; }
			public double step_x { get; }
			public double step_y { get; }
			public Vector2[,] data_m { get; }
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
				string str = "";
				foreach (V3Data data in list_mc)
				{
					str = str + data.ToLongString(format) + "\n-----------------\n";
				}
				return str;
			}
			public override string ToString()
			{
				string str = "";
				foreach (V3Data data in list_mc)
				{
					str = str + data.ToString() + "\n-----------------\n";
				}
				return str;
			}
		}	

		static class Functions
		{
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


		static void Main(string[] args)
		{
			V3DataArray array_1 = new V3DataArray("array_1", DateTime.Now, 2, 4, 1.5, 1.0, Functions.Field1);
			Console.WriteLine(array_1.ToLongString("F3"));
			V3DataList list_1 = array_1;
			Console.WriteLine(list_1.ToLongString("F3"));
			Console.WriteLine(String.Format("array_1:\nCount = {0}   MaxDistance = {1}\n", array_1.Count, array_1.MaxDistance));
			Console.WriteLine(String.Format("list_1:\nCount = {0}   MaxDistance = {1}\n", list_1.Count, list_1.MaxDistance));
			V3MainCollection collection_1 = new V3MainCollection();
			collection_1.Add(array_1);
			collection_1.Add(list_1);
			collection_1.Add(new V3DataArray("array_2", DateTime.Now, 5, 0, 2.5, 1.0, Functions.Field3));
			V3DataList list_2 = new V3DataList("list_2", DateTime.Now);
			list_2.AddDefaults(5, Functions.Field3);
			collection_1.Add(list_2);
			Console.WriteLine(collection_1.ToLongString("F3"));
			collection_1.ToLongString("F3");
			int count = collection_1.Count;
			for (int i = 0; i < count; i++)
			{
				Console.WriteLine(String.Format("[{0}]   Count = {1}   MaxDistance = {2}\n", i, collection_1[i].Count, collection_1[i].MaxDistance));
			}
		}
	}
}
