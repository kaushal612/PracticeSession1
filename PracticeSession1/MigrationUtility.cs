// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks.Dataflow;


public class MigrationUtility
{
    public SqlConnection Conn
    {
        get;
        private set;
    }

    public int StartRange
    {
        get;
        private set;
    }
    public int EndRange
    {
        get;
        private set;
    }
    public int CompletedCount
    {
        get;
        private set;
    }
    public bool MigrationCompletedFlag
    {
        get;
        private set;
    }

    public MigrationUtility(int startRange, int endRange)
    {
        this.StartRange = startRange;
        this.EndRange = endRange;
        CompletedCount = 0;
        MigrationCompletedFlag = false;

        Conn = new SqlConnection();
        Conn.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=MyDb;Integrated Security=True";
    }

    public void MigrationTask()
    {
        //AddDataIntoSourceTable();

        int start = StartRange;
        int end = EndRange;

        while (start <= end)
        {
            var sourceData = getDataFromSourceTable(start, end);
            ExecuteBatch(sourceData);

            //int NoOfBatch = (int)Math.Ceiling((double)(sourceData.Count / 100));
            //for(int i = 1; i<=NoOfBatch; i++)
            //{
            //    Dictionary<int, (int , int)> BatchData= (Dictionary<int, (int, int)>)sourceData.Take(100);
            //    ExecuteBatch(BatchData);
            //    sourceData= (Dictionary<int, (int, int)>)sourceData.Skip(100);
            //}

            start += 100;
        }

        MigrationCompletedFlag = true;
        Console.WriteLine($"Migration completed!! Press Enter to show Details");

        // to close connection
        Conn.Close();
    }

    private void AddDataIntoSourceTable()
    {
        //---Adding 1 million record
        Conn.Open();
        for (int i = 1; i <= 1000000; i++)
        {
            SqlCommand command = new SqlCommand();
            String query = "insert into SourceTable(FirstNumber, SecondNumber) values(@n1,@n2)";

            Random random = new Random();
            int FirstNumber = random.Next(1, 1000);
            int SecondNumber = random.Next(1, 1000);

            command.Parameters.AddWithValue("@n1", FirstNumber);
            command.Parameters.AddWithValue("@n2", SecondNumber);
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            command.Connection = Conn;

            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine($"{i} data entered");
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        Conn.Close();
    }

    Dictionary<int, (int, int)> getDataFromSourceTable(int start, int end)
    {
        Dictionary<int, (int, int)> sourceData = new Dictionary<int, (int, int)>();
        

        int limit = Math.Min(100, end - start + 1);
        String query = "SELECT * FROM Sourcetable ORDER BY ID OFFSET " + (start - 1) + " ROWS FETCH NEXT " + limit + " ROWS ONLY;";
        
        SqlCommand command = new SqlCommand();
        command.CommandType = CommandType.Text;
        command.CommandText = query;
        command.Connection = Conn;

        Conn.Open();
        using (SqlDataReader reader = command.ExecuteReader())
        {
            // while there is another record present
            while (reader.Read())
            {
                int id = Convert.ToInt32(reader[0]);
                int n1 = Convert.ToInt32(reader[1]);
                int n2 = Convert.ToInt32(reader[2]);

                sourceData[id] = (n1, n2);
            }
        }
        Conn.Close();
        return sourceData;
    }

    

    private void ExecuteBatch(Dictionary<int, (int FirstNumber, int SecondNumber)> SourceData)
    {
        DataTable tbl = new DataTable();
        tbl.Columns.Add(new DataColumn("SourceID", typeof(Int32)));
        tbl.Columns.Add(new DataColumn("Sum", typeof(Int32)));

        foreach (var Data in SourceData)
        {
            int id = Data.Key;
            int n1 = Data.Value.FirstNumber;
            int n2 = Data.Value.SecondNumber;

            //   Console.WriteLine($"{id} --> {n1}, {n2}");
            DataRow dr = tbl.NewRow();
            dr["SourceID"] = id;
            dr["Sum"] = n1 + n2;

            tbl.Rows.Add(dr);
            //MigrateRecord(id, n1, n2);
        }
        SqlBulkCopy objbulk = new SqlBulkCopy(Conn);

        //assign Destination table name  
        objbulk.DestinationTableName = "DestinationTable";
        objbulk.ColumnMappings.Add("SourceID", "SourceID");
        objbulk.ColumnMappings.Add("Sum", "Sum");

        Conn.Open();

        try
        {
            //insert bulk Records into DataBase.  
            objbulk.WriteToServer(tbl);
            CompletedCount += tbl.Rows.Count;
            Console.WriteLine("-----------------------------------------------------------------------------------");
            Console.WriteLine($"Data Batch of size {tbl.Rows.Count} successffuly inserted into DestinationTable");
            Console.WriteLine("-----------------------------------------------------------------------------------");

            Thread.Sleep(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Conn.Close();
        }

    }

    //void StartMigration(Dictionary<int, (int, int)> result)
    //{
    //    Dictionary<int, (int, int)> d1 = new Dictionary<int, (int, int)>();

    //    int cnt = 0;
    //    foreach (var row in result)
    //    {
    //        cnt++;
    //        d1[row.Key] = (row.Value.Item1, row.Value.Item2);
    //        if (cnt == 100)
    //        {
    //            ExecuteBatch(d1);

    //          //  Console.WriteLine("----------------------------------------");
    //            //Console.WriteLine($"100 data batch is completed");
    //            //Console.WriteLine("----------------------------------------");

    //            d1.Clear();
    //            cnt = 0;
    //        }
    //    }
    //    if (cnt != 0)
    //    {
    //        ExecuteBatch(d1);
    //    }
    //}

    //void MigrateRecord(int id, int n1, int n2)
    //{
    //    String query = "insert into DestinationTable(SourceID, Sum) values(@SourceID,@Sum)";

    //    SqlCommand command = new SqlCommand();

    //    command.Parameters.AddWithValue("@SourceID", id);
    //    command.Parameters.AddWithValue("@Sum", n1 + n2);
    //    command.CommandType = CommandType.Text;
    //    command.CommandText = query;
    //    command.Connection = Conn;

    //    try
    //    {
    //        command.ExecuteNonQuery();
    //        //count++;
    //        Console.WriteLine($"   sourceId {id} with sum {n1 + n2} added");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine(ex.ToString());
    //    }

    //    Thread.Sleep(500);
    //}
}