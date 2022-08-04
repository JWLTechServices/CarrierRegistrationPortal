using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class ReportService : IReportService
    {
        private readonly JWLDBContext _jWLDBContext;

        public ReportService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }
        public async Task<List<audit>> GetReports()
        {
            try
            {
                var tempAudit = await _jWLDBContext.audit.AsNoTracking().Include(t => t.Users).Where(t => t.auditUser != null && t.auditUser != 0 && (t.pageName == null || t.pageName == "")).ToListAsync();
                tempAudit = ReturnNotes(tempAudit);
                tempAudit.RemoveAll(t => t.notes == null || t.notes == "" || t.notes == "null");
                tempAudit.RemoveAll(t => t.notes.Contains("from  to ."));
                tempAudit = tempAudit.OrderByDescending(t => t.auditDateTime).ToList();
                return tempAudit;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public List<audit> ReturnNotes(List<audit> tempAudit)
        {
            List<audit> audit = new List<audit>();
            if (tempAudit != null && tempAudit.Count > 0)
            {
                for (int i = 0; i < tempAudit.Count; i++)
                {
                    if (tempAudit[i].tableName == "users")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            users NewUsers = JsonConvert.DeserializeObject<users>(tempAudit[i].newValues);
                            tempAudit[i].notes = "User Added With " + NewUsers.name + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            users OldUsers = JsonConvert.DeserializeObject<users>(tempAudit[i].oldValues);
                            users NewUsers = JsonConvert.DeserializeObject<users>(tempAudit[i].newValues);
                            users Value = JsonConvert.DeserializeObject<users>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {

                                var old = OldUsers.GetType().GetProperty(columns[j]).GetValue(OldUsers, null);
                                var New = NewUsers.GetType().GetProperty(columns[j]).GetValue(NewUsers, null);
                                string name = _jWLDBContext.users.AsNoTracking().Where(t => t.userId == Value.userId).Select(t => t.name).FirstOrDefault();
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "name":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Updated Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Updated Name from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Updated Name from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "email":
                                            tempAudit[i].notes = "User " + name + " Updated Email from " + old + " to " + New + ".";
                                            break;
                                        case "password":
                                            tempAudit[i].notes = "User " + name + " Updated the Password.";
                                            break;
                                        case "userType":
                                            {
                                                New = New.ToString() == "CarrierUser" ? "Carrier User" : New;
                                                old = old.ToString() == "CarrierUser" ? "Carrier User" : old;
                                                tempAudit[i].notes = "User " + name + " Updated User Type from " + old + " to " + New + ".";
                                                break;
                                            }
                                        case "isActive":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "User " + name + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "User " + name + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "name":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Updated Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Updated Name from " + old + " to " + New + ".";
                                                }
                                            }
                                            break;
                                        case "email":
                                            a.notes = "User " + name + " Updated Email from " + old + " to " + New + ".";
                                            break;
                                        case "password":
                                            a.notes = "User " + name + " Updated the Password.";
                                            break;
                                        case "userType":
                                            {
                                                New = New.ToString() == "CarrierUser" ? "Carrier User" : New;
                                                old = old.ToString() == "CarrierUser" ? "Carrier User" : old;
                                                a.notes = "User " + name + " Updated User Type from " + old + " to " + New + ".";
                                                break;
                                            }
                                        case "isActive":
                                            {

                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "User " + name + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "User " + name + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            users id = JsonConvert.DeserializeObject<users>(tempAudit[i].keyValues);
                            users users = _jWLDBContext.users.AsNoTracking().Where(t => t.userId == id.userId).FirstOrDefault();
                            tempAudit[i].notes = "User Deleted with " + users.name + " and " + id.userId + " User ID.";
                        }
                    }

                    if (tempAudit[i].tableName == "carrierusers")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            carrierusers Newcarrierusers = JsonConvert.DeserializeObject<carrierusers>(tempAudit[i].newValues);
                            tempAudit[i].notes = "Carrier Added With " + Newcarrierusers.authorizedPerson + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            carrierusers Oldcarrierusers = JsonConvert.DeserializeObject<carrierusers>(tempAudit[i].oldValues);
                            carrierusers Newcarrierusers = JsonConvert.DeserializeObject<carrierusers>(tempAudit[i].newValues);
                            carrierusers Values = JsonConvert.DeserializeObject<carrierusers>(tempAudit[i].keyValues);
                            string data = _jWLDBContext.carrierusers.Where(t => t.cuId == Values.cuId).Select(t => t.authorizedPerson).FirstOrDefault();
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {

                                var old = Oldcarrierusers.GetType().GetProperty(columns[j]).GetValue(Oldcarrierusers, null);
                                var New = Newcarrierusers.GetType().GetProperty(columns[j]).GetValue(Newcarrierusers, null);

                                if (j == 0 && columns[j] != "mbcaUrl" && columns[j] != "ndaUrl" && columns[j] != "createdBy"
                                    && columns[j] != "CreatedDate")
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    switch (columns[j])
                                    {
                                        case "cuEmail":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Email from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Email from " + old + " to " + New + ".";
                                                }

                                                break;
                                            }
                                        case "MC":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated MC# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated MC# from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated MC# from " + old + " to " + New + ".";
                                                    }
                                                }

                                                break;
                                            }

                                        case "authorizedPerson":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Authorized Person from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Authorized Person from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "title":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Title from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Title from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "legalCompanyName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Legal Company Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Legal Company Name from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Legal Company Name from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "DBA":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated DBA from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated DBA from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated DBA from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "physicalAddress":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Physical Address from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Physical Address from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "city":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated City from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated City from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "state":
                                            {
                                                string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated State from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    string oldName = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated State from " + oldName + " to " + Name + ".";
                                                }
                                                break;
                                            }
                                        case "zipcode":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Zip code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Zip code from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "telephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "fax":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Fax from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Fax from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Fax from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "status":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    if (New.ToString() == "Inprocess")
                                                    {
                                                        New = "In-process";
                                                    }
                                                    if (New.ToString() == "Onhold")
                                                    {
                                                        New = "On-hold";
                                                    }
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Status from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (old.ToString() == "Inprocess")
                                                    {
                                                        old = "In-process";
                                                    }
                                                    if (old.ToString() == "Onhold")
                                                    {
                                                        old = "On-hold";
                                                    }
                                                    if (New.ToString() == "Inprocess")
                                                    {
                                                        New = "In-process";
                                                    }
                                                    if (New.ToString() == "Onhold")
                                                    {
                                                        New = "On-hold";
                                                    }
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Status from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "factoryCompanyName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Name from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Name from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "factoryContactName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Contact from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Contact from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Contact from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryPhysicalAddress":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Address from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Address from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Address from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryCity":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company City from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company City from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company City from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "factoryState":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company State from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Name2 = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company State from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                        string Name2 = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company State from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryZipCode":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Zip Code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Zip Code from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Zip Code from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Phone from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Phone from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryFax":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Fax from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Fax from " + old + " to (blank)   .";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Factoring Company Fax from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalPersonName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Disptach Person Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Disptach Person Name from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalPersonTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Dispatch Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Dispatch Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalAfterHoursPersonName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated After-Hours Dispatch Person Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated After-Hours Dispatch Person Name from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalAfterHoursPersonTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated After-Hours Dispatch Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated After-Hours Dispatch Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalDot":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated DOT# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated DOT# from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additionalScac":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated SCAC Code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated SCAC Code from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated SCAC Code from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalFedaralID":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Federal Tax ID from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Federal Tax ID from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additionalHazmatCertified":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Hazmat Certified? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Hazmat Certified? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additinalHazmatExpiryDate":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Hazmat Expiration Date from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Hazmat Expiration Date from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Hazmat Expiration Date from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalPreferredLanes":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Preferred Lanes from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Preferred Lanes from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Preferred Lanes from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalMajorMakets":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Operating States from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');
                                                        string Name2 = "";
                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp2[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Operating States from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');
                                                        string Name2 = "";
                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp2[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        string[] sp = New.ToString().Split(',');
                                                        string Name = "";
                                                        for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                        {
                                                            Name += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name += ",";
                                                        }
                                                        Name = Name.Remove(Name.Length - 1, 1);
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Operating States from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "authorizedSignature":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Authorized Signature from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Authorized Signature from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "completedMBCA":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Completed MBCA from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Completed MBCA from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Completed MBCA from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "reportSortKey":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Report Sort key from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Report Sort key from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Report Sort key from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ndaReturned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated NDA returned from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated NDA returned from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated NDA returned from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "onboardingCompleted":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Onboarding completed from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Onboarding completed from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Onboarding completed from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "insuranceType":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    New = New.ToString().Replace("ins", "");
                                                    New = New.ToString().Replace("Ins", "");
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type Cargo from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type Cargo from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        New = New.ToString().Replace("Ins", "");
                                                        New = New.ToString().Replace("ins", "");
                                                        old = old.ToString().Replace("ins", "");
                                                        old = old.ToString().Replace("Ins", "");
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type Cargo from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "Ins1MGeneral":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M General from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M General from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M General from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "Ins1MAuto":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "paymentTerms":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Payment Terms from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Payment Terms from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Payment Terms from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ModifiedDate":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Modified Date from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Modified Date from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "twic":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated TWIC Certified from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated TWIC Certified from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated TWIC Certified from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ca":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated CA# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated CA# from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated CA# from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isFemaleOwned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Female Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Female Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Female Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isVeteranOwned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Veteran Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Veteran Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Veteran Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isMinorityBusiness":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Minority Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Minority Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Minority Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "businessYear":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Years in Business from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Years in Business from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Years in Business from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ethnicIdentification":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string Identification = string.Empty;
                                                    if (New.ToString() == "1")
                                                        Identification = "American Indian or Alaska Native";
                                                    else if (New.ToString() == "2")
                                                        Identification = "Asian";
                                                    else if (New.ToString() == "3")
                                                        Identification = "Black or African American";
                                                    else if (New.ToString() == "4")
                                                        Identification = "Hispanic or Latino";
                                                    else if (New.ToString() == "5")
                                                        Identification = "Native Hawaiian or Other Pacific Islander";
                                                    else if (New.ToString() == "6")
                                                        Identification = "White";
                                                    else if (New.ToString() == "7")
                                                        Identification = "Other";
                                                    else if (New.ToString() == "8")
                                                        Identification = "I prefer not to say";
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Racial or ethnic identification from (not selected) to " + Identification + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Identification2 = string.Empty;
                                                        if (old.ToString() == "1")
                                                            Identification2 = "American Indian or Alaska Native";
                                                        else if (old.ToString() == "2")
                                                            Identification2 = "Asian";
                                                        else if (old.ToString() == "3")
                                                            Identification2 = "Black or African American";
                                                        else if (old.ToString() == "4")
                                                            Identification2 = "Hispanic or Latino";
                                                        else if (old.ToString() == "5")
                                                            Identification2 = "Native Hawaiian or Other Pacific Islander";
                                                        else if (old.ToString() == "6")
                                                            Identification2 = "White";
                                                        else if (old.ToString() == "7")
                                                            Identification2 = "Other";
                                                        else if (old.ToString() == "8")
                                                            Identification2 = "I prefer not to say";
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Racial or ethnic identification from " + Identification2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Identification2 = string.Empty;
                                                        if (old.ToString() == "1")
                                                            Identification2 = "American Indian or Alaska Native";
                                                        else if (old.ToString() == "2")
                                                            Identification2 = "Asian";
                                                        else if (old.ToString() == "3")
                                                            Identification2 = "Black or African American";
                                                        else if (old.ToString() == "4")
                                                            Identification2 = "Hispanic or Latino";
                                                        else if (old.ToString() == "5")
                                                            Identification2 = "Native Hawaiian or Other Pacific Islander";
                                                        else if (old.ToString() == "6")
                                                            Identification2 = "White";
                                                        else if (old.ToString() == "7")
                                                            Identification2 = "Other";
                                                        else if (old.ToString() == "8")
                                                            Identification2 = "I prefer not to say";
                                                        string Identification = string.Empty;
                                                        if (New.ToString() == "1")
                                                            Identification = "American Indian or Alaska Native";
                                                        else if (New.ToString() == "2")
                                                            Identification = "Asian";
                                                        else if (New.ToString() == "3")
                                                            Identification = "Black or African American";
                                                        else if (New.ToString() == "4")
                                                            Identification = "Hispanic or Latino";
                                                        else if (New.ToString() == "5")
                                                            Identification = "Native Hawaiian or Other Pacific Islander";
                                                        else if (New.ToString() == "6")
                                                            Identification = "White";
                                                        else if (New.ToString() == "7")
                                                            Identification = "Other";
                                                        else if (New.ToString() == "8")
                                                            Identification = "I prefer not to say";
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Racial or ethnic identification from " + Identification2 + " to " + Identification + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "milesPerYear":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Miles per year from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Miles per year from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Miles per year from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "cargoSpecification":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Cargo Specialties from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Name2 = "";
                                                        if (old != null)
                                                        {
                                                            string[] sp2 = old.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                            {
                                                                Name2 += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp2[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name2 += " ";
                                                            }
                                                            Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        }
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Cargo Specialties from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Name = "";
                                                        if (New != null)
                                                        {
                                                            string[] sp = New.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                            {
                                                                Name += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name += ",";
                                                            }
                                                            Name = Name.Remove(Name.Length - 1, 1);
                                                        }
                                                        string Name2 = "";
                                                        if (old != null)
                                                        {
                                                            string[] sp2 = old.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                            {
                                                                Name2 += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp2[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name2 += ",";
                                                            }
                                                            Name2 = Name2.Remove(Name.Length - 1, 1);
                                                        }
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Cargo Specialties from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "paymentMethods":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Payment Methods from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    string Name = "";
                                                    string Name2 = "";
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');

                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp2[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Payment Methods from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string[] sp = New.ToString().Split(',');

                                                        for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                        {
                                                            Name += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name += ",";
                                                        }
                                                        Name = Name.Remove(Name.Length - 1, 1);
                                                        string[] sp2 = old.ToString().Split(',');

                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp2[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Payment Methods from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "serviceArea":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Service Area from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Service Area from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Service Area from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "brokerOptions":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Broker or Carrier Option from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Broker or Carrier Option from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        tempAudit[i].notes = "Carrier User " + data + " Updated Broker or Carrier Option from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                if (j > 0 && columns[j] != "mbcaUrl" && columns[j] != "ndaUrl" && columns[j] != "createdBy"
                                    && columns[j] != "CreatedDate")
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "cuEmail":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Email from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Email from " + old + " to " + New + ".";
                                                }

                                                break;
                                            }
                                        case "MC":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated MC# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated MC# from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated MC# from " + old + " to " + New + ".";
                                                    }
                                                }

                                                break;
                                            }

                                        case "authorizedPerson":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Authorized Person from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Authorized Person from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "title":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Title from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Title from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "legalCompanyName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Legal Company Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Legal Company Name from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Legal Company Name from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "DBA":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated DBA from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated DBA from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated DBA from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "physicalAddress":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Physical Address from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Physical Address from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "city":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated City from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated City from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "state":
                                            {
                                                string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated State from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    string oldName = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                    a.notes = "Carrier User " + data + " Updated State from " + oldName + " to " + Name + ".";
                                                }
                                                break;
                                            }
                                        case "zipcode":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Zip code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Zip code from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "telephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "fax":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Fax from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Fax from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Fax from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "status":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    if (New.ToString() == "Inprocess")
                                                    {
                                                        New = "In-process";
                                                    }
                                                    if (New.ToString() == "Onhold")
                                                    {
                                                        New = "On-hold";
                                                    }
                                                    a.notes = "Carrier User " + data + " Updated Status from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (old.ToString() == "Inprocess")
                                                    {
                                                        old = "In-process";
                                                    }
                                                    if (old.ToString() == "Onhold")
                                                    {
                                                        old = "On-hold";
                                                    }
                                                    if (New.ToString() == "Inprocess")
                                                    {
                                                        New = "In-process";
                                                    }
                                                    if (New.ToString() == "Onhold")
                                                    {
                                                        New = "On-hold";
                                                    }
                                                    a.notes = "Carrier User " + data + " Updated Status from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "factoryCompanyName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Name from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Name from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "factoryContactName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Contact from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Contact from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Contact from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryPhysicalAddress":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Address from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Address from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Address from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryCity":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company City from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company City from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company City from " + old + " to " + New + ".";
                                                    }

                                                }
                                                break;
                                            }
                                        case "factoryState":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company State from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Name2 = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company State from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Name = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(New)).Select(t => t.stateName).FirstOrDefault();
                                                        string Name2 = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(old)).Select(t => t.stateName).FirstOrDefault();
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company State from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryZipCode":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Zip Code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Zip Code from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Zip Code from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Phone from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Phone from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "factoryFax":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Factoring Company Fax from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Fax from " + old + " to (blank)   .";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Factoring Company Fax from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalPersonName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Disptach Person Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Disptach Person Name from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalPersonTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Dispatch Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Dispatch Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalAfterHoursPersonName":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated After-Hours Dispatch Person Name from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated After-Hours Dispatch Person Name from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalAfterHoursPersonTelephone":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated After-Hours Dispatch Phone from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated After-Hours Dispatch Phone from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "addtionalDot":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated DOT# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated DOT# from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additionalScac":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated SCAC Code from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated SCAC Code from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated SCAC Code from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalFedaralID":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Federal Tax ID from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Federal Tax ID from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additionalHazmatCertified":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Hazmat Certified? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Hazmat Certified? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "additinalHazmatExpiryDate":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Hazmat Expiration Date from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Hazmat Expiration Date from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Hazmat Expiration Date from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalPreferredLanes":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Preferred Lanes from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Preferred Lanes from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Preferred Lanes from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "additionalMajorMakets":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    a.notes = "Carrier User " + data + " Updated Operating States from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');
                                                        string Name2 = "";
                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp2[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        a.notes = "Carrier User " + data + " Updated Operating States from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');
                                                        string Name2 = "";
                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp2[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        string[] sp = New.ToString().Split(',');
                                                        string Name = "";
                                                        for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                        {
                                                            Name += _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Convert.ToInt32(sp[Counterp])).Select(t => t.stateName).FirstOrDefault();
                                                            Name += ",";
                                                        }
                                                        Name = Name.Remove(Name.Length - 1, 1);
                                                        a.notes = "Carrier User " + data + " Updated Operating States from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "authorizedSignature":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Authorized Signature from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Authorized Signature from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "completedMBCA":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Completed MBCA from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Completed MBCA from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Completed MBCA from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "reportSortKey":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Report Sort key from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Report Sort key from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Report Sort key from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ndaReturned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated NDA returned from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated NDA returned from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated NDA returned from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "onboardingCompleted":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Onboarding completed from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Onboarding completed from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Onboarding completed from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "insuranceType":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    New = New.ToString().Replace("ins", "");
                                                    New = New.ToString().Replace("Ins", "");
                                                    a.notes = "Carrier User " + data + " Updated Insurance Type Cargo from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type Cargo from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        New = New.ToString().Replace("Ins", "");
                                                        New = New.ToString().Replace("ins", "");
                                                        old = old.ToString().Replace("ins", "");
                                                        old = old.ToString().Replace("Ins", "");
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type Cargo from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "Ins1MGeneral":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Insurance Type 1M General from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type 1M General from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type 1M General from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "Ins1MAuto":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Insurance Type 1M Auto from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "paymentTerms":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Payment Terms from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Payment Terms from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Payment Terms from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ModifiedDate":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Modified Date from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Modified Date from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        case "twic":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated TWIC Certified from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated TWIC Certified from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated TWIC Certified from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ca":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated CA# from (blank) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated CA# from " + old + " to (blank).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated CA# from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isFemaleOwned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Female Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Female Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Female Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isVeteranOwned":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Veteran Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Veteran Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Veteran Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "isMinorityBusiness":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Minority Owned Business? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Minority Owned Business? from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Minority Owned Business? from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "businessYear":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Years in Business from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Years in Business from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Years in Business from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "ethnicIdentification":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string Identification = string.Empty;
                                                    if (New.ToString() == "1")
                                                        Identification = "American Indian or Alaska Native";
                                                    else if (New.ToString() == "2")
                                                        Identification = "Asian";
                                                    else if (New.ToString() == "3")
                                                        Identification = "Black or African American";
                                                    else if (New.ToString() == "4")
                                                        Identification = "Hispanic or Latino";
                                                    else if (New.ToString() == "5")
                                                        Identification = "Native Hawaiian or Other Pacific Islander";
                                                    else if (New.ToString() == "6")
                                                        Identification = "White";
                                                    else if (New.ToString() == "7")
                                                        Identification = "Other";
                                                    else if (New.ToString() == "8")
                                                        Identification = "I prefer not to say";
                                                    a.notes = "Carrier User " + data + " Updated Racial or ethnic identification from (not selected) to " + Identification + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Identification2 = string.Empty;
                                                        if (old.ToString() == "1")
                                                            Identification2 = "American Indian or Alaska Native";
                                                        else if (old.ToString() == "2")
                                                            Identification2 = "Asian";
                                                        else if (old.ToString() == "3")
                                                            Identification2 = "Black or African American";
                                                        else if (old.ToString() == "4")
                                                            Identification2 = "Hispanic or Latino";
                                                        else if (old.ToString() == "5")
                                                            Identification2 = "Native Hawaiian or Other Pacific Islander";
                                                        else if (old.ToString() == "6")
                                                            Identification2 = "White";
                                                        else if (old.ToString() == "7")
                                                            Identification2 = "Other";
                                                        else if (old.ToString() == "8")
                                                            Identification2 = "I prefer not to say";
                                                        a.notes = "Carrier User " + data + " Updated Racial or ethnic identification from " + Identification2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Identification2 = string.Empty;
                                                        if (old.ToString() == "1")
                                                            Identification2 = "American Indian or Alaska Native";
                                                        else if (old.ToString() == "2")
                                                            Identification2 = "Asian";
                                                        else if (old.ToString() == "3")
                                                            Identification2 = "Black or African American";
                                                        else if (old.ToString() == "4")
                                                            Identification2 = "Hispanic or Latino";
                                                        else if (old.ToString() == "5")
                                                            Identification2 = "Native Hawaiian or Other Pacific Islander";
                                                        else if (old.ToString() == "6")
                                                            Identification2 = "White";
                                                        else if (old.ToString() == "7")
                                                            Identification2 = "Other";
                                                        else if (old.ToString() == "8")
                                                            Identification2 = "I prefer not to say";
                                                        string Identification = string.Empty;
                                                        if (New.ToString() == "1")
                                                            Identification = "American Indian or Alaska Native";
                                                        else if (New.ToString() == "2")
                                                            Identification = "Asian";
                                                        else if (New.ToString() == "3")
                                                            Identification = "Black or African American";
                                                        else if (New.ToString() == "4")
                                                            Identification = "Hispanic or Latino";
                                                        else if (New.ToString() == "5")
                                                            Identification = "Native Hawaiian or Other Pacific Islander";
                                                        else if (New.ToString() == "6")
                                                            Identification = "White";
                                                        else if (New.ToString() == "7")
                                                            Identification = "Other";
                                                        else if (New.ToString() == "8")
                                                            Identification = "I prefer not to say";
                                                        a.notes = "Carrier User " + data + " Updated Racial or ethnic identification from " + Identification2 + " to " + Identification + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "milesPerYear":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Miles per year from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Miles per year from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Miles per year from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "cargoSpecification":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    a.notes = "Carrier User " + data + " Updated Cargo Specialties from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string Name2 = "";
                                                        if (old != null)
                                                        {
                                                            string[] sp2 = old.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                            {
                                                                Name2 += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp2[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name2 += ",";
                                                            }
                                                            Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        }
                                                        a.notes = "Carrier User " + data + " Updated Cargo Specialties from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string Name = "";
                                                        if (New != null)
                                                        {
                                                            string[] sp = New.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                            {
                                                                Name += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name += ",";
                                                            }
                                                            Name = Name.Remove(Name.Length - 1, 1);
                                                        }
                                                        string Name2 = "";
                                                        if (old != null)
                                                        {
                                                            string[] sp2 = old.ToString().Split(',');
                                                            for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                            {
                                                                Name2 += _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Convert.ToInt32(sp2[Counterp])).Select(t => t.cargoName).FirstOrDefault();
                                                                Name2 += ",";
                                                            }
                                                            Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        }
                                                        a.notes = "Carrier User " + data + " Updated Cargo Specialties from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "paymentMethods":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    string[] sp = New.ToString().Split(',');
                                                    string Name = "";
                                                    for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                    {
                                                        Name += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                        Name += ",";
                                                    }
                                                    Name = Name.Remove(Name.Length - 1, 1);
                                                    tempAudit[i].notes = "Carrier User " + data + " Updated Payment Methods from (not selected) to " + Name + ".";
                                                }
                                                else
                                                {
                                                    string Name = "";
                                                    string Name2 = "";
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        string[] sp2 = old.ToString().Split(',');

                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp2[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name2 += ",";
                                                        }
                                                        Name2 = Name2.Remove(Name2.Length - 1, 1);
                                                        a.notes = "Carrier User " + data + " Updated Payment Methods from " + Name2 + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        string[] sp = New.ToString().Split(',');

                                                        for (int Counterp = 0; Counterp < sp.Length; Counterp++)
                                                        {
                                                            Name += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name += ",";
                                                        }
                                                        Name = Name.Remove(Name.Length - 1, 1);
                                                        string[] sp2 = old.ToString().Split(',');
                                                        for (int Counterp = 0; Counterp < sp2.Length; Counterp++)
                                                        {
                                                            Name2 += _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Convert.ToInt32(sp2[Counterp])).Select(t => t.paymentName).FirstOrDefault();
                                                            Name2 += " ";
                                                        }

                                                        a.notes = "Carrier User " + data + " Updated Payment Methods from " + Name2 + " to " + Name + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "serviceArea":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Service Area from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Service Area from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Service Area from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        case "brokerOptions":
                                            {
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Carrier User " + data + " Updated Broker or Carrier Option from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    if (New == null || New.ToString() == "null")
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Broker or Carrier Option from " + old + " to (not selected).";
                                                    }
                                                    else
                                                    {
                                                        a.notes = "Carrier User " + data + " Updated Broker or Carrier Option from " + old + " to " + New + ".";
                                                    }
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            carrierusers id = JsonConvert.DeserializeObject<carrierusers>(tempAudit[i].keyValues);
                            carrierusers users = _jWLDBContext.carrierusers.AsNoTracking().Where(t => t.cuId == id.cuId).FirstOrDefault();
                            tempAudit[i].notes = "Carrier Deleted with " + users.authorizedPerson + " and " + id.cuId + " Carrier ID.";
                        }
                    }

                    if (tempAudit[i].tableName == "vehicltype")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            vehicltype Newvehicltype = JsonConvert.DeserializeObject<vehicltype>(tempAudit[i].newValues);
                            tempAudit[i].notes = "Vehicle Types Added With " + Newvehicltype.vehicleName + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            vehicltype Oldvehicltype = JsonConvert.DeserializeObject<vehicltype>(tempAudit[i].oldValues);
                            vehicltype Newvehicltype = JsonConvert.DeserializeObject<vehicltype>(tempAudit[i].newValues);
                            vehicltype Value = JsonConvert.DeserializeObject<vehicltype>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {

                                var old = Oldvehicltype.GetType().GetProperty(columns[j]).GetValue(Oldvehicltype, null);
                                var New = Newvehicltype.GetType().GetProperty(columns[j]).GetValue(Newvehicltype, null);
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "vehicleName":
                                            tempAudit[i].notes = "Updated Vehicle Type from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.vehicltype.AsNoTracking().Where(t => t.vehicleId == Value.vehicleId).Select(t => t.vehicleName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Vehicle Type " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Vehicle Type " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }

                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "vehicleName":
                                            a.notes = "Updated Vehicle Type from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.vehicltype.AsNoTracking().Where(t => t.vehicleId == Value.vehicleId).Select(t => t.vehicleName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Vehicle Type " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Vehicle Type " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            vehicltype id = JsonConvert.DeserializeObject<vehicltype>(tempAudit[i].keyValues);
                            vehicltype users = _jWLDBContext.vehicltype.AsNoTracking().Where(t => t.vehicleId == id.vehicleId).FirstOrDefault();
                            tempAudit[i].notes = "Vehicle Type Deleted with " + users.vehicleName + " and " + id.vehicleId + " Vehicle Type ID.";
                        }
                    }
                    if (tempAudit[i].tableName == "cargospecialties")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            cargospecialties Newcargospecialties = JsonConvert.DeserializeObject<cargospecialties>(tempAudit[i].newValues);
                            tempAudit[i].notes = "Cargo Specialities Added With " + Newcargospecialties.cargoName + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            cargospecialties Oldcargospecialties = JsonConvert.DeserializeObject<cargospecialties>(tempAudit[i].oldValues);
                            cargospecialties Newcargospecialties = JsonConvert.DeserializeObject<cargospecialties>(tempAudit[i].newValues);
                            cargospecialties Value = JsonConvert.DeserializeObject<cargospecialties>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {
                                var old = Oldcargospecialties.GetType().GetProperty(columns[j]).GetValue(Oldcargospecialties, null);
                                var New = Newcargospecialties.GetType().GetProperty(columns[j]).GetValue(Newcargospecialties, null);
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "cargoName":
                                            tempAudit[i].notes = "Updated Cargo Specialities from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Value.cargoId).Select(t => t.cargoName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Cargo Specialities " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Cargo Specialities " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "cargoName":
                                            a.notes = "Updated Cargo Specialities from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == Value.cargoId).Select(t => t.cargoName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Cargo Specialities " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Cargo Specialities " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            cargospecialties id = JsonConvert.DeserializeObject<cargospecialties>(tempAudit[i].keyValues);
                            cargospecialties users = _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == id.cargoId).FirstOrDefault();
                            tempAudit[i].notes = "Cargo Specialities Deleted with " + users.cargoName + " and " + id.cargoId + " Cargo Specialities ID.";
                        }
                    }
                    if (tempAudit[i].tableName == "paymenttype")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            paymenttype Newvehicltype = JsonConvert.DeserializeObject<paymenttype>(tempAudit[i].newValues);
                            tempAudit[i].notes = "Payment Methods Added With " + Newvehicltype.paymentName + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            paymenttype Oldvehicltype = JsonConvert.DeserializeObject<paymenttype>(tempAudit[i].oldValues);
                            paymenttype Newvehicltype = JsonConvert.DeserializeObject<paymenttype>(tempAudit[i].newValues);
                            paymenttype Value = JsonConvert.DeserializeObject<paymenttype>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {
                                var old = Oldvehicltype.GetType().GetProperty(columns[j]).GetValue(Oldvehicltype, null);
                                var New = Newvehicltype.GetType().GetProperty(columns[j]).GetValue(Newvehicltype, null);
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "paymentName":
                                            tempAudit[i].notes = "Updated Payment Method from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Value.paymentTypeId).Select(t => t.paymentName).FirstOrDefault();
                                                tempAudit[i].notes = "Updated Payment Method Active from " + old + " to " + New + ".";
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Payment Method " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Payment Method " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "paymentName":
                                            a.notes = "Updated Payment Method from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == Value.paymentTypeId).Select(t => t.paymentName).FirstOrDefault();
                                                tempAudit[i].notes = "Updated Payment Method Active from " + old + " to " + New + ".";
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Payment Method " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Payment Method " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            paymenttype id = JsonConvert.DeserializeObject<paymenttype>(tempAudit[i].keyValues);
                            paymenttype users = _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == id.paymentTypeId).FirstOrDefault();
                            tempAudit[i].notes = "Payment Method Deleted with " + users.paymentName + " and " + id.paymentTypeId + " Payment Method ID.";
                        }
                    }
                    if (tempAudit[i].tableName == "state")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            state Newstate = JsonConvert.DeserializeObject<state>(tempAudit[i].newValues);
                            tempAudit[i].notes = "States Added With " + Newstate.stateName + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            state Oldstate = JsonConvert.DeserializeObject<state>(tempAudit[i].oldValues);
                            state Newstate = JsonConvert.DeserializeObject<state>(tempAudit[i].newValues);
                            state Value = JsonConvert.DeserializeObject<state>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {

                                var old = Oldstate.GetType().GetProperty(columns[j]).GetValue(Oldstate, null);
                                var New = Newstate.GetType().GetProperty(columns[j]).GetValue(Newstate, null);
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "stateName":
                                            tempAudit[i].notes = "Updated State Name from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Value.stateId).Select(t => t.stateName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "State " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "State " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }

                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "stateName":
                                            a.notes = "Updated State Name from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == Value.stateId).Select(t => t.stateName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "State " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "State " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            state id = JsonConvert.DeserializeObject<state>(tempAudit[i].keyValues);
                            state users = _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == id.stateId).FirstOrDefault();
                            tempAudit[i].notes = "State Deleted with " + users.stateName + " and " + id.stateId + " State ID.";
                        }
                    }
                    if (tempAudit[i].tableName == "trailer")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            trailer Newtrailer = JsonConvert.DeserializeObject<trailer>(tempAudit[i].newValues);
                            tempAudit[i].notes = "Trailer Types Added With " + Newtrailer.trailerName + " Name.";
                        }
                        if (tempAudit[i].auditType == "Update")
                        {
                            trailer Oldtrailer = JsonConvert.DeserializeObject<trailer>(tempAudit[i].oldValues);
                            trailer Newtrailer = JsonConvert.DeserializeObject<trailer>(tempAudit[i].newValues);
                            trailer Value = JsonConvert.DeserializeObject<trailer>(tempAudit[i].keyValues);
                            List<string> columns = JsonConvert.DeserializeObject<List<string>>(tempAudit[i].changedColumns);
                            for (int j = 0; j < columns.Count; j++)
                            {

                                var old = Oldtrailer.GetType().GetProperty(columns[j]).GetValue(Oldtrailer, null);
                                var New = Newtrailer.GetType().GetProperty(columns[j]).GetValue(Newtrailer, null);
                                if (j == 0)
                                {
                                    switch (columns[j])
                                    {
                                        case "trailerName":
                                            tempAudit[i].notes = "Updated Trailer Type from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.trailer.AsNoTracking().Where(t => t.trailerId == Value.trailerId).Select(t => t.trailerName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    tempAudit[i].notes = "Trailer Type " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    tempAudit[i].notes = "Trailer Type " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                }
                                if (j > 0)
                                {
                                    if (old == New)
                                    {
                                        continue;
                                    }
                                    var a = new audit();
                                    a.auditDateTime = tempAudit[i].auditDateTime;
                                    a.auditType = tempAudit[i].auditType;
                                    a.Users = tempAudit[i].Users;
                                    switch (columns[j])
                                    {
                                        case "trailerName":
                                            a.notes = "Updated Trailer Type from " + old + " to " + New + ".";
                                            break;
                                        case "isActive":
                                            {
                                                var temp = _jWLDBContext.trailer.AsNoTracking().Where(t => t.trailerId == Value.trailerId).Select(t => t.trailerName).FirstOrDefault();
                                                if (old == null || old.ToString() == "null")
                                                {
                                                    a.notes = "Trailer Type " + temp + " Updated Active? from (not selected) to " + New + ".";
                                                }
                                                else
                                                {
                                                    a.notes = "Trailer Type " + temp + " Updated Active? from " + old + " to " + New + ".";
                                                }
                                                break;
                                            }
                                        default:
                                            break;
                                    }
                                    audit.Add(a);
                                }
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            trailer id = JsonConvert.DeserializeObject<trailer>(tempAudit[i].keyValues);
                            trailer users = _jWLDBContext.trailer.AsNoTracking().Where(t => t.trailerId == id.trailerId).FirstOrDefault();
                            tempAudit[i].notes = "Trailer Type Deleted with " + users.trailerName + " and " + id.trailerId + " Trailer Type ID.";
                        }
                    }
                    if (tempAudit[i].tableName == "carrierTrailer")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            carrierTrailer carrierTrailer = JsonConvert.DeserializeObject<carrierTrailer>(tempAudit[i].newValues);
                            var temp = _jWLDBContext.carrierTrailer.AsNoTracking().Where(t => t.carrierId == carrierTrailer.carrierId && t.numberOfVehicle == carrierTrailer.numberOfVehicle && t.trailerId == carrierTrailer.trailerId).Include(t => t.carrierusers).Include(t => t.trailer).FirstOrDefault();
                            if (temp != null && temp.carrierusers != null)
                            {
                                tempAudit[i].notes = "Carrier User " + temp.carrierusers.authorizedPerson + " Added Trailer Type " + temp.trailer.trailerName + ".";
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            carrierTrailer id = JsonConvert.DeserializeObject<carrierTrailer>(tempAudit[i].keyValues);
                            var carrier = _jWLDBContext.carrierTrailer.AsNoTracking().Where(t => t.carrierTrailerId == id.carrierTrailerId).Include(t => t.trailer).Include(t => t.carrierusers).FirstOrDefault();
                            if (carrier != null && carrier.carrierusers != null)
                                tempAudit[i].notes = "Carrier User " + carrier.carrierusers.authorizedPerson + " Deleted Trailer Type " + carrier.trailer.trailerName + ".";
                        }
                    }
                    if (tempAudit[i].tableName == "carriervehicle")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            carriervehicle carrierTrailer = JsonConvert.DeserializeObject<carriervehicle>(tempAudit[i].newValues);
                            var temp = _jWLDBContext.carriervehicle.AsNoTracking().Where(t => t.carrierId == carrierTrailer.carrierId && t.numberOfVehicle == carrierTrailer.numberOfVehicle && t.vehicleId == carrierTrailer.vehicleId).Include(t => t.carrierusers).Include(t => t.vehicltype).FirstOrDefault();
                            if (temp != null && temp.carrierusers != null)
                            {
                                tempAudit[i].notes = "Carrier User " + temp.carrierusers.authorizedPerson + " Added Vehicle Type " + temp.vehicltype.vehicleName + ".";
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            carriervehicle id = JsonConvert.DeserializeObject<carriervehicle>(tempAudit[i].keyValues);
                            var carrier = _jWLDBContext.carriervehicle.AsNoTracking().Where(t => t.carrierVehicleId == id.carrierVehicleId).Include(t => t.vehicltype).Include(t => t.carrierusers).FirstOrDefault();
                            if (carrier != null && carrier.carrierusers != null)
                                tempAudit[i].notes = "Carrier User " + carrier.carrierusers.authorizedPerson + " Deleted Vehicle Type " + carrier.vehicltype.vehicleName + ".";
                        }
                    }
                    if (tempAudit[i].tableName == "authorizedpath")
                    {
                        if (tempAudit[i].auditType == "Create")
                        {
                            authorizedpath carrierTrailer = JsonConvert.DeserializeObject<authorizedpath>(tempAudit[i].newValues);
                            var temp = _jWLDBContext.authorizedpath.AsNoTracking().Where(t => t.carrierId == carrierTrailer.carrierId && t.documentPath == carrierTrailer.documentPath && t.selectedOptions == carrierTrailer.selectedOptions).Include(t => t.carrierusers).FirstOrDefault();
                            if (temp != null && temp.carrierusers != null)
                            {
                                temp.selectedOptions = temp.selectedOptions.Replace("_txt", "Other-");
                                tempAudit[i].notes = "Carrier User " + temp.carrierusers.authorizedPerson + " Added Document Type " + temp.selectedOptions + ".";
                            }
                        }
                        if (tempAudit[i].auditType == "Delete")
                        {
                            authorizedpath id = JsonConvert.DeserializeObject<authorizedpath>(tempAudit[i].keyValues);
                            var temp = _jWLDBContext.authorizedpath.AsNoTracking().Where(t => t.authorizedId == id.authorizedId).Include(t => t.carrierusers).FirstOrDefault();
                            if (temp != null && temp.carrierusers != null)
                            {
                                temp.selectedOptions = temp.selectedOptions.Replace("_txt", "Other-");
                                tempAudit[i].notes = "Carrier User " + temp.carrierusers.authorizedPerson + " Deleted Document Type " + temp.selectedOptions + ".";
                            }
                        }
                    }
                }
            }
            tempAudit.AddRange(audit);
            return tempAudit;
        }
        public async Task<List<audit>> GetReportsByUserID(int id)
        {
            var tempAudit = new List<audit>();
            if (id == 0)
            {
                tempAudit = await _jWLDBContext.audit.AsNoTracking().Include(t => t.Users).Where(t => t.auditUser != null && t.auditUser != 0 && (t.pageName == null || t.pageName == "")).ToListAsync();
            }
            else
            {
                tempAudit = await _jWLDBContext.audit.AsNoTracking().Where(t => t.auditUser == id && (t.pageName == null || t.pageName == "")).Include(t => t.Users).ToListAsync();
            }
            tempAudit = ReturnNotes(tempAudit);
            tempAudit.RemoveAll(t => t.notes == null || t.notes == "" || t.notes == "null");
            tempAudit.RemoveAll(t => t.notes.Contains("from  to ."));
            tempAudit = tempAudit.OrderByDescending(t => t.auditDateTime).ToList();
            return tempAudit;
        }
        public async Task<List<audit>> GetReportsByDate(string from, string to, int id)
        {
            var tempAudit = new List<audit>();
            DateTime datefrom = Convert.ToDateTime(from);
            DateTime dateto = Convert.ToDateTime(to);
            dateto = dateto.AddDays(1);
            if (id != null && id != 0)
            {
                tempAudit = await _jWLDBContext.audit.AsNoTracking().Where(t => t.auditDateTime > datefrom && t.auditDateTime < dateto && t.auditUser != null && t.auditUser != 0 && (t.pageName == null || t.pageName == "") && t.auditUser != null && t.auditUser != 0 && t.auditUser == id).Include(t => t.Users).ToListAsync();
            }
            else
            {
                tempAudit = await _jWLDBContext.audit.AsNoTracking().Where(t => t.auditDateTime > datefrom && t.auditDateTime < dateto && t.auditUser != null && t.auditUser != 0 && (t.pageName == null || t.pageName == "") && t.auditUser != null && t.auditUser != 0).Include(t => t.Users).ToListAsync();
            }
            tempAudit = ReturnNotes(tempAudit);
            tempAudit.RemoveAll(t => t.notes == null || t.notes == "" || t.notes == "null");
            tempAudit.RemoveAll(t => t.notes.Contains("from  to ."));
            tempAudit = tempAudit.OrderByDescending(t => t.auditDateTime).ToList();
            return tempAudit;
        }
    }
}
