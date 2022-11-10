using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using Telegram.Bot.SCL.Auth.Enums;
using Telegram.Bot.SCL.Auth.Models;

namespace Telegram.Bot.SCL.Auth.DataAccess
{
    public class UserDao : BaseDao
    {
        public DateTime Date { get; }
        public UserDao(SqlConnection connection) : base(connection)
        {

        }

        #region Single object load

        internal List<UserProfile> LoadByUserData(string FirstName = "", string LastName = "", string Username = "", long userId = 0)
        {
            string query = "";
            if (userId > 0)
            {
                query += @"select top(1) * from UserProfiles where UserId =" + userId + " ORDER BY Id desc";
            }
            else
                query = @" select top(1) * from UserProfiles where FirstName like '%" + FirstName + "%' or " +
                                "LastName like '%" + LastName + "%' or Username like '%" + Username + "%'  ORDER BY Id desc";

            //da = new SqlDataAdapter(query, this.sqLiteConnection);
            //da.SelectCommand.CommandTimeout = 250; //seconds
            //DataTable dt = new DataTable();
            //da.Fill(dt);

            //List<UserProfile> resultset = ConvertToList<UserProfile>(dt);
            //return resultset;
            this.command.CommandText = query;
            return Fill();
        }

        #endregion

        #region Operation

        public bool Insert(UserProfile obj)
        {
            try
            {
                string queryStr = string.Format(@"INSERT INTO  UserProfiles
                                               (
                                                [UserId]    
                                                ,[ContactNo] 
                                                , FirstName
                                                , LastName
                                                , UserName
                                                , UserStatus
                                                , CreationDate
                                                , Email
                                               )
                                            VALUES({0}, '{1}', '{2}', '{3}', '{4}', {5}, '{6}', '{7}')
                                          ",
                                            obj.UserId,
                                            AddSlashes(obj.ContactNo),
                                            AddSlashes(obj.FirstName),
                                            AddSlashes(obj.LastName),
                                            AddSlashes(obj.UserName),
                                            (int)UserStatusEnum.WaitingForApproval,
                                            AddSlashes(obj.CreationDate),
                                            AddSlashes(obj.Email)
                                           );
                int numOfRows = ExecuteOperationQuery(queryStr);
                return numOfRows > 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal bool UpdateEmail(UserProfile userProfile)
        {
            try
            {
                //update identity table with valid email address from here
                if (!String.IsNullOrEmpty(userProfile.Email))
                {
                    //string queryStr = string.Format(@"UPDATE UserProfiles SET [ContactNo] ='{0}', FirstName='{1}',
                    //                            LastName='{2}', Email='{3}' WHERE UserId = {4}",

                    //                          userProfile.ContactNo,
                    //                          userProfile.FirstName,
                    //                          userProfile.LastName,
                    //                          userProfile.Email,
                    //                          userProfile.UserId);

                    //int numOfRows = ExecuteOperationQuery(queryStr);

                    //update identity table 
                    string query = string.Format(@"UPDATE [dbo].[IdentityUsers] SET [Email] ='{0}', UserName='{1}'
                                                 WHERE TelegramUserId = '{2}' ",
                                                 userProfile.Email,
                                               userProfile.Email,
                                               userProfile.UserId.ToString());
                    query += @"(Select Id from [dbo].[IdentityUsers] where TelegramUserId='" + userProfile.UserId + "')";

                    //ExecuteOperationQuery(query);
                    var result = ExecuteQuery(query);
                    string querynotif = string.Format(@"UPDATE [notifications].[PushNotificationDestinations] SET [DestinationAddress] ='{0}'
                                                 WHERE SubscriberId = '{1}'",
                                                 userProfile.Email,

                                               result.ToString());
                    int updated = ExecuteOperationQuery(querynotif);
                    return updated > 0;
                }
                return true;

            }
            catch (Exception e) { return false; }
        }

        internal bool UpdateUserProfileEmail(UserProfile userProfile)
        {
            try
            {
                //update UserProfile table with valid email address from here
                if (!String.IsNullOrEmpty(userProfile.Email))
                {
                    string queryStr = string.Format(@"UPDATE UserProfiles SET FirstName='{0}',
                                                LastName='{1}', Email='{2}' WHERE UserId = {3}",

                                              userProfile.FirstName,
                                              userProfile.LastName,
                                              userProfile.Email,
                                              userProfile.UserId);

                    int numOfRows = ExecuteOperationQuery(queryStr);

                    return numOfRows > 0;
                }
                return true;

            }
            catch (Exception e) { return false; }
        }

        internal bool Update(UserProfile userProfile)
        {
            try
            {
                string queryStr = string.Format(@"UPDATE UserProfiles SET [ContactNo] ='{0}', FirstName='{1}',
                                                LastName='{2}' WHERE UserId = {3}",

                                               userProfile.ContactNo,
                                               userProfile.FirstName,
                                               userProfile.LastName,

                                               userProfile.UserId);

                int numOfRows = ExecuteOperationQuery(queryStr);

                //update identity table with valid email address from here
                try
                {
                    //if (!String.IsNullOrEmpty(userProfile.Email))
                    //{
                    //update identity table 
                    string query = string.Format(@"UPDATE [dbo].[IdentityUsers] SET PhoneNumber='{0}'
                                                 WHERE TelegramUserId = '{1}'",
                                               userProfile.ContactNo,
                                               userProfile.UserId);
                    ExecuteOperationQuery(query);
                    //}
                }
                catch
                {

                }

                return numOfRows > 0;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal bool Delete(List<int> list)
        {
            try
            {
                string queryStr = string.Format(@"DELETE FROM UserProfiles 
                                           WHERE Id in ({0})
                                          ", String.Join(",", list.ToArray())
                                              );
                ExecuteOperationQuery(queryStr);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        #endregion

        #region Others

        public List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    try
                    {
                        if (columnNames.Contains(pro.Name))
                        {
                            if (pro.Name == "CreationDate")
                            {
                                PropertyInfo pI = objT.GetType().GetProperty(pro.Name);
                                if (row[pro.Name] == DBNull.Value)
                                {
                                    pro.SetValue(objT, null);
                                }
                                else
                                {
                                    pro.SetValue(objT, Convert.ChangeType(row[pro.Name], Date.GetTypeCode()));
                                }

                            }
                            else
                            {
                                PropertyInfo pI = objT.GetType().GetProperty(pro.Name);
                                pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : Convert.ChangeType(row[pro.Name], pI.PropertyType));
                            }

                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                return objT;
            }).ToList();
        }

        private List<UserProfile> Fill()
        {
            var resList = new List<UserProfile>();
            try
            {
                //now execute query
                this.reader = this.command.ExecuteReader();
                if (this.reader.HasRows)
                {
                    while (this.reader.Read())
                    {
                        int i = 0;
                        var row = new UserProfile();
                        row.Id = reader.GetInt32(i++);
                        row.UserId = reader.GetInt64(i++);
                        row.ContactNo = reader.GetString(i++);
                        row.FirstName = reader.GetString(i++);
                        row.LastName = reader.GetString(i++);
                        row.UserName = reader.GetString(i++);
                        row.UserStatus = reader.GetInt32(i++);
                        row.CreationDate = reader.GetString(i++);
                        row.Email = reader.GetString(i++);
                        resList.Add(row);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                //close the reader
                if (this.reader != null && this.reader.IsClosed == false)
                    this.reader.Close();
            }

            //return value
            if (resList == null || resList.Count < 1)
                return null;
            return resList;
        }

        #endregion

    }
}
