using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ExcelReader : DataReader
{
    DataTable datatable;

    protected override void Load(string file)
    {
        datatable = ReadExcelFile();
    }

    protected override int GetRowCount()
    {
        return datatable.Rows.Count + 1;
    }

    protected override int GetColCount()
    {
        return datatable.Columns.Count;
    }

    protected override string GetCellContent(int row, int col)
    {
        return datatable.Rows[row - 2][col - 1].ToString();
    }

    public DataTable ReadExcelFile()
    {
        string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
        OleDbConnection conn = new OleDbConnection(strConn);
        conn.Open();
        DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
        string tableName = schemaTable.Rows[0][2].ToString().Trim();
        DataTable dt = new DataTable();
        new OleDbDataAdapter("select * from [" + tableName + "]", conn).Fill(dt);
        conn.Close();

        return dt;
    }
}
