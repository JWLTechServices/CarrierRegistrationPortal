using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Excel = Microsoft.Office.Interop.Excel;
namespace CarrierDataPushing
{
    public class clsExcelHelper
    {
        public static DataSet ImportExcelXLSX(string Filepath, bool hasHeaders, bool FutureMapping = false)
        {
            clsCommon objCommon = new clsCommon();
            DataSet output = new DataSet();

            string HDR = (hasHeaders ? "Yes" : "No");

            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Filepath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=1\"";


            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();

                System.Data.DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] {
                null,
                null,
                null,
                "TABLE"
            });
                int i = 0;
                foreach (DataRow row in dt.Rows)
                {
                    string sheet = string.Empty;
                    if (FutureMapping == true)
                    {
                        sheet = row["TABLE_NAME"].ToString();
                    }
                    else
                    {
                        if (i == 0)
                        {
                            sheet = "Template$"; //row["TABLE_NAME"].ToString();
                        }
                        //else if (i == 1)
                        //{
                        //    sheet = "Dispatch Track$"; //row["TABLE_NAME"].ToString();

                        //}
                    }
                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
                    cmd.CommandType = CommandType.Text;


                    System.Data.DataTable outputTable = new System.Data.DataTable(sheet);

                    output.Tables.Add(outputTable);

                    OleDbDataAdapter d = new OleDbDataAdapter(cmd);
                    try
                    {

                        d.Fill(outputTable);
                        if (i == 0)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        objCommon.WriteErrorLog(ex, "ImportExcelXLSX");
                    }
                    i++;
                }
            }

            if (HDR == "No")
            {
                foreach (DataColumn column in output.Tables[0].Columns)
                {
                    string cName = output.Tables[0].Rows[0][column.ColumnName].ToString();
                    if (!output.Tables[0].Columns.Contains(cName) && cName != "")
                    {
                        column.ColumnName = cName;
                    }
                }
                output.Tables[0].Rows[0].Delete();
                output.Tables[0].AcceptChanges();
            }
            return output;
        }

        public static void ExportDataToXLSX(System.Data.DataTable dt, string strInputFilePath, string fileName)
        {
            clsCommon objCommon = new clsCommon();
            try
            {
                string strFilePath;

                if (!System.IO.Directory.Exists(strInputFilePath + @"\"))
                    System.IO.Directory.CreateDirectory(strInputFilePath + @"\");

                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);


                strFilePath = strInputFilePath + @"\" + fileName + ".xlsx"; // ".csv";

                Application oXL;
                Workbook oWB;
                Worksheet oSheet;
                Microsoft.Office.Interop.Excel.Range oRange;

                try
                {
                    // Start Excel and get Application object. 
                    oXL = new Microsoft.Office.Interop.Excel.Application();

                    // Set some properties 
                    oXL.Visible = false;
                    oXL.DisplayAlerts = false;

                    // Get a new workbook. 
                    oWB = oXL.Workbooks.Add(Type.Missing);

                    // Get the Active sheet 
                    oSheet = (Microsoft.Office.Interop.Excel.Worksheet)oWB.ActiveSheet;
                    oSheet.Name = dt.TableName;

                    //  sda.Fill(dt);
                    //    System.Data.DataTable dt = ds.Tables[0];
                    int rowCount = 1;
                    foreach (DataRow dr in dt.Rows)
                    {
                        rowCount += 1;
                        for (int i = 1; i < dt.Columns.Count + 1; i++)
                        {
                            // Add the header the first time through 
                            if (rowCount == 2)
                            {
                                oSheet.Cells[1, i] = dt.Columns[i - 1].ColumnName;
                            }
                            oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
                        }
                    }

                    // Resize the columns 
                    // Range c1 = oSheet.Cells[1, 1];
                    // Range c2 = oSheet.Cells[rowCount, dt.Columns.Count];
                    //  oRange = oSheet.get_Range(c1, c2);

                    oRange = oSheet.get_Range(oSheet.Cells[1, 1],
                             oSheet.Cells[rowCount, dt.Columns.Count]);

                    oRange.EntireColumn.AutoFit();

                    // Save the sheet and close 
                    oSheet = null;
                    oRange = null;

                    oWB.SaveAs(strFilePath, XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
    false, false, XlSaveAsAccessMode.xlNoChange,
    Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    oWB.Close(Type.Missing, Type.Missing, Type.Missing);
                    oWB = null;
                    oXL.Quit();
                }
                catch (Exception ex)
                {
                    objCommon.WriteErrorLog(ex, "ExportDataToXLSX");
                    throw;
                }
                finally
                {
                    // Clean up 
                    // NOTE: When in release mode, this does the trick 
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }

            }
            catch (Exception ex)
            {
                objCommon.WriteErrorLog(ex, "ExportDataToXLSX");
            }
        }


        public static void ExportOutputtoXLSXFile(DataSet ds, string strInputFilePath, string fileName, string strDatetime)
        {
            clsCommon objCommon = new clsCommon();
            try
            {
                string strOutputFileLocation;
                string strOutputFile;
                string strExecutionLogFileLocation;

                strExecutionLogFileLocation = objCommon.GetConfigValue("ExecutionLogFileLocation");
                strOutputFileLocation = strInputFilePath + @"\Outputs\Error";

                if (!System.IO.Directory.Exists(strOutputFileLocation + @"\"))
                    System.IO.Directory.CreateDirectory(strOutputFileLocation + @"\");


                int fileExtPos = fileName.LastIndexOf(".");
                if (fileExtPos >= 0)
                    fileName = fileName.Substring(0, fileExtPos);

                strOutputFile = ""+ fileName;
                strOutputFile = strOutputFileLocation + @"\" + strOutputFile + ".xlsx"; // ".csv";

                if (File.Exists(strOutputFile))
                {
                    strOutputFile = fileName;
                    strOutputFile = strOutputFileLocation + @"\" + strOutputFile + "_" + strDatetime + ".xlsx"; // ".csv";
                    // File.Delete(strOutputFile);
                }
                Excel.Application ExcelApp = new Excel.Application();
                Workbook xlWorkbook = ExcelApp.Workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
                // Loop over DataTables in DataSet.
                DataTableCollection collection = ds.Tables;
                for (int i = collection.Count; i > 0; i--)
                {
                    Sheets xlSheets = null;
                    Worksheet xlWorksheet = null;

                    //Create Excel Sheets

                    xlSheets = ExcelApp.Sheets;
                    xlWorksheet = (Worksheet)xlSheets.Add(xlSheets[1],
                                   Type.Missing, Type.Missing, Type.Missing);

                    System.Data.DataTable table = collection[i - 1];
                    xlWorksheet.Name = table.TableName;
                    for (int j = 1; j < table.Columns.Count + 1; j++)
                    {
                        ExcelApp.Cells[1, j] = table.Columns[j - 1].ColumnName;
                    }

                    // Storing Each row and column value to excel sheet

                    for (int k = 0; k < table.Rows.Count; k++)
                    {
                        for (int l = 0; l < table.Columns.Count; l++)
                        {
                            ExcelApp.Cells[k + 2, l + 1] =
                            table.Rows[k].ItemArray[l].ToString();
                        }
                    }
                    ExcelApp.Columns.AutoFit();
                }

                ExcelApp.ActiveWorkbook.SaveAs(strOutputFile, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing,
                false, false, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlNoChange,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                ExcelApp.ActiveWorkbook.Close();
                xlWorkbook = null;
                ExcelApp.Quit();

            }
            catch (Exception ex)
            {
                string strExecutionLogMessage = "Exception in ExportOutputtoXLSXFile" + System.Environment.NewLine;
                objCommon.WriteErrorLog(ex, strExecutionLogMessage);
            }
            finally
            {
                // Clean up 
                // NOTE: When in release mode, this does the trick 
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
    }
}
