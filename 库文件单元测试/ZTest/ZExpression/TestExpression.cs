using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestDLL;

namespace ZTest.ZExpression
{
    public class TestExpression
    {
        public void Start()
        {
            ToPrint.Print("");
            ConstantExpression expA = Expression.Constant(30.0d);
            MethodCallExpression expCall = Expression.Call(null, typeof(Math).GetMethod("Sin", BindingFlags.Static | BindingFlags.Public), expA);
            ToPrint.Print(expCall.ToString());
            Console.ReadKey();
        }
    }
}
