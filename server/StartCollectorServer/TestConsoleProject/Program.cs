using LMLiblary.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestConsoleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            float number = 1.5f;
            Console.WriteLine(Math.Round(number, 0));
            Console.ReadLine();
            //List<MPlayer> listTest = new List<MPlayer>()
            //{
            //    new MPlayer()
            //    {
            //        name = "ducanh",
            //        id =1
            //    },
            //    new MPlayer()
            //    {
            //        name = "hoanganh",
            //        id = 2
            //    }
            //};

            //byte[] arrByte = LMLiblary.General.GeneralFunc.Serialize(listTest);

            //List<MPlayer> listDeserialize = LMLiblary.General.GeneralFunc.Deserialize<List<MPlayer>>(arrByte);

            //foreach (var player in listDeserialize)
            //{
            //    Console.WriteLine(player.name);
            //}
            
        }
    }
}
