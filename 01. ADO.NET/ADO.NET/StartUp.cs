using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ADO.NET
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            using SqlConnection sqlConnection = 
                new SqlConnection(Config.ConnectionString);

            sqlConnection.Open();
            //Problem 01
            string vilianNames = GetVilliansNamesWithMinionsCount(sqlConnection);
            Console.WriteLine($"Villain name: {vilianNames}");
            //End Problem 01

            //Problem 02
            //Validaton of the data

            int villainCount = GetCountOfVillains(sqlConnection);
            int villainId = int.Parse(Console.ReadLine());

           /* int villainId = InputValidator("Enter villain id: ", "Invalid villain id!", villainCount);*/

            //End of validaton

            string villainName = GetVillainNameById(sqlConnection, villainId);
            Console.WriteLine($"Villain: {villainName}");

            string minionsInfoByVillain = GetMinionsInfoByVillainId(sqlConnection, villainId);
            Console.WriteLine(minionsInfoByVillain);
            //End Problem 02

            //Problem 03
            string[] minionInfo = Console.ReadLine()
                    .Split(':', StringSplitOptions.RemoveEmptyEntries)[1]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();

            string villainName2 = Console.ReadLine()
                .Split(": ", StringSplitOptions.RemoveEmptyEntries)[1];

            string result = AddNewMinion(sqlConnection, minionInfo, villainName2);
            Console.WriteLine(result);
            //End Problem 03

            //Problem 04

            int villainId2 = int.Parse(Console.ReadLine());
            string result2 = DeleteVillain(sqlConnection, villainId2);
            Console.WriteLine(result2);

            //End problem 04

            //Problem 05

            int minionId = int.Parse(Console.ReadLine());
            string result3 = IncreaseMinionAge(sqlConnection, minionId);
            Console.WriteLine(result3);

            //End Problem 05

            sqlConnection.Close();
        }

        private static string IncreaseMinionAge(SqlConnection sqlConnection, int minionId)
        {
            StringBuilder output = new StringBuilder();
            string increaseAgeQuery = @"EXEC dbo.usp_GetOlder @Minionid";

            SqlCommand increaseAgeCmd = new SqlCommand(increaseAgeQuery, sqlConnection);
            increaseAgeCmd.Parameters.AddWithValue("@Minionid", minionId);

            increaseAgeCmd.ExecuteNonQuery();

            string getMinionInfoQuery = @"SELECT 
	                                          [Name]
	                                          ,Age
                                             FROM Minions
                                          WHERE Id = @MinionId";

            SqlCommand getMinionInfoCmd = new SqlCommand(getMinionInfoQuery, sqlConnection);
            getMinionInfoCmd.Parameters.AddWithValue("@MinionId", minionId);

            using SqlDataReader infoReader = getMinionInfoCmd.ExecuteReader();

            while (infoReader.Read())
            {
                output
                    .AppendLine($"{infoReader["Name"]} - {infoReader["Age"]} years old");
            }

            return output
                .ToString()
                .TrimEnd();
        }
        private static string DeleteVillain(SqlConnection sqlConnection, int villainId)
        {
            StringBuilder output = new StringBuilder();

            string villainNameQuery = @"SELECT
	                                        [Name]
                                        FROM Villains
                                        WHERE [Id] = @VillainId";

            SqlCommand villainNameCmd = new SqlCommand(villainNameQuery, sqlConnection);
            villainNameCmd.Parameters.AddWithValue("@VillainId", villainId);

            string villaiName = (string)villainNameCmd.ExecuteScalar();
            if (villaiName == null)
            {
                return $"No such villain was found.";
            }

            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

            try
            {
                string releaseMinionQuery = @"DELETE FROM MinionsVillains
	                                            WHERE VillainId = @VillainId";

                SqlCommand releaseMinionCmd = new SqlCommand(releaseMinionQuery, sqlConnection, sqlTransaction);
                releaseMinionCmd.Parameters.AddWithValue("@VillainId", villainId);

                int minionsReleased = releaseMinionCmd.ExecuteNonQuery();

                string deleteVillainQuery = @"DELETE FROM Villains
	                                         WHERE Id = @VillainId";

                SqlCommand deleteVillainCmd = new SqlCommand(deleteVillainQuery, sqlConnection, sqlTransaction);
                deleteVillainCmd.Parameters.AddWithValue("@VillainId", villainId);
                int villainsDeleted = deleteVillainCmd.ExecuteNonQuery();

                if (villainsDeleted != 1)
                {
                    sqlTransaction.Rollback();
                }

                output
                    .AppendLine($"{villaiName} was deleted.")
                    .AppendLine($"{minionsReleased} minions were released.");

            }
            catch (Exception e  )
            {
                sqlTransaction.Rollback();
                return e.ToString();
            }

            sqlTransaction.Commit();

            return output
                .ToString()
                .TrimEnd();
        }
        private static string AddNewMinion(SqlConnection sqlConnection, string[] minionInfo, string villainName)
        {
            StringBuilder output = new StringBuilder();

            string minionName = minionInfo[0];
            int minionAge = int.Parse(minionInfo[1]);
            string minionTown= minionInfo[2];

            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

            try
            {
                int townId = GetTownId(sqlConnection, sqlTransaction, output, minionTown);
                int villainId = GetVillainId(sqlConnection, sqlTransaction, output, villainName);
                int addedMinionId = AddMinion(sqlConnection, sqlTransaction, minionName, minionAge, townId);

                string addMinionToVillainQuery = @"INSERT INTO MinionsVillains
	                                                    (MinionId, VillainId)
	                                                    VALUES
                                                    (@MinionId, @VillainId)";

                SqlCommand adddMinionToVillainCmd = new SqlCommand(addMinionToVillainQuery, sqlConnection, sqlTransaction);
                adddMinionToVillainCmd.Parameters.AddWithValue("@MinionId", addedMinionId);
                adddMinionToVillainCmd.Parameters.AddWithValue("@VillainId", villainId);

                adddMinionToVillainCmd.ExecuteNonQuery();
                output
                    .AppendLine($"Successfully added {minionName} to be minion of {villainName}.");

                sqlTransaction.Commit();
            }

            catch (Exception e)
            {
                sqlTransaction.Rollback();
                return e.ToString();
            }

                

            return output
                .ToString()
                .TrimEnd();
        }
        private static int AddMinion(SqlConnection sqlConnection, SqlTransaction sqlTransaction,
            string minionName, int minionAge, int minionTownId)
        {
            string addMinionQuery = @"INSERT INTO Minions
	                                        ([Name], Age, TownId)
	                                        VALUES
                                        (@MinionName, @MinionAge, @TownId)";

            SqlCommand addMinionCmd = new SqlCommand(addMinionQuery, sqlConnection, sqlTransaction);
            addMinionCmd.Parameters.AddWithValue("@MinionName", minionName);
            addMinionCmd.Parameters.AddWithValue("@MinionAge", minionAge);
            addMinionCmd.Parameters.AddWithValue("@TownId", minionTownId);

            addMinionCmd.ExecuteNonQuery();

            string addedMinionIdQuery = @"SELECT
	                                        [Id]
                                        FROM Minions
                                        WHERE [Name] = @MinionName AND Age = @MinionAge AND TownId = @TownId";

            SqlCommand addedMinionCmd = new SqlCommand(addedMinionIdQuery, sqlConnection, sqlTransaction);
            addedMinionCmd.Parameters.AddWithValue("@MinionName", minionName);
            addedMinionCmd.Parameters.AddWithValue("@MinionAge", minionAge);
            addedMinionCmd.Parameters.AddWithValue("@TownId", minionTownId);

            object addedMinionId = addedMinionCmd.ExecuteScalar();
            return (int)addedMinionId;
        }
        private static int GetVillainId(SqlConnection sqlConnection, SqlTransaction sqlTransaction,
            StringBuilder output, string villainName)
        {
            string villainQuery = @"SELECT
                                       [Id]
                                       FROM Villains
                                   WHERE [Name] = @VillainName";

            SqlCommand vilianIdCmd = new SqlCommand(villainQuery, sqlConnection, sqlTransaction);
            vilianIdCmd.Parameters.AddWithValue("@VillainName", villainName);

            object villainIdObj = vilianIdCmd.ExecuteScalar();
            if (villainIdObj == null)
            {
                string evilnessFactorQuery = @"SELECT 
			                                        Id
			                                       FROM EvilnessFactors
			                                       WHERE [Name] = 'Evil'";
                SqlCommand evilnessFactorCmd = new SqlCommand(evilnessFactorQuery, sqlConnection, sqlTransaction);

                int evilnessFactorId = (int)evilnessFactorCmd.ExecuteScalar();

                string insertVillainQuery = @"INSERT INTO Villains
                                                        ([Name], EvilnessFactorId)
	                                                    VALUES
                                                    (@VillainName, @EvilnessFactorId)";

                SqlCommand insertVillianCmd = new SqlCommand(insertVillainQuery, sqlConnection, sqlTransaction);
                insertVillianCmd.Parameters.AddWithValue("@VillainName", villainName);
                insertVillianCmd.Parameters.AddWithValue("@EvilnessFactorId", evilnessFactorId);

                insertVillianCmd.ExecuteNonQuery();
                output
                    .AppendLine($"Villain {villainName} was added to the database.");

                villainIdObj = vilianIdCmd.ExecuteScalar();
            }

            return (int)villainIdObj;
        }
        private static int GetTownId(SqlConnection sqlConnection, SqlTransaction sqlTransaction,
            StringBuilder output, string minionTown)
        {
            string existingMinionTown = @"SELECT
	                                        [Id]
                                        FROM Towns
                                        WHERE [Name] = @Town";

            SqlCommand townIdCmd = new SqlCommand(existingMinionTown, sqlConnection, sqlTransaction);
            townIdCmd.Parameters.AddWithValue("@Town", minionTown);

            object townIdObj = townIdCmd.ExecuteScalar();
            if (townIdObj == null)
            {
                string addTownQuery = @"INSERT INTO Towns([Name])
                                                VALUES
                                            (@TownName)";

                SqlCommand addTownCommand = new SqlCommand(addTownQuery, sqlConnection, sqlTransaction);
                addTownCommand.Parameters.AddWithValue(@"TownName", minionTown);

                addTownCommand.ExecuteNonQuery();
                output
                    .AppendLine($"Town {minionTown} was added to the database.");

                townIdObj = townIdCmd.ExecuteScalar();
            }

            return (int)townIdObj;
        }
        private static string GetMinionsInfoByVillainId(SqlConnection sqlConnection, int villainId)
        {
            StringBuilder output = new StringBuilder();

            string query = @"SELECT
	                            ROW_NUMBER() OVER (ORDER BY m.[Name]) AS [RowNumber]
	                            ,m.[Name]
	                            ,m.[Age]
	                            FROM Minions
		                            AS m
                            JOIN MinionsVillains
	                            AS mv
	                            ON mv.MinionId = m.Id
                            JOIN Villains 
	                            AS v
	                            ON mv.VillainId = v.Id
                            WHERE v.Id = @Id";

            SqlCommand getMinionsInfo = new SqlCommand(query, sqlConnection);
            getMinionsInfo.Parameters.AddWithValue("@Id", villainId);

            using SqlDataReader minionReader = getMinionsInfo.ExecuteReader();

            if (!minionReader.HasRows)
            {
                output
                    .AppendLine("(no minions)");
            }

            else
            {
                while (minionReader.Read())
                {
                    output
                        .AppendLine($"{minionReader["RowNumber"]}. {minionReader["Name"]} {minionReader["Age"]}");
                }
            }
            
            return output.ToString().TrimEnd();
        }

        private static int GetCountOfVillains(SqlConnection sqlConnection)
        {
            string query = @"SELECT 
	                            COUNT(*)
	                         FROM Villains";

            SqlCommand getCount = new SqlCommand(query, sqlConnection);

            int villainCount = (int)getCount.ExecuteScalar();

            return villainCount;
        }

        private static int InputValidator(string inputMessage, string errorMessage, int limit)
        {
            while (true)
            {
                try
                {
                    Console.Write(inputMessage);
                    int villainId = int.Parse(Console.ReadLine());

                    if (villainId < 1 || villainId > limit)
                    {
                        throw new ArgumentOutOfRangeException(errorMessage);
                    }

                    return villainId;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Invalid type of input!!! Try again!");
                }

                catch (ArgumentOutOfRangeException outOfRange) {
                    Console.WriteLine(outOfRange.Message);
                }
            }
        }

        private static string GetVillainNameById(SqlConnection sqlConnection, int villainId)
        {
            string villianNameQuery = @"
                                SELECT
	                                v.[Name]
	                                FROM Villains
		                                AS v
                                WHERE v.Id = @VillainId";

            SqlCommand getVilianNameCmd = new SqlCommand(villianNameQuery, sqlConnection);
            getVilianNameCmd.Parameters.AddWithValue("@VillainId", villainId);

            string villainName = (string)getVilianNameCmd.ExecuteScalar();

            if (villainName == null)
            {
                return $"No villain with ID {villainId} exists in the database.";
            }

            return villainName;
        }

        private static string GetVilliansNamesWithMinionsCount(SqlConnection sqlConnection)
        {
            StringBuilder output = new StringBuilder();
            string query = @"SELECT 
	                             v.[Name] AS [VilianName]
	                             ,COUNT(*) AS MinionsCount
                             FROM Villains AS v
                             JOIN MinionsVillains 
                                 AS mv
                                 ON v.Id = mv.VillainId
                             JOIN Minions 
                                 AS m
                                 ON mv.MinionId = m.Id
                             GROUP BY v.[Name]
                                 HAVING COUNT(*) > 3
                             ORDER BY MinionsCount DESC";
                
            SqlCommand cmd = new SqlCommand(query, sqlConnection);

            using SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                output
                    .AppendLine($"{reader["VilianName"]} - {reader["MinionsCount"]}");
            }

            return output.ToString().TrimEnd();
        }
    }
}
