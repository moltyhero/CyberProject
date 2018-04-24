using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Network namespaces
using System.Net;
using NetworkCommsDotNet;
using NetworkCommsDotNet.DPSBase;
using NetworkCommsDotNet.Tools;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using System.Net.Sockets;
using System.IO;

namespace TheMaze
{
    public class ScreenOrginizerWrapper
    {
        /// <summary>
        /// Private store of the screen orginizer data as a byte[]
        /// This will be populated automatically when the object is serialised
        /// </summary>
        [ProtoMember(1)]
        private byte[] _screenOrginizerData;


        /// <summary>
        /// The public accessor for the screen orginizer. This will be populated
        /// automatically when the object is deserialised.
        /// </summary>
        public ScreenOrginizer ScreenOrginizer{ get; set; }

        /// <summary>
        /// Private parameterless constructor required for deserialisation
        /// </summary>
        private ScreenOrginizerWrapper() { }

        /// <summary>
        /// Create a new ScreenOrginzerWrapper
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="image"></param>
        public ScreenOrginizerWrapper(ScreenOrginizer screenOrginizer)
        {
            this.ScreenOrginizer = screenOrginizer;
        }

        /// <summary>
        /// Before serialising this object convert the ScreenOrginizer into binary data
        /// </summary>
        [ProtoBeforeSerialization]
        private void Serialize()
        {
            if (ScreenOrginizer != null)
            {
                //We need to decide how to convert our ScreenOrginizer to its raw binary form here
                using (MemoryStream inputStream = new MemoryStream())
                {
                    //For basic image types the features are part of the .net framework
                    //Image.Save(inputStream, Image.RawFormat);

                    //If we wanted to include additional data processing here
                    //such as compression, encryption etc we can still use the features provided by NetworkComms.Net
                    //e.g. see DPSManager.GetDataProcessor<LZMACompressor>()

                    //Store the binary image data as bytes[]
                    _screenOrginizerData = inputStream.ToArray();
                }
            }
        }

        /// <summary>
        /// When deserialising the object convert the binary data back into an image object
        /// </summary>
        [ProtoAfterDeserialization]
        private void Deserialize()
        {
            MemoryStream ms = new MemoryStream(_screenOrginizerData);

            //If we added custom data processes we have the perform the reverse operations here before 
            //trying to recreate the image object
            //e.g. DPSManager.GetDataProcessor<LZMACompressor>()

            //ScreenOrginizer = ScreenOrginizer.FromStream(ms);
            //_imageData = null;
        }
    }



}