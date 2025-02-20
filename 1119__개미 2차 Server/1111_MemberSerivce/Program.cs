using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace _1111_MemberSerivce
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //coinfigration에 키,value 값을 등록->코드 활용가능
            Uri nettcp_uri = new Uri(ConfigurationManager.AppSettings["nettcp_uri"]);
            Uri wsdl_uri = new Uri(ConfigurationManager.AppSettings["wsdl_uri"]);
            //Contract-> Setting
            //Binding -> App.Config
            ServiceHost host = new ServiceHost(typeof(TradeWCFSerivce));

            //오픈
            host.Open();
            Console.WriteLine("채팅 서비스를 시작합니다... ");
            Console.WriteLine($"WSDL_URI :{wsdl_uri}");
            Console.WriteLine($"WSDL_URI :{nettcp_uri}");
            Console.WriteLine("멈추시려면 엔터를 눌러주세요..");
            Console.ReadLine();
            //서비스
            host.Abort();
            host.Close();


        }
    }
}
