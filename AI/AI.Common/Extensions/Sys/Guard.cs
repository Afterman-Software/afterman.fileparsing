using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public static class Guard
    {
        public static void IsNotNull<T>(string name, T val)
        {
            if (val == null)
                throw new InvalidOperationException(String.Format("{0} is null.  This is not allowed.", name));

        }

        public static void IsNotZero(string name,int num)
        {
            if(num == 0)
                throw new InvalidOperationException(String.Format("{0} is 0.  This is not allowed.", name));
        }

        public static void IsNotZero(string name, long num)
        {
            if (num == 0)
                throw new InvalidOperationException(String.Format("{0} is 0.  This is not allowed.", name));
        }

        public static void IsNotZero(string name, float num)
        {
            if (num == 0)
                throw new InvalidOperationException(String.Format("{0} is 0.  This is not allowed.", name));
        }

        public static void IsNotZero(string name, decimal num)
        {
            if (num == 0)
                throw new InvalidOperationException(String.Format("{0} is 0.  This is not allowed.", name));
        }

        public static void IsNotZero(string name, short num)
        {
            if (num == 0)
                throw new InvalidOperationException(String.Format("{0} is 0.  This is not allowed.", name));
        }
    }

