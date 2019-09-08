﻿using System;

namespace GeoServer
{
    public partial class Client
    {
        public void SendingRandomData(int count)
        {
            Random rnd = new Random();

            // creates a number between 1 and 12
            for (int i = 0; i < count; i++)
            {
                int numb = rnd.Next(1, 12);

                //NEW STUFF
                AlternativeTestDataMsg testClass = new AlternativeTestDataMsg
                {
                    txt = name,
                    arr = Serialisation.FillArr(numb)
                };

                Send(testClass);

                numb = rnd.Next(1, 200000000);

                TestDataMsg testClass2 = new TestDataMsg
                { number = numb };

                Send(testClass2);
            }
        }

        public void DoesDllWork()
        {
            SendMessage("Geometry Dll is correctly loaded");
        }

    }
}
