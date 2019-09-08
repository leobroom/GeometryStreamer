using System;

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
                AlternativeTestData testClass = new AlternativeTestData
                {
                    txt = name,
                    arr = Serialisation.FillArr(numb)
                };

                Send(testClass);

                numb = rnd.Next(1, 200000000);

                TestData testClass2 = new TestData
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
