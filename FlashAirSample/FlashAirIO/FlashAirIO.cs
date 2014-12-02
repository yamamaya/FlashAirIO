using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;

namespace OaktreeLab.IO {
    class FlashAirIO {
        /// <summary>
        /// Initialize new FlashAirIO instance.
        /// Use "Name" property to designate the name of FlashAir.
        /// </summary>
        public FlashAirIO() {
            Pins = new GPIOPins();
            Timeout = 3000;
        }

        /// <summary>
        /// Initialize new FlashAirIO instance.
        /// </summary>
        /// <param name="name">Name of a FlashAir to access</param>
        public FlashAirIO( string name )
            : this() {
            Name = name;
        }

        /// <summary>
        /// Name of a FlashAir to access
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Pins of the FlashAir
        /// </summary>
        public GPIOPins Pins {
            get;
            private set;
        }

        /// <summary>
        /// The number of milliseconds to wait before the request times out.
        /// Default: 3000ms
        /// </summary>
        public int Timeout {
            get;
            set;
        }

        /// <summary>
        /// GPIO data read from / write to FlashAir
        /// </summary>
        public UInt32 Data {
            get {
                UInt32 data = 0;
                if ( Pins[ 2 ].value != 0 ) {
                    data |= 0x01;
                }
                if ( Pins[ 7 ].value != 0 ) {
                    data |= 0x02;
                }
                if ( Pins[ 8 ].value != 0 ) {
                    data |= 0x04;
                }
                if ( Pins[ 9 ].value != 0 ) {
                    data |= 0x08;
                }
                if ( Pins[ 1 ].value != 0 ) {
                    data |= 0x10;
                }
                return data;
            }
            set {
                Pins[ 2 ].value = ( ( value & 0x01 ) != 0 ) ? 1 : 0;
                Pins[ 7 ].value = ( ( value & 0x02 ) != 0 ) ? 1 : 0;
                Pins[ 8 ].value = ( ( value & 0x04 ) != 0 ) ? 1 : 0;
                Pins[ 9 ].value = ( ( value & 0x08 ) != 0 ) ? 1 : 0;
                Pins[ 1 ].value = ( ( value & 0x10 ) != 0 ) ? 1 : 0;
            }
        }

        /// <summary>
        /// I/O control bits read from / write to FlashAir
        /// </summary>
        public UInt32 Ctrl {
            get {
                UInt32 ctrl = 0;
                if ( Pins[ 2 ].mode == IOMode.Write ) {
                    ctrl |= 0x01;
                }
                if ( Pins[ 7 ].mode == IOMode.Write ) {
                    ctrl |= 0x02;
                }
                if ( Pins[ 8 ].mode == IOMode.Write ) {
                    ctrl |= 0x04;
                }
                if ( Pins[ 9 ].mode == IOMode.Write ) {
                    ctrl |= 0x08;
                }
                if ( Pins[ 1 ].mode == IOMode.Write ) {
                    ctrl |= 0x10;
                }
                return ctrl;
            }
            set {
                Pins[ 2 ].mode = ( ( value & 0x01 ) != 0 ) ? IOMode.Write : IOMode.Read;
                Pins[ 7 ].mode = ( ( value & 0x02 ) != 0 ) ? IOMode.Write : IOMode.Read;
                Pins[ 8 ].mode = ( ( value & 0x04 ) != 0 ) ? IOMode.Write : IOMode.Read;
                Pins[ 9 ].mode = ( ( value & 0x08 ) != 0 ) ? IOMode.Write : IOMode.Read;
                Pins[ 1 ].mode = ( ( value & 0x10 ) != 0 ) ? IOMode.Write : IOMode.Read;
            }
        }

        /// <summary>
        /// Write current Ctrl and Data to FlashAir and modify them with new value retrieved
        /// </summary>
        /// <returns></returns>
        public virtual void GPIOReadWrite() {
            string command = "command.cgi?op=190&CTRL=0x" + Ctrl.ToString( "x2" ) + "&DATA=0x" + Data.ToString( "x2" );

            string result_str = SendCommand( command );

            Dictionary<string, string> result = ParseJsonData( result_str );
            Ctrl = ParseHex( result[ "CTRL" ] );
            Data = ParseHex( result[ "DATA" ] );
        }

        /// <summary>
        /// Write current Ctrl and Data to FlashAir and modify them with new value asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task GPIOReadWriteAsync() {
            await System.Threading.Tasks.Task.Run( () => {
                GPIOReadWrite();
            } );
        }

        /// <summary>
        /// Retrieve firmware version of the FlashAir
        /// </summary>
        /// <returns></returns>
        public string GetVersion() {
            string command = "command.cgi?op=108";

            string result_str = SendCommand( command );

            return result_str;
        }

        /// <summary>
        /// Retrieve firmware version of the FlashAir asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetVersionAsync() {
            string ver = null;
            await System.Threading.Tasks.Task.Run( () => {
                ver = GetVersion();
            } );
            return ver;
        }

        /// <summary>
        /// Write to shared memory
        /// </summary>
        /// <param name="address">Address to write, 0 - 511</param>
        /// <param name="data">Data to write</param>
        public void WriteSharedMemory( UInt32 address, string data ) {
            string command = "command.cgi?op=131&ADDR=" + address.ToString() + "&LEN=" + data.Length.ToString() + "&DATA=" + data;
            SendCommand( command );
        }

        /// <summary>
        /// Write to shared memory asynchronously
        /// </summary>
        /// <param name="address">Address to write, 0 - 511</param>
        /// <param name="data">Data to write</param>
        public async Task WriteSharedMemoryAsync( UInt32 address, string data ) {
            await System.Threading.Tasks.Task.Run( () => {
                WriteSharedMemory( address, data );
            } );
        }

        /// <summary>
        /// Read from shared memory
        /// </summary>
        /// <param name="address">Address to read, 0 - 511</param>
        /// <param name="length">Length to read, 0 - 511</param>
        /// <returns>Data read from shared memory</returns>
        public string ReadSharedMemory( UInt32 address, UInt32 length ) {
            string command = "command.cgi?op=130&ADDR=" + address.ToString() + "&LEN=" + length.ToString();
            return SendCommand( command );
        }

        /// <summary>
        /// Read from shared memory asynchronously
        /// </summary>
        /// <param name="address">Address to read, 0 - 511</param>
        /// <param name="length">Length to read, 0 - 511</param>
        /// <returns>Data read from shared memory</returns>
        public async Task<string> ReadSharedMemoryAsync( UInt32 address, UInt32 length ) {
            string res = null;
            await System.Threading.Tasks.Task.Run( () => {
                res = ReadSharedMemory( address, length );
            } );
            return res;
        }

        private string SendCommand( string command ) {
            string url = "http://" + Name + "/" + command;
            System.Diagnostics.Debug.Print( "FlashAirIO: >> [" + url + "]" );

            HttpClientHandler ch = new HttpClientHandler();
            ch.UseProxy = false;
            HttpClient client = new HttpClient( ch );
            client.Timeout = TimeSpan.FromMilliseconds( Timeout );
            HttpResponseMessage res;
            try {
                res = client.GetAsync( url ).Result;
            } catch ( AggregateException e ) {
                throw e.GetBaseException();
            }
            if ( res.StatusCode == HttpStatusCode.OK ) {
                string result_str = res.Content.ReadAsStringAsync().Result;
                System.Diagnostics.Debug.Print( "FlashAirIO: << [" + result_str + "]" );
                return result_str;
            } else {
                throw new HttpException( (int)res.StatusCode, res.ReasonPhrase );
            }
        }

        private static Dictionary<string, string> ParseJsonData( string json ) {
            string str = json.TrimStart( '{' ).TrimEnd( '}' );
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach ( string KeyValuePair in str.Split( ',' ) ) {
                string[] kv = KeyValuePair.Split( ':' );
                result[ kv[ 0 ].Trim( '"' ) ] = kv[ 1 ].Trim( '"' );
            }
            return result;
        }

        private static UInt32 ParseHex( string hex ) {
            if ( hex.IndexOf( "0x" ) == 0 ) {
                hex = hex.Substring( 2 );
            }
            return UInt32.Parse( hex, System.Globalization.NumberStyles.AllowHexSpecifier );
        }

        /// <summary>
        /// GPIO mode
        /// </summary>
        public enum IOMode {
            Read,
            Write
        }

        /// <summary>
        /// GPIO pin class
        /// </summary>
        public class GPIOPin {
            /// <summary>
            /// I/O mode
            /// </summary>
            public IOMode mode;
            /// <summary>
            /// Value, 0 or other
            /// </summary>
            public int value;
        }

        public class GPIOPins {
            private List<GPIOPin> pins;

            /// <summary>
            /// GPIO pin class
            /// </summary>
            public GPIOPins() {
                pins = new List<GPIOPin>();
                for ( int i = 0 ; i <= 9 ; i++ ) {
                    pins.Add( new GPIOPin() );
                }
            }

            /// <summary>
            /// Access to each pin
            /// </summary>
            /// <param name="PinNumber">Pin number to access, 1, 2, 7, 8 or 9</param>
            public GPIOPin this[ int PinNumber ] {
                get {
                    if ( IsValidPinNumber( PinNumber ) ) {
                        return pins[ PinNumber ];
                    } else {
                        throw new IndexOutOfRangeException();
                    }
                }
            }

            private bool IsValidPinNumber( int PinNumber ) {
                if ( PinNumber == 1 || PinNumber == 2 || ( PinNumber >= 7 && PinNumber <= 9 ) ) {
                    return true;
                }
                return false;
            }
        }
    }
}
