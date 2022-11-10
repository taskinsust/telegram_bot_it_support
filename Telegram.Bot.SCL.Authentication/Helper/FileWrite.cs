using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BOTAuthentication.Helper
{
    public static class FileWrite
    {
        public static void Example(Exception data)
        {
            using (StreamWriter writer = new StreamWriter("WriteLines2.txt", append: true))
            {
                writer.WriteLine(data);
            }      
        }
    }

}
