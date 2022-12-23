// See https://aka.ms/new-console-template for more information
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.ExceptionServices;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, Welcome to the Data Migration Utility Apllication");

        string CheckCondition = "YES";

        while (CheckCondition.ToUpper() == "YES")
        {
            int startRange, endRange;

            //Read startRange and endRange
            ReadRangeInput(out startRange, out endRange);

            MigrationUtility mg1 = new MigrationUtility(startRange, endRange);

            Thread MigrationThread = new Thread(mg1.MigrationTask);
            MigrationThread.IsBackground = true;
            MigrationThread.Start();

            CheckingUserActivity(mg1);

            MigrationThread.Join();

            mg1.Conn.Close();

            Console.WriteLine("If you want to Migrate other data then Enter - YES");
            CheckCondition = Console.ReadLine().ToString();
        }
    }

    private static void ReadRangeInput(out int startRange, out int endRange)
    {
        Console.WriteLine($"Enter range between 1 to 1000000(1 Million)");
        do
        {
            Console.WriteLine($"Enter start number:");
            var v1 = Console.ReadLine();

            while (!Int32.TryParse(v1, out startRange))
            {
                Console.WriteLine("Not a valid number, try again.");
                v1 = Console.ReadLine();
            }

            Console.WriteLine($"Enter end number:");
            var v2 = Console.ReadLine();

            while (!Int32.TryParse(v2, out endRange))
            {
                Console.WriteLine("Not a valid number, try again.");
                v2 = Console.ReadLine();
            }

            if (startRange < endRange && startRange >= 1 && endRange <= 1000000)
            {
                break;
            }

            Console.WriteLine($"Invalid Range");
            Console.WriteLine($"Enter Range again");

        } while (true);
    }

    private static void CheckingUserActivity(MigrationUtility mg1)
    {
        while (true)
        {
            string input = Console.ReadLine().ToString();

            if (input.ToUpper() == "CANCEL")
            {
                mg1.RequestToCancel = true;
                Console.WriteLine("\n\n-------------------------------------*****   MIGRATION CANCELED  *****--------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - mg1.CompletedCount + 1} Records Migration Canceled ");
                Console.WriteLine("--------------------------------------------------------------------------------------------------\n\n");

                break;
            }
            else if (input.ToUpper() == "STATUS")
            {
                Console.WriteLine("\n\n---------------------------------------*****  MIGRATION STATUS  *****--------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - (mg1.CompletedCount) + 1} Records Migration Ongoing ");
                Console.WriteLine("-------------------------------------------------------------------------------------------------\n\n");
            }
            else if (mg1.MigrationCompletedFlag == true)
            {
                Console.WriteLine("\n\nSuccessfully Data Migrated");
                Console.WriteLine("\n-------------------------------------*****  MIGRATION COMPLETED  *****-------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - mg1.CompletedCount + 1} Records Migration Remaining ");
                Console.WriteLine("--------------------------------------------------------------------------------------------------\n\n");
                break;
            }
        }
    }
}

