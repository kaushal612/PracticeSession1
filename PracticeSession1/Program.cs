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
        //MyMigration Mg1 = new MyMigration();
        int startRange, endRange;

        ReadRangeInput(out startRange, out endRange);

        MigrationUtility mg1 = new MigrationUtility(startRange, endRange);

        Thread backgroundThread = new Thread(mg1.MigrationTask);
        backgroundThread.IsBackground = true;
        backgroundThread.Start();

        CheckingUserActivity(startRange, endRange, mg1);

        mg1.Conn.Close();
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

            if (startRange < endRange && startRange > 0 && endRange <= 1000000)
            {
                break;
            }

            Console.WriteLine($"invalid Range");
            Console.WriteLine($"Enter Range again");

        } while (true);
    }

    private static void CheckingUserActivity(int startRange, int endRange, MigrationUtility mg1)
    {
        while (true)
        {
            string input = Console.ReadLine().ToString();

            if (input.ToUpper() == "STATUS")
            {
                Console.WriteLine("\n\n---------------------------------------*****  STATUS  *****--------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - (mg1.CompletedCount) + 1} Records Migration Ongoing ");
                Console.WriteLine("-------------------------------------------------------------------------------------------------\n\n");
            }
            else if (input.ToUpper() == "CANCEL")
            {
                Console.WriteLine("\n\n-------------------------------------*****   CANCELED  *****--------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - mg1.CompletedCount + 1} Records Migration Canceled ");
                Console.WriteLine("--------------------------------------------------------------------------------------------------\n\n");
                break;
            }
            else if (mg1.MigrationCompletedFlag == true)
            {
                Console.WriteLine("Successfully Data Migrated");
                Console.WriteLine("\n\n-------------------------------------*****   COMPLETED  *****-------------------------------------");
                Console.WriteLine($"{mg1.CompletedCount} Records Migration completed ");
                Console.WriteLine($"{mg1.EndRange - mg1.StartRange - mg1.CompletedCount + 1} Records Migration Remaining ");
                Console.WriteLine("--------------------------------------------------------------------------------------------------\n\n");
                break;
            }
        }
    }
}

