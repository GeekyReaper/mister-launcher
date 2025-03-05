using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    [JsonConverter(typeof(JsonStringEnumConverter))] 
    public enum GuestAccessState {  PENDING, APPROUVED, CONSUMED, DENIED, BLOCK, NOTFOUND }
    public enum TokenType { GUEST, ADMIN }
    public class GuestAccess
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime Expire { get; set; } = DateTime.Now;

        [JsonConverter(typeof(JsonStringEnumConverter))] 
        public GuestAccessState State { get; set; } = GuestAccessState.NOTFOUND;

        internal int _hittryconsumed = 0;
        internal string Signature { get; set; }
        public string Code { get; set; } = "";
        public int CodeIdx { get; set; } = -1;
        
        public bool isValid ()
        {
            return CodeIdx > -1;
        }
        public bool hasExpired ()
        {
            return DateTime.Now > Expire;
        }

        public bool Approuved (bool approuved, TimeSpan expiretime)
        {
            if (State == GuestAccessState.PENDING)
            {
                State = approuved ? GuestAccessState.APPROUVED : GuestAccessState.DENIED;
                Expire = Created.Add(expiretime);
                return true;
            }
            return false;

        }

        public bool Consumed(string secretvalue, TimeSpan expiretime, int maxRetry)
        {
            if (_hittryconsumed > maxRetry-1)
            {
                State = GuestAccessState.BLOCK;
                Expire = Created.Add(expiretime);
                return false;
            }

            if (State!=GuestAccessState.APPROUVED)
            {
                _hittryconsumed++;
                return false;
            }
            
            if (!checkSignature(secretvalue))
            {
                _hittryconsumed++;
                return false;
            }

            State = GuestAccessState.CONSUMED;
            Expire = Created.Add(expiretime);
            return true;
        }
       

        public void Create(int codeidx, string signature, TimeSpan expiretime, int maxcode)
        {
            CodeIdx = codeidx;
            Code = GetCode(codeidx, maxcode);
            State = GuestAccessState.PENDING;
            Expire = Created.Add(expiretime);
            Signature = signature;
        }

        internal string GetCode (int codeidx, int maxcode)
        {
            var lenthmax = maxcode.ToString().Length;
            var result = codeidx.ToString();
            if (result.Length <  lenthmax)
            {
                result = new string('0', lenthmax- result.Length) + result;
            }
            return result;
        }

        public bool checkSignature (string value)
        {
            using var provider = System.Security.Cryptography.MD5.Create();
            StringBuilder builder = new StringBuilder();

            foreach (byte b in provider.ComputeHash(Encoding.ASCII.GetBytes(value)))
                builder.Append(b.ToString("x2").ToLower());

            
            return builder.ToString() == Signature;
        }


    }
}
