using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

abstract class DataReader
{
    protected string filename;

    protected abstract void Load(string file);

    protected abstract int GetRowCount();

    protected abstract int GetColCount();

    protected abstract string GetCellContent(int row, int col);

    public string FileName
    {
        get
        {
            return filename;
        }
        set
        {
            filename = value;
            Load(filename);
        }
    }

    public int RowCount
    {
        get
        {
            return GetRowCount();
        }
    }

    public int ColCount
    {
        get
        {
            return GetColCount();
        }
    }

    public string this[int row, int col]
    {
        get
        {
            return GetCellContent(row, col);
        }
    }
}
